# Project status

Last reviewed: 2026-10-11 (America/New_York). Inspect `git status` and the project before each task.

## Current state

- M0 documentation was merged into `develop` by [PR #1](https://github.com/Jadeni77/ArcadeShooter/pull/1). M1 first-person exploration was squash-merged into `develop` by [PR #2](https://github.com/Jadeni77/ArcadeShooter/pull/2). `main` remains at the historical initial scaffold commit.
- M2 first-person weapons and shooting is implemented on `feature/weapon-shooting` for review through a PR to `develop`. The project owner selected this local combat slice before the previously proposed online foundation; the roadmap now reflects that order.
- `Assets/Scenes/FirstPersonTest.unity` remains the enabled three-room test scene. It now contains five resettable targets. `Assets/Prefabs/FirstPersonPlayer.prefab` retains M1 movement and look, with a separate automatic rifle, placeholder view model, and weapon HUD.
- The rifle uses the existing Input System `Attack` action and a new `Reload` action bound to R. It also reloads automatically on the last shot when reserve ammunition remains. Inspector fields control damage, rounds per second, magazine and reserve ammunition, reload duration, range, recoil, reload model drop and tilt, flash duration, and asset references. Hits are local camera raycasts against `IDamageable` targets; there is no network synchronization yet.
- Gunfire, impact, and reload clips are self-generated temporary audio. The target and rifle meshes/materials are Unity primitives. The ammo HUD uses imported Unity TextMesh Pro essentials; the bundled Liberation Sans font and its OFL terms are recorded in `ASSET_LICENSES.md`.

## M2 verification

- Unity 6000.6.5f1 recompiled the refined scripts with zero errors and warnings. A final Play test after clearing the console reported zero errors and warnings.
- The Game view visibly showed the placeholder rifle, central crosshair, five targets, and ammunition readout. A shot against the first target reduced health from 60 to 40 and magazine ammunition from 30 to 29. Two more hits depleted its health, disabled its renderer and collider, and it later reset to 60 health with both restored.
- Two immediate fire attempts produced one shot, confirming the configured fire gate. Held automatic fire exhausted a 30-round magazine. Firing the final round from 1/90 started automatic reload immediately, showed `RELOADING  0 / 90`, and completed at 30/60 after 1.6 seconds.
- Repeated reload attempts did not restart an active reload, and firing was blocked during it. The existing R-key path reloaded a partial magazine from 20/15 to 30/5. With zero reserve, the final shot left the magazine at zero, no reload began, and a further shot was blocked.
- At the midpoint of a controlled reload pose test, the model was 0.22 units lower and about 17° tilted; at the end of the configured duration, its position and rotation matched the original pose. A 5/7 partial-reserve test completed at 12/0, with the HUD updated to `12 / 0`.
- On a target hit, the muzzle flash, hit marker, impact effect, and AudioSource were active. A W-key movement check moved the player from x = -10.00 to x = -7.86; M1 movement remained available.
- The gunfire and reload clips were assigned and played through the AudioSource. Audible quality, animation feel, and all keyboard/mouse combinations still need a human Game view test.

## Manual play test

Open `Assets/Scenes/FirstPersonTest.unity` in Unity and press Play. Click the Game view to capture the cursor. Use WASD, mouse look, Shift, and Space as before. Aim the center crosshair at a teal target and click; hold left click to fire automatically. Watch the ammo counter, target flash, hit confirmation, impact, and muzzle flash; listen for gunfire and impacts. Empty the magazine and verify reload starts automatically, shows `RELOADING`, lowers and tilts the weapon, blocks shooting, then returns it to rest and refills from reserve. Fire a few rounds and press R to check manual reload uses the same motion. Destroy a target with three hits, wait about four seconds for it to reappear, then shoot it again. Press Escape to release the cursor.

## Existing Git change to preserve

`ProjectSettings/ProjectSettings.asset` has an older uncommitted cloud project, organization, and project-name edit. Its cloud association still needs owner confirmation before committing. Keep it out of the M2 PR and do not discard it.

## Next milestone

Review and merge M2 only with the project owner's approval and required human review. Next, validate package compatibility, service setup, ownership, and any costs before the two-player online foundation. No online packages or paid services were introduced in M2.
