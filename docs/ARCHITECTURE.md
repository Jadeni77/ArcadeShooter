# Technical architecture

This document separates observed project state from proposed implementation. Proposals are not installed or implemented by this document.

## Version control and delivery

`develop` is the staging branch and the base for feature and normal fix PRs. Approved releases move from `develop` to `main` by PR; urgent production hotfixes start from `main` and are brought back into `develop` by PR. `main` and `develop` receive no direct commits or pushes after the approved bootstrap. See [GIT_WORKFLOW.md](GIT_WORKFLOW.md) for branch names, reviews, checks, and the pending repository setup. This workflow governs delivery; it does not change the gameplay architecture below.

## Confirmed project baseline

| Area | Observed state |
| --- | --- |
| Editor and language | Unity 6000.6.5f1, C# |
| Development machine | macOS on Apple M4 with 24 GB RAM. |
| Rendering | Universal Render Pipeline 17.6.0, with Mobile and PC quality assets |
| Input | Input System 1.20.0 installed; `Assets/InputSystem_Actions.inputactions` includes Move, Look, Attack, Jump, Sprint, and a new keyboard R Reload action. |
| Navigation | AI Navigation 2.0.15 is installed. The M3 test scene has a `NavMeshSurface` on the graybox and a saved baked data asset; three melee enemies use `NavMeshAgent`. |
| Scene and code | `Assets/Scenes/FirstPersonTest.unity` is the enabled build scene. `SampleScene.unity` remains in the project. `Assets/Player` contains the M1 controller and M3 local health; `Assets/Combat` and `Assets/UI` contain the rifle, targets, effects, and HUDs; `Assets/Enemies` contains local melee AI, health, and feedback. `Assets/Encounters` contains the M4 local danger zone and wave spawner. There is no session code. |
| Online packages | `com.unity.netcode.gameobjects` and `com.unity.services.multiplayer` are not in `Packages/manifest.json`. `com.unity.pipeline` is installed for Unity CLI Editor control; it is not the gameplay networking stack. |
| Platform | macOS is the first development and test target; Windows is a later compatibility goal. |

## Proposed implementation boundaries

| System | Proposed responsibility | Validation needed |
| --- | --- | --- |
| Input and player | Implemented in M1: `PlayerInput` reads the Player action map. `FirstPersonMovement` uses a `CharacterController` and a short floor probe; `FirstPersonLook` controls local yaw and clamped camera pitch. Disable these local control components, the camera, and local weapon input on remote player instances when ownership is added in the online milestone. | Human play testing must still judge movement feel and slopes. The online milestone must verify ownership behavior. |
| Online group | Use Unity Multiplayer Services' unified Sessions API to create/join an online group, initially by join code. A host selects the mission. | Verify installed package versions, project/service configuration, authentication flow, two-player connection, and any usage or billing requirements before enabling a service. |
| Gameplay sync | Use Netcode for GameObjects with GameObjects/MonoBehaviours. Start with one player prefab and the state needed for the two-player slice. | Prove spawn, ownership, movement replication, disconnect cleanup, and latency behavior in two separate clients. |
| Authority | A player-hosted match is the initial proposal. The host decides enemy AI, spawns, danger-zone transitions, objective state, and final damage/health outcomes. Local input and immediate cosmetic feedback stay responsive on each client. | Decide movement authority and validation details through a real network test. Host departure, migration, and cheating limits remain open questions. |
| World and missions | Each mission identifies a map, objective, encounter placements, and extraction. Scene objects hold runtime triggers and spawn points. | Confirm scene loading and joining behavior before adding multiple missions. |
| Enemies | M3 `MeleeEnemy` has a local Idle/Chase/Attack/Dead state machine. Its `NavMeshAgent` uses the baked three-room surface; a raycast blocks attacks through walls. `EnemyHealth` owns HP and `EnemyFeedback` owns color cues. In the later online slice, only the host should advance AI and final enemy state; clients render the result. | Verify replicated movement, target selection among 2–4 players, and authority during the online combat milestone. A human should test the full route and combat feel. |
| Combat and player health | M2 `AutomaticRifle` owns input, ammo, fire/reload timing, and hitscan. `WeaponViewModel` reads its reload state and `Fired` event for local motion. The existing `IDamageable` seam now covers `ShootingTarget`, `EnemyHealth`, and `PlayerHealth`. M3 `PlayerHealth` disables local movement and shooting at zero HP; `PlayerHealthHud` displays health and a death prompt. The R-key scene reload is a test reset. For future online play, move final hit/health outcomes to the host and synchronize them while retaining responsive local effects. | Verify latency, ownership, host validation, teammate damage/revival policy, and multiplayer reset behavior later. |
| Encounters | M4 `DangerZone` owns local Inactive/Warning/Active/Cleared state and player-only trigger activation. `EnemyWaveSpawner` owns serialized waves and NavMesh spawn markers, tracks only its own `EnemyHealth.Died` events, and reports wave completion. `EncounterHud` observes zone events. An invalid wave configuration stops the local encounter and logs an error. A future host should own these same transitions and spawns for all players. | Test host-only activation, shared state, late joins, multiple player deaths, and cleanup after failure or extraction when networking is introduced. |
| Data | Use ScriptableObjects for stable definitions such as weapon, enemy, encounter, and mission settings when reuse is demonstrated. Keep live health, ammo, wave progress, and network ownership in runtime state. | Avoid creating abstract bases or large registries before a second concrete use case. |
| Audio and VFX | M2 uses one local AudioSource with self-generated gunfire, impact, and reload clips, plus flash, recoil, impact, and hit marker effects. M3 adds enemy damage/attack/death color feedback and a player damage overlay. An AudioMixer can group weapons, enemies, ambience, music, and UI later. | Human listening and visual-feel test; then test feedback on both clients when networking is added. |

The session API is proposed because Unity documents it as the unified route for creating and joining multiplayer games, and documents Netcode for GameObjects as the GameObject/MonoBehaviour gameplay stack. A client-hosted network is a practical first test for a small cooperative game; the choice is not yet validated for resilience or production cost. [Unity Sessions overview](https://docs.unity.com/en-us/mps-sdk/sessions), [create a session](https://docs.unity.com/en-us/mps-sdk/create-session), [Sessions with Netcode](https://docs.unity.com/en-us/mps-sdk/build-sessions-landing).

The M1 test map is separate from the starter scene. The Editor menu command `ArcadeShooter > Build First-Person Test Scene` replaces the original prefab and graybox and would remove later milestone content. After an intentional regeneration, run `ArcadeShooter > Add Weapon Shooting Test Content`, then `ArcadeShooter > Add Enemy Combat Test Content`, then `ArcadeShooter > Add Danger Zone Test Content`. The M3 command updates the existing player prefab and graybox through Editor APIs, creates the enemy prefab and saved NavMesh data, and places three enemies without removing targets. The M4 command adds two zones and an encounter HUD, and disables the original room-two and room-three enemy instances while retaining them in the scene.

## Suggested flow for the first online slice

1. Initialize Unity services and authenticate players as required by the installed SDK.
2. Host creates a session for two players and shares a join code; the second player joins.
3. Host selects the one available mission. Both clients load the same map and spawn their owned player objects.
4. Replicate only the state required for movement and presence. Prove disconnect and rejoin behavior before adding combat.
5. Add host-owned encounter, damage, objective, and extraction state in later milestones.

Use the Sessions API for the group and network lifecycle rather than manually composing lower-level service calls unless a demonstrated requirement demands it. Unity documents both connection-on-session-creation and delayed network start; choose the simpler flow after testing the lobby-to-map transition. [Session networking](https://docs.unity.com/en-us/mps-sdk/session-networking).

## Content and code organization

Keep code near its responsibility as it appears: `Player`, `Networking`, `Combat`, `Enemies`, `Encounters`, `Missions`, `UI`, and `Audio` are possible folders, not a scaffold to create now. Prefer composition and explicit interfaces at real seams. Put tuning data in assets; keep runtime and synchronized state in scene objects/components. Avoid a global manager for every feature.

## Verification strategy

- For every code milestone: Unity import and C# compilation, then a focused play test and any meaningful automated tests.
- For online milestones: two separate clients on macOS, one host and one joiner; check host and joiner views, late join or disconnect where applicable, and logs on both sides.
- Before calling the first prototype complete: exercise the entire mission from selection to extraction with two players and confirm that neither player ever loses manual movement or camera control.
- Record exact package versions and service configuration when introduced. Report tests that could not run.

References for later implementation: [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/index.html), [AI Navigation](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/index.html), [AudioSource](https://docs.unity3d.com/6000.0/Documentation/Manual/AudioSource-overview.html). Verify APIs against the versions installed at the implementation milestone.
