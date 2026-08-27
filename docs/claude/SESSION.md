# Session handoff — White Lightning

> The living "resume here" note for Claude Code sessions across machines.
> Read this first. Update it before handing back. See `docs/claude/README.md`.

---

**Last updated:** 2026-08-27T19:15:00Z
**Machine:** WINDOWS (`C:\Projects\MoonshineSimUnity`)
**Milestone:** 1 — make the still-run *feel* good in isolation
**VCS:** git + Git LFS — `github.com/stervon1/MoonshineSimUnity`, branch `main`

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

- **First push not done yet (2026-08-27):** the initial commit was rebuilt with
  Git LFS (1489 binaries pointerized, `core.autocrlf=false`, `ignore.conf` +
  `.plastic/` dropped) but has **not been pushed**. Pushing ~1.9 GB of LFS
  objects will blow GitHub's free LFS tier (1 GB storage + 1 GB/mo bandwidth) —
  buy a GitHub LFS data pack first, or repoint `origin` at GitLab (10 GB free).
  Command: `git push -u origin main`. Optional local cleanup to drop the orphaned
  pre-LFS blobs (~2 GB, frees disk, removes the migration undo path):
  `git reflog expire --expire=now --all && git gc --prune=now`.
- The old Plastic/UVCS setup is abandoned. `.plastic/` still sits on disk
  (gitignored) — safe to delete once the git push is confirmed working.

## Rolling log (newest first, trim to ~15)

- 2026-08-27 — Migrated VCS from Unity Version Control (Plastic) to **git + Git
  LFS**. Rebuilt the single initial commit so `.gitattributes`-matched binaries
  (fbx/png/wav/pdf/…) are LFS pointers; set `core.autocrlf=false`; dropped
  `ignore.conf` and gitignored `.plastic/`. Continuity docs/scripts/hooks updated
  from UVCS wording to git. **Not pushed yet** — see Open questions.
- 2026-08-27 — Added `docs/claude/` continuity system: `SESSION.md`, generated
  `project-map.md` (`Tools/regen-claude-map.ps1` / `.sh`, plus editor menu
  *Tools > White Lightning > Regenerate Claude Map*), synced `memory/` copy, and
  a **Cross-machine continuity** section in `CLAUDE.md`. Plus `.claude/settings.json`
  with SessionStart + SessionEnd hooks that fire the continuity reminder on every
  session switch (open `/hooks` once on a new machine to trust it).
