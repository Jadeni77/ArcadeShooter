# M5 multiplayer setup and macOS test

## Service setup

The game uses Unity Multiplayer Services Sessions with Relay transport and anonymous Authentication. In Unity Dashboard, select the intended project and environment, verify the project is linked in **Edit > Project Settings > Services**, and enable or configure Authentication and Relay as required by that project. Review the current Relay/Multiplayer Services usage terms and billing limits in the Dashboard before inviting more testers. Both clients must use the same Unity project and environment. The repository does not contain service credentials or a committed cloud project ID; the existing local `ProjectSettings/ProjectSettings.asset` linkage edit is deliberately excluded from the M5 PR pending owner confirmation.

The project was developed with Unity 6000.6.5f1. Installed packages are `com.unity.netcode.gameobjects` 2.13.3, `com.unity.services.multiplayer` 2.4.0, and `com.unity.multiplayer.playmode` 3.0.0. The default network prefab list registers the first-person player and melee enemy. Build scenes are `MainMenu` first and `FirstPersonTest` second.

## Two-player test on one Mac

1. Open the project in Unity and confirm the Dashboard linkage above. Run **ArcadeShooter > Configure Online Multiplayer Scenes** only if intentionally rebuilding the scene/prefab setup; the generated scenes and prefabs are already in the repository.
2. For Multiplayer Play Mode, open **Window > Play Mode > Scenarios**. Create a scenario with the main Editor and one **additional Editor** instance. Use separate Editor processes, so the anonymous Authentication profiles are independent. Open `MainMenu` in each and enter Play mode. Alternatively, make a macOS build from the enabled scenes, launch one copy as the host and a second copy as the joiner. Do not share one local instance/profile for both clients.
3. In the host, press **Host Game** and copy the displayed join code. In the second instance, enter the code and press **Join Game**. Both lobby views should show 2 / 4 players; only the host should see **Start Game**. Try an invalid code separately and confirm an error appears.
4. Press **Start Game** on the host. Confirm both clients load the test map at separate spawn points. Each client should move, sprint, jump, and aim with its own controls and camera. The other player should appear as a cyan capsule, move smoothly, and flash when firing. The remote first-person weapon and HUD should stay hidden.
5. Fire and reload independently. Each ammunition HUD should reflect only its player. Have both players shoot the same enemy; confirm matching health/death on both screens. Check that rapid requests do not cause extra damage. Let enemies approach each player in turn and confirm attacks reduce the intended player's health while the other can continue fighting after one dies.
6. Have either player enter Zone A. Both should see one warning, the same wave number and remaining count, and exactly three new enemies. Leave and re-enter the zone; it should not restart. Clear it together. Repeat Zone B (two enemies, then four). If everyone dies during an active zone, both should see Failed. Start a fresh session to reset health and zone state.
7. Have the joiner press **Leave Game**; the host should continue without duplicate players/enemies. Join a fresh lobby and have the host leave from the lobby and then during gameplay in separate runs; the joiner should return to the menu with a session-ended message. Test a full four-player lobby/full-session rejection if extra clients are available.

The scene is still a test map, not a full mission. Join is supported only before host start. Host exit ends the session; host migration, late join, reconnect, revive, and automatic respawn are not implemented. Player movement is owner-authored and therefore not protected against speed/teleport cheats. Shot requests have host-side plausibility, cadence, ammo, and health checks but this is not a production anti-cheat system.

Record client logs and screenshots for any failure, including which instance hosted, the join code result, scene name, player count, and whether the issue reproduces after a new session.
