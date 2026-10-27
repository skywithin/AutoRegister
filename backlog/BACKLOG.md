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

**Areas:** `core` is `src/AutoRegister.DI/` (`IoCExtensions.cs`, `AutoRegisterAttribute.cs`, `ConsoleHelper.cs`); `tests` is the test project; `example` is the Clean Architecture demo under `example/`; `packaging` is the csproj, `Directory.Build.props`, NuGet metadata and GitHub workflows; `docs` is README.md, `wiki/` and CLAUDE.md.

**Size guide:**

| Size | Scope | Typical effort |
|------|-------|----------------|
| `S` | One or two files, mechanical change | < 30 min |
| `M` | Library change plus tests plus README/wiki | 30 min - 2 hours |
| `L` | Cross-cutting: attribute, registration logic, tests, example and docs together | Half day |
| `XL` | Public API redesign; needs a written proposal in the item and a major version | Day+ |

- `BL-` prefix for all non-bug items, `BUG-` for bugs, each numbered sequentially.
- `breaking: true` items are collected under "Next major version" below and should ship together in one major version. Releases are cut by git tag (`v1.2.3`, `rc-v1.2.3`); see CLAUDE.md.

---

## Open

No open items.

## Done (recent)

Older completed items stay in `backlog/items/`; this table shows the most recent completions.

Nothing completed yet.

