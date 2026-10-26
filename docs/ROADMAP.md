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
| M7 — weapon visuals and animations | Merged by PR #10 on 2026-10-17. ADS, view motion, reload presentation, remote rifle, and optional rig adapter use project-made interim geometry. | See [WEAPON_VISUALS_TESTING.md](WEAPON_VISUALS_TESTING.md) for the remaining human feel and two-client checks. |
| M8A — environment and lighting | Merged by PR #11 on 2026-10-18. The existing map has an approved research facility look with CC0 modular art and restrained URP lighting. | The [manual checklist](ENVIRONMENT_VISUALS_TESTING.md) still needs a recorded runtime pass. |
| M8B — audio and combat VFX | Owner accepted the feature for merge on 2026-10-21 through PR #12. Improved weapon/impact/enemy presentation and added local state-driven audio/music without changing gameplay authority. | Project-made placeholder clips and mixer routing are assigned. Two-player runs verified local/remote gunfire and Exploration → Combat → Exploration playback. The owner reports the milestone works; the remaining cue, duplicate-effect, and full-route observations stay documented in the [8B checklist](COMBAT_AUDIO_VFX_TESTING.md) and [acceptance audit](COMBAT_AUDIO_VFX_REVIEW.md), without a claim that each was measured. |
| M9 — progression and replayability | In feature PR: per-player local XP and levels, two cosmetics, Normal/Hard, and two Zone B layouts. The later milestone brief did not include the previously planned portal in D-020; its placement remains an open owner decision. | See [M9 checklist](PROGRESSION_TESTING.md). Automated tests and two-player manual validation must be recorded before acceptance. |
| M10 — advanced UI/UX and visual identity | Stage A cinematic design system and mockups are in draft PR #14. Stage B menu/lobby, Stage C HUD, and Stage D loadout/profile each have separate review work. Stages E–F cover extraction/results, pause/settings, responsive polish, multiplayer testing, and performance. | The owner reviews actual Unity screenshots for each screen. Functional flows, focus, multiple resolutions, two-client state, and final visual quality must pass before M10 acceptance; see [Stage B review](M10_STAGE_B_REVIEW.md) and [Stage D review](M10_STAGE_D_REVIEW.md). |

## Milestone discipline

- Add only the packages and cloud configuration needed for the active milestone. Confirm any paid service or asset before enabling it.
- Placeholder geometry, sounds, and effects are acceptable. Record external asset sources and licenses before use.
- If a milestone exposes a design problem, update the decision record and acceptance criteria before broadening implementation.
- Long-term progression, rankings, achievements, difficulty modes, bosses, and content updates stay outside the first prototype.
- Build only the active milestone. Record ideas that belong to a later milestone in [DECISIONS.md](DECISIONS.md) or under that milestone, and do not build them early.

**First development milestone:** M1, a small explorable graybox with responsive first-person movement and no automatic travel.

M1–M4 were merged into `develop` by PRs #2–#5, M5 by PR #6, M6 by PR #7, M7 by PR #10, and M8A by PR #11. The owner accepted M8B PR #12 for merge on 2026-10-21. See [STATUS.md](STATUS.md).

## Not yet scheduled

These items came out of M6 and are not part of M7–M9 as planned. Agree on a milestone with the owner before building any of them.

- Test three and four players. This was the old M7 (validate and extend), which the owner's M7–M9 plan replaced.
- A shared post-mission lobby that the squad returns to without the join code, with clearer **Leave Session** versus **Return to Lobby** wording on the results screen.
- A recorded run with two standalone players on separate machines.
- Probing a possible race when a client shot reaches the host in the same frame its reload finishes, using the Multiplayer Tools network simulator. Adding that package needs owner approval.

## Future audiovisual polish after functional M8B acceptance

- Replace the project-made gunfire, impact, reload, weapon handling, footstep, enemy, zone, extraction, countdown, and result WAVs with approved, licensed variations after working placeholder playback has been demonstrated. Approve and log each third-party asset before import.
- Replace the project-made muzzle cubes, impact fragments, and color-flash reactions with restrained URP particles, surface-specific textures, and richer enemy effects. Keep the current host-confirmed timing and pooling.
- Add rigged first-person hands and enemy attack, damage, and death animations when suitable approved art and rigs exist. Advanced character animation remains outside M8B; the current timed weapon motion and enemy tint/death burst are the interim presentation.
- Replace the synthesized exploration, warning, combat, and facility ambience loops with licensed music and sound design. Add detailed mechanical room ambience where it improves the existing facility. Boss music remains an extension point until a boss milestone is approved.

These are asset and presentation replacements. The owner accepted M8B for merge with project-made placeholders. The unmeasured playback, transition, mixer, and multiplayer details remain follow-up validation in the [8B checklist](COMBAT_AUDIO_VFX_TESTING.md); no full-route or 60 FPS result is claimed.
