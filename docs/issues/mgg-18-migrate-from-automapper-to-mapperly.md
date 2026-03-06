---
identifier: MGG-18
title: Migrate from AutoMapper to Mapperly
id: a39308bf-6279-4ad1-871f-b18cc59ee8d4
status: Done
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-18/migrate-from-automapper-to-mapperly"
gitBranchName: mggarofalo/mgg-18-migrate-from-automapper-to-mapperly
createdAt: "2026-01-14T10:43:56.378Z"
updatedAt: "2026-02-11T22:50:01.910Z"
completedAt: "2026-02-11T22:50:01.883Z"
---

# Migrate from AutoMapper to Mapperly

## Summary

Migrate from AutoMapper to Mapperly, a free and open-source mapping library that's faster and has no licensing concerns.

## Why Migrate?

### Licensing Issues

AutoMapper 15.0+ (July 2024) changed from MIT to a dual commercial/open-source license requiring:

* Free Community Edition for companies < $5M revenue
* Paid licenses ($799-$6,399/year) for larger companies
* License key configuration in code

### Mapperly Benefits

* **Free & Open Source:** Apache 2.0 license, no licensing concerns
* **Performance:** 8.61x faster than AutoMapper in benchmarks
* **Compile-Time Safety:** Uses source generators, catches mapping errors at build time
* **No Reflection:** Zero runtime overhead
* **Debuggable:** Generated code is readable and can be stepped through
* **Active Development:** Strong community support

## Migration Steps

### 1\. Update Dependencies

```xml
<!-- Remove from Directory.Packages.props -->
<PackageVersion Include="AutoMapper" Version="16.0.0" />

<!-- Add -->
<PackageVersion Include="Riok.Mapperly" Version="4.2.1" />
```

### 2\. Create Mapper Classes

Mapperly uses partial classes with `[Mapper]` attribute instead of Profile classes.

**Infrastructure Layer** (Domain ↔ Entity):

```csharp
[Mapper]
public partial class InfrastructureMapper
{
    public partial AccountEntity ToEntity(Account domain);
    public partial Account ToDomain(AccountEntity entity);
    // ... other mappings
}
```

**API Layer** (Domain ↔ ViewModel):

```csharp
[Mapper]
public partial class ApiMapper
{
    public partial AccountVM ToViewModel(Account domain);
    public partial Account ToDomain(AccountVM viewModel);
    // ... other mappings
}
```

### 3\. Update Service Registration

Replace AutoMapper DI with Mapperly singleton instances.

**Remove from:**

* `src/Application/Services/ApplicationService.cs` (line 17)
* `src/Infrastructure/Services/InfrastructureService.cs` (line 45)
* `src/Presentation/API/Services/ProgramService.cs` (line 12)

**Add:**

```csharp
services.AddSingleton<InfrastructureMapper>();
services.AddSingleton<ApiMapper>();
```

### 4\. Update Mapping Calls

**Before (AutoMapper):**

```csharp
Account account = _mapper.Map<Account>(entity);
```

**After (Mapperly):**

```csharp
Account account = _mapper.ToDomain(entity);
```

### 5\. Handle Complex Mappings

For complex scenarios, use `[MapProperty]`, `[MapperIgnore]`, and custom mapping methods.

### 6\. Update Tests

Replace AutoMapper mock setup with Mapperly mapper instances.

## Affected Files

### Remove AutoMapper Profiles

* `src/Infrastructure/Mapping/*.cs`
* `src/Presentation/API/Mapping/*.cs`

### Create Mapperly Mappers

* `src/Infrastructure/Mapping/InfrastructureMapper.cs` (new)
* `src/Presentation/API/Mapping/ApiMapper.cs` (new)

### Update Service Registration

* `src/Application/Services/ApplicationService.cs`
* `src/Infrastructure/Services/InfrastructureService.cs`
* `src/Presentation/API/Services/ProgramService.cs`

### Update All Usages

* All repositories in `src/Infrastructure/Repositories/`
* All controllers in `src/Presentation/API/Controllers/`
* All test files using mappers

## Testing Strategy

1. Create Mapperly mappers matching existing AutoMapper profiles
2. Update one layer at a time (Infrastructure first, then API)
3. Run all tests after each layer migration
4. Verify integration tests pass

## References

* [Mapperly GitHub](https://github.com/riok/mapperly)
* [Mapperly Documentation](https://mapperly.riok.app/)
* [Best Free Alternatives to AutoMapper - Mapperly](https://abp.io/community/articles/best-free-alternatives-to-automapper-in-.net-why-we-moved-to-mapperly-l9f5ii8s)
* [Mapperly vs AutoMapper Comparison](https://blog.nergy.space/blogs/mapperly)
* [.NET Object Mappers Benchmark](https://github.com/mjebrahimi/DotNet-Mappers-Benchmark)

## Blockers

* **MGG-17** must be resolved first to unblock the build
