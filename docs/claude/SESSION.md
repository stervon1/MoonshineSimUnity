# Session handoff — White Lightning

> The living "resume here" note for Claude Code sessions across machines.
> Read this first. Update it before handing back. See `docs/claude/README.md`.

---

**Last updated:** 2026-08-28T02:15:00Z
**Machine:** MAC (`/Users/sterlingevon/MoonshineSimUnity`)
**Milestone:** 3 prep — tactile depth backlog (before the M3 art pass)
**VCS:** git + Git LFS — `github.com/stervon1/MoonshineSimUnity`, branch `main`

## Current focus

Tactile depth backlog from the 2026-08-27 playtest (`docs/project-plan.md` →
*Tactile depth backlog*): make each workshop step hands-on (hold-E, watch a
meter) before the M3 reskin so props dress the final interaction. Two parts —
**tactile transfers** (this session) and **intermittent tasks** (a parallel
session started the still-run cooling-water task, merged as PR #1).

## In progress

- **Tactile transfers — core loop landed, needs a playtest + tuning pass.**
  New hold-interaction framework: `IHoldInteractable` (+ `PlayerInteractor`
  hold loop and `(NN%)` meter), `FillSource` / `PourTarget` components,
  `Carryable` vessel state (`capacity` / `Contents` / `FillLevel` / `GrainStyle`
  + greybox `fillVisual`). Grain bins are now a `FillSource`; `GrainBin.cs`
  deleted. Flow: pick up bucket → hold-E at a bin (fills 0→100%, stamps style,
  sets batch size) → hold-E at the mash tub (`MashStation` drives a sibling
  `PourTarget`) → mash starts. Proofing station is hold-E now: proof falls
  continuously, meter spans 150→70. `BatchState.batchSizeGallons` is set from
  how much grain you pour (rig-capped via `BatchController.MashCapForTier`) and
  scales `heartsVolumeL` at run end. Compiles clean. **The rebuilt
  `Prototype.unity` is NOT committed** (a full regen is a ~59k-line YAML diff
  best reviewed by a human) — after checkout run *Tools > White Lightning >
  Steps > Add Workshop Stations* (or *Rebuild Prototype*) to pick up the new
  components. **Not yet playtested in this session.**
- **Version HUD (side task).** `Assets/Scripts/UI/VersionHud.cs` shows
  `v{ver} · build N · {sha}` top-left in every scene (F2 toggles);
  `BuildVersionStamper` (editor) bumps `Assets/Resources/BuildInfo.json` on
  every recompile whose sources changed and on every player build.
  `BuildInfo.json` is gitignored (per-checkout artifact).

## Next steps

1. **Playtest the transfer loop** and tune `unitsPerSecond` / `minGrainToMash`
   on `FillSource` / `PourTarget` / `MashStation`, and `proofPerSecond` on
   `ProofingStation`. Confirm the meter reads well against the reticle.
2. **Tactile transfers — go wider** (deferred from this pass): water tap
   station + water jug vessel, still-outlet → jar fill, mash-liquor step,
   spill / short-fill quality penalties. See the backlog table.
3. **Intermittent tasks** — continue from PR #1's `StillTaskCue` pattern:
   mash stir, ferment gravity check, cuts airing/dilution. Feed
   `heartsQuality` / `smoothness` (reference §9.3).
4. Then the M3 art pass, one prop at a time.

## Open questions / decisions pending

- _(none blocking — the earlier "push blocked / diverged history" item is
  resolved: the GitHub repo was recreated, LFS pack in place, and `main` now
  shares history with the remote. A parallel session's work landed as PR #1.)_
- `.plastic/` still sits on disk (gitignored) — safe to delete.
- CLI note (Mac): `Unity …` on PATH is the new Unity CLI wrapper
  (`~/.unity/bin`), which rejects `-batchmode`. For the headless build call the
  editor binary directly:
  `"/Applications/Unity/Hub/Editor/6000.5.10f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -projectPath . -executeMethod BuildScript.Build -quit -logFile build.log`
  (and close the editor first — batch build can't open a project already open).

## Rolling log (newest first, trim to ~15)

- 2026-08-28 (MAC) — **Tactile transfers, core loop.** Added
  `IHoldInteractable` + hold support in `PlayerInteractor`, `FillSource` /
  `PourTarget`, `Carryable` vessel state (+ `fillVisual`). `MashStation` /
  `ProofingStation` converted to hold-E; `BatchController` graduated
  `AddProofingWater(float)` + `SetBatchSize` (rig cap) + batch-size → yield;
  `BatchState.batchSizeGallons`. `GrainBin.cs` removed (bins are `FillSource`);
  `WorkshopStationsBuilder` adds a bucket + wires the new components. Fixed
  `regen-claude-map.sh` for bash 3.2 (`mapfile` → read loop).
- 2026-08-28 (MAC) — Added a top-left **version HUD** (`VersionHud`) +
  `BuildVersionStamper` auto-bumping `Assets/Resources/BuildInfo.json` on
  recompile/build; `BuildInfo.json` gitignored.
- 2026-08-27 — Headless CI build verified green on Mac (`BuildScript.Build`,
  StandaloneOSX, ~7.5 min) once the editor was closed. Documented the Unity CLI
  wrapper gotcha (see Open questions).
- 2026-08-27 — Migrated VCS from Unity Version Control (Plastic) to **git + Git
  LFS**. Rebuilt the initial commit so `.gitattributes`-matched binaries are LFS
  pointers; `core.autocrlf=false`; dropped `ignore.conf`, gitignored `.plastic/`.
- 2026-08-27 — Added `docs/claude/` continuity system: `SESSION.md`, generated
  `project-map.md` (`Tools/regen-claude-map.{ps1,sh}` + editor menu), synced
  `memory/` copy, **Cross-machine continuity** section in `CLAUDE.md`, and
  `.claude/settings.json` SessionStart/SessionEnd hooks.
