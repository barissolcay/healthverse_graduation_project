using AspNetCoreRateLimit;
using HealthVerse.Gamification.Domain.Services;
using HealthVerse.Competition.Application.Services;
using HealthVerse.Competition.Infrastructure;
using HealthVerse.Social.Infrastructure;
using HealthVerse.Tasks.Infrastructure;
using HealthVerse.Missions.Infrastructure;
using HealthVerse.Gamification.Infrastructure;
using HealthVerse.Notifications.Infrastructure;
using HealthVerse.Identity.Infrastructure;
using HealthVerse.Infrastructure.Auth;
using HealthVerse.Infrastructure.Clock;
using HealthVerse.Infrastructure.Persistence;
using HealthVerse.Infrastructure.Jobs;
using HealthVerse.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// 1. Servisler
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<HealthVerse.Api.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HealthVerse API", Version = "v1" });
});

// 2. Domain Services & Infrastructure
builder.Services.AddSingleton<IClock, TurkeySystemClock>();
builder.Services.AddScoped<PointCalculationService>();
builder.Services.AddScoped<StreakService>();
builder.Services.AddScoped<LeagueFinalizeService>();

// 2.5 Module Infrastructure
builder.Services.AddCompetitionInfrastructure();
builder.Services.AddSocialInfrastructure();
builder.Services.AddTasksInfrastructure();
builder.Services.AddMissionsInfrastructure();
builder.Services.AddGamificationInfrastructure();
builder.Services.AddNotificationsInfrastructure();
builder.Services.AddIdentityInfrastructure();

// 2.6 MediatR - Register all Application assemblies for handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); // API içindeki handlerlar için
    cfg.RegisterServicesFromAssembly(typeof(HealthVerse.Competition.Application.Commands.JoinLeagueCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(HealthVerse.Identity.Application.Commands.RegisterCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(HealthVerse.Social.Application.Commands.FollowUserCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(HealthVerse.Tasks.Application.Commands.ClaimTaskRewardCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(HealthVerse.Missions.Application.Commands.JoinGlobalMissionCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(HealthVerse.Gamification.Application.Commands.SyncStepsCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(HealthVerse.Notifications.Application.Commands.MarkNotificationReadCommand).Assembly);
});

// 2.8 System Infrastructure
builder.Services.AddScoped<ISystemCheckService, HealthVerse.Infrastructure.Services.SystemCheckService>();

// 2.7 Domain Event Dispatcher
builder.Services.AddScoped<DomainEventDispatcherInterceptor>();

// 3. VERİTABANI BAĞLANTISI
if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<HealthVerseDbContext>((sp, options) =>
        options.UseInMemoryDatabase("HealthVerseTestDb")
               .AddInterceptors(sp.GetRequiredService<DomainEventDispatcherInterceptor>()));
}
else
{
    // Production/Dev OR Integration (Integration uses Npgsql registration here, but it gets replaced by Factory)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<HealthVerseDbContext>((sp, options) =>
        options.UseNpgsql(connectionString, b => b.MigrationsAssembly("HealthVerse.Infrastructure"))
               .AddInterceptors(sp.GetRequiredService<DomainEventDispatcherInterceptor>()));
}

// 4. FIREBASE AUTH (skip in Test/Integration environment - tests use TestAuthHandler)
if (!builder.Environment.IsEnvironment("Test") && !builder.Environment.IsEnvironment("Integration"))
{
    builder.Services.ConfigureFirebase(builder.Configuration);
}

// 4.5 RATE LIMITING
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// 5. QUARTZ SCHEDULER
builder.Services.AddQuartz(q =>
{
    // Job'ları ekle
    q.AddJob<ExpireJob>(opts => opts.WithIdentity("ExpireJob"));
    q.AddJob<DailyStreakJob>(opts => opts.WithIdentity("DailyStreakJob"));
    q.AddJob<WeeklyLeagueFinalizeJob>(opts => opts.WithIdentity("WeeklyLeagueFinalizeJob"));
    q.AddJob<StreakReminderJob>(opts => opts.WithIdentity("StreakReminderJob"));
    q.AddJob<ReminderJob>(opts => opts.WithIdentity("ReminderJob"));
    q.AddJob<GlobalMissionFinalizeJob>(opts => opts.WithIdentity("GlobalMissionFinalizeJob"));
    q.AddJob<PartnerMissionFinalizeJob>(opts => opts.WithIdentity("PartnerMissionFinalizeJob"));
    q.AddJob<WeeklySummaryJob>(opts => opts.WithIdentity("WeeklySummaryJob"));

    // ExpireJob: Her saat çalışır
    q.AddTrigger(opts => opts
        .ForJob("ExpireJob")
        .WithIdentity("ExpireJob-trigger")
        .WithCronSchedule("0 0 * * * ?") // Her saatin başında
    );

    // DailyStreakJob: Her gün 00:05 TR (21:05 UTC)
    q.AddTrigger(opts => opts
        .ForJob("DailyStreakJob")
        .WithIdentity("DailyStreakJob-trigger")
        .WithCronSchedule("0 5 21 * * ?") // UTC 21:05 = TR 00:05
    );

    // WeeklyLeagueFinalizeJob: Her Pazartesi 00:05 TR (Pazar 21:05 UTC)
    q.AddTrigger(opts => opts
        .ForJob("WeeklyLeagueFinalizeJob")
        .WithIdentity("WeeklyLeagueFinalizeJob-trigger")
        .WithCronSchedule("0 5 21 ? * SUN") // UTC Pazar 21:05 = TR Pazartesi 00:05
    );

    // StreakReminderJob: Her gün 17:00 TR (14:00 UTC)
    q.AddTrigger(opts => opts
        .ForJob("StreakReminderJob")
        .WithIdentity("StreakReminderJob-trigger")
        .WithCronSchedule("0 0 14 * * ?") // UTC 14:00 = TR 17:00
    );

    // ReminderJob: Her saat çalışır (saatin 30. dakikasında)
    q.AddTrigger(opts => opts
        .ForJob("ReminderJob")
        .WithIdentity("ReminderJob-trigger")
        .WithCronSchedule("0 30 * * * ?") // Her saatin 30. dakikasında
    );

    // GlobalMissionFinalizeJob: Her saat çalışır (saatin 45. dakikasında)
    q.AddTrigger(opts => opts
        .ForJob("GlobalMissionFinalizeJob")
        .WithIdentity("GlobalMissionFinalizeJob-trigger")
        .WithCronSchedule("0 45 * * * ?") // Her saatin 45. dakikasında
    );

    // PartnerMissionFinalizeJob: Her Pazar 23:55 TR (Pazar 20:55 UTC)
    q.AddTrigger(opts => opts
        .ForJob("PartnerMissionFinalizeJob")
        .WithIdentity("PartnerMissionFinalizeJob-trigger")
        .WithCronSchedule("0 55 20 ? * SUN") // UTC Pazar 20:55 = TR Pazar 23:55
    );

    // WeeklySummaryJob: Her Pazartesi 09:00 TR (Pazartesi 06:00 UTC)
    q.AddTrigger(opts => opts
        .ForJob("WeeklySummaryJob")
        .WithIdentity("WeeklySummaryJob-trigger")
        .WithCronSchedule("0 0 6 ? * MON") // UTC Pazartesi 06:00 = TR Pazartesi 09:00
    );

    // MilestoneCheckJob: Her gün 02:00 TR (23:00 UTC önceki gün)
    q.AddJob<MilestoneCheckJob>(opts => opts.WithIdentity("MilestoneCheckJob"));
    q.AddTrigger(opts => opts
        .ForJob("MilestoneCheckJob")
        .WithIdentity("MilestoneCheckJob-trigger")
        .WithCronSchedule("0 0 23 * * ?") // UTC 23:00 = TR 02:00
    );

    // PushDeliveryJob: Her 30 saniyede çalışır
    q.AddJob<PushDeliveryJob>(opts => opts.WithIdentity("PushDeliveryJob"));
    q.AddTrigger(opts => opts
        .ForJob("PushDeliveryJob")
        .WithIdentity("PushDeliveryJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(30)
            .RepeatForever())
        .StartNow()
    );
});

// Quartz Hosted Service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// 6. Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global Exception Handler
app.UseExceptionHandler();

// Rate Limiting Middleware
app.UseIpRateLimiting();

// Firebase Auth Middleware (JWT doğrulama)
// Development: X-User-Id header ile çalışır
// Production: Firebase token doğrulaması yapar
// Test/Integration: Uses TestAuthHandler configured in CustomWebApplicationFactory
if (app.Environment.IsEnvironment("Test") || app.Environment.IsEnvironment("Integration"))
{
    app.UseAuthentication();
}
else
{
    app.UseFirebaseAuth();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose Program for WebApplicationFactory in tests.
public partial class Program { }
