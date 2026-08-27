---
name: keep-project-plan-updated
description: Update docs/project-plan.md after every step of work on MoonshineSimUnity
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 7f012ad2-9e22-4d81-b3b0-cad68f0b1ac8
  modified: 2026-08-27T15:54:29.644Z
---

The user wants `docs/project-plan.md` treated as a living document: review it and
update it after **every** step — check boxes as work lands, mark partial work
`[~]` with a note, keep the "Current state — <date>" block accurate, and log new
infrastructure (editor tools, scenes, systems) that isn't in the milestone list.

**Why:** the repo has no issue tracker and isn't a git repo; the markdown
checklist in `project-plan.md` is the single source of truth for progress.

**How to apply:** after finishing a change, before handing back, edit
`project-plan.md` to reflect reality. Convert relative dates to absolute in the
"Current state" heading. Related: [[moonshinesim-editor-tooling]].
