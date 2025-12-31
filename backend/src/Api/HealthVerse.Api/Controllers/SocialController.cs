using HealthVerse.SharedKernel.Abstractions;
using HealthVerse.Social.Application.Commands;
using HealthVerse.Social.Application.DTOs;
using HealthVerse.Social.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SocialController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SocialController> _logger;

    public SocialController(IMediator mediator, ICurrentUser currentUser, ILogger<SocialController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Bir kullanıcıyı takip et.
    /// </summary>
    [HttpPost("follow/{targetUserId:guid}")]
    public async Task<ActionResult<FollowResponse>> Follow(Guid targetUserId)
    {
        var response = await _mediator.Send(new FollowUserCommand(_currentUser.UserId, targetUserId));

        if (!response.Success)
        {
            if (response.Message == "Kendinizi takip edemezsiniz." || response.Message == "Bu kullanıcıyı takip edemezsiniz.")
                return BadRequest(response);
            
            if (response.Message == "Kullanıcı bulunamadı.")
                return NotFound(response); 
            
             return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Bir kullanıcıyı takipten çık.
    /// </summary>
    [HttpDelete("unfollow/{targetUserId:guid}")]
    public async Task<ActionResult<FollowResponse>> Unfollow(Guid targetUserId)
    {
        var response = await _mediator.Send(new UnfollowUserCommand(_currentUser.UserId, targetUserId));
        return Ok(response);
    }

    /// <summary>
    /// Takipçi listesi.
    /// </summary>
    [HttpGet("followers")]
    public async Task<ActionResult<FollowListResponse>> GetFollowers(int page = 1, int pageSize = 20)
    {
        var response = await _mediator.Send(new GetFollowersQuery(_currentUser.UserId, page, pageSize));
        return Ok(response);
    }

    /// <summary>
    /// Takip edilen listesi.
    /// </summary>
    [HttpGet("following")]
    public async Task<ActionResult<FollowListResponse>> GetFollowing(int page = 1, int pageSize = 20)
    {
        var response = await _mediator.Send(new GetFollowingQuery(_currentUser.UserId, page, pageSize));
        return Ok(response);
    }

    /// <summary>
    /// Mutual arkadaş listesi (karşılıklı takip).
    /// </summary>
    [HttpGet("friends")]
    public async Task<ActionResult<FollowListResponse>> GetFriends(int page = 1, int pageSize = 20)
    {
        var response = await _mediator.Send(new GetMutualFriendsQuery(_currentUser.UserId, page, pageSize));
        return Ok(response);
    }

    /// <summary>
    /// Bir kullanıcıyı engelle.
    /// </summary>
    [HttpPost("block/{targetUserId:guid}")]
    public async Task<ActionResult<BlockResponse>> Block(Guid targetUserId)
    {
        var response = await _mediator.Send(new BlockUserCommand(_currentUser.UserId, targetUserId));

        if (!response.Success)
        {
            if (response.Message == "Kendinizi engelleyemezsiniz.")
                return BadRequest(response);
            
            return BadRequest(response); // Generic fail
        }

        return Ok(response);
    }

    /// <summary>
    /// Engeli kaldır.
    /// </summary>
    [HttpDelete("unblock/{targetUserId:guid}")]
    public async Task<ActionResult<BlockResponse>> Unblock(Guid targetUserId)
    {
        var response = await _mediator.Send(new UnblockUserCommand(_currentUser.UserId, targetUserId));
        return Ok(response);
    }
}
