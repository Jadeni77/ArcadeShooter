# Technical architecture

This document records the current implementation and preserves the earlier planning baseline below it.

## Version control and delivery

`develop` is the staging branch and the base for feature and normal fix PRs. Approved releases move from `develop` to `main` by PR; urgent production hotfixes start from `main` and are brought back into `develop` by PR. `main` and `develop` receive no direct commits or pushes after the approved bootstrap. See [GIT_WORKFLOW.md](GIT_WORKFLOW.md) for branch names, reviews, checks, and the live branch protection. This workflow governs delivery; it does not change the gameplay architecture below.

## M7 weapon presentation

`AutomaticRifle` still owns local input, the camera-based shot ray, and local ammunition prediction. `NetworkRifle` remains the host authority for accepted shots, damage, ammunition, and reload completion. `WeaponAim` reads the right-mouse Aim action and changes only local FOV and look sensitivity. `WeaponViewModel` is attached to the replaceable first-person visual root; it reads rifle state and input for sway, bob, sprint pose, recoil, and the interim magazine motion. `WeaponAudio` observes reload starts and plays optional sounds; missing clip slots do not change game state. `WeaponRigAnimator` is an optional adapter for future imported rigged hands and weapon animation assets.

The local first-person model remains under its owner's camera, which is disabled on remote player objects. A separate non-networked rifle lives under the remote avatar body. `NetworkRifle` relays only existing shot events and the authoritative reload flag to `RemoteWeaponPresentation`; it does not synchronize per-frame cosmetic poses. New visual meshes or rigged hands can replace these presentation children without changing combat code. See [WEAPON_VISUALS_TESTING.md](WEAPON_VISUALS_TESTING.md) for asset gaps and manual validation.

## M5 online implementation

Unity 6000.6.5f1 uses Netcode for GameObjects 2.13.3, Multiplayer Services 2.4.0, and Multiplayer Play Mode 3.0.0. `MainMenu` is the first build scene; the host starts `FirstPersonTest` through NGO scene management. `MultiplayerSessionController` initializes Unity Services, signs in anonymously with a per-process profile, creates a private four-player Session backed by Relay, joins by code, locks the lobby before starting, and ends the session when the host leaves. It rejects connections after the match begins. See [MULTIPLAYER_TESTING.md](MULTIPLAYER_TESTING.md) for service setup and test steps.

The `FirstPersonPlayer` network prefab is instantiated for each connection. Its owner alone reads Input System actions, drives its `CharacterController`, camera, first-person weapon, and local HUD. `OwnerNetworkTransform` replicates owner movement and yaw. Other players see an interpolated placeholder capsule; their cameras, input, first-person weapon, and HUD remain disabled. Each player has independent networked health and ammunition. The host receives a shot direction, checks health, match state, ammunition, reload state, fire interval, direction length and yaw cone, then raycasts and applies damage. The client plays immediate local firing effects; the host confirms hits and broadcasts basic remote shot feedback. Owner-controlled movement is currently trusted, so speed/teleport prevention is outside this milestone.

The host alone runs `MeleeEnemy` AI, selects the nearest living in-match player, moves its NavMeshAgent, chooses attacks, and changes player/enemy/target health. Enemy movement, health, and death replicate to clients. The host also receives zone triggers, advances `DangerZone` state and wave counters, and spawns registered enemy network prefabs. A zone stays active when a player exits and fails only when every connected player is dead. Scene and prefab serialization is maintained by `ArcadeShooter > Configure Online Multiplayer Scenes`; do not rerun the older M1–M4 scene generators on the online scene without intentionally rebuilding subsequent content.

The host has gameplay authority, but the host player is also a player and has the same local input path. This is a player-hosted prototype: host migration, late join, reconnect, dedicated servers, and production anti-cheat are deferred. M6 testing exercised these paths with two players in the Editor (Multiplayer Play Mode over Relay); a recorded run with two standalone players on separate machines is still open.

## M6 mission implementation

The scene-owned `MissionController` holds the authoritative Waiting, In Progress, Objectives Completed, Extraction Active, Success, and Failure state, plus a synchronized completion mask for a small array of `MissionObjective` components. Each zone objective observes its existing `DangerZone.Changed` event; a zone that clears completes it, and a zone that stops for good fails the mission (reason Objective Failed) unless the squad is already down. The research sample sends an E interaction request that the host validates against the sending connection, living player, map, range, and unobstructed path. Completed bits reject duplicate credit. The scene uses existing three-room geometry, two danger zones, a new sample, and a marked extraction area.

`EnemyWaveSpawner` asks the pure `SpawnPlanner` for wave positions. It tries each marker first, then open NavMesh ground on rings up to 2.5 m around it (snapped to the NavMesh), keeps 2 m from every living player, and lets a marker with no usable ground give its enemy to space around another marker. `DangerZone` retries a wave for up to 8 seconds while players or bodies block the space, and reports a configuration error at once.

`ExtractionPoint` recomputes its roster from connected living player objects on the host, checks their world positions against its trigger volume, and synchronizes the countdown deadline. Leaving the area cancels the timer; a player who dies or disconnects simply leaves the roster, and the deadline stands if everyone remaining is inside. At least one living player is required. The mission ends once with a replicated end reason (Extracted, All Players Down, Objective Failed), and the final state stops combat and encounter progression. Host-scored enemy damage, kills, and accepted shots populate a replicated `MissionScoreTracker`; the host's `ShotCadence` accepts a client shot up to half a fire interval early to absorb network jitter without raising the fire rate. Only the sample collector receives an individual objective count; each completed objective contributes once to team score. `MissionScoringRules` holds the serialized constants, the success bonuses (time under par, team accuracy, extracted share of the squad), and rank thresholds, and returns a replicated score breakdown. The mission HUD reads replicated state and shows the final results, including a TEAM row and the end reason.

`MultiplayerSessionController` preserves the Session/Relay connection for host-controlled replay and lobby return. A single-mode network load despawns the previous avatars and wave enemies (all spawned to be destroyed with the scene) only once NGO accepts the load, and a fresh player network object is spawned for each client that finished loading, resetting health, ammo, input, and visual state. A refused load leaves the match intact and is reported on the results screen. The network menu suppresses its duplicate `NetworkManager` when reloaded while the original persistent session system remains active. On the results screen only the host can replay or return everyone to the lobby; **Leave Session** returns that player to the main menu, where rejoining needs the join code. Host departure still ends the session as in M5; a client the host removes is told the reason and leaves the session. Movement ownership and trust remain unchanged from M5. Two-client behavior was verified in the Editor with a Multiplayer Play Mode virtual player; two standalone players remain to be tested.

## Pre-M5 baseline (historical)

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

## Pre-M5 implementation plan (historical)

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

## Earlier suggested flow for the first online slice (historical)

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
