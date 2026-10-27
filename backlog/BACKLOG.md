# Backlog

Backlog for the Skywithin.AutoRegister library. Each item is a standalone file in `backlog/items/`. Everything below the size guide is generated from item frontmatter by `dotnet run tools/backlog-sync.cs`; do not hand-edit it.

## Quick Commands

| Action | Command | Example |
|--------|---------|---------|
| File a bug | `/backlog-add-bug` | `/backlog-add-bug` |
| Add enhancement / tech debt | `/backlog-add-item` | `/backlog-add-item` |
| Fix a bug | `/backlog-fix-bug BUG-XXXX` | `/backlog-fix-bug BUG-0001` |
| Process a non-bug item | `/backlog-process-item BL-XXXX` | `/backlog-process-item BL-0003` |
| View backlog status | `/backlog-status` | `/backlog-status` |

## Item Template

```yaml
---
id: BL-XXXX          # or BUG-XXXX; get it from: dotnet run tools/backlog-next-id.cs BL|BUG
title: "Short title" # REQUIRED; used by backlog-sync for the index
type: enhancement    # enhancement|feature|tech-debt|test-gap|bug|investigation|chore
area: core           # core|tests|example|packaging|docs
priority: medium     # critical|high|medium|low
size: M              # S|M|L|XL
status: open         # open|in-progress|done|rejected|wont-fix
completed: null      # YYYY-MM-DD; set by backlog-set-field.cs on a terminal status
breaking: false      # true if the change alters the public API or documented behaviour
blocked_by: null     # free text: what blocks this item (an item ID or an external dependency)
---
```

Change `status` with `dotnet run tools/backlog-set-field.cs <ID> status <value>`; it sets `completed` and rebuilds this index. Do not hand-edit `status` or `completed`. Validate everything with `dotnet run tools/backlog-lint.cs`.

**Areas:** `core` is `src/AutoRegister.DI/` (`IoCExtensions.cs`, `AutoRegisterAttribute.cs`, `ConsoleHelper.cs`); `tests` is the test project; `example` is the Clean Architecture demo under `example/`; `packaging` is the csproj, `Directory.Build.props`, NuGet metadata and GitHub workflows; `docs` is README.md, `docs/` and CLAUDE.md.

**Size guide:**

| Size | Scope | Typical effort |
|------|-------|----------------|
| `S` | One or two files, mechanical change | < 30 min |
| `M` | Library change plus tests plus README/docs | 30 min - 2 hours |
| `L` | Cross-cutting: attribute, registration logic, tests, example and docs together | Half day |
| `XL` | Public API redesign; needs a written proposal in the item and a major version | Day+ |

- `BL-` prefix for all non-bug items, `BUG-` for bugs, each numbered sequentially.
- `breaking: true` items are collected under "Next major version" below and should ship together in one major version. Releases are cut by git tag (`v1.2.3`, `rc-v1.2.3`); see CLAUDE.md.

---

## Next major version

Open items with `breaking: true`: they change the public API or documented behaviour, so ship them together in one major version. They are listed again under Open.

| ID | Title | Type | Area | Priority | Size | Blocked |
|----|-------|------|------|----------|------|---------|
| BL-0004 | Replace hard-wired console output with configurable logging | enhancement | core | high | M | BL-0003 |
| BL-0005 | Fail fast on invalid [AutoRegister] configuration | enhancement | core | high | M | BL-0003 |
| BL-0007 | Share one instance between Self and Interface registrations | enhancement | core | high | M |  |
| BL-0008 | Make automatic interface selection deterministic | enhancement | core | high | M |  |
| BL-0006 | Prevent duplicate registrations when an assembly is scanned twice | enhancement | core | medium | M | BL-0003 |
| BL-0009 | Redesign the AutoRegisterAttribute surface for extensibility | tech-debt | core | medium | M |  |

## Open

### Features & Enhancements

| ID | Title | Type | Area | Priority | Size | Blocked |
|----|-------|------|------|----------|------|---------|
| BL-0003 | Add an options-based registration entry point | feature | core | high | L |  |
| BL-0004 | Replace hard-wired console output with configurable logging | enhancement | core | high | M | BL-0003 |
| BL-0005 | Fail fast on invalid [AutoRegister] configuration | enhancement | core | high | M | BL-0003 |
| BL-0007 | Share one instance between Self and Interface registrations | enhancement | core | high | M |  |
| BL-0008 | Make automatic interface selection deterministic | enhancement | core | high | M |  |
| BL-0006 | Prevent duplicate registrations when an assembly is scanned twice | enhancement | core | medium | M | BL-0003 |
| BL-0010 | Allow registering a class under several interfaces | feature | core | medium | M | BL-0009 |

### Test Coverage

| ID | Title | Type | Area | Priority | Size |
|----|-------|------|------|----------|------|
| BL-0013 | Add cross-assembly registration tests | test-gap | tests | medium | M |

### Tech Debt

| ID | Title | Type | Area | Priority | Size |
|----|-------|------|------|----------|------|
| BL-0002 | Add CI workflow that builds and tests on push and pull request | chore | packaging | high | S |
| BL-0009 | Redesign the AutoRegisterAttribute surface for extensibility | tech-debt | core | medium | M |

### Investigations

| ID | Title | Type | Area | Priority | Size |
|----|-------|------|------|----------|------|
| BL-0011 | Investigate a source generator instead of runtime reflection | investigation | core | low | L |

## Done (recent)

Older completed items stay in `backlog/items/`; this table shows the most recent completions.

| ID | Title | Type | Area | Priority | Size |
|----|-------|------|------|----------|------|
| BL-0012 | Trim the example to what demonstrates the library | chore | example | low | S |
| BL-0001 | Replace Demo.Web.Api with a console demo app | chore | example | high | L |

