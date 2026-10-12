# Incremental roadmap

Each milestone should leave a playable, testable slice. Complete its acceptance criteria before expanding scope. Update [STATUS.md](STATUS.md) after each milestone and record new decisions in [DECISIONS.md](DECISIONS.md).

| Milestone | Scope | Acceptance criteria |
| --- | --- | --- |
| M0 — context and baseline | Project instructions, vision, architecture proposal, roadmap, status, decisions, and license log. | All required documents exist; observed project facts are separated from proposals; existing Git changes remain untouched. |
| M1 — first-person exploration | One small greybox 3D space, first-person controller, camera, WASD, mouse look, sprint, jump, and collision. Reuse or deliberately extend the existing Input System asset. | A macOS play test allows free exploration, stopping, backtracking, sprinting, and jumping. No automatic player movement or camera path. Unity compiles without new errors. |
| M2 — first-person weapons and shooting | One local hitscan assault rifle, finite magazine and reserve, automatic fire, R reload, placeholder audiovisual feedback, ammunition HUD, and resettable damage targets in the existing map. | Movement still works; targets take configured damage; fire cadence, empty magazine, and reload rules work; feedback appears and plays; Unity compiles without new errors. |
| M3 — enemy AI and combat | One local NavMesh melee enemy with Idle, Chase, Attack, and Dead states; enemy and player health, damage and death feedback, health HUD, and test reset. Place three enemies in the existing graybox. | Players can trigger detection, flee through doorways, take damage, shoot and kill enemies, and restart after death. Existing controller, rifle, and targets still work. Unity compiles cleanly. |
| M4 — local danger zones and waves | Two reusable player-triggered zones in the existing graybox, warnings, NavMesh enemy waves, progress HUD, completion, and scene reset. | Room two spawns three enemies once; room three spawns two then four; only each zone's enemy deaths advance it. The player remains free to move and shoot. Unity compiles cleanly. |
| M5 — two-player online foundation | Validate and configure selected multiplayer packages and online project setup; host/join flow; two owned player objects in the map. Use a simple join code and one mission. | Two separate macOS clients connect online, each controls only its own player and sees the other move. Joining, leaving, and a host exit produce understandable results without orphaned player objects. Package versions and service requirements are recorded. |
| M6 — networked combat and health | Apply host-confirmed rifle damage and health to two players, enemies, and targets; synchronize relevant shot and hit outcomes. | Host and joiner can fire; damage and health agree on both clients; rapid fire does not apply duplicate damage; both clients receive appropriate feedback. |
| M7 — shared danger zones | Make the M4 encounter states, activation, enemy spawns, and completion host-owned and synchronized. | Either player can activate a zone once; both see consistent waves and progress; only the host advances enemy decisions and encounter completion. |
| M8 — mission and extraction | One selectable mission, one simple objective, extraction trigger, success/failure result, and a minimal reward/result placeholder. | Two players can complete the full loop from group creation and mission selection through combat, objective, and extraction. Both clients agree on the result. Movement and camera remain player-controlled throughout. This is the first playable prototype. |
| M9 — validate and extend | Stabilize the prototype, test 3–4 player capacity, improve combat/audio feel, and decide the next content feature. | Record a 3–4 player test result, known performance/network limits, and a prioritized next slice. Add a second map, enemy, or weapon only after the first loop is stable. |

## Milestone discipline

- Add only the packages and cloud configuration needed for the active milestone. Confirm any paid service or asset before enabling it.
- Placeholder geometry, sounds, and effects are acceptable. Record external asset sources and licenses before use.
- If a milestone exposes a design problem, update the decision record and acceptance criteria before broadening implementation.
- Long-term progression, rankings, achievements, difficulty modes, bosses, and content updates stay outside the first prototype.

**First development milestone:** M1, a small explorable graybox with responsive first-person movement and no automatic travel.

M1, M2, and M3 were merged into `develop` by PRs #2, #3, and #4. The owner requested local danger zones as M4 before the online foundation. M4 is implemented on `feature/danger-zones`, with a human full-route and input play test still needed. See [STATUS.md](STATUS.md).
