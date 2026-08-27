# Session handoff — White Lightning

> The living "resume here" note for Claude Code sessions across machines.
> Read this first. Update it before handing back. See `docs/claude/README.md`.

---

**Last updated:** 2026-08-27T18:30:00Z
**Machine:** WINDOWS (`C:\Projects\MoonshineSimUnity`)
**Milestone:** 1 — make the still-run *feel* good in isolation

## Current focus

Milestone 1 polish on the still-run. Per `CLAUDE.md`, still open:
liquid-stream VFX reacting to cut phase/quality, steam VFX, a real proof-gauge
widget, and sound cues for clean vs early/late cuts. Tune against
`docs/distillation-reference.md` §9.

## In progress

- _(nothing mid-change — this handoff file was just introduced)_

## Next steps

1. Pick one of the four open Milestone-1 items above and build it.
2. Milestone 2 (not started): rename `JobSpec`/`JobGenerator`/`GameState.CurrentJob`/
   `OnJobAccepted`/`JobBoardPrototypeUI` → batch/buyer vocabulary. Tracked in
   `docs/project-plan.md`; see memory `design-spine-v3-self-directed`.

## Open questions / decisions pending

- **Mac checkout blocked (2026-08-27):** Unity Hub on Mac reports "repository does
  not contain a valid Unity project — missing ProjectSettings/ProjectVersion.txt".
  The file exists in the Windows working copy (`m_EditorVersion: 6000.5.10f1`).
  Windows workspace `.plastic/` is bound to cloud repo
  `MoonshineSimUnity/MoonshineSimUnity@cloud`, branch `/main` (created 2026-08-27).
  Likely cause: `/main` has no checkin yet, or the checkin omitted `ProjectSettings/`.
  **Action:** on Windows, Unity Version Control app → Pending Changes → confirm
  `ProjectSettings/ProjectVersion.txt` + the rest are listed (not under Private /
  filtered by `ignore.conf`) → Checkin to `/main`. On Mac, get the workspace with
  the Unity Version Control desktop app / `cm` CLI (not Hub's "get from version
  control"), then Unity Hub → Add → the checked-out folder. Then install Unity
  **6000.5.10f1**. Make sure the new `ignore.conf` is also checked in.

## Rolling log (newest first, trim to ~15)

- 2026-08-27 — Added `docs/claude/` continuity system: this file, generated
  `project-map.md` (`Tools/regen-claude-map.ps1` / `.sh`, plus editor menu
  *Tools > White Lightning > Regenerate Claude Map*), synced `memory/` copy,
  root `ignore.conf`, and a **Cross-machine continuity** section in `CLAUDE.md`.
  Also `.claude/settings.json` with SessionStart + SessionEnd hooks that fire the
  continuity reminder on every session switch (needs `.claude/` added to UVCS;
  open `/hooks` once on a new machine to trust it).
