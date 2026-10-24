# M9 progression and replayability verification

## Rules

`ProgressionRules` on the persistent `MultiplayerSessionController` holds the tuneable values. Level 1 starts at 0 XP. The total XP threshold for level `L` is `400 × (L−1) + 100 × (L−1) × (L−2)` with the default 400 first step and 200 added per subsequent step. One award can pass several thresholds.

The host uses each player's final personal combat score (kills × 100 + confirmed hits × 10). A living player at successful extraction earns `300 + floor(personal score / 20)` XP. Hard adds `floor(25% × that reward)`. A connected player who did not extract receives `floor(25% × floor(personal score / 20))` XP, including on mission failure; Hard adds no bonus to that amount. Leaving before results earns nothing. Objectives remain team score, not personal XP. The teal rifle finish unlocks at level 2; the Pathfinder title at level 3. Cosmetic selection changes no combat stats.

The host creates a new random result ID for each mission start and sends one calculated result to each connected owner. Each application saves only its own result. Claimed IDs in the local profile reject duplicate deliveries, including after restart. A replay loads a new scene and gets a fresh result ID. The host alone changes mission score, difficulty, health, and encounter variation. Local saves and cosmetic claims are not tamper-proof, and host authority cannot protect against a malicious host. There is no cloud sync or account identity. Two computers maintain separate profiles even when they use the same display name.

The profile is a version 1 JSON file at `Application.persistentDataPath/player-profile-v1.json`. On macOS the exact directory is set by Unity for this app, typically under `~/Library/Application Support/DefaultCompany/ArcadeShooter`. A missing file starts at level 1. A damaged or unsupported file remains on disk and disables writes for that run; the warning appears in the lobby. Successful saves atomically replace the file and keep a `.bak` copy of the previous version.

Normal retains 60-HP enemies and the existing Zone B 2/4 waves. Hard raises enemy maximum health and starting health by 25%. The host stores selected difficulty in the Session property before scene load, then copies it to the mission NetworkVariable for the full run. For Zone B, the host randomly chooses existing 2/4 waves or alternate 3/3 waves and marker order. Both use the same six baked NavMesh markers, total six enemies, and the existing spawn planner's reachable, clear-ground checks. The chosen variant is replicated for the match. Replays can choose a different variant; a random choice can also repeat.

## Automated verification

Unity 6000.6.5f1 batch compilation reported no new C# errors. `ProgressionTests` passed 5/5: level thresholds, a multi-level award, Normal/Hard/failure XP, save/reload, independent directories, duplicate IDs, locked selection, preserving an unknown save version, and both Zone B layouts in the scene. The full EditMode run passed 44/45. The sole failing test was the existing M8B mixer parameter readback test: it got 0 dB instead of −12.04 dB in batch mode, both with and without `-nographics`. Live Editor mixer behavior on this branch was not tested.

## Manual two-player macOS checklist (not yet performed)

1. Use two separate macOS app instances or computers connected to the same Unity Dashboard project. Host from one, join by code from the other. Confirm both lobby screens show **Normal**, each machine's own level/XP and locked items, and that the joiner cannot change difficulty. Have the host switch to **Hard** and confirm both lobbies update.
2. Start the mission. Confirm both results screens retain Hard. Inspect an enemy at spawn and a spawned wave enemy: each should have 75 rather than 60 starting HP, and both peers should see the same HP. Confirm movement, shooting, AI, danger-zone objectives, sample, and extraction still work.
3. Clear Zone B and record whether its waves are 2/4 or 3/3. Verify both peers see the same composition and enemies remain on reachable ground. Complete extraction with both living. Compare each XP award with its personal score and the documented formula, including Hard bonus. Check levels, unlock messages, and different local totals when player scores differ.
4. Replay repeatedly until Zone B's other variant appears. Verify six total enemies, objective completion, fresh score, fresh XP, and no carryover health/ammunition. Repeatedly viewing results must not award XP again. Use a debug duplicate-result delivery if available to confirm the claimed ID gate.
5. Return to lobby, select the teal finish and Pathfinder title once unlocked; confirm locked items were not selectable earlier. Start another run and verify the teal rifle on its owner and the remote rifle visible to the other peer. Close both apps and relaunch: level, XP, unlocks, and selections must persist independently.
6. Repeat with a failed mission, a player dying before a successful extraction, and a player leaving before results. Confirm reduced personal XP for connected non-extractors and no award after leaving. Check both logs for exceptions and verify profile files are separate on the two computers.

The existing standalone macOS build and full two-player route have not been run for this branch until results are recorded in `STATUS.md`.
