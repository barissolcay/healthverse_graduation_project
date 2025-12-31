## Description
<!-- What does this PR do? Briefly describe the change. -->

## Type of Change
<!-- Mark the relevant option(s) with an "x" -->
- [ ] 🐛 Bug fix (non-breaking change that fixes an issue)
- [ ] ✨ New feature (non-breaking change that adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to change)
- [ ] 🏗️ Refactor (code change that neither fixes a bug nor adds a feature)
- [ ] 📝 Documentation update
- [ ] 🧪 Test update (adding or updating tests)
- [ ] 🔧 Configuration change (CI, build, etc.)

## Hexagonal Architecture Checklist
<!-- Ensure your changes follow our hexagonal architecture rules -->
- [ ] Domain layer has no framework dependencies (EF, HTTP, Quartz)
- [ ] Application layer does not reference Infrastructure
- [ ] Controllers are thin (MediatR send/return only)
- [ ] Jobs are orchestrator-only (no business logic)
- [ ] Cross-module communication uses Contracts project

## Testing
<!-- How has this been tested? -->
- [ ] Unit tests pass locally: `dotnet test tests/HealthVerse.UnitTests`
- [ ] Architecture tests pass locally: `dotnet test tests/HealthVerse.ArchitectureTests`
- [ ] Integration tests pass locally: `dotnet test tests/HealthVerse.IntegrationTests`

## ADR Impact
<!-- Does this change require updating or creating an ADR? -->
- [ ] No ADR impact
- [ ] Updated existing ADR: `docs/architecture/adr/XXXX-*.md`
- [ ] Created new ADR: `docs/architecture/adr/XXXX-*.md`

## Migration Impact
<!-- Does this change include database migrations? -->
- [ ] No migration changes
- [ ] New migration added (incremental, backward compatible)
- [ ] Migration modifies existing data (requires careful review)

## Screenshots (if applicable)
<!-- Add screenshots to help explain your changes if UI-related -->

## Related Issues
<!-- Link related issues: Closes #123, Fixes #456 -->

## Additional Notes
<!-- Any additional context or notes for reviewers -->
