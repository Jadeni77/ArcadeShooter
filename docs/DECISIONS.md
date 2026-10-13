# Decisions and open questions

Use this file to distinguish decisions the project owner has made from technical options to test. Add a date, reason, and impact when a proposal becomes a decision. Do not silently change the confirmed game pillars.

## Confirmed decisions

| ID | Decision | Reason and impact |
| --- | --- | --- |
| D-001 | Build a 3D online cooperative first-person shooter for 2–4 players. | This is the stated game vision. Systems must support independently controlled players working toward a shared mission. |
| D-002 | Players control movement and camera freely; the game is not a rail shooter. | Exploration and player agency are core to the experience. No automatic travel path or forced camera progression. |
| D-003 | Danger zones can start warnings, waves, ambushes, defense goals, and bosses over time. | Encounters should be reusable, but only one simple zone is needed in the first prototype. |
| D-004 | Use Unity 6000.6.5f1, URP, C#, and Git. Develop and test on macOS first; plan for Windows later. | Matches the current project and development environment. |
| D-005 | Build a narrow two-player first prototype before broader progression and content. | The first slice validates movement, online cooperation, shooting, an enemy, a danger zone, an objective, and extraction without implementing all future features at once. |
| D-006 | Use placeholders first and track third-party asset sources and licenses. Do not introduce paid services or assets without approval. | Keeps the early prototype inexpensive and its assets auditable. |
| D-007 | Use `main` for approved stable releases and `develop` for staging; branch features and normal fixes from `develop`, and urgent production hotfixes from `main`. | Separates release history from active development. The remote branch bootstrap was completed by PR #1. |
| D-008 | Route repository changes through PRs. Feature and normal fix PRs target `develop`; release PRs target `main`; production hotfixes return to `develop`. | Gives each change an explicit review and integration path. |
| D-009 | After bootstrap, do not commit or push directly to `main` or `develop`, force push protected branches, or let Codex merge PRs without explicit approval. Require automated checks once configured and human review where GitHub can enforce it. | Protects both branches and keeps merge authority with the project owner. |
| D-010 | For M1, use the installed Input System Player actions and a `CharacterController` with separate movement and camera scripts. Keep the player local and add ownership checks in the later online milestone. | Reuses the existing controls, gives collision and grounded jumping, and avoids adding a networking dependency before the online slice. |
| D-011 | 2026-10-11: For the owner-requested M2, build a local hitscan automatic rifle and resettable damage targets before the online foundation. R reloads from a finite reserve after a timed delay. | Proves the shooting loop in the existing map without installing network packages or services. Host authority and synchronization remain for a later milestone. |
| D-012 | 2026-10-12: Build local enemy AI and player combat as M3 before the online foundation. Use the installed AI Navigation package, the existing `IDamageable` seam, three placeholder enemies, and an R-key test reset after player death. | Proves detection, pathfinding, melee, rifle damage, and health in the graybox without adding networking. The reset is only for testing; teammate revive and host authority remain later work. |
| D-013 | 2026-10-12: Build two local danger zones as M4 before the online foundation. Use serialized scene components for per-zone warning, waves, delays, and NavMesh spawn markers; reuse `EnemyHealth.Died` for completion. | Proves reusable room encounters without a second health system or networking package. The encounter state resets with the test scene; shared host authority remains for the later online slice. |
| D-014 | 2026-10-12: Use NGO 2.13.3, Multiplayer Services Sessions 2.4.0 with Relay, and Multiplayer Play Mode 3.0.0 for the M5 join-code prototype. | Supported packages installed on Unity 6000.6.5f1. Private four-player sessions provide a small lobby and network connection without public matchmaking. Service use and billing must be reviewed in the Unity Dashboard before broader testing. |
| D-015 | 2026-10-12: Player owners control their CharacterController, camera, and immediate weapon cosmetics; the host owns ammunition, final damage and health, AI, target state, and danger-zone progression. | Local controls stay responsive while consequential gameplay decisions are shared. The host checks shot cadence, ammo, alive state, and aim plausibility, but owner movement remains trusted in this prototype. |
| D-016 | 2026-10-12: Join before match start; the host locks the session and loads the one test map. A host exit ends the session and returns clients to the menu. A dead player stays dead for the match; leaving and hosting again resets the test. | Keeps the first network loop bounded. Host migration, late joining, reconnect, revival, and advanced respawn await later milestones. |

## Proposed architecture, pending verification

| Topic | Proposal | Why it is open |
| --- | --- | --- |
| Online stack | Implemented in M5; see D-014. | Two-client functional behavior and Dashboard service settings still need verification. |
| Hosting | Implemented in M5; see D-015 and D-016. | Two-client latency, disconnects, and operating constraints still need verification. |
| Reuse | Use small mission, weapon, enemy, and encounter definitions where a second use case justifies them. | Avoid premature frameworks while preserving room for new content. |
| Audio | Use AudioSource and an AudioMixer for weapons, enemies, ambience, music, and UI as the audio slice grows. | No game audio assets or mixer exist yet. |

## Questions to resolve at the relevant milestone

1. How should the existing Interact action be used in the eventual mission loop? The first gun uses keyboard R for Reload. (Later mission milestone)
2. What movement replication and validation model gives responsive controls while preserving host authority over consequential state? (M5/M6)
3. Does the M2 hitscan rifle's eight rounds per second, 30-round magazine, 90-round reserve, and 1.6-second reload feel right in human play testing? (Before networked combat)
4. What should happen when the host leaves: end the match, allow reconnect, or migrate host state? The first prototype can choose a simple documented behavior. (M5/M8)
5. How are players invited or matched beyond the first join-code flow? (After M8)
6. What concrete scoring, rewards, progression, and retention systems should follow a proven mission loop? (After M8)
7. What online service setup and usage terms apply before enabling external services? (M5)
8. Does the GitHub repository plan and reviewer roster support enforceable required human approvals on both protected branches? Confirm during the repository bootstrap. (Before the first PR merge)
