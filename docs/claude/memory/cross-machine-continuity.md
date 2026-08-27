---
name: cross-machine-continuity
description: "docs/claude/ carries Claude session resume-state across machines since ~/.claude memory doesn't sync via UVCS"
metadata: 
  node_type: memory
  type: project
  originSessionId: a89b1268-dfc9-4573-ad73-b3856ee34ea2
  modified: 2026-08-27T18:29:31.173Z
---

The project syncs Windows ⇄ Mac via **Unity Version Control**; `~/.claude/`
memory does not. So cross-session/cross-machine resume state lives in
`docs/claude/` (added 2026-08-27):

- `SESSION.md` — the handoff: current focus, in-progress, next steps, rolling
  log. Read at session start, update before handing back / on "checkpoint".
- `project-map.md` — generated structural snapshot (scripts/scenes/docs + line
  counts, third-party art omitted). Regenerate via `Tools/regen-claude-map.ps1`
  / `Tools/regen-claude-map.sh` or editor menu *Tools > White Lightning >
  Regenerate Claude Map* (the menu item just shells out to the scripts).
  Staleness check: the `<!-- newest-source -->` header line vs newest file under
  `Assets/Scripts` / `Assets/Editor`.
- `memory/` — synced mirror of this auto-memory; seeds a fresh machine and is
  the merge target.

Also added root `ignore.conf` (UVCS ignore for Library/Temp/Build/etc.) and a
**Cross-machine continuity** section in `CLAUDE.md` with the full protocol.

`.claude/settings.json` has **SessionStart + SessionEnd hooks** that fire the
continuity reminder on every session switch (plain `echo` of static JSON, works
under PowerShell and bash). `.claude/` must be added to UVCS for them to travel;
on a new machine open `/hooks` once (or restart) to trust the project settings.
Related: [[moonshinesim-editor-tooling]], [[keep-project-plan-updated]].
