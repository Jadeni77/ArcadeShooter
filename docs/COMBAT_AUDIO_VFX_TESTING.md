# Milestone 8B combat audio and VFX checks

Open `Assets/Scenes/MainMenu.unity` and host a mission from the lobby. Direct Play of `FirstPersonTest` does not spawn a player. For two-player validation, join with Multiplayer Play Mode or a second macOS client. A clean checkout needs the intended Unity Dashboard project link for Relay; see [MULTIPLAYER_TESTING.md](MULTIPLAYER_TESTING.md).

## Phase A — combat visuals

1. Fire from hip and while aiming down sights. Verify that each accepted local shot shows the existing brief barrel flash and recoil. Check the flash stays at the barrel and does not cover the sight picture.
2. Shoot gray walls/floors, metal surfaces, and enemies. Verify the small impact burst follows the surface normal, with gray dust, amber sparks, and red enemy particles respectively. Verify impacts disappear quickly, including during sustained fire.
3. Hit an enemy without killing it, then land a killing shot. The normal hit marker should use its original color; a confirmed kill should briefly use amber. Fire into cover and verify no enemy hit marker appears. Check that client markers follow host-confirmed hits, including under latency.
4. Damage and kill an enemy. Verify its damage tint and death tint/burst occur once on host and joiner. Check that damage, AI, death, and score remain unchanged.

## Phase B — combat and player sound

1. Listen to local automatic gunfire at full rate, then listen to the other player's shots from near and far. Remote fire should come from that player's position and become quieter with distance. Neither player should hear duplicate shots.
2. Reload manually and after an empty magazine. Listen once for reload start, magazine removal/insertion, and completion where clips are assigned. Empty-trigger clicks should be rate limited. Verify magazine/refill timing and ammunition are unchanged.
3. Trigger enemy detection, attacks, damage, and death at several distances. Audio should come from the enemy, stay quiet at long range, and not stack excessively during waves. No attack or idle sound should play after death.
4. Walk, sprint, stop, jump, and land. Steps should accelerate during sprint, stop when stationary or airborne, and stay local/spatial as appropriate. Take damage and confirm the owner hears the feedback once.

## Phase C — environment and music

1. Explore before entering a zone and listen for the exploration loop and restrained facility ambience where clips are assigned.
2. Enter Zone A, then Zone B. Confirm each warning cue, the combat transition during active waves, and an area-cleared cue. Clear one zone while the other is active and verify combat music continues; after both clear, verify it crossfades to exploration without restarting unnecessarily.
3. Complete objectives and start extraction. Verify activation and countdown cues, then finish and listen for the success cue. Replay, let all players die or force a required objective failure, and verify the failure cue and clean results transition.
4. Return to the lobby, replay, and leave the session. Check for duplicate loops, sounds continuing after scene unload, and extra active AudioListeners.

## Phase D — multiplayer and performance

Run the entire route with a host and joiner on macOS. Record each player's observation for flash, impacts, markers, enemy reactions, audio, zone music, extraction, and results. Confirm neither client independently applies damage or kills. Check the Console on both peers for errors, and use Unity Profiler to record frame time and audio voices during a wave. Do not claim 60 FPS without a measured run at a stated resolution and quality level.

## Asset-dependent checks

Only gunfire, impact, and basic reload clips existed before 8B. Any unassigned clip field is intentionally silent. Record the exact imported clip assignments and repeat the matching checks above after asset approval and import; code support alone is not audible integration.

## Automated verification on 2026-10-18

- Unity 6000.6.5f1 compiled the final 8B scripts with no errors; the final Editor console had zero errors and warnings.
- All 38 existing EditMode tests passed after the final scene and prefab setup.
- Direct Play of `FirstPersonTest` confirmed the music controller entered Exploration without an assigned clip and that three manually spawned impact types returned to their pool after 0.25 seconds. This scene path has no spawned player or active gameplay AudioListener, so it does not verify the multiplayer or audible checks above.
- No two-player 8B session, listening pass, or frame-time measurement has been recorded yet.
