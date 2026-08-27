---
name: design-spine-v3-self-directed
description: MoonshineSimUnity pivoted from client jobs to self-directed batches + back-door buyers (v3)
metadata: 
  node_type: memory
  type: project
  originSessionId: 7f012ad2-9e22-4d81-b3b0-cad68f0b1ac8
  modified: 2026-08-27T16:30:16.257Z
---

As of 2026-08-27 the game's structural spine changed. **Out:** client-given
job/spec sheets. **In:** the player self-directs — pick your own recipe + target
proof + batch size (size hard-capped by the rig tier), make a small amount in a
jury-rigged still, sell jars quietly out the back of restaurants/bars for cash.
Buyers have **preference profiles** (style, proof band, smooth↔characterful,
price sensitivity); matching demand pays more. Rapport unlocks bigger appetite +
introductions. The whole game points at buying a **license + legal facility**
(Tier 4), which flips selling to wholesale.

**Illegality is texture, NOT tension** — no busts, no heat meter, no crime-drama.
Still low-stakes/cozy; a weak batch just sells cheap.

**Code lag:** still uses `JobSpec` / `JobGenerator` / `GameState.CurrentJob` /
`OnJobAccepted` / `JobBoardPrototypeUI`. Rename to batch/buyer is a tracked
Milestone-2 task, not yet done. `design-doc.md` is v3; `project-plan.md` M2–M4
rewritten. See [[keep-project-plan-updated]].
