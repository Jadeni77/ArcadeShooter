# M7 weapon visuals and animation checks

## Asset state

The playable rifle remains a project-made primitive model, now with a rail, sights, handguard, magazine well, muzzle brake, and receiver details. The remote avatar has a simpler rifle. The project has no rigged first-person arms or hands and no articulated reload source animation. `WeaponRigAnimator` is ready to drive an approved rig with `Fire` (trigger), `Reloading` (bool), `ReloadProgress` (float), and `Aim` (float). Empty click and separate magazine handling clips are also absent. A model and compatible rig require owner approval before import.

## Local play check

For Relay testing from a fresh worktree, link it to the intended Unity Dashboard project as described in `MULTIPLAYER_TESTING.md` before entering Play mode. Keep that local cloud linkage out of the PR.

1. Open `Assets/Scenes/MainMenu.unity`, start a hosted mission, and enter `FirstPersonTest`. Confirm the rifle has the added details and does not intersect the near plane at rest or while aiming.
2. Move, stop, sprint, and turn. Confirm subtle bob and sway, a lower sprint pose, and smooth return. Mouse look must still directly control the camera.
3. Hold and release right mouse. Confirm the weapon sights move to the center, FOV narrows then returns, and look sensitivity changes smoothly. Shoot the teal targets both aimed and un-aimed; hit results must agree with the camera crosshair.
4. Fire automatic bursts. Confirm visible recoil, existing gunfire audio, flash, hit feedback, and unchanged eight-rounds-per-second cadence. Empty the reserve and hold fire to check that no reload starts and any optional empty clip plays at a capped rate.
5. Press R with a partial magazine and reserve. Check the lower/tilt, magazine down/up movement, existing reload audio, blocked shooting, and ammunition update only at authoritative reload completion. Repeat by firing the last round to trigger automatic reload. Repeated R presses must not restart the sequence.
6. Press Escape and click UI. ADS, firing, and reload input must stay inactive while the cursor is released. Return to the mission and verify no camera FOV is stuck at its aimed value after death or results.

## Two-client check

1. Host and join with two clients, then start the mission. Each owner should see only their first-person rifle; each should see the other's third-person rifle on the avatar.
2. Fire and reload independently on both clients. The other peer should see each shot's recoil/body flash and the rifle lower during reload. A player's reload must not animate or block the other's rifle.
3. Shoot shared enemies and targets while aiming. Compare damage, ammunition, and HUD state on both peers. Test a client firing near the end of reload and under ordinary Relay latency.
4. Finish or fail the mission, replay, and return to lobby. Check that local cameras, audio listeners, weapon models, and input ownership remain correct after the transition.

The Editor batch import, compilation, and EditMode test results belong in `STATUS.md`. A human must judge animation feel, sound quality, camera clipping, and the two-client visual checks above.
