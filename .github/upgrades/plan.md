# .github/upgrades/plan.md

# Upgrade Plan: .NET 10.0 (All-At-Once)

## Table of contents

- Executive Summary
- Migration Strategy
- Detailed Dependency Analysis
- Project-by-Project Plans
- Package Update Reference
- Breaking Changes Catalog
- Testing & Validation Strategy
- Risk Management
- Complexity & Effort Assessment
- Source Control Strategy
- Success Criteria
- Appendices

---

## Executive Summary

### Scenario
Upgrade the solution `WarriorsGuild.sln` from .NET 6.0 to .NET 10.0 using the All-At-Once Strategy. The repository currently has 11 projects (including a Razor Pages app) all targeting `net6.0` per the assessment. The upgrade branch `upgrade-to-NET10` has been created and pending changes were committed.

### Scope
- Projects: 11 (all projects listed in assessment.md)
- Target framework: `net10.0` (per assessment proposed target)
- Package updates: 20 packages flagged for update or attention; notable security/deprecation flags present (see Package Update Reference)

### Selected Strategy
**All-At-Once Strategy** ? All projects will be upgraded simultaneously in one atomic operation. Rationale:
- Solution size and structure: 11 projects, predominantly SDK-style class libraries and one Razor Pages web app; assessment classifies projects as Low difficulty except the main web app which has the most API/package issues.
- Dependencies are clear and manageable; test projects exist.
- Team accepted short period of unified upgrade risk by selecting this strategy.

Key constraint: follow the All-At-Once principles ? update all project TFMs and package versions in a single coordinated change, restore, build, fix compilation errors, then run tests.

## Migration Strategy

### Summary
Perform an atomic upgrade: update `TargetFramework` to `net10.0` in every project file and apply all package upgrades from the assessment in one coordinated pass. After updates, restore and build the entire solution, fix compilation issues discovered, then run tests and address failures.

### Prerequisites
- Ensure .NET 10 SDK is installed on the build environment. If `global.json` exists, update or validate it to reference an SDK that supports .NET 10. Use CI pipeline to install SDK where needed.
- **Fallback**: If .NET 10 SDK has environment issues (e.g., WorkloadAutoImportPropsLocator), use `net8.0` with package versions `8.0.x` as an intermediate LTS target.
- Branch: work on `upgrade-to-NET10` branch (already created). All changes should be committed as a single logical commit or a small set of coordinated commits (see Source Control Strategy).

### Atomic Upgrade Scope (single operation)
- Update `TargetFramework` / `TargetFrameworks` in all project files to include `net10.0` (per policy: append when multi-targeting ? not applicable here).
- Update all NuGet package references listed in the assessment that have a suggested version or are flagged (security, deprecated, incompatible).
- Update Directory.Build.props / Directory.Packages.props or other centrally imported MSBuild files if they contain version properties.
- Restore packages and build the full solution to surface compilation errors.
- Fix compilation errors caused by framework/package changes in the same atomic operation.

### Post-upgrade verification
- Build succeeds with 0 compilation errors for the solution.
- All automated tests pass (see Testing & Validation Strategy).

### Exceptions & special handling
- Projects that intentionally must remain on `netstandard` or lower should be documented and excluded with an explicit justification.


## Detailed Dependency Analysis

Assessment summary (derived from `.github/upgrades/assessment.md`):
- Total projects: 11
- Root application: `WarriorsGuild\WarriorsGuild.csproj` (ASP.NET Razor Pages app)
- Test project: `WarriorsGuild.Tests\WarriorsGuild.Tests.csproj` (depends on nearly all projects)

Dependency observations:
- Most libraries are lower-level and consumed by the web app. There are no circular dependencies reported.
- Respect dependency graph for understanding impact, however All-At-Once approach upgrades all projects simultaneously to avoid intermediate incompatibilities.

Project list (all will be upgraded simultaneously):
- `WarriorsGuild.Common\WarriorsGuild.Common.csproj`
- `WarriorsGuild.Crosses\WarriorsGuild.Crosses.csproj`
- `WarriorsGuild.Data\WarriorsGuild.Data.csproj`
- `WarriorsGuild.Email\WarriorsGuild.Email.csproj`
- `WarriorsGuild.FileUpload\WarriorsGuild.FileUpload.csproj`
- `WarriorsGuild.Ranks\WarriorsGuild.Ranks.csproj`
- `WarriorsGuild.Rings\WarriorsGuild.Rings.csproj`
- `WarriorsGuild.Storage\WarriorsGuild.Storage.csproj`
- `WarriorsGuild.Tests\WarriorsGuild.Tests.csproj`
- `WarriorsGuild.Users\WarriorsGuild.Users.csproj`
- `WarriorsGuild\WarriorsGuild.csproj`

Note: although All-At-Once upgrades everything together, use the dependency graph during troubleshooting to prioritize fixes that unblock multiple dependants (e.g., `WarriorsGuild.Data` issues will likely affect many projects).

## Project-by-Project Plans

For each project: current state, target state, migration steps (project file changes, package updates, expected breaking-change areas, validation checklist).

### Common steps applied to every project (atomic)
1. Update project file `TargetFramework` element to `net10.0`.
2. Update or consolidate PackageReference entries to the versions recommended in the assessment (see Package Update Reference).
3. If project imports centralized MSBuild props/targets that set frameworks or package versions (e.g., Directory.Build.props), update those files accordingly.
4. Run `dotnet restore` and `dotnet build` for the full solution to observe compile errors.
5. Fix all compilation issues caused by the framework/package updates (see Breaking Changes Catalog for likely areas).
6. Run unit and integration tests for the project (if applicable).

### Project: `WarriorsGuild\WarriorsGuild.csproj` (Razor Pages web app)
- Current: `net6.0`  
- Target: `net10.0`
- Notable package issues (from assessment): many (18 package issues), including security vulnerable `Azure.Identity (1.8.2 -> 1.17.1)`, deprecated `Microsoft.ApplicationInsights.AspNetCore (2.21.0)`, and multiple `Microsoft.AspNetCore.*` packages that should be bumped to `10.0.2`.
- Expected breaking-change areas: Authentication/OpenIdConnect/JwtBearer APIs, Identity packages, Configuration binder usage, EF Core provider versions (SqlServer, Tools), runtime behavior changes for `System.Uri` and `TimeSpan` constructors/usages.
- Validation checklist:
  - [ ] Project file updated to `net10.0`.
  - [ ] All package references updated per Package Update Reference.
  - [ ] Solution builds with 0 errors.
  - [ ] Web app starts locally (smoke) and critical pages render (manual step: documented, not automated here).
  - [ ] Authentication flows tested (OpenID Connect, JwtBearer) in a dev environment.

### Project: `WarriorsGuild.Tests\WarriorsGuild.Tests.csproj`
- Current: `net6.0`  
- Target: `net10.0`
- Notable: Unit-test SDK `Microsoft.NET.Test.Sdk (17.5.0)` is compatible per assessment. Some binary incompatibility noted in test project; expect to update test package references or address API changes in test code using ConfigurationBinder or project API changes.
- Validation checklist:
  - [ ] Tests restored and run successfully.
  - [ ] Test adapters and runner versions compatible with .NET 10.

### Other library projects (Common, Crosses, Data, Email, FileUpload, Ranks, Rings, Storage, Users)
- Current: `net6.0` ? Target: `net10.0`
- Most are low difficulty per assessment. Data and FileUpload list NuGet suggestions; update EF Core-related packages in `Data` to `10.0.2` per assessment.
- Validation checklist for each:
  - [ ] Project file updated
  - [ ] Packages updated
  - [ ] Builds without errors

?? For projects that reference older `Microsoft.Extensions.*` packages (3.1.x or 6.0.x), update to the `10.0.2` family where assessment suggests. Verify consumers of those abstraction types compile as API changes may exist.

## Per-project TargetFramework change snippets

Apply the following exact changes to each project file's `PropertyGroup` that defines `TargetFramework`.

- Example (replace existing `TargetFramework` element):

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <!-- ...other properties... -->
</PropertyGroup>
```

Specific project file targets to change (all in one atomic commit):
- `WarriorsGuild\WarriorsGuild.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Tests\WarriorsGuild.Tests.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Common\WarriorsGuild.Common.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Crosses\WarriorsGuild.Crosses.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Data\WarriorsGuild.Data.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Email\WarriorsGuild.Email.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.FileUpload\WarriorsGuild.FileUpload.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Ranks\WarriorsGuild.Ranks.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Rings\WarriorsGuild.Rings.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Storage\WarriorsGuild.Storage.csproj`  ? change `net6.0` ? `net10.0`
- `WarriorsGuild.Users\WarriorsGuild.Users.csproj`  ? change `net6.0` ? `net10.0`

?? If any project uses `TargetFrameworks` multi-targeting, append `net10.0` to the list instead of replacing existing frameworks.

## Project-specific notes and likely code changes

### WarriorsGuild (web app)
- Check `Program.cs` or `Startup.cs` for hosting model changes between .NET 6 and .NET 10. The Razor Pages app should follow minimal-host patterns; verify middleware ordering, `IHostBuilder` vs `WebApplication` usage remains compatible.
- Review places where `ConfigurationBinder.GetValue<T>` is used and refactor to safe overloads if errors appear.
- Review authentication registration and options mapping (OpenIdConnect, JwtBearer, Facebook, Google).

### WarriorsGuild.Data
- Update EF Core packages to `10.0.2`. Verify migrations and `DbContext` configuration. EF Core major version changes may require updating some APIs for query translations or service registration.

### WarriorsGuild.Tests
- Ensure test project `TargetFramework` update preserves `IsPackable` and test SDK versions. Update NUnit or adapters only if build errors indicate incompatibility.

### WarriorsGuild.FileUpload
- Assessment flagged `Api.0002` (source incompatible) ? examine any APIs used that the new runtime changed; likely small surface area.


## Package Update Reference

Group updates by scope (use exact versions shown in assessment.md):

### Security & Critical updates
- `Azure.Identity`: `1.8.2` ? `1.17.1` (WarriorsGuild.csproj) ? security vulnerability; upgrade during atomic pass.

### ASP.NET Core and Identity (web app)
- `Microsoft.AspNetCore.Authentication.Facebook` `6.0.16` ? `10.0.2`
- `Microsoft.AspNetCore.Authentication.Google` `6.0.16` ? `10.0.2`
- `Microsoft.AspNetCore.Authentication.JwtBearer` `6.0.16` ? `10.0.2`
- `Microsoft.AspNetCore.Authentication.OpenIdConnect` `6.0.16` ? `10.0.2`
- `Microsoft.AspNetCore.Identity.UI` `6.0.16` ? `10.0.2`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` `3.1.32` ? `10.0.2` (also used by Data project)
- `Microsoft.AspNetCore.Mvc.NewtonsoftJson` `6.0.16` ? `10.0.2`
- `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` `6.0.16` ? `10.0.2`

### Entity Framework (Data)
- `Microsoft.EntityFrameworkCore.Design` `3.1.32` ? `10.0.2`
- `Microsoft.EntityFrameworkCore.SqlServer` `3.1.32` ? `10.0.2`
- `Microsoft.EntityFrameworkCore.Tools` `3.1.32` ? `10.0.2`

### Microsoft.Extensions / Tooling
- `Microsoft.Extensions.Caching.StackExchangeRedis` `6.0.15` ? `10.0.2`
- `Microsoft.Extensions.Configuration.Abstractions` `3.1.23` ? `10.0.2`
- `Microsoft.Extensions.Logging.Abstractions` `3.1.23` ? `10.0.2`

### Deprecated / Incompatible packages (require special attention)
- `Microsoft.ApplicationInsights.AspNetCore` `2.21.0` ? deprecated, evaluate replacement or updated package before or during upgrade.
- `Microsoft.Extensions.Azure` `1.5.0` ? deprecated in this assessment; evaluate newer `Azure.Extensions` packages.
- `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` `1.17.2` ? marked incompatible; remove or replace with compatible tooling package in CI if necessary.

Notes:
- Apply these updates centrally where possible (Directory.Packages.props) to simplify the atomic update.
- Include exact versions from assessment when making changes.
- **Lamar.Microsoft.DependencyInjection**: Must use `15.0.1` (not 10.0.0) for .NET 10; v10 constrains Microsoft.Extensions.* to < 8.0 causing NU1107 conflicts.

## Breaking Changes Catalog

This catalog highlights the most likely breaking-change categories discovered by the analysis and where to look when compilation errors appear after the atomic upgrade.

High-confidence breaking-change areas:
- Configuration API: `ConfigurationBinder.GetValue<T>` binary incompatibilities ? check calls to `GetValue<T>(IConfiguration, string)` and adjust overload usage or update using new binding semantics.
- Authentication and Identity: OpenIdConnect, JwtBearer, Facebook/Google authentication options and registration extension methods may have moved or changed signatures. Review `AddOpenIdConnect`, `AddJwtBearer`, `AddFacebook`, `AddGoogle` usages and option property names.
- EF Core: major version bump (3.1 -> 10) requires updating migrations, design packages and provider APIs; regenerate or validate migrations and provider-specific code.
- System APIs: `System.Uri` and `TimeSpan` constructor/Parse semantics and certain encoding types (UTF7) have behavioral changes ? add test coverage for code paths using these.
- Claims mapping: `JwtSecurityTokenHandler.DefaultInboundClaimTypeMap` and claim mapping behavior may differ; review token handling and claim mapping.

When build errors occur, capture the error text and map to these categories. Use `upgrade_get_member_info` or `upgrade_get_namespace_info` if more granular guidance is required for a specific symbol.

## Testing & Validation Strategy

Testing will run after the atomic upgrade completes. Levels:

1) Local developer validation
  - `dotnet restore`
  - `dotnet build` (solution)
  - `dotnet test` for test projects discovered (WarriorsGuild.Tests)

2) CI validation
  - Ensure CI uses .NET 10 SDK image or installs SDK during job.
  - Run full solution build and tests in CI.

3) Focused runtime checks (manual / automated where possible)
  - Start Razor Pages app and exercise key authentication flows, pages that use Identity, and EF-backed features.
  - Verify external integrations (Azure.Identity usage) in a dev environment.

Automated test list (from analysis):
 - `WarriorsGuild.Tests` (unit tests) ? run after package updates and code fixes.

Validation criteria:
 - All unit tests pass
 - Solution builds with 0 errors
 - No critical security-vulnerable packages remain (see Package Update Reference)

## Risk Management

Key risks:
- Security-vulnerable packages (e.g., `Azure.Identity`) ? moderate risk. Mitigation: upgrade to secure versions in atomic pass.
- Deprecated/incompatible packages (ApplicationInsights, Microsoft.Extensions.Azure, VisualStudio Azure Targets) ? mitigation: replace or remove and validate CI.
- Authentication/Identity breaking changes ? mitigation: allocate developer review for OpenID/JWT code paths and add targeted tests.
- EF Core major upgrade ? mitigation: validate migrations locally and ensure provider compatibility.

Rollback approach (manual):
- If atomic upgrade introduces blocking issues that cannot be resolved quickly, revert `upgrade-to-NET10` branch or create a hotfix branch from the pre-upgrade commit and continue troubleshooting.

Contingency:
- Keep a snapshot of the pre-upgrade state (the current `main` branch is preserved); do not force-push main.

## Complexity & Effort Assessment

Relative complexity (Low/Medium/High) ? per assessment:
- `WarriorsGuild` (web app): High (multiple API and package issues)
- `WarriorsGuild.Data`: Medium (package updates for EF Core)
- `WarriorsGuild.Common`, `FileUpload`: Low (small code changes expected)
- Other libraries: Low

Notes: no time estimates provided. Use these ratings to allocate reviewers and testers.

## Source Control Strategy

- Working branch: `upgrade-to-NET10` (created and pending changes committed during analysis). Continue work on this branch.
- Commit policy: Prefer a single atomic commit that includes all project file TFMs and package version changes, followed by one or few commits for compilation fixes if needed. Keep commits logical and reviewable.
- Pull request: Open a PR from `upgrade-to-NET10` to `main` with the checklist: build passes in CI on .NET 10 runner, unit tests pass, security packages addressed.

Notes: All-At-Once approach prefers a consolidated change set rather than many per-project branches.

## Success Criteria

The upgrade is complete when all of the following are true:
1. All projects target `net10.0` in their project files (or Directory.Build.props where applicable).
2. All package updates from the assessment are applied (including security fixes and replacements for deprecated packages) or documented exceptions exist.
3. The solution builds with 0 compilation errors.
4. All automated tests pass (unit tests in `WarriorsGuild.Tests` and any other test projects).
5. No critical security vulnerabilities remain in NuGet dependencies per the assessment.


## Execution Order (Recommended)

Perform the upgrade in this order to minimize cascading failures:

1. **Class libraries (no app dependencies)**: Common, Email, Users, FileUpload, Storage
2. **Data layer**: WarriorsGuild.Data (EF Core + Identity packages)
3. **Domain libraries**: Crosses, Ranks, Rings (depend on Data)
4. **Web application**: WarriorsGuild (main app)
5. **Test project**: WarriorsGuild.Tests

*Note: All-At-Once updates all simultaneously; this order aids troubleshooting.*

## IdentityServer4 Migration Note

IdentityServer4 is in maintenance mode. The plan keeps IdentityServer4.* at current versions (4.1.2, 3.0.1) as they are compatible with .NET 6?10. For long-term support, consider migrating to:
- **Duende IdentityServer** (commercial, requires license for production), or
- **OpenIddict** (open source alternative)

**IdentityServerAspNetIdentity project** (standalone, not in main solution): Upgraded to net10.0 with packages updated. Uses `UseMigrationsEndPoint()` instead of deprecated `UseDatabaseErrorPage()`.

## Entity Framework Core 3.1 ? 10.0 Considerations

Major version jump; verify after upgrade:
- String-based `Include("NavigationProperty")` may need strongly typed `Include(e => e.NavigationProperty)` where required
- Raw SQL usage (`FromSqlRaw`, etc.) ? validate parameterization
- Migrations: Run `dotnet ef migrations list` to verify; regenerate only if schema changes needed
- `IGuildDbContext` and `ApplicationDbContext` ? no structural changes expected

## Deprecated Package Replacements

| Deprecated Package | Replacement / Action | Status |
|-------------------|----------------------|--------|
| Microsoft.ApplicationInsights.AspNetCore | Migrated to `Azure.Monitor.OpenTelemetry.AspNetCore` (UseAzureMonitor) | Done |
| Microsoft.Extensions.Azure 1.5.0 | Removed; AddAzureClients block was commented out, AzureClientFactoryBuilderExtensions deleted | Done |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | Updated from 1.17.2 to 1.22.1 for compatibility | Done |

## Post-Migration Notes (Implementation Complete)

**Build status**: Solution builds successfully with 0 compilation errors.

**Test status**: 39 of 204 tests fail. Root causes:
- EF Core 10: `IQueryable` must implement `IAsyncEnumerable` for async operations; MockQueryable / test DbSet mocks may need updates.
- Moq Strict mode: Additional mock setups needed for `RankStatusCrosses`, `SaveChangesAsync`, and other EF Core paths.

**Next steps for tests**:
1. Update MockQueryable / CreateDbSetMock usage for EF Core 10 async compatibility.
2. Add missing mock setups for new code paths (e.g., `IGuildDbContext.RankStatusCrosses`).
3. Consider `MockBehavior.Loose` for integration-style tests if Strict proves brittle.

**Files modified in implementation**:
- All 11 `.csproj` files: `TargetFramework` ? `net10.0`, package versions updated.
- `WarriorsGuild.FileUpload\MultipartFormReader.cs`: Replaced obsolete `Encoding.UTF7` with WebName check.
- `global.json`: Added to pin SDK version 10.0.102.

## Appendices

- Assessment reference: `.github/upgrades/assessment.md`
- Solution: `WarriorsGuild.sln`

