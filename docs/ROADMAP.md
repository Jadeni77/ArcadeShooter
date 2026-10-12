# Incremental roadmap

Each milestone should leave a playable, testable slice. Complete its acceptance criteria before expanding scope. Update [STATUS.md](STATUS.md) after each milestone and record new decisions in [DECISIONS.md](DECISIONS.md).

| Milestone | Scope | Acceptance criteria |
| --- | --- | --- |
| M0 — context and baseline | Project instructions, vision, architecture proposal, roadmap, status, decisions, and license log. | All required documents exist; observed project facts are separated from proposals; existing Git changes remain untouched. |
| M1 — first-person exploration | One small greybox 3D space, first-person controller, camera, WASD, mouse look, sprint, jump, and collision. Reuse or deliberately extend the existing Input System asset. | A macOS play test allows free exploration, stopping, backtracking, sprinting, and jumping. No automatic player movement or camera path. Unity compiles without new errors. |
| M2 — two-player online foundation | Add and configure the selected multiplayer packages and required online project setup; host/join flow; two owned player objects in the map. Use a simple join code and one mission. | Two separate macOS clients connect online, each controls only its own player and sees the other move. Joining, leaving, and a host exit produce understandable results without orphaned player objects. Package versions and service requirements are recorded. |
| M3 — one gun and damage | One usable gun, basic fire/reload feedback, health and damage on both players and a target, with host-confirmed outcomes. | Host and joiner can fire; damage/health agree on both clients; rapid fire does not apply duplicate damage; placeholder gunshot, hit, and reload feedback are audible/visible. |
| M4 — enemy and danger zone | One NavMesh-driven enemy and one host-controlled danger zone with warning, a small wave, and completion. | Entering the zone triggers the encounter once according to its configured policy; both clients see consistent enemies and health; the host controls enemy decisions and wave completion. |
| M5 — mission and extraction | One selectable mission, one simple objective, extraction trigger, success/failure result, and a minimal reward/result placeholder. | Two players can complete the full loop from group creation and mission selection through combat, objective, and extraction. Both clients agree on the result. Movement and camera remain player-controlled throughout. This is the first playable prototype. |
| M6 — validate and extend | Stabilize the prototype, test 3–4 player capacity, improve combat/audio feel, and decide the next content feature. | Record a 3–4 player test result, known performance/network limits, and a prioritized next slice. Add a second map, enemy, or weapon only after the first loop is stable. |

## Milestone discipline

- Add only the packages and cloud configuration needed for the active milestone. Confirm any paid service or asset before enabling it.
- Placeholder geometry, sounds, and effects are acceptable. Record external asset sources and licenses before use.
- If a milestone exposes a design problem, update the decision record and acceptance criteria before broadening implementation.
- Long-term progression, rankings, achievements, difficulty modes, bosses, and content updates stay outside the first prototype.

**Recommended first development milestone:** M1, a small explorable greybox with responsive first-person movement and no automatic travel.
