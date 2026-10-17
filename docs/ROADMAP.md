# Incremental roadmap

Each milestone should leave a playable, testable slice. Complete its acceptance criteria before expanding scope. Update [STATUS.md](STATUS.md) after each milestone and record new decisions in [DECISIONS.md](DECISIONS.md).

| Milestone | Scope | Acceptance criteria |
| --- | --- | --- |
| M0 — context and baseline | Project instructions, vision, architecture proposal, roadmap, status, decisions, and license log. | All required documents exist; observed project facts are separated from proposals; existing Git changes remain untouched. |
| M1 — first-person exploration | One small greybox 3D space, first-person controller, camera, WASD, mouse look, sprint, jump, and collision. Reuse or deliberately extend the existing Input System asset. | A macOS play test allows free exploration, stopping, backtracking, sprinting, and jumping. No automatic player movement or camera path. Unity compiles without new errors. |
| M2 — first-person weapons and shooting | One local hitscan assault rifle, finite magazine and reserve, automatic fire, R reload, placeholder audiovisual feedback, ammunition HUD, and resettable damage targets in the existing map. | Movement still works; targets take configured damage; fire cadence, empty magazine, and reload rules work; feedback appears and plays; Unity compiles without new errors. |
| M3 — enemy AI and combat | One local NavMesh melee enemy with Idle, Chase, Attack, and Dead states; enemy and player health, damage and death feedback, health HUD, and test reset. Place three enemies in the existing graybox. | Players can trigger detection, flee through doorways, take damage, shoot and kill enemies, and restart after death. Existing controller, rifle, and targets still work. Unity compiles cleanly. |
| M4 — local danger zones and waves | Two reusable player-triggered zones in the existing graybox, warnings, NavMesh enemy waves, progress HUD, completion, and scene reset. | Room two spawns three enemies once; room three spawns two then four; only each zone's enemy deaths advance it. The player remains free to move and shoot. Unity compiles cleanly. |
| M5 — online multiplayer and lobby | Add Sessions/Relay join codes, NGO ownership and scene sync, host-owned combat, health, AI, and shared danger zones in one reviewable multiplayer slice. | Two separate macOS clients can host/join, control independent players, damage shared enemies, clear shared waves, and leave cleanly. A compile/build and documented manual test cover the full slice. |
| M6 — missions, extraction, and scoring | Merged by PR #7 on 2026-10-17: clear both danger zones, collect a sample, extract together, show success/failure and synchronized statistics, then replay or return to the lobby. | Two players can complete the full loop from lobby through combat, three objectives, extraction, and results. Both clients agree on the result; replay resets all match state; movement and camera remain player-controlled until results. |
| M7 — validate and extend | Stabilize the prototype, test 3–4 player capacity, improve combat/audio feel, and decide the next content feature. | Record a 3–4 player test result, known performance/network limits, and a prioritized next slice. Add a second map, enemy, or weapon only after the first loop is stable. |

## Milestone discipline

- Add only the packages and cloud configuration needed for the active milestone. Confirm any paid service or asset before enabling it.
- Placeholder geometry, sounds, and effects are acceptable. Record external asset sources and licenses before use.
- If a milestone exposes a design problem, update the decision record and acceptance criteria before broadening implementation.
- Long-term progression, rankings, achievements, difficulty modes, bosses, and content updates stay outside the first prototype.

**First development milestone:** M1, a small explorable graybox with responsive first-person movement and no automatic travel.

M1–M4 were merged into `develop` by PRs #2–#5, M5 by PR #6, and M6 by PR #7 on 2026-10-17. M7 is next; [STATUS.md](STATUS.md) lists the open items carried over from M6.
