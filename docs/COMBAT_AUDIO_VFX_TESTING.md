# Milestone 8B combat audio and VFX checks

Open `Assets/Scenes/MainMenu.unity` and host a mission from the lobby. Direct Play of `FirstPersonTest` does not spawn a player. For two-player validation, join with Multiplayer Play Mode or a second macOS client. A clean checkout needs the intended Unity Dashboard project link for Relay; see [MULTIPLAYER_TESTING.md](MULTIPLAYER_TESTING.md).

## Phase A — combat visuals

1. Fire from hip and while aiming down sights. Verify that each accepted local shot shows the existing brief barrel flash and recoil. Check the flash stays at the barrel and does not cover the sight picture.
2. Shoot gray walls/floors, metal surfaces, and enemies. Verify the bright impact burst follows the surface normal, with gray dust, amber sparks, and red enemy particles respectively. Verify impacts disappear in under a second, including during sustained fire.
3. Hit an enemy without killing it, then land a killing shot. The normal hit marker should use its original color; a confirmed kill should briefly use amber. Fire into cover and verify no enemy hit marker appears. Check that client markers follow host-confirmed hits, including under latency.
4. Damage and kill an enemy. Verify its damage tint and fragment-only death burst occur once on host and joiner without covering the sight picture. Check that damage, AI, death, and score remain unchanged.

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

Gunfire, impact, and basic reload clips existed before 8B. The 26 new project-made placeholders are assigned to player/enemy prefabs and the game scene. Listen to each event above before accepting it; the EditMode reference checks do not prove audible gameplay. Boss music remains unassigned because no boss encounter exists.

## Automated verification on 2026-10-18

- Unity 6000.6.5f1 compiled the final 8B scripts with no errors; the final Editor console had zero errors and warnings.
- All 38 existing EditMode tests passed after the final scene and prefab setup.
- Direct Play of `FirstPersonTest` confirmed the music controller entered Exploration without an assigned clip and that three manually spawned impact types returned to their pool after 0.25 seconds. This scene path has no spawned player or active gameplay AudioListener, so it does not verify the multiplayer or audible checks above.
- No two-player 8B session, listening pass, or frame-time measurement has been recorded yet.

## Follow-up visual check on 2026-10-19

- The project-made muzzle flash and impact materials now use URP Unlit so they remain bright under facility lighting. The impact prefab is larger and lasts 0.38 seconds; enemy death uses eight outward fragments over roughly 0.68 seconds instead of a solid red disc.
- A frozen Game-view preview visibly distinguished concrete, metal, and enemy/death bursts. Unity recompiled the revised scripts without errors, and all 38 EditMode tests passed. The preview does not prove shot-triggered effects on both peers; a fresh two-player firing pass is still needed.
- The earlier session reached the map with two players. The owner later approved local linkage to the existing Dashboard project; its IDs remain uncommitted.

## Acceptance review and functional smoke checks on 2026-10-19

- See [COMBAT_AUDIO_VFX_REVIEW.md](COMBAT_AUDIO_VFX_REVIEW.md) for the then-current status and merge recommendation. At this point the scene/prefabs still had empty enemy, foley, zone cue, music, and ambience fields; code hooks were not audible completion.
- The existing `Impact.wav` now plays from the pooled 3D impact prefab, including the shared host-confirmed impact path. A direct-scene Play Mode spawn reported an active source with spatial blend 1; a reused enemy death burst did not play the bullet-impact sound. An actual shot heard by both peers is still unverified.
- Using only temporary in-memory test clips and local zone-state changes, the manager entered Warning, Combat, Warning, Combat, and Exploration in the expected order. A second zone's warning cue played during Combat, and Combat stayed active until the last active zone cleared. This tests state/source logic without claiming audible gameplay music or network delivery.
- Unity 6000.6.5f1 compiled, all 38 EditMode tests passed, and the Editor console reported zero errors/warnings after the changes. A real two-player firing/listening run and Profiler measurement are still pending.

## Placeholder wiring and host check on 2026-10-20

- Unity imported 26 locally synthesized WAV placeholders. `ArcadeShooter > Assign M8B Placeholder Audio` assigned player, enemy, zone/mission, loop, and ambience references plus the `CombatAudio.mixer` routing. The mixer has Master, Music, SFX, Ambience, and UI buses. No third-party assets were downloaded.
- All 40 EditMode tests passed after the assignments. The new tests check serialized references, buses and exposed volume parameters, and a Music-volume change to about −12.04 dB at 25%. They do not establish audible playback.
- With the owner-approved local Unity project link, the MainMenu created a Relay lobby with a join code and 1/4 players. The Editor sign-in button separately requested installing Unity Hub even though Hub 3.22.2 was installed; anonymous Authentication for the game still succeeded. A new host/joiner combat and listening pass was not completed after the assignments. Do not mark phases B–D complete from the host lobby or static tests.
