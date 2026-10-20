# Milestone 8B acceptance review

Reviewed 2026-10-19 against the original M8B brief. `feature/combat-audio-vfx` is open as draft PR #12. **Recommendation: do not merge yet.** The visuals use acceptable project-made placeholders, but required gameplay audio is still silent and the two-player combat effects pass is outstanding.

## Verified functional pieces (narrow scope)

No original M8B audio acceptance item has been fully verified in two-player gameplay. The checks below establish specific working pieces and define their limits.

- The pooled impact prefab now plays the existing project-made `Impact.wav` from a 3D AudioSource at the confirmed hit point. A direct-scene Play Mode spawn reported `isPlaying=True` and `spatialBlend=1`; reuse for an enemy death burst reported `isPlaying=False`, avoiding a second bullet-impact sound. This verifies the prefab playback path, not an actual network shot heard by two players.
- A direct-scene Play Mode test with temporary in-memory clips exercised Exploration → Warning → Combat → Warning → Combat → Exploration across two zone states. A second zone's warning cue reported playing while Combat remained active. These clips were not saved or imported; the real scene's music fields remain empty.
- Unity 6000.6.5f1 compiled the current code, all 38 EditMode tests passed, and the Editor console reported zero compilation errors and warnings after this review. The user observed the red bullet-hit effect in gameplay. The separate concrete/metal/enemy preview and pooled cleanup were verified previously; an actual two-client firing pass remains outstanding.

## Working with project-made placeholders

| Feature | Evidence and limit |
| --- | --- |
| Local muzzle flash, recoil, hit marker, and surface impacts | Existing firing presentation and the M8B geometry/materials are wired. The user saw the red bullet-hit effect; a direct Editor preview distinguished concrete, metal, and enemy colors. Hip/ADS clipping, confirmed-kill marker, and remote visuals still need the checklist pass. |
| Enemy damage tint and death burst | Replicated-health handlers and the project-made fragment effect are wired; the death burst was previewed. Actual damage/death on both peers and one-time presentation are not yet observed in an M8B two-player run. |
| Gunfire, basic reload, and impact sound assets | Existing locally synthesized `Gunfire.wav`, `Reload.wav`, and `Impact.wav` are assigned. Local/remote gunfire and reload are event-wired; impact playback was smoke-tested as above. Shot-to-sound timing, distance falloff, and duplicate prevention need listening on both peers. |
| Timed weapon motion and remote rifle | Existing M7 project-made presentation remains; M8B adds remote muzzle timing. Rigged hands and articulated enemy animations do not exist. |

## Code support that is silent or not yet proven in gameplay

| Feature | Current state |
| --- | --- |
| Enemy idle, alert, attack, damage, and death | `EnemyAudio` follows replicated health and host cosmetic events, with 3D rolloff and a wave budget. Every clip array on `MeleeEnemy.prefab` is empty, so none plays. Death audio duration relative to enemy despawn needs checking after a clip is assigned. |
| Walking/sprinting steps, jump, landing, and player damage | `PlayerFoley` follows owner/remote movement and health, but all its clip slots are empty. Cadence and distance behavior have not been heard in gameplay. |
| Empty trigger, magazine out/in, and reload completion | `WeaponAudio` has timing hooks, including a completion hook that only fires after a refill, but these clips are unassigned. The existing basic reload-start WAV does not cover the missing events. |
| Zone warning/clear, extraction, countdown, success/failure | `DangerZone` and `MusicManager` detect the corresponding replicated states, but all cue references are empty. A prior cue transition bug was fixed; the temporary-clip test above does not establish real audible integration. |
| Exploration/warning/combat music and ambience | `MusicManager` selects and crossfades local 2D sources, prioritizing active combat across zones. Every real music/ambience clip field is empty; gameplay is silent. Boss music is a reserved extension point only. |
| Mixer and volume control | No AudioMixer asset or Master/Music/SFX/Ambience/UI groups exist. `AudioMixSettings.SetLevel` stores values but returns before changing output while its mixer reference is null. Required routing and user volume control are incomplete. |
| Multiplayer duplication and regression | `RemoteShotRpc` excludes the owner; `ImpactRpc` broadcasts one host-confirmed impact; enemy events follow authority. Only lobby-to-map with two players was previously observed. Shooting, enemy effects, listening, and mission results were not retested on two peers after M8B. |

## Missing assets and required checks

- No third-party M8B assets were downloaded or imported. Required audible placeholders or approved clips are missing for enemy behavior, player foley, weapon handling, zone/extraction/results cues, ambience, and exploration/warning/combat loops. New assets require owner approval and a license record in `ASSET_LICENSES.md`.
- The AudioMixer asset/groups, assigned routing, and a real volume adjustment test are missing. These are functional M8B requirements, not future cosmetic polish.
- A host and joiner must see and hear the same relevant host-confirmed combat events; check no duplicate effects, local/remote spatialization, enemy death audio tail, and zone/music transitions. Unity Services currently reports this checkout unlinked, so online validation requires the owner-approved project link. No 60 FPS claim is made without a measured Profiler run.
- Rigged first-person arms, detailed enemy animations, richer VFX particles/textures, and professional sound/music replacements are future polish. See `ROADMAP.md`; they do not turn silent M8B acceptance items into completed features.
