# Version Management

Releases are driven by Git tags. The package version comes from the tag, not from the project file.

| Tag | Workflow | Published version |
|-----|----------|-------------------|
| `v1.2.3` | `build-and-publish.yml` | `1.2.3` |
| `rc-v1.2.3` | `pre-release-publish.yml` | `1.2.3-rc.<run number>` |

```bash
git tag v1.2.3
git push origin v1.2.3
```

Both workflows restore, build the whole solution, run the tests, pack, and push to NuGet.org. A failing build or test stops the release.

Versions follow [Semantic Versioning](https://semver.org/). Changes that alter the public API or documented behaviour need a new major version.
