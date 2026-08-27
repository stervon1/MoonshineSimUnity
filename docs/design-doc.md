# Moonshine — Game Design Document (v3)

**Working title:** *White Lightning* (placeholder)
**Genre:** Self-directed craft & come-up sim — realistic distilling, cozy/satisfying tone
**Reference points:** Car Mechanic Simulator (diagnose → tool → fix), PowerWash Simulator (tactile completion, low stakes). Progression fantasy: basement rig → licensed still house.
**Engine:** Unity 6 (URP, C#, VFX Graph for liquid/steam effects)
**Tone:** Grounded, technical, Appalachian folk-craft aesthetic. Low-stakes: the illegality early on is **texture** — cash out the back door of a restaurant, keep it quiet — **never danger**. No busts, no heat meter, no crime-drama. The satisfaction is craft mastery plus the come-up from a jury-rigged pot to a legal facility.

> This is the design vision. For build status see `docs/project-plan.md`; for
> the distilling facts and the still-run simulation model see
> `docs/distillation-reference.md`. Prototype decisions so far: **first-person**
> presentation (not fixed diorama), **untimed**, uGUI for prototype UI,
> **self-directed batches + back-door buyers** (v3 — replaced the client-job spine).

---

## 1. Core Fantasy

The player is a self-taught distiller running a tiny, off-the-books operation. You decide what to make, run real distilling chemistry and process work by hand, and sell the jars quietly — out the back door of a restaurant or bar — for cash. That cash goes into a better rig and a bigger hidden space, and eventually buys the thing the whole game is pointed at: **a license and a real facility**, where you finally do it legit and at scale. Mastery comes from precision and better tools; progress comes from the hustle, not from managing danger.

---

## 2. Core Gameplay Loop

```
Plan a batch  (you choose: spirit style, target proof, batch size —
               size is capped by your current rig) →
Design mash bill → Ferment → Run the still (the tactile centerpiece) →
Proof it → Take it to a back-door buyer (restaurant/bar; they appraise it) →
Get paid  (more when it matches what that buyer wants)  + build rapport →
Reinvest: better still (more volume, more control), bigger hidden space →
         then save toward the LICENSE →
Licensed facility → legit, larger runs, the endgame sandbox
```

No risk sandbox and no heat system — a weak batch just sells for less; there is no bust, no confiscation, no "redo it or else."

---

## 3. Self-Directed Batches & Back-Door Buyers

The client spec sheet is gone. What replaces it:

### 3.1 Batch plan (player-authored)
- Before a run the player sets a **batch plan**: spirit style (corn whiskey, rye, sugar shine…), target proof, and batch size.
- **Batch size is hard-capped by the rig** — the Tier-1 jury-rigged pot makes only a few jars at a time. Raising the cap is a core reason to upgrade.
- The plan is intent, not a contract: you can miss your own target proof and still sell the result.

### 3.2 Back-door buyers (demand)
- A small roster of **buyers** — the chef at a diner, a bartender, a caterer — each with a **preference profile**: favoured spirit style, an acceptable proof band, a lean toward *smooth* vs *characterful*, and price sensitivity.
- Selling = walk a jar over; the buyer **appraises** it against their preference and offers a price. Close match → good price; off-profile → they still buy, just low.
- Buyers have a **standing appetite** (how much they'll take per visit) that grows with rapport.
- No fail state: every batch finds a buyer somewhere, even if it's the one who'll take anything for pennies.

### 3.3 Rapport & the network
- Repeated good sales build **rapport** with a buyer → higher prices, bigger appetite, and **introductions to new buyers** (more demand variety, including rare tastes — aged product, a specific grain character).
- Rapport is the soft progression gate alongside cash; the license needs both a bankroll and enough of a network to make going legit worthwhile.

---

## 4. Production Systems (the craft, kept tactile)

### 4.1 Mash Bill Design
- Grain ratio selection (corn, rye, malted barley, wheat, sugar) drives a flavor vector and fermentable sugar yield.
- Presented as a simple, satisfying mixing interface — pour/measure animations, visual grain color in the mash tub.
- Water source (workshop-dependent) modifies conversion efficiency — a passive bonus, not a puzzle to solve.

### 4.2 Fermentation
- Day-based timer, not real-time. Player checks in, adjusts temperature, reads a hydrometer.
- Visual/tactile feedback: bubbling airlock rate signals fermentation activity — a satisfying ambient indicator, like a car engine idling.
- No "stuck fermentation" punishment mechanic — keep this stage low-friction; it's downtime between the tactile centerpieces.

### 4.3 The Still Run — the centerpiece interaction
This is the game's PowerWash-nozzle equivalent. Deserves the most polish.
*Simulation model, tuning numbers, and the purity-vs-character scoring axis:
`docs/distillation-reference.md` §4, §7, §9.*
- Player applies heat, watches a **live temperature/flow gauge**, and calls three cuts: heads, hearts, tails.
- Visual feedback per cut: a clean hearts cut should *look and sound* satisfying — clear stream, a pleasant chime, a proof reading ticking into the "good" zone. A late/early cut shows visibly cloudier distillate and a duller sound cue, not a punishing failure state.
- Precision affects the completion score, not survival — a mediocre cut still finishes the job, just with a lower quality report.
- Tool tiers change *feel*, not risk:
  - **Basement pressure-cooker rig:** imprecise gauge, some needle wobble — charming jankiness, not danger. Occasional "adjust the jury-rigged tubing" mini-interaction (like a quick QTE), framed as quirky rather than hazardous.
  - **Proper pot still:** stable, accurate gauge, smooth cut curve.
  - **Column/reflux still:** near-automatic purity — fast, easy, high consistency, but flattens flavor complexity (a character trade-off, not a strict upgrade).

### 4.4 Proofing
- A simple, tactile mixing task: add water to cut proof down to spec, watching a live proof readout — same satisfying "dial it in" feel as the still run, lower stakes.

### 4.5 Aging (optional side content)
- Barrel/wood-chip aging over in-game weeks for jobs that request a bourbon-style product. Purely additive content, not required for core loop.

### 4.6 Batch Clipboard (toggleable HUD)
- A clipboard the player toggles on/off (currently **Tab**), carried on the person — the tactile stand-in for a menu.
- Two panes: a **checklist** of batch steps (plan → mash → ferment → strip → spirit run → cuts → proof → sell) that ticks itself as stages complete, and a **live data** readout (batch plan, current phase, proof, cut quality, sale price).
- Prototype is read-only/auto-ticked; later: manual ticking, per-batch checklists, buyer notes (who wants what), and pinning your own target numbers for glanceable reference during the still run.
- Model is engine-agnostic (`ClipboardModel`) so the same data can feed a physical in-world clipboard prop later.

### 4.7 Tactile execution (hold-to-do, not click-to-do)
The prototype uses single presses; the target is **Farming Simulator / PowerWash**-style *hold* actions.
- **Transfers** are two held actions with a fill/drain meter: hold **E** at a source to fill a vessel (bucket of grain, jug of water, the jar), hold **E** at the destination to pour it in. Over-pour spills, short-fill shrinks the batch — quality/yield cost, never a hard fail. This replaces the current one-press grain pick and water-cut.
- **Intermittent tasks** during a stage — stir the mash on a timer, hold the rest temp, read the hydrometer, manage take-off rate and cooling water, vent the pressure rig, air and dilute jars before blending. Surfaced as a station cue (wobble / sound / pulsing marker), resolved with a quick hold-E. Tending them feeds the run's quality score, so a careful operator out-earns a masher.
- Real tasks and numbers: `docs/distillation-reference.md` §2–§6, §9.3. Build plan: `project-plan.md` → "Tactile depth backlog" (do before the M3 art reskin).

---

## 5. Workshop Progression — the come-up

Spaces are **capability upgrades**, bought with cash + rapport. Tiers 1–3 are
progressively bigger *off-the-books* spaces; **Tier 4 is the license — the goal
the game is pointed at**, not a stretch tier.

| Tier | Space | Still | Batch cap | What it adds |
|---|---|---|---|---|
| 1 | Basement | Jury-rigged pressure cooker | a few jars | Charming jankiness, wobbly gauge, the tutorial come-up |
| 2 | Mountain hollow | Pot still | small run | Spring-water bonus (better mash/cuts), cleaner control |
| 3 | Riverside barn | Column still + multiple fermenters | bulk | Parallel batches, higher-volume buyers, aged product |
| 4 | **Licensed distillery** | Industrial + aging warehouse | commercial | **Legit** — no more back doors; branded product, wholesale, the largest runs. Endgame sandbox |

No visibility/discovery risk system at any tier — upgrades are pure capability
gains. Going legit (Tier 4) is a **one-time purchase gate** (cash + a rapport
threshold, maybe a fixer contact) that flips the selling model from back-door
buyers to open wholesale.

**3D presentation note:** each workshop tier is a walkable/lookable 3D space (basement, hillside dugout, riverside barn), not a menu screen — explored in **first-person** (WASD + mouse look, `FirstPersonController`), with in-world objects driven by look-at interaction (`Interactable` / `PlayerInteractor`). In Unity, URP + VFX Graph handles the liquid stream/steam effects for the still-run cut, with baked/mixed lighting doing the heavy lifting on mood rather than fully dynamic GI — a deliberate trade for iteration speed over Unreal's Lumen.

---

## 6. Meta-Systems

### 6.1 Economy
- Earn **cash per sale**. Price ≈ base(volume) × quality × proof-match × buyer-preference-match × rapport multiplier.
- Spend on: better gauges/thermometers (reduce guesswork), still upgrades (tighter control **and** a higher batch cap), space upgrades, grain/yeast variety (unlocks styles/flavours buyers pay a premium for).
- **The big money sink is the license** — the run to Tier 4. Everything before it is reinvestment toward that.
- No risk-driven spend — no fines, no confiscation, no bribes-to-avoid-a-bust.

### 6.2 Buyers, Rapport & Demand Variety
- A roster of **back-door buyers** (diner chef, bartender, caterer…), each with a preference profile (style, proof band, smooth↔characterful, price sensitivity, appetite).
- **Rapport** rises with good sales → higher prices, larger appetite, and **introductions** to new buyers — this is how demand variety and rarer tastes (aged, specialty grain) enter the game.
- **Post-license:** individual back-door buyers give way to **wholesale accounts** — steadier, larger, less personality-driven; the endgame is volume and consistency.

### 6.3 Difficulty/Accessibility
- Buyer proof-band tightness and gauge precision as difficulty sliders — punishing appraisal + vague gauges for a challenge, forgiving buyers + crisp gauges for players who just want the tactile loop.

---

## 7. Open Questions
- ~~Soft time limit vs. fully untimed?~~ **Untimed.**
- ~~Fixed diorama vs. free-look?~~ **First-person.**
- ~~Client jobs vs. self-directed?~~ **Self-directed batches + back-door buyers** (v3).
- **License gate:** pure cash, or cash + a rapport threshold + a "fixer" contact? What's the transition beat — a cutscene, a new space unlocking, a one-time paperwork interaction?
- **Post-license:** stay self-directed wholesale, or reintroduce explicit contracts (the old spec-job idea) as endgame content?
- **Do buyers ever place explicit, time-bounded orders** ("5 jars ~100-proof corn this week"), or only standing preferences? Standing-only keeps the game untimed.
- **Selling interaction:** a menu/dialogue at the buyer, or physically carry jars to a spot and hand them over (more tactile, matches the first-person craft framing)?
- Equipment "mess/cleanup" secondary loop between batches — worth it?
- Final UI layer: uGUI (prototype) vs. UI Toolkit — decide during the M1 "feels good" pass.

---

## 8. Recommended Build Order (prototype scope)
1. **Still-run vertical slice**, Tier 1 rig only — mash/ferment minimal, all polish on the cut moment. The game's identity.
2. **The "make" half:** batch plan (pick style / target proof / size) → proofing mini-task → a batch-result readout (proof, clarity, flavour vector).
3. **The "sell" half:** one back-door buyer who appraises a batch against a preference profile and pays; cash + rapport into `GameState`. Minimal make→sell→cash loop closed.
4. **Widen it:** several buyers with distinct profiles, rapport, introductions; still/space upgrades that raise control **and** the batch cap.
5. **The goal:** license purchase → Tier-4 facility → wholesale selling model.
6. **Stretch:** aged product, Tier 3 depth, richer buyer characters/dialogue.
