# Project status

Last reviewed: 2026-10-12 (America/New_York). Inspect `git status` and the project before each task.

## Current state

- M0 documentation was merged into `develop` by [PR #1](https://github.com/Jadeni77/ArcadeShooter/pull/1). M1 first-person exploration was squash-merged into `develop` by [PR #2](https://github.com/Jadeni77/ArcadeShooter/pull/2). `main` remains at the historical initial scaffold commit.
- M2 first-person weapons and shooting was squash-merged into `develop` by [PR #3](https://github.com/Jadeni77/ArcadeShooter/pull/3). M3 enemy AI and combat is implemented on `feature/enemy-ai-combat` for review by PR to `develop`. `main` remains at the historical initial scaffold commit.
- `Assets/Scenes/FirstPersonTest.unity` remains the enabled three-room test scene. It retains five resettable targets and now has a baked NavMesh plus three placeholder melee enemies. `Assets/Prefabs/FirstPersonPlayer.prefab` retains M1 movement/look and the M2 rifle, view model, and ammunition HUD; it now also has player health and a health/death HUD.
- The rifle uses the existing Input System `Attack` action and a new `Reload` action bound to R. It also reloads automatically on the last shot when reserve ammunition remains. `AutomaticRifle` inspector fields control damage, rounds per second, magazine and reserve ammunition, reload duration, range, flash duration, and asset references. Reloads finish on the rifle's own timer. A separate `WeaponViewModel` on the rifle model owns recoil and the reload drop and tilt; it only reads `IsReloading`, `ReloadProgress`, and the `Fired` event, so changing, disabling, or replacing the animation cannot change reload timing or ammunition. Hits are local camera raycasts against `IDamageable` targets; there is no network synchronization yet.
- Gunfire, impact, and reload clips are self-generated temporary audio. The target and rifle meshes/materials are Unity primitives. The ammo HUD uses imported Unity TextMesh Pro essentials; the bundled Liberation Sans font and its OFL terms are recorded in `ASSET_LICENSES.md`.

## M3 implementation and verification

- `MeleeEnemy` has Idle, Chase, Attack, and Dead states. A `NavMeshAgent` follows an acquired living player; a physics raycast prevents melee damage through walls. Detection radius, movement speed, attack range, damage, and cooldown are Inspector fields. `EnemyHealth` implements the existing `IDamageable` interface, and `EnemyFeedback` provides damage, attack, and death color cues. The model is made from Unity primitives.
- `PlayerHealth` has Inspector-configurable maximum health. At zero HP it disables movement and the rifle; the HUD shows health and a death prompt. Press R while dead, or use the component's **Restart Test Scene** context menu in Play mode, to reload the test scene. This is a local testing reset, not multiplayer respawn or revival.
- Unity 6000.6.5f1 compiled the final scripts with zero errors and warnings. The final cleared console contained zero errors and warnings. The Game view showed the health and ammunition HUDs, existing targets, and enemy placeholder; the death message was hidden at 100 HP after correcting HUD initialization.
- Editor Play mode checks: three agents were on the NavMesh; paths from room 1 to 2 and room 2 to 3 were complete. An enemy began Idle outside its 4.5-unit detection radius, entered Chase after the player approached, and reached Attack; health fell by 12 per attack. Two attack decisions at the same simulation time reduced health 100 → 88 → 88, confirming the cooldown gate. Moving the player out of range returned it to Chase. A line-of-sight check across a solid wall returned false at 1.56 units, within the 1.7-unit melee range.
- The existing rifle raycast hit the enemy body. Three shots reduced enemy HP 60 → 40 → 20 → 0 and ammunition 30 → 29 → 28 → 27. At zero HP, the enemy entered Dead, its NavMeshAgent was disabled, subsequent damage returned false, and the body showed the death color. Player death at zero HP disabled movement and the rifle and showed `HEALTH 0 / 100` plus the restart prompt. Invoking the same scene-reset method used by R replaced the player instance and restored 100 HP, the rifle, and all three enemies.
- The R-key restart gesture, movement feel while kiting, mouse aim, attack/damage flash readability, and the full route through both doorways still need a human Game view play test. Unity CLI key simulation did not reliably drive the R-key path; the scene-reset method itself was verified. No online synchronization, respawn, or revive system was added.

## M2 verification

- Unity 6000.6.5f1 recompiled the refined scripts with zero errors and warnings. A final Play test after clearing the console reported zero errors and warnings.
- The Game view visibly showed the placeholder rifle, central crosshair, five targets, and ammunition readout. A shot against the first target reduced health from 60 to 40 and magazine ammunition from 30 to 29. Two more hits depleted its health, disabled its renderer and collider, and it later reset to 60 health with both restored.
- Two immediate fire attempts produced one shot, confirming the configured fire gate. Held automatic fire exhausted a 30-round magazine. Firing the final round from 1/90 started automatic reload immediately, showed `RELOADING  0 / 90`, and completed at 30/60 after 1.6 seconds.
- Repeated reload attempts did not restart an active reload, and firing was blocked during it. The existing R-key path reloaded a partial magazine from 20/15 to 30/5. With zero reserve, the final shot left the magazine at zero, no reload began, and a further shot was blocked.
- At the midpoint of a controlled reload pose test, the model was 0.22 units lower and about 17° tilted; at the end of the configured duration, its position and rotation matched the original pose. A 5/7 partial-reserve test completed at 12/0, with the HUD updated to `12 / 0`.
- On a target hit, the muzzle flash, hit marker, impact effect, and AudioSource were active. A W-key movement check moved the player from x = -10.00 to x = -7.86; M1 movement remained available.
- Reload/animation separation check (Play mode, driven through the Unity CLI): with `WeaponViewModel` removed, firing the final round from 1/90 started a reload that completed at 30/60 after its scheduled 1.6-second duration while the model stayed at rest. With it present, a normal shot kicked the model back 0.070 units; during a reload, the measured drop and tilt matched the values computed from `ReloadProgress` (peak 0.220 units and 17.0°), and the model returned to rest at 30/60. Disabling it halfway through a second reload restored the rest pose immediately, and the reload still completed at 30/30. Scripts compiled with zero errors and warnings, and the console stayed empty.
- The gunfire and reload clips were assigned and played through the AudioSource. Audible quality, animation feel, and all keyboard/mouse combinations still need a human Game view test.

## Manual play test

Open `Assets/Scenes/FirstPersonTest.unity` in Unity and press Play. Click the Game view to capture the cursor. Use WASD, mouse look, Shift, and Space; approach the red capsule enemy in room 1 and then back away to watch it chase. Kite it around the crate and through both doorways. Let it attack, watch the health counter and damage flash, then shoot it three times and check that its body turns gray and it stops attacking. Shoot a teal target to confirm it still resets. After player health reaches zero, verify movement and shooting stop and press R to reload the test scene. While alive, R still reloads the rifle. Press Escape to release the cursor.

## Existing Git change to preserve

`ProjectSettings/ProjectSettings.asset` has an older uncommitted cloud project, organization, and project-name edit. Its cloud association still needs owner confirmation before committing. Keep it out of the M3 PR and do not discard it.

## Next milestone

Review M3 through its PR; do not merge without the project owner's approval and required human review. Next, validate package compatibility, service setup, ownership, and any costs before the two-player online foundation. No online packages or paid services were introduced in M3.
