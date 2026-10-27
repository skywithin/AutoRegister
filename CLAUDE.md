# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

`Skywithin.AutoRegister` is a small .NET 10 NuGet library: classes decorated with `[AutoRegister(Lifetime, RegisterAs[, typeof(IService)])]` are discovered by reflection and registered into `IServiceCollection` via `services.AddAutoRegisteredServicesFromAssembly(params Assembly[])`. The only runtime dependency is `Microsoft.Extensions.DependencyInjection.Abstractions`.

## Commands

```bash
dotnet restore AutoRegister.slnx
dotnet build AutoRegister.slnx -c Release
dotnet test tests/AutoRegister.DI.Tests/AutoRegister.DI.Tests.csproj
dotnet test tests/AutoRegister.DI.Tests/AutoRegister.DI.Tests.csproj --filter "FullyQualifiedName~<TestName>"   # single test
dotnet run --project example/Demo.Web.Api   # run the demo API
```

Tests use NUnit + FluentAssertions. The library project has `GeneratePackageOnBuild=true`, so every build also produces a `.nupkg`.

## Layout

- `src/AutoRegister.DI/` — the library (namespace `Skywithin.AutoRegister.DI`). All logic lives in `IoCExtensions.cs`. `AutoRegisterAttribute.cs` defines the attribute plus the `Lifetime` and `RegisterAs` (a `[Flags]` enum) types.
- `tests/AutoRegister.DI.Tests/` — the test service types (`ServiceA`…, `IServiceA`…) are declared at the top of `IoCExtensionsTests.cs`, and tests scan `Assembly.GetExecutingAssembly()`. Any new fixture type you add there is picked up by every test that scans the assembly, so registration-count assertions may need updating.
- `example/` — a Clean Architecture demo (Domain → Application → Infrastructure → Web.Api, plus SharedKernel). It references the library by `ProjectReference`. Each layer has `Properties/AssemblyReference.cs`, which exposes `AssemblyReference.Assembly`, and a `Bootstrap/DependencyInjection.cs` extension (`AddDemoX()`) that chains the lower layer and then calls `AddAutoRegisteredServicesFromAssembly(AssemblyReference.Assembly)`. This is the usage pattern the library recommends.
- `wiki/` — user-facing Markdown docs, wrapped in a NoTargets project (`ExcludeFromSolutionBuild`). Update these along with `README.md` when behavior changes. `README.md` is also packed into the NuGet package.
- Per-folder `Directory.Build.props` (`src/`, `tests/`, `example/`) set LangVersion 13, nullable, and NuGet audit settings. There is no root-level props file and no central package management.

## Registration semantics (IoCExtensions)

- Only concrete, non-abstract classes with the attribute declared directly on them are picked up (`inherit: false`).
- `RegisterAs.Self` and `RegisterAs.Interface` are independent flags. Both can be set, which produces two separate registrations (not a forwarded/shared instance).
- Interface resolution: an explicit `ServiceInterface` is validated (it must be an interface the class implements; otherwise the type is skipped). Without one, the first interface is used after filtering out `IDisposable`, `IAsyncDisposable` and `System.*` interfaces. If filtering removes everything, it falls back to the first interface of any kind.
- Invalid configurations don't throw. They print colored warnings/errors to the console through `ConsoleHelper` and skip the registration. Only an empty or null assembly list throws (`ArgumentException`). Every scan and registration is also logged to the console.

## Releases

Publishing is tag-driven through GitHub Actions (both workflows build, test, pack and push to NuGet.org):
- `vX.Y.Z` → `build-and-publish.yml` publishes version `X.Y.Z`.
- `rc-vX.Y.Z` → `pre-release-publish.yml` publishes `X.Y.Z-rc.<run_number>`.

The package version comes from the tag (`-p:Version=`), not from the csproj.
