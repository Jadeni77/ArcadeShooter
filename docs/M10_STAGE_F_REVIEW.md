# M10 Stage F integration review

The integration branch combines Stage A's visual direction, Stage B's menu/lobby, Stage C's combat/extraction HUD, Stage D's armory/profile, and Stage E's results/pause/settings. Stage A and B both used decision ID D-025; integration preserves Stage A as D-025 and records the approved Stage B direction as D-026. The original owner checkout and its uncommitted scene, prefab, material, and cloud settings edits were not changed.

## Visual and build evidence

- Unity 6000.6.5f1 imported the combined project and rendered the integrated [Stage E screens](M10_STAGE_E_REVIEW.md), including extraction and failure, at 1280×720, 1600×900, and a 2560×1600 results view. The [Stage B menu/lobby](M10_STAGE_B_REVIEW.md), [Stage C HUD](M10_STAGE_C_REVIEW.md), and [Stage D armory/profile](M10_STAGE_D_REVIEW.md) have their own Unity captures. Static capture values are representative layout data.
- A macOS Development Player build completed successfully from the combined branch with the previously approved local Unity Services project link. That link is uncommitted. The Player opened and rendered the cinematic MainMenu at a windowed Mac resolution. The build output is local and is not part of the repository.
- The combined EditMode suite passed 44/45. The only failure is the previously documented M8B batch-mode mixer exposed-parameter readback, expected around −12.04 dB and observed 0 dB. No M10 test or C# compilation failed.
- [Before/after Unity captures](M10_BEFORE_AFTER.md) now compare the pre-M10 `d03d873` menu and mission results with the integrated UI at 1600×900. Both results builders use the same representative values.
- The local in-match menu does not change `Time.timeScale`, mission authority, or network state. `NetworkPlayer` disables the owner's movement, look, weapon, and input components while the menu is open. The results score and XP progress start their animation when replicated data and the local XP receipt arrive.

## Open acceptance checks

- In a live cloud-linked two-client session, test lobby name/ready/difficulty/finish sync, host Start gating, remote health/name HUD, danger-zone alerts, extraction occupancy/countdown, result totals and XP, replay, and squad return.
- On both peers, test Escape → Settings → Back → Resume, cursor lock and focus, death while paused, and confirmed Leave Match. The game window rendered in the macOS Player, but this task's computer-control layer could not deliver coordinate clicks to that Unity window (`noWindowsAvailable`); no interaction pass is claimed.
- Check 1920×1080, 2560×1440, and a live 16:10 Game view; confirm font rendering and clipping with populated 2–4 player state. Measure frame time and UI Canvas rebuilds during combat. The static captures and batch build do not establish a 60 FPS result.
- Capture matching **live runtime** before/after shots if required for final acceptance; the linked Edit-mode Unity comparison is complete. Existing Stage A mockups remain design references.

These checks remain open for final M10 acceptance even if the reviewed implementation is staged on `develop`.
