# Project Plan — White Lightning (Unity Edition)

Milestones follow the design doc's build order, adapted for Unity's
workflow. Each milestone should be playable/testable before moving on.

> **Living document — review and update this file after every step.**
> Check boxes as work lands; annotate partial work with `[~]` and a note.
>
> Docs set: `design-doc.md` (vision) · this file (status) ·
> `distillation-reference.md` (process facts + still-run sim model). `README.md`
> has the rebuild-from-scratch tool order.

---

## Current state — 2026-08-27

> **Session end (2026-08-27):** M1 + M2 playable end-to-end and feels good.
> Greybox everywhere. Open threads: finish URP material migration; save the
> outdoor scene as its own file; **tactile-transfer loop + intermittent tasks**
> (new, from playtest — see [Tactile depth backlog](#tactile-depth-backlog));
> then M3 art pass one prop at a time.

- **Design spine (v3):** pivoted from client-given jobs to **self-directed
  batches + back-door buyers** — you choose the recipe, sell jars quietly out
  the back of a restaurant for cash, grind toward a **license + legal facility**.
  Illegality is texture, **no risk system**. See `design-doc.md` v3. Code
  renamed job→batch (`BatchPlan`, `GameState.CurrentBatch`/`Rapport`,
  `OnBatchStarted`/`OnBatchSold`, `BatchPlanPrototypeUI`); scene rebuilt +
  verified. Buyer/demand system is M2.
- **Render pipeline:** URP is now set up (`Assets/Settings/URP-Pipeline.asset`
  + `URP-UniversalRenderer.asset`) and assigned as the default. The project had
  silently been on Built-in despite the URP package being present. Pack
  materials are being migrated via each pack's bundled `*_URP_*.unitypackage`
  plus the Render Pipeline Converter (Built-in → URP, Material Upgrade).
- **Playable scene:** `Assets/Scenes/Prototype.unity` — first-person walk in a
  greybox basement (swap to the outdoor clearing with a menu item), look at the
  white-block controls and press **E** to drive the still-run.
- **Outdoor sandbox:** exists as a swappable environment root
  (`OutdoorEnvironment`); not yet saved as its own `OutdoorSandbox.unity`.
- **Editor tooling** — one command **`Tools > White Lightning > Rebuild Prototype`**
  runs the whole pipeline (URP → base scene → **outdoor clearing** → player →
  still rig → workshop stations → clipboard). Individual builders live under
  `Tools > White Lightning > Steps > …` (basement is `Steps > Build Basement Room`).
- **Everything is a world interaction** (design constraint): no screen panels.
  `IInteractable` + `PlayerInteractor` (look-at + E, carries one `Carryable`,
  right-click = station info card). Stations: 6 grain bins → mash tub →
  fermenter → still (white-block rig: Start / Cut hearts / Cut tails / Vent) →
  proofing station → 3 buyer counters → workbench (`UpgradeStation`). The still
  has a 3-dial board (`StillGauge`: proof / temperature / pressure) + signs +
  a bobbing next-step arrow (`NextStepMarker`). The **clipboard** (Tab) is the
  only screen UI.
- **M2 loop is coded:** `BatchController` state machine (`BatchState`/`BatchStage`),
  `BuyerGenerator` + `BatchAppraisal` (price = style × proof-band × lean × quality
  × rapport). Sale → `GameState.Cash` / `Rapport`.
- **CLI:** `Unity -batchmode -executeMethod BuildScript.Build -quit` compile-gates
  headlessly; produces a real player build now that `Prototype.unity` is in
  Build Settings.
- **Docs:** `docs/distillation-reference.md` — process knowledge base for tuning
  cuts, proofing, job specs, flavour text. Sources: HomeDistiller wiki + forum,
  Clawhammer, Still Spirits, DistilCalc (which publishes its physics model),
  reconciled in a §8 table. **§9 is a synthesised still-run simulation model**
  (state vars, update loop, quality/character/roughness scoring, tier modifiers)
  to drive `StillRunController`. Has a "sources to mine next" list.

---

## Milestone 0 — Scaffold ✅ done

- [x] Unity 6 project + package manifest (URP, VFX Graph, TextMeshPro, Input System)
- [x] `GameState` singleton (current job/batch, reputation, payout, workshop tier)
- [x] `JobGenerator` / `JobSpec` (spirit style, quantity, target proof, tolerance, flavor request)
- [x] `StillRunController` (heads/hearts/tails cuts, quality scoring, equipment-tier wobble)
- [x] `ProofingUtility` (Pearson's Square water-cut math)
- [x] Minimal prototype UI scripts (`JobBoardPrototypeUI`, `StillRunPrototypeUI`)
- [x] Prototype scene built (`PrototypeSceneBuilder` → `Assets/Scenes/Prototype.unity`),
      loop confirmed end to end: accept job → start run → cut to hearts →
      cut to tails → result.

---

## Milestone 1 — Still-run feel in 3D (the centerpiece) — in progress

- [x] **Block out a basement still** — `BasementRoomBuilder` (BrokenVector
      LowPolyDungeon kit, 18 × 14 m). Bonus: `OutdoorSceneBuilder` clearing.
- [~] **Liquid-stream + steam VFX reading `StillCutPhase` + cut quality** —
      `StillFX` drives placeholder `ParticleSystem`s (stream colour/clarity by
      phase + last cut quality; steam ramps with the run). Real **VFX Graph**
      version is still an in-editor task; the driver stays.
- [x] **Gauges** — `StillGauge` (physical needle), a 3-dial board on the still:
      **Proof · Temperature · Pressure**. Run it like a real pot still: vapour
      temp warms to ~78 C, plateaus through the hearts, climbs to the 90s as the
      boiler runs dry — cut by watching it. Tier-1 pressure-cooker rig builds
      **pressure**; a "Vent the boiler" block bleeds it; redlining during the
      hearts costs quality (no hard fail). Smooth Perlin sway, not jitter.
- [~] **Sound cues** — `StillAudio` hooks: run-start / cut (pitch by quality) /
      finish / boil loop / cooling-water cue. Clip slots empty — needs audio assets.
- [x] **Real `AnimationCurve` for `proofCurve`** — code-authored in
      `StillInteractionRigSetup` (90 s run); tune on the `Still` Inspector.
- [ ] **Playtest: does the cut moment feel satisfying?** — waits on real VFX
      Graph + audio.

### Interaction, camera & HUD

- [x] `FirstPersonController` — WASD, mouse look, sprint, jump, Esc.
- [x] `IInteractable` + `PlayerInteractor` — look-at + E, one-item carry
      (`Carryable`). Every station and the white blocks implement it.
- [x] White-block still controls (Start / Cut hearts / Cut tails).
- [x] **Screen-space prototype panels retired** (`BatchPlanPrototypeUI`,
      `StillRunPrototypeUI` deleted) — everything is a world interaction now.
- [x] **Clipboard** (`ClipboardModel` + `ClipboardController`) — the only screen
      UI. Checklist + batch data + **buyer preference list** + cash/rapport.

---

## Milestone 2 — Make → sell → cash loop

The batch/buyer spine. See `design-doc.md` §3, §8.

- [x] **Renamed** the job system → batch: `JobSpec`→`BatchPlan`,
      `JobGenerator`→`BatchPlanGenerator` (`GenerateJob`→`GeneratePlan`),
      `GameState.CurrentJob`→`CurrentBatch`, `Reputation`→`Rapport`,
      `OnJobAccepted`→`OnBatchStarted`, `OnJobCompleted`→`OnBatchSold`,
      `StartNewJob`→`StartBatch`, `CompleteJob`→`SellBatch`,
      `BatchCompletionReport`→`BatchSaleReport` (`reputationGain`→`rapportGain`),
      `JobBoardPrototypeUI`→`BatchPlanPrototypeUI`, clipboard labels
      ("Plan the batch" / "Sell the batch"). Scene rebuilt; still-run + clipboard
      verified working in-editor (2026-08-27).
- [x] `BuyerGenerator` — rolls a roster of buyer preference profiles.
- [x] **Batch plan = pick a grain bin** (`GrainBin` ×3) — sets style + starts a
      batch. No UI. Target proof is per-buyer (on the clipboard), not typed.
- [~] Mash bill — `MashStation` (grain → mash). Full grain-**ratio** flavour
      vector deferred to M3; style-only for now.
- [x] Fermentation — `FermentStation` time-skip → wash ABV.
- [x] Still-run output → proofing — `BatchController` captures the run result;
      `ProofingStation` waters it down step by step (physical `ProofGauge`).
      _Note:_ uses a flat `proof − step`; wire real `ProofingUtility` math in M3.
- [x] Batch-result readout — the clipboard (proof / stage / cut quality / cash)
      + the world gauge. No pass/fail.
- [x] **Back-door buyers** — `BuyerCounter` ×3 + `BatchAppraisal` (style ×
      proof-band × lean × quality × rapport → price). Carry the jar over, sell.
      Cash + rapport → `GameState`.
- [x] **Guidance layer** — `NextStepMarker` (bobbing arrow over the next
      station), world-space **signs** on every station, right-click **info
      cards** (`StationInfoCard`, `Billboard`) explaining what each step does to
      make moonshine. Buyer counters show their preference in the look-at prompt.
- [x] **Six grains** (`SpiritStyle`: corn / rye / malt / wheat / sugar /
      molasses) — one bin each. Buyers now ask for the **drink** (rye whiskey,
      rum, sugar shine…) not the grain (`Spirits.DrinkName`).
- [x] **Economy MVP** — `UpgradeTrack` (linear: bigger boiler → copper worm →
      pot still → hollow → column → **licence**), `UpgradeStation` (the
      workbench), `GameState.Cash`/`UpgradeLevel`/`BuyNextUpgrade`, clipboard
      shows next step + cost. Effects are loose (WorkshopTier bumps); real
      per-upgrade effects = M3.
- [ ] Reskin — **reverted to greybox primitives** (a prop `InstantiatePrefab`
      threw and aborted the pipeline, losing clipboard/arrow/buyers). Redo the
      art pass carefully in M3, one prop at a time. `MoonshinePipeline` now
      isolates each step in try/catch so one failure can't cascade.
- [x] **Hover progress %** — looking at a station mid-process shows completion:
      `MashStation`/`FermentStation` prompts ("Mashing… 42%"), and a new
      `StillStatusReadout` on the still ("Hearts - 61% - 82 C - 9 psi").

---

## Milestone 3 — The come-up (buyers, rapport, upgrades)

- [ ] **Tactile transfers + intermittent tasks** — see
      [Tactile depth backlog](#tactile-depth-backlog). Do this *before* the
      art reskin so props dress the final interaction.
- [ ] Several buyers with distinct preference profiles + appetites.
- [ ] Rapport: good sales → better price / bigger appetite / introductions to
      new buyers (demand variety, rarer tastes).
- [ ] Selling interaction — carry jars to the buyer vs. a menu (design-doc §7).
- [ ] Still + space upgrades: raise control **and** the batch cap. Economy for it.
- [ ] **Grain market** — buy grain with cash instead of free bins; stock/price
      per grain; feeds the mash-bill flavour vector.
- [ ] Basement polish pass; Tier 2 mountain-hollow space (`OutdoorSceneBuilder`
      start), player traversal between spaces.

---

## Milestone 4 — Going legit (the goal) + scale

- [ ] **License purchase** — cash (+ rapport threshold?) gate; transition beat.
- [ ] Tier 4 licensed facility space; selling model flips to **wholesale
      accounts** (steadier, larger, less personality-driven).
- [ ] Tier 3 riverside barn: column still, multiple fermenters, parallel batches.
- [ ] Aging content (barrels, char level, time-based flavour development).
- [ ] Lighting/art pass (baked GI, post-processing) once mechanics prove fun.

---

## Tactile depth backlog

From the 2026-08-27 playtest — "feels good, now make each step *hands-on*."
The Farming-Simulator / PowerWash reference: you don't press a button, you
**hold to do the thing** and watch a fill/drain meter. Land this before the M3
art pass so the reskin dresses the final interaction, not the placeholder.

### 1. Tactile transfers (hold-E fill / pour)

Every ingredient or liquid move becomes two held actions with a **percent meter**:

- **Fill** — hold **E** at a source (grain bin, water tap, still outlet). A
  vessel you're carrying fills 0 → 100 %. Release to stop.
- **Pour / empty** — hold **E** at a destination (mash tub, fermenter, jar,
  proofing vessel). The vessel drains into it.

Vessels & routes:

| Vessel | Route |
|---|---|
| Bucket / sack | grain bin → mash tub (fill = how much grain; sets batch size, capped by rig upgrade) |
| Jug / can | water tap → mash tub (mash liquor), water tap → proofing vessel (the cut) |
| The jar | still outlet → (proofing) → buyer counter |

Design notes:
- `Carryable` gains `capacity`, `fillLevel`, `contents` (grain type / wash /
  low wines / hearts). Spilling (over-pour) and short-filling both cost yield /
  quality — no hard fail.
- Replaces today's single-press `GrainBin.ChooseGrain` and
  `ProofingStation.AddProofingWater` step-clicks.
- Fill rate can be a per-vessel / per-upgrade stat (bigger scoop later).
- Batch-size cap (economy upgrades) = max grain the mash tub accepts.

### 2. Intermittent tasks (from `distillation-reference.md`)

Recurring things you must tend *during* a stage. Surface as a station cue
(needle wobble, a sound, the next-step arrow pulsing) resolved with a quick
hold-E or press. Missing them nudges quality down, never blocks.

| Stage | Task | Source |
|---|---|---|
| Mash | **Stir every ~10 min**; hold the rest temp near 148 °F / 64 °C | `[CH]` §2 |
| Ferment | Check airlock bubble rate; take a **hydrometer / gravity reading**; skim krausen; add nutrient | `[HD]` §2 |
| Spirit run | **Manage heat** — keep take-off ≲ 25–30 ml/min or you pull tails early; **adjust cooling water** (built — `StillRunController.AdjustCoolingWater`, cue every 12 s, untended = quality penalty on hearts samples, mirrors the pressure-redline pattern); **vent** (tier 1, built); collect into numbered jars | `[HDF]` §3a, §4 |
| Cuts | **Air the jars** ~24 h, then **dilute-to-35–40 %** before nosing/blending | `[HD]` §4 |
| Proofing | Add water **slowly**, rest, **re-measure at 20 °C** (exothermic + volume contraction) | §5 |
| Aging (M4) | Rouse / rotate barrels; top up for the angels' share | §6 |

Still open: mash-stir and ferment tending tasks, and the tactile-transfer
loop itself (§1 above) — needs new carryable vessels/prefabs and scene
wiring, deferred pending a session with Unity Editor access to verify.

Scoring hook: each tended task feeds the run's `heartsQuality` / `smoothness`
(reference §9.3), so a careful operator out-earns a button-masher.

---

## Open questions

*(design-doc.md §7 is the authority; the live ones for build order:)*

- **License gate** — pure cash, or cash + rapport threshold + a fixer contact?
  What's the transition beat?
- **Post-license** — self-directed wholesale, or reintroduce spec-contracts as
  endgame content?
- **Buyer orders** — standing preferences only (keeps it untimed), or explicit
  time-bounded asks?
- ~~Selling interaction~~ — **carry the jar to the buyer counter** (built).
- **Equipment-cleaning** secondary loop — worth it?
- **Asset sourcing** — free packs for greybox (BrokenVector, Polytope,
  SimpleNaturePack, Toon Gas Station); custom / paid later.
- **uGUI vs. UI Toolkit** — prototype stays uGUI; decide at the M1 polish pass.
- *Resolved:* camera = first-person; jobs = untimed; spine = self-directed
  batches + buyers.

---

## Housekeeping / tech debt

- [ ] Import + delete the loose `*.unitypackage` files under `Assets/` (incl.
      the pack URP upgrade packages once applied).
- [ ] Finish the URP material migration — verify nothing renders magenta after
      the converter pass.
- [x] Removed BrokenVector `Demo Scenes/ScreenshotMaker.cs` (broke compilation;
      pack demo scripts can do this — watch for more).
- Generated greybox materials live in `Assets/Scenes/_*.mat` — acceptable for now.
- No CI wiring beyond the `BuildScript` compile gate.
- `CoffeeShopStarterPack` — deleted its `Scripts/` + `Prefabs/Gameplay/`
  (2026-08-27); kept the cafe models/props. `Prefabs/OrderGenerator.prefab` +
  `Prefabs/Bewerage/*` + `Scenes/` + `Test/` still ref the removed scripts
  (benign missing-script if opened) — delete if you want it fully clean.
- Scene-light shadows: builders now cap additional-light shadows (outdoor: only
  the sun; basement: only `StillLamp`) so the 4096 shadow atlas doesn't overflow.
- `MoonshinePipeline` isolates each build step in try/catch — one failing
  builder no longer wipes the rest (learned when a prop `InstantiatePrefab`
  aborted the run and took the clipboard/arrow/buyers with it).
- Prop reskin is **reverted to greybox**; redo it one prop at a time in M3,
  after the tactile-transfer work.
- **`Assets/Scenes/OutdoorSandbox.unity` not saved yet** — outdoor still lives
  only as a swappable root inside `Prototype.unity`.
- Per-upgrade effects are stubbed — buying only bumps `WorkshopTier`; wire real
  effects (batch cap, wobble, still tier) in M3.

### Next session — start here
1. Save the outdoor scene as its own file (or decide `Prototype.unity` stays outdoor).
   **Blocked without Unity Editor access** — scenes are editor-tool-generated,
   not hand-authored; needs a session that can run `Tools > White Lightning`.
2. Tactile-transfer loop — `Carryable` gains `capacity`/`fillLevel`/`contents`;
   hold-E fill at bins/tap, hold-E pour at mash/fermenter/proofing (backlog §1).
   Needs new carryable vessel prefabs + scene wiring — do this with Editor
   access so it can be verified in Play mode before landing.
3. ~~Intermittent tasks — start with the still run~~ **Cooling-water task done**
   (2026-08-28, no-Editor session): `StillRunController.AdjustCoolingWater`,
   `StillTaskCue` pulsing light, `StillAudio.coolingCueClip` hook, fifth white
   block wired in `StillInteractionRigSetup`. Mash-stir + ferment tending still
   open (backlog §2).
4. Then M3 buyers/rapport/economy-effects, then the art reskin.
