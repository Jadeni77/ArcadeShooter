# Milestone 8B acceptance review

Reviewed 2026-10-20 against the original brief. `feature/combat-audio-vfx` remains draft PR #12. **Recommendation: keep it open for the two-player combat and listening pass.** The functional audio slots now have project-made clips and the mixer is wired, but asset assignment and EditMode tests alone cannot prove playback, synchronization, or absence of duplicate effects in a match.

## Implemented and tested

- The user observed the red, host-confirmed bullet-hit effect in gameplay. An Editor preview distinguished concrete, metal, and enemy impacts; pooled objects returned after their lifetime. The impact prefab played the existing `Impact.wav` through a 3D AudioSource in direct-scene Play Mode, and reusing that prefab for an enemy death burst did not replay the bullet-impact sound. This is local evidence, not a two-player firing pass.
- The scene's music state logic crossed Exploration → Warning → Combat → Warning → Combat → Exploration with two independently changing zone states in a direct-scene Play Mode smoke test using temporary clips. A second warning cue played while Combat remained active. This verifies the transition code, not the newly assigned loop playback in an online match.
- Unity 6000.6.5f1 compiled the latest source and all **40 EditMode tests passed** on 2026-10-20. New tests check clip and mixer references, all five exposed volume parameters, and `AudioMixSettings.SetLevel` changing Music volume to about −12.04 dB at 25%. These tests do not listen to the output.
- With the owner-approved local Dashboard linkage, the MainMenu created a Relay lobby and displayed a join code with 1/4 players on 2026-10-20. This confirms service startup and hosting, not two-player M8B acceptance. The local Dashboard IDs remain uncommitted.

## Wired to project-made placeholder assets

| Feature | Current evidence and limit |
| --- | --- |
| Muzzle flash, recoil, hit markers, surface impacts, enemy tint/death burst | Project-made URP visuals and host-confirmed events are wired. Hip/ADS clipping, kill-marker readability, and one-time behavior on both peers still need the manual pass. |
| Gunfire, basic reload, impact | Existing locally synthesized WAVs are assigned. Impact playback was smoke-tested; local/remote gunfire and reload timing still need a two-player listening pass. |
| Weapon handling, player foley, enemy behavior | New locally synthesized clips are assigned on the player and enemy prefabs for empty click, magazine out/in, reload completion, three footsteps, jump, landing, player damage, and idle/alert/attack/damage/death. The EditMode test verifies references; cadence, 3D attenuation, event timing, and actual audible playback remain unverified in gameplay. |
| Zone, mission, music, ambience | New locally synthesized warning/clear, extraction/countdown, success/failure cues and exploration/warning/combat/ambience loops are assigned in `FirstPersonTest`. `MusicManager` uses local 2D sources and reads replicated states; the new files' playback and crossfades have not been heard in a live match. |
| Mixer | `CombatAudio.mixer` has Master, Music, SFX, Ambience, and UI buses with exposed volumes. Sources and controller references are assigned. The volume setter passed its EditMode behavior test; a listening test of each bus remains. |

The 26 new WAVs were synthesized in this repository without downloaded samples or third-party assets. They are deliberately simple sounds; acceptance still depends on real playback and gameplay timing.

## Implemented in code but not functioning or proven in gameplay

- Boss music is only an extension point: no boss encounter or clip exists, as intended for a future milestone.
- `WeaponAudio` provides local magazine stage sounds and remote reload-start/completion hooks. Remote magazine out/in presentation is not sent separately; the original acceptance asks for relevant remote combat effects, so confirm whether the basic remote reload sound is sufficient during the two-player pass.
- `EnemyAudio`, `PlayerFoley`, and `MusicManager` have clip references now, but a live host/joiner listening pass has not confirmed that every event is audible, correctly spatialized, free of duplicates, and cut off at the right time. Treat those items as **unverified**, not complete.
- The required two-player firing, enemy, zone, extraction, and results route was not repeated after assigning the real placeholder clips. The 2026-10-19 two-player run reached the map before these assignments. An Editor sign-in button incorrectly requested installation of Unity Hub even though Hub 3.22.2 was installed; anonymous Authentication still allowed the live 1/4 Relay host test. The second-client run remains open.
- No wave Profiler sample or measured 60 FPS result has been recorded.

## Missing assets and future polish

No rigged first-person hands, articulated enemy attack/damage/death animations, surface-specific particle textures, professionally designed weapon/enemy/footstep sounds, or composed music are present. Those are **replacement and polish** tasks in [ROADMAP.md](ROADMAP.md), not a reason to call silent or broken M8B playback complete. No external asset was downloaded or purchased for this review; [ASSET_LICENSES.md](ASSET_LICENSES.md) records the project-made placeholders and optional candidates awaiting approval.

Before merge, complete the [two-player checklist](COMBAT_AUDIO_VFX_TESTING.md) and record each peer's audible/visible observations, fix any broken playback or transition it reveals, and check the Console on both clients. Performance can be reported separately with an actual Profiler measurement; no 60 FPS claim is made here.
