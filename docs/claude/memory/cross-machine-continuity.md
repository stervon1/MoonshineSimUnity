---
name: cross-machine-continuity
description: docs/claude/ carries Claude session resume-state across machines; project uses git + Git LFS
metadata: 
  node_type: memory
  type: project
  originSessionId: a89b1268-dfc9-4573-ad73-b3856ee34ea2
  modified: 2026-08-27T18:55:17.910Z
---

The project syncs Windows ⇄ Mac via **git** — remote
`github.com/stervon1/MoonshineSimUnity`, branch `main`, binary assets in **Git
LFS** (`.gitattributes` rules; `git lfs install` once per machine,
`core.autocrlf=false`). `~/.claude/` memory does not sync, so cross-machine
resume state lives in `docs/claude/` and rides along in commits (added
2026-08-27):

- `SESSION.md` — the handoff: current focus, in-progress, next steps, rolling
  log. Read at session start (after `git pull`), update before handing back / on
  "checkpoint", then commit + push.
- `project-map.md` — generated structural snapshot (scripts/scenes/docs + line
  counts, third-party art omitted). Regenerate via `Tools/regen-claude-map.ps1`
  / `Tools/regen-claude-map.sh` or editor menu *Tools > White Lightning >
  Regenerate Claude Map* (the menu item shells out to the scripts). Staleness
  check: the `<!-- newest-source -->` header line vs newest file under
  `Assets/Scripts` / `Assets/Editor`.
- `memory/` — synced mirror of this auto-memory; seeds a fresh machine, merge
  target.

`.claude/settings.json` (committed) has **SessionStart + SessionEnd hooks** that
fire the continuity reminder on every session switch — plain `echo` of static
JSON, works under PowerShell and bash. On a fresh clone open `/hooks` once (or
restart) to trust the project settings.

**History:** was on Unity Version Control (Plastic) briefly; migrated to git +
LFS on 2026-08-27 (rebuilt the initial commit to pointerize ~1489 binaries).
`.plastic/` + `ignore.conf` are gitignored leftovers. Unity editor version is in
`ProjectSettings/ProjectVersion.txt` — **6000.5.10f1** (CLAUDE.md's old
`6000.0.36f1` was stale). Related: [[moonshinesim-editor-tooling]],
[[keep-project-plan-updated]].
