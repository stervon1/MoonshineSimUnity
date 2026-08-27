# `docs/claude/` — cross-machine continuity for Claude Code

The project moves between machines (Windows ⇄ Mac) through **git** (remote
`github.com/stervon1/MoonshineSimUnity`, branch `main`; binaries in Git LFS).
Claude Code's own memory lives in `~/.claude/` and does **not** sync, so anything
a session needs to resume work on another machine is kept here instead, inside
the repo, where commits carry it.

## Files

| File | What it is | Who writes it |
|------|-----------|---------------|
| `SESSION.md` | The handoff. Current focus, what's half-done, next steps, open questions, and a short rolling log. Read at the start of every session; updated at the end or on request. | Claude (hand-edited) |
| `project-map.md` | Generated structural snapshot of the hand-authored project — every script/scene/doc with line counts, third-party art omitted. Lets a session orient without crawling the tree. | Generator scripts only — never hand-edit |
| `memory/` | A synced copy of the auto-memory (`MEMORY.md` + one file per fact). Seeds a fresh machine whose local `~/.claude` memory is empty, and is the merge target when memory changes. | Claude (mirrors `~/.claude/.../memory/`) |

## Regenerating the map

No Unity needed:

```bash
pwsh Tools/regen-claude-map.ps1   # Windows / any pwsh
bash Tools/regen-claude-map.sh    # macOS / Linux
```

Or in the editor: **Tools > White Lightning > Regenerate Claude Map** (it just
shells out to the script above).

The map's top two lines are:

```
<!-- generated: <utc> by <script> on <os> -->
<!-- newest-source: <utc of the newest file it listed> -->
```

If `newest-source` is behind the real newest file under `Assets/Scripts` or
`Assets/Editor`, the map is stale — regenerate.

## Session protocol

The rules Claude follows are in the root `CLAUDE.md` under **Cross-machine
continuity**. In short: read `SESSION.md` + `project-map.md` at start; reconcile
`memory/` with local memory; update `SESSION.md` (and the map, if structure
changed) before handing back; then `git commit` + `git push` so the next machine
gets it (`git pull` first thing on the other side).

`.claude/settings.json` adds two hooks so the reminder fires on every session
switch automatically:

- **SessionStart** — surfaces the resume-point reminder and injects the protocol
  into context.
- **SessionEnd** — reminds to update `SESSION.md`, commit, and push.

Both are plain `echo` of static JSON, identical under PowerShell and bash. After
cloning to a new machine, open `/hooks` once (or restart) so Claude Code picks up
`.claude/settings.json` and prompts you to trust it. `.claude/settings.json` is
committed, so the hooks travel with the repo.

## First run on a new machine

1. `git lfs install` (once), then `git clone https://github.com/stervon1/MoonshineSimUnity.git`.
2. `git config core.autocrlf false` in the clone (Unity + cross-platform line endings).
3. Unity Hub → *Add project from disk* → the clone. Hub offers to install the
   editor version from `ProjectSettings/ProjectVersion.txt` (currently 6000.5.10f1).
4. Open `docs/claude/SESSION.md` — that's the state to resume from.
5. If `~/.claude/projects/<slug>/memory/MEMORY.md` is missing or thinner than
   `docs/claude/memory/MEMORY.md`, copy the missing files across (the `<slug>`
   is derived from the project's path on that machine).
6. `bash Tools/regen-claude-map.sh` to confirm the map matches local files.
