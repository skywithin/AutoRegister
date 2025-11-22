# Version Management

AutoRegister.DI uses a **tag-based versioning system** where versions are determined by Git tags.

## Production Releases

Create production releases by pushing a tag with the `v` prefix:

```bash
git tag v1.0.0
git push origin v1.0.0
```

**Tag format**: `v1.0.0`, `v2.1.3`, etc.

**Result**: Package version `1.0.0` published to NuGet.org

## Pre-Releases

Create pre-releases by pushing a tag with the `rc-v` prefix:

```bash
git tag rc-v1.0.0
git push origin rc-v1.0.0
```

**Tag format**: `rc-v1.0.0`, `rc-v2.1.3`, etc.

**Result**: Package version `1.0.0-rc.123` published to NuGet.org (where 123 is the workflow run number)

## Tag Naming

- **Production**: `v1.0.0` (lowercase `v`)
- **Pre-release**: `rc-v1.0.0` (lowercase `rc-v`)

The version follows [Semantic Versioning](https://semver.org/): `MAJOR.MINOR.PATCH`
