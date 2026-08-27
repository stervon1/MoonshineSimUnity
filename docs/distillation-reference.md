# Distillation Reference

Working knowledge base for the White Lightning sim — process facts we pull into
mechanics, tuning, job specs, tooltips, and flavour text.

> **Living document.** Sources, all retrieved 2026-08-27:
>
> | tag | source | strong on |
> |---|---|---|
> | `[HD]` | HomeDistiller **wiki** — homedistiller.org/wiki | cut theory, terminology |
> | `[HDF]` | HomeDistiller **forum** (via web search) | real operating rates & temps |
> | `[CH]` | Clawhammer Supply — *How to Make Moonshine* | all-grain mash recipe, temps/times |
> | `[SS]` | Still Spirits — *How to Take Cuts During Distillation* | jarring / air-out / blend method |
> | `[DC]` | DistilCalc — *Pot Still Distillation Simulator* + *Spirit Cuts Calculator* | a published physics model we can port |
>
> `[X+Y]` = sources agree. `[general knowledge]` = well-established, uncited.
> [§8](#8-source-reconciliation) tracks agreement/conflict. [§9](#9-still-run-simulation-model)
> is the synthesised model spec for `StillRunController`.

---

## 1. Process overview

Grain/sugar/fruit → **wash** (ferment) → **stripping run** (fast, crude) →
**spirit run** (slow, with cuts) → **cuts** (discard/keep/recycle fractions) →
**proofing** (dilute to bottling strength) → **maturation** (optional barrel).

The sim's centrepiece is the **spirit run + cuts**. Everything else is context.

---

## 2. Wash, mash, fermentation

### Corn-whiskey mash — worked example `[CH]`

- **Grain bill:** 6.75 gal water · 9 lb flaked maize · 2 lb malted barley
  (CH gives a scaling table for 2.5–30 gal). Optional **1–9 lb sugar** to lift ABV.
- **Mash steps:**
  1. Heat water to **165 °F (73.9 °C)**.
  2. Stir in flaked maize → temp drops to **148 °F (64.4 °C)**.
  3. Stir in malted barley; hold **60 min**, stir every 10 min.
     _[general knowledge]_ flaked maize is pre-gelatinised, so barley enzymes
     convert both grains at this one ~148 °F rest — no cereal boil.
  4. Optional pasteurise: **≥170 °F (76.7 °C) for 10 min**.
  5. Cool to **75 °F (23.9 °C)** to pitch.

### Fermentation

- **Pitch** `[CH]`: ~**1 tbsp yeast per 5 gal**.
- **Temp** `[CH]`: **70–80 °F (21–27 °C)**.
- **Duration** `[CH]`: **7–10 days**.
- **Done when** `[CH]`: bubbling stops **and** mash tastes dry; or SG steady over
  a couple of days **at/near 1.000**.
- **Wash ABV going into the still** `[DC]` presets: sugar wash **8–12 %**,
  grain **6–9 %**, fruit **8–14 %**.
- **Methanol in the wash** `[DC]`: sugar **50–300 ppm**, fruit **500–3 000 ppm**.
- Rate eyeballed by airlock bubble rate / CO₂ (`[HD]` "gas flow analysis").

### Terminology `[HD]`

*mash* = grain + hot water to convert starch · *wort* = sweet liquid drawn off ·
*wash* = fermented liquid ready to distill · *must* = fruit/grape juice ·
*marc* = pressed grape skins/pomace. Beer ≠ wash. Wine → brandy.

**Maps to sim:** M2 fermentation check-in. Mash rest temp + sugar + yeast →
potential ABV → still charge strength. `FG ≈ 1.000` + "tastes dry" = success signal.
Feedstock (grain/sugar/fruit) sets methanol ppm → foreshots severity.

---

## 3. Distillation

- **Definition** `[HD]`: "purifying a liquid by successive evaporation and
  condensation." Separates by **boiling point**.
- **A still = three parts** `[HD]`: **pot** (heated) · **condenser** (vapour→liquid)
  · **receiver** (collects). Pot still adds a **lyne arm** with a bend (the *lyne
  angle*); optional **thumper/doubler**, **heat diffuser** (anti-scorch) `[HD]`.
- **Boiling points:** ethanol **78.4 °C / 174 °F**, water **100 °C / 212 °F**
  `[CH+general]`. Methanol **64.7 °C** `[general]`.
- **Azeotrope:** ~**95.6 % ABV** (≈ **89.4 mol %** ethanol `[DC]`) — practical
  ceiling; a pot still never gets near it, a good reflux column reaches **95–96 %**
  `[HD]`.

### 3a. Stripping run vs. spirit run

| | Stripping run | Spirit run |
|---|---|---|
| Goal `[HD]` | strip alcohol from wash fast, **no cuts** | refine + make cuts |
| Speed | fast, high power | **slow** — slower = easier cuts, better separation, longer run `[HDF]` |
| Front discard `[CH]` | first **100 ml** | at least first **50 ml** |
| Jars `[SS]` | ~**24 × 400 ml** | numbered **250–500 ml** lots `[HD+SS+HDF]` |
| Output | "low wines", ~**40 % ABV** combined `[HD]`; dilute to **≤40 %** before the spirit run `[SS]` | foreshots/heads/hearts/tails |
| Practice | combine several washes' low wines into one spirit-run charge `[HD]` | — |

`[HD]` cut ABV figures assume a **40 % ABV low-wines charge**.

**Freeze concentration** `[HD]` — stripping alternative: 5 gal @ 7–10 % → 2.5 gal
@ 17–20 %, no heat off-notes (applejack method).

### 3b. Still types

| Type `[HD]` | Parts | Output ABV | Use |
|---|---|---|---|
| **Pot still** | boiler, lyne arm, condenser (+ optional thumper) | single **30–40 %**; double **70–80 %**; careful **~60 %** on run 1 `[HD]` | whisky, rum, brandy — flavour from the mash |
| **Reflux / column** | boiler, column (packed), condenser, column condenser | **95–96 %** `[HD]` | vodka, gin, neutral |

`[HD]`: "the reflux still can produce mind-stomping purity, the pot still must be
run with a sense of art." Reflux operator controls: **reflux ratio**, **take-off
rate**, cooling water, packing (HETP via Onda's method); control schemes **VM /
LM / CM** (vapour / liquid / cooling management).

**Thumper/doubler workflow** `[HD]`: boiler full of wash; doubler **⅓ full** with
wash (or 50/50 wash + neutral). After each run, tip the doubler charge into the
next fresh wash; add that run's **tails** into the doubler. Discard boiler +
doubler liquids at day's end, keep tails for next time.

---

## 4. Cuts and fractions — the centrepiece

Collected front-to-back. `[HD]` rule: **cut by taste and smell**; temperature and
ABV are "rough guides at best." `[HDF]`/`[DC]` add temperature bands and
volume/ethanol-progress heuristics as scaffolding.

### Pot-still temperature story `[HDF]`
- **Warm-up:** turn on condenser cooling water at vapour **~60 °C** `[HD]`.
  A 25 L boiler @ 2.4 kW reaches bubble point in **~55–65 min** `[DC]`.
- **Foreshots/early heads:** ~**40–45 °C** first drops `[HD]`; volatiles come off
  as vapour climbs to **65–70 °C** `[HDF]`.
- **Hearts:** temperature "suddenly rises" and settles **78 – low-80s °C** `[HDF]`.
  Whisky: start collecting at **78 °C**; clean spirit: wait for the temp to
  **stabilise** `[HDF]`.
- **Tails:** temperature "suddenly heads for the 90s °C" `[HDF]`.

### Collection rate (≈ 5 gal / 19 L pot still) `[HDF]`
- **Slow 15–20 ml/min · medium 25–30 ml/min · >30 ml/min pulls tails early.**
- ≈ **1.5 L/hour** ≈ 25 ml/min.
- Drops/sec: heads **1–3**, hearts **5+ / broken stream**, tails **2–4**.

### Foreshots
- **First** distillate, mostly **acetone** (**not** methanol — see §7). Sweet
  smell, headache. **Always discard** `[HD+CH+SS]`.
- **How much** (spread across sources): `[HD]` 150 ml/25 L wash · `[HD]` 100 ml/20 L
  (10 ml tiny stills) · `[HDF]` 50 ml/20 L low wines ≈ **2–3 ml per L** ·
  `[SS]` **50–200 ml** · `[CH]` 100 ml strip / ≥50 ml spirit / "first 10 %".
  → **model as a tunable band ≈ 2–8 ml per L of wash**; more for fruit.
- ~**82 % ABV** off a 40 % charge `[HD]`.

### Heads
- Compounds: methanol, **acetone, ethyl acetate, ethanol** `[HD]`; +
  **acetaldehyde** `[CH]`.
- ABV ~**82 → 80 %** `[HD]`. Volume ~**2–3 L** from three stripped 25 L sugar
  washes `[HD]`. "Sweet & buttery" nose, "solvent sting", hangover fraction.
- **Save as feints** for reprocessing `[HD]` — not discarded.

### Hearts
- The **body** — clean, no chemical bite `[HD]`.
- Starts ~**80 % ABV** `[HD]`. **Hearts→tails cut is style-dependent:**
  - `[HDF]` pot still: hearts run **~75–65 % down to ~58–55 % ABV** by style.
  - `[HD]` off low wines: cut at **~70 %** (whisky) down to **~50 %** (rum,
    full-bodied).
- Largest kept fraction; the blending base.

### Tails
- Smell "**wet dog, wet cardboard, damp socks**" `[HD]`; taste "**dull, watery**"
  `[CH]` → bitter → cardboard → "dirty water" `[HD]`.
- Fusel oils — oily film / crystals `[HD]`.
- Still alcohol-rich. **Stop** ~**40 %** (pot still, keep-quality) `[HDF]`, down to
  **10–20 %** `[HD]` / **≤10 %** `[SS]` if collecting for feints.
- **Recycle into feints** with heads `[HD]`.

### The sensory method `[HD+SS]`
1. Collect into **numbered 250–500 ml jars**.
2. **Air out** jars — 24 h `[SS]` / 1–2 days `[HD]` under muslin / coffee filter.
3. **Dilute the sample to 35–40 % ABV** before nosing/tasting `[HD]`.
4. Taste tiny amounts, **spit, rinse** between jars `[HD]`.
5. **Blend:** hearts base, add heads/tails **incrementally** `[HD]`.

### Yields & recovery `[HD]`
- Final drinkable ≈ **30–50 % of total collected volume**; rest = feints.
- Blend gone wrong: discard foreshots, re-distill the rest with **water or
  backset**.

### The core tradeoff — verbatim `[HDF]`
> "Start it too late and stop it too early, and you collect mostly pure ethanol
> and no flavours. Start too early and stop too late, well you make something that
> will have a lot of flavour, but will give you ripper hangovers."

This **is** the sim's quality/character axis. Tight cut = clean but characterless;
wide cut = characterful but rough. Job spec picks where the sweet spot sits.

---

## 5. Proofing / dilution

In the codebase already — `ProofingUtility`, Pearson's Square:

- `waterToAdd = heartsVolume × (heartsProof − targetProof) / targetProof`
- _[general knowledge]_ add water **slowly**, rest, re-measure at **20 °C**
  (mixing is exothermic + volume-contracting). Whisky/vodka floor **40 % ABV
  (80 proof)**; cask strength 55–65 %.
- `[HD]` data: **Unit Conversion / real-time converters**, **Tables, Charts**.

---

## 6. Maturation, storage, flavouring _(stub)_

### Mechanisms `[HD]`
1. **Evaporation** of spirit through the dry part of the barrel (angel's share).
2. **Leaching** — oak chemicals into the spirit (extraction).
3. **Diffusion** of spirit through the wood (oxidation via ingress).

### Compounds that shift during ageing `[HD]`
aldehydes · colour · esters · fixed & total acidity · furfural · fusel oil · pH ·
solids · proof · tannin. (Extraction adds colour/tannin/vanillin; oxidation
mellows harsh aldehydes/fusels; proof drifts with humidity.)

### To expand `[general knowledge]`
oak species; **barrel size** — smaller = higher surface-area:volume = faster,
harsher if overdone; **char level** 1–4 (deeper char = more carbon filtering +
"red layer" colour/caramel); warehouse temp/humidity swings drive the wood
"breathing"; years not months for a full barrel, weeks–months for chips/small
casks. Chip/stave shortcuts `[HD]`: oak chips, charred chips, cork, sherry add.

**Maps to sim:** M4 aging content (barrels, char level, time-based flavour).

---

## 7. Methanol — corrected model `[HD]` + `[DC]`

- **Not** "the foreshots." Methanol is **present throughout the run**; the
  methanol:ethanol **ratio rises** slowly through heads/hearts and **peaks in the
  tails**, even though absolute methanol *volume* falls as ABV drops `[HD]`.
- "Not produced in any harmful quantity in a standard grain or fruit mash" `[HD]`.
  Sugar wash **50–300 ppm**, fruit **500–3 000 ppm** `[DC]`.
- The high ethanol:methanol ratio **buffers** metabolism (ethanol out-competes
  methanol for the enzyme), letting the body clear formic-acid products safely
  `[HD]`.
- So **foreshots discard targets acetone / acetaldehyde / ethyl acetate**, not
  methanol. Fruit/pectin washes are the ones that actually warrant caution.

**Maps to sim:** feedstock → methanol ppm → a "roughness/hangover" contribution
that's *worst if you run deep into tails* (not just if you skip foreshots). Fruit
jobs are higher-risk, higher-reward.

---

## 8. Source reconciliation

| Topic | Consensus | Notes / spread |
|---|---|---|
| Cut order foreshots→heads→hearts→tails | **All agree** | — |
| Discard foreshots entirely | **All agree** | — |
| Foreshots **volume** | — | 2–8 ml per L of wash across `[HD] [HDF] [CH] [SS]`; `[SS]` 50–200 ml flat; more for fruit. **Tunable.** |
| Make the cut by… | smell/taste primary `[HD]` | temp bands `[HDF]`, volume/ethanol-progress `[DC]` are scaffolding, not contradictions |
| Hearts→tails ABV | style-dependent | **50–70 %** `[HD]` vs **55–75→58–55 %** `[HDF]`; compatible, both say "lower = fuller/rum, higher = cleaner/whisky" |
| Pot still hearts start | ~80 % `[HD]` / high-70s–low-80s °C `[HDF]` | consistent (ABV vs temperature framing) |
| Stop the run | ~40 % ABV to keep quality `[HDF]`; 10–20 % `[HD]` / ≤10 % `[SS]` for feints | **not a conflict** — different goals (drinkable vs recover-for-redistill) |
| Methanol = foreshots? | **Myth.** `[HD]` methanol spread through run, peaks in tails | foreshots = acetone/acetaldehyde/esters |
| Ethanol BP | 78.4 °C `[general]` ≈ 174 °F `[CH]` | rounding |
| Mash temps/times | **`[CH]` is the source** | 165→148 °F, 60 min, cool 75 °F |
| Ferment time/temp | **`[CH]` is the source** | 7–10 d, 70–80 °F, FG ~1.000 |
| Physics model | **`[DC]` is the source** | see §9 |
| Bottling dilution | none of them detail it | `ProofingUtility` + general knowledge |

**Net:** no load-bearing conflicts. `[HD]` = cut theory + terminology; `[HDF]` =
operating rates/temps; `[CH]` = mash recipe; `[SS]` = jarring workflow; `[DC]` =
the maths.

---

## 9. Still-run simulation model

Synthesised target model for `StillRunController`. `[DC]` publishes a real
thermodynamic model; we can port a **simplified** version of it.

### 9.1 `[DC]` model (reference — what a "correct" sim does)

**Inputs:** boiler volume · wash ABV · spirit-style preset (whisky/rum/brandy/
gin/vodka — adjusts cut thresholds) · heat input **2–3 kW** home · methanol ppm ·
advanced: atmospheric pressure/altitude, thermometer error, ambient temp, still
insulation, theoretical plates.

**Outputs (live curves over time):** boiler temp ambient→bubble point · vapour
ABV (distillate strength) · boiler ABV decline · collected volume + flow rate ·
auto heads/hearts/tails/spent classification · methanol concentration · VLE
operating point.

**Equations `[DC]`:**
- Heat-up: `dT/dt = (Q·η − Q_loss) / (m·Cp)` — `m` boiler mass, `Cp` mixture heat
  capacity.
- Vapour rate: `Q̇_vap = Q·ε / ΔHvap(x_E)` — latent heats **ethanol 841 kJ/kg**,
  **water 2260 kJ/kg**, weighted by distillate composition. → as ethanol depletes,
  more energy goes to boiling water, so **flow rate naturally falls late in the
  run**.
- VLE (modified Raoult + Van Laar activity):
  `P_total = γ_E·x_E·P_E^sat(T) + γ_W·x_W·P_W^sat(T)`;
  `y_E = γ_E·x_E·P_E^sat(T) / P_atm`.
  Van Laar constants **A₁₂ = 1.6798, A₂₁ = 0.9227**.
- Bubble point solved at local `P_atm`; azeotrope ceiling **89.4 mol %** ethanol.
- Cut classification driven by **"recovered ethanol progress"** (fraction of the
  charge's total ethanol collected so far), with vapour ABV + temp as guardrails.

### 9.2 What to replicate (simplified, arcade-tuned)

State per run:
- `washVolume`, `washAbv` (from the mash/ferment stage), `methanolPpm` (feedstock),
  `powerKw` (player-set 1–3), `insulation`, `equipmentTier`.
- Derived: `boilerAbv` (falls as ethanol leaves), `vaporAbv` (what's dripping now),
  `boilerTempC`, `flowRateMlMin`, `ethanolRecoveredFrac`.

Update loop (per tick):
1. **Heat-up phase** until `boilerTempC` ≈ bubble point (~78 → 92 °C rising with
   `boilerAbv`). Duration scales with `washVolume / powerKw` (~55–65 min for
   25 L @ 2.4 kW `[DC]` → compress for gameplay).
2. **Vapour ABV** from `boilerAbv` via a fitted curve (stand-in for VLE): high
   (~85–90 %) while boiler is strong, decaying toward ~10–20 % as boiler nears 0.
   Clamp ≤ azeotrope (95.6 %); reflux tiers pull `vaporAbv` up toward 95 %.
3. **Flow rate** ∝ `powerKw` but ÷ latent-heat term → drifts down through the run.
   Player raises power → faster fill, worse separation (band-widening), higher
   scorch risk on Tier 1.
4. **Cut phase** is where `ethanolRecoveredFrac` lands:
   - Foreshots: first `tunable ml` (2–8 ml/L wash, ×fruit factor) — always toss.
   - Heads: next slice (~10–20 % of ethanol).
   - Hearts: the broad middle (~55–65 %).
   - Tails: the rest.
   The player doesn't see these bands — they see temp, `vaporAbv`, drip rate,
   smell/haze cues, and call `CallCutToHearts()` / `CallCutToTails()`.
5. **Boiler temp** rises with falling `boilerAbv` (78 °C strong → 90s °C spent) —
   the "sudden rise" at the heads→hearts and hearts→tails transitions is a
   readable tell.

### 9.3 Quality & character scoring

Two axes, from §4's tradeoff quote:
- **Purity / cleanliness** — high when the kept cut sits inside the true hearts
  band; drops for every ml of heads or deep-tails included. Worsened by high
  `powerKw` (band-widening) and Tier-1 wobble.
- **Character / body** — *rewarded* for taking a wider, lower cut… **up to the
  job's target**. A vodka job wants a tight high cut; a rum/"heavier corn
  sweetness" job wants a wide low cut.
- **Roughness / hangover** — driven by `methanolPpm` × how deep into tails +
  amount of heads kept + skipped foreshots. Fruit jobs punish deep tails hard.

Final score = how close (purity, character, proof) the batch lands to a
**target profile**. In the v3 spine that target is the player's own **batch
plan** (style + target proof) judged against the chosen **buyer's preference
profile** (style, proof band, smooth↔characterful) — closer match = higher sale
price. **No hard fail** — a bad cut just sells for less, per the design
constraint. (Code today still calls this `JobSpec`; rename pending — project-plan
M2.)

### 9.4 Equipment tiers → modifiers

| Tier | Rig | vaporAbv ceiling | Separation | Wobble / scorch | Notes |
|---|---|---|---|---|---|
| 1 | pressure cooker | ~80 % | poor (wide bands) | high needle wobble, scorch if power high | single pass, `[HD]` "30–40 %" territory before care |
| 2 | pot still (+thumper) | ~88 % | fair | low | `[HD]` 60–80 % with care; thumper = free half-redistill |
| 3 | reflux / column | ~95 % | good; **reflux-ratio control** | none | vodka/gin viable; player also manages take-off vs reflux |
| 4 | licensed still | ~96 % | excellent | none | commercial column, batch scheduling (M4) |

Maps to `StillRunController.equipmentTier` / `GameState.WorkshopTier`.

---

## Sources to mine next

`[HD]` wiki is mostly stubs past *Cuts and fractions* and *Using a Pot Still* —
the depth is in the **forum**. Priority:

1. HD forum: **"Pot run: how low and how slow?"**, **"Pot Still Collection Rate"**,
   **"Length of time to distill"** — nail down run-duration + rate curves for §9.2.
2. `[DC]` **Spirit Cuts Calculator** page — grab its per-style cut % splits for
   §9.3 job targets.
3. **Cranky's Spoonfeeding** (linked from HD Beginner's Guide) — canonical
   step-by-step spirit run.
4. HD **Methanol** deep dive + **Safety** / **The Rules We Live By** — roughness
   model + tooltip copy.
5. HD **Reflux Column Design** / a *VM/LM/CM* explainer — Tier 3 controls.
6. Maturation depth: barrel-size / char-level / time curves for M4.
7. HD **Tried and True Recipes** + `[CH]` recipe → realistic `JobSpec` presets.
8. `[DC]` simulator itself — a working comparable to sanity-check our curves.
