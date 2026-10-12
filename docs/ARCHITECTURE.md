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
| Input | Input System 1.20.0 installed; `Assets/InputSystem_Actions.inputactions` exists, with Move, Look, Attack, Jump, Sprint, and other actions. Reload is not currently an action. |
| Navigation | AI Navigation 2.0.15 installed; no gameplay navigation system exists yet. |
| Scene and code | `Assets/Scenes/SampleScene.unity` is the only enabled build scene. The C# files under `Assets/TutorialInfo` are template/readme code; there is no game controller, combat, enemy, or session code. |
| Online packages | `com.unity.netcode.gameobjects` and `com.unity.services.multiplayer` are not in `Packages/manifest.json`. `com.unity.pipeline` is installed for Unity CLI Editor control; it is not the gameplay networking stack. |
| Platform | macOS is the first development and test target; Windows is a later compatibility goal. |

## Proposed implementation boundaries

| System | Proposed responsibility | Validation needed |
| --- | --- | --- |
| Input and player | Read actions from the existing Input System asset. A local first-person controller owns mouse look, WASD movement, sprint, and jump. A player camera follows only that player's input. | Test mouse feel, cursor capture, frame-rate independence, slopes/collisions, and the existing action bindings on macOS. |
| Online group | Use Unity Multiplayer Services' unified Sessions API to create/join an online group, initially by join code. A host selects the mission. | Verify installed package versions, project/service configuration, authentication flow, two-player connection, and any usage or billing requirements before enabling a service. |
| Gameplay sync | Use Netcode for GameObjects with GameObjects/MonoBehaviours. Start with one player prefab and the state needed for the two-player slice. | Prove spawn, ownership, movement replication, disconnect cleanup, and latency behavior in two separate clients. |
| Authority | A player-hosted match is the initial proposal. The host decides enemy AI, spawns, danger-zone transitions, objective state, and final damage/health outcomes. Local input and immediate cosmetic feedback stay responsive on each client. | Decide movement authority and validation details through a real network test. Host departure, migration, and cheating limits remain open questions. |
| World and missions | Each mission identifies a map, objective, encounter placements, and extraction. Scene objects hold runtime triggers and spawn points. | Confirm scene loading and joining behavior before adding multiple missions. |
| Enemies | AI Navigation supplies NavMesh paths. Only the host advances enemy decisions and authoritative state; clients render the result. | Verify NavMesh baking, replicated movement, combat target choice, and navigation around the greybox map. |
| Combat | One simple gun first. Separate input, weapon rules, damage resolution, and cosmetic effects. The host confirms hits/damage and shares the result. | Choose hitscan/projectile behavior, fire cadence, ammo rules, and latency handling by testing the first gun. |
| Encounters | A reusable danger-zone trigger starts a small encounter state machine: inactive, warning, active, complete. It can request enemy waves and report completion to the mission objective. | Test host-only activation, repeat/one-shot policy, late joins, and cleanup after failure or extraction. |
| Data | Use ScriptableObjects for stable definitions such as weapon, enemy, encounter, and mission settings when reuse is demonstrated. Keep live health, ammo, wave progress, and network ownership in runtime state. | Avoid creating abstract bases or large registries before a second concrete use case. |
| Audio and VFX | AudioSources play local, spatial sound; an AudioMixer can group weapons, enemies, ambience, music, and UI. Local cosmetic effects respond immediately; confirmed impacts follow authoritative results. | Test audible feedback on host and client, mix levels, transitions into/out of danger, and asset licenses. |

The session API is proposed because Unity documents it as the unified route for creating and joining multiplayer games, and documents Netcode for GameObjects as the GameObject/MonoBehaviour gameplay stack. A client-hosted network is a practical first test for a small cooperative game; the choice is not yet validated for resilience or production cost. [Unity Sessions overview](https://docs.unity.com/en-us/mps-sdk/sessions), [create a session](https://docs.unity.com/en-us/mps-sdk/create-session), [Sessions with Netcode](https://docs.unity.com/en-us/mps-sdk/build-sessions-landing).

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
