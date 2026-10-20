# Milestone 8B acceptance review

Reviewed 2026-10-20 against the original brief. `feature/combat-audio-vfx` remains draft PR #12. **Recommendation: keep it open.** A live two-player Relay run now proves local and remote gunfire source playback and both clients' Exploration → Combat music transition with the assigned clips. It did not complete the enemy, foley, zone clear, extraction, results, duplicate-effect, or listening checks required by the original acceptance criteria.

## Implemented and tested

- The user observed the red, host-confirmed bullet-hit effect in gameplay. An Editor preview distinguished concrete, metal, and enemy impacts; pooled objects returned after their lifetime. The impact prefab played the existing `Impact.wav` through a 3D AudioSource in direct-scene Play Mode, and reusing that prefab for an enemy death burst did not replay the bullet-impact sound. These impact observations remain local; the later two-player run did not inspect hit effects.
- The scene's music state logic crossed Exploration → Warning → Combat → Warning → Combat → Exploration with two independently changing zone states in a direct-scene Play Mode smoke test using temporary clips. A second warning cue played while Combat remained active. This verified the transition logic; the live run below separately verified the assigned exploration and combat loops on both peers.
- Unity 6000.6.5f1 compiled the latest source and all **40 EditMode tests passed** on 2026-10-20. New tests check clip and mixer references, all five exposed volume parameters, and `AudioMixSettings.SetLevel` changing Music volume to about −12.04 dB at 25%. These tests do not listen to the output.
- With the owner-approved local Dashboard linkage, the MainMenu created a Relay lobby, Player 2 joined (2/4), and both loaded `FirstPersonTest` with two player objects. Each client started `ExplorationLoop` and `FacilityAmbienceLoop` on its Music and Ambience groups. Entering Zone A through actual player movement activated its three-enemy wave; both clients then reported the `Combat` state and a playing `CombatLoop` on Music. The warning state/cue was too brief to capture in this run.
- Holding fire on Player 2 spent ammunition (30 → 19 in the first sample). Its SFX source was playing during fire, and the host's remote Player 2 SFX source was also playing during another sustained volley. This verifies a real local and remote firing path, but it does not establish audible quality, distance attenuation, exact one-shot count, or every visual effect. The local Dashboard IDs remain uncommitted.

## Wired to project-made placeholder assets

| Feature | Current evidence and limit |
| --- | --- |
| Muzzle flash, recoil, hit markers, surface impacts, enemy tint/death burst | Project-made URP visuals and host-confirmed events are wired. Hip/ADS clipping, kill-marker readability, and one-time behavior on both peers still need the manual pass. |
| Gunfire, basic reload, impact | Existing locally synthesized WAVs are assigned. Impact playback was smoke-tested; both local and remote gunfire sources played in a live two-player session. Reload event timing and audible quality still need a listening pass. |
| Weapon handling, player foley, enemy behavior | New locally synthesized clips are assigned on the player and enemy prefabs for empty click, magazine out/in, reload completion, three footsteps, jump, landing, player damage, and idle/alert/attack/damage/death. The EditMode test verifies references; cadence, 3D attenuation, event timing, and actual audible playback remain unverified in gameplay. |
| Zone, mission, music, ambience | New locally synthesized warning/clear, extraction/countdown, success/failure cues and exploration/warning/combat/ambience loops are assigned in `FirstPersonTest`. Both peers played exploration, ambience, and then combat sources on the intended groups during a replicated Zone A encounter. Warning, zone clear, results cues, and the return to Exploration were not captured with these clips. |
| Mixer | `CombatAudio.mixer` has Master, Music, SFX, Ambience, and UI buses with exposed volumes. Sources and controller references are assigned. The volume setter passed its EditMode behavior test; a listening test of each bus remains. |

The 26 new WAVs were synthesized in this repository without downloaded samples or third-party assets. They are deliberately simple sounds; acceptance still depends on real playback and gameplay timing.

## Implemented in code but not functioning or proven in gameplay

- Boss music is only an extension point: no boss encounter or clip exists, as intended for a future milestone.
- `WeaponAudio` provides local magazine stage sounds and remote reload-start/completion hooks. Remote magazine out/in presentation is not sent separately; the original acceptance asks for relevant remote combat effects, so confirm whether the basic remote reload sound is sufficient during the two-player pass.
- `EnemyAudio` and `PlayerFoley` have clip references but their actual event playback, spatial attenuation, cadence, and death cutoffs remain unverified. `MusicManager` has proven exploration/ambience/combat source playback on both peers, while warning/clear/results cues and the return crossfade remain unverified with the assigned clips.
- The two-player run ended before the full enemy, second zone, extraction, and results route. Manual reload was not established by Unity CLI key simulation; an earlier project note also records unreliable R-key simulation. Do not treat the lack of an observed reload from this simulator as proof of a game bug or as a passing test.
- Both Editors compiled during the live run. The host Console had zero current errors. The Player 2 clone displayed two `TopView` assertions from Unity Multiplayer Play Mode itself; its historical log also contains older setup and Relay shutdown errors. These are recorded separately from game code, but the original zero-error acceptance check still needs a clean repeat.
- No wave Profiler sample or measured 60 FPS result has been recorded.

## Missing assets and future polish

No rigged first-person hands, articulated enemy attack/damage/death animations, surface-specific particle textures, professionally designed weapon/enemy/footstep sounds, or composed music are present. Those are **replacement and polish** tasks in [ROADMAP.md](ROADMAP.md), not a reason to call silent or broken M8B playback complete. No external asset was downloaded or purchased for this review; [ASSET_LICENSES.md](ASSET_LICENSES.md) records the project-made placeholders and optional candidates awaiting approval.

Before merge, complete the remaining [two-player checklist](COMBAT_AUDIO_VFX_TESTING.md) and record each peer's audible/visible observations, fix any broken playback or transition it reveals, and repeat the Console check. Performance can be reported separately with an actual Profiler measurement; no 60 FPS claim is made here.
