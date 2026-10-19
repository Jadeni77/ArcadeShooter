# Milestone 8A environment checks

Open `Assets/Scenes/MainMenu.unity` and start a host match from the lobby. A direct Play of `FirstPersonTest` does not spawn a player. To check synchronization, join with a second macOS client or Multiplayer Play Mode player; a fresh checkout must be linked to the intended Unity Dashboard project for Relay as described in [MULTIPLAYER_TESTING.md](MULTIPLAYER_TESTING.md).

## Automated results

- Unity 6000.6.5f1 imported and compiled the completed scene and both modified prefabs with no C# errors.
- All 38 EditMode tests passed on 2026-10-17: 36 existing tests and 2 environment checks.
- The new checks verify that the separate visual hierarchy has no colliders, both saved NavMesh routes remain complete, the spawn, zones, sample, and extraction are present, the scene Volume and player post-processing are configured, and the enemy agent and hit collider retain their original dimensions.
- Editor-rendered 1280×720 previews of [room one](previews/m8a-room1.png), [room two](previews/m8a-room2.png), [room three](previews/m8a-room3.png), and [extraction](previews/m8a-extraction.png) were visually inspected. These previews show static scene presentation; they are not a Game-view or performance measurement.

## Manual play checklist

1. Start the mission with a host and joiner. Confirm both see the same floor, wall panels, ceiling, open passages, lights, and signs. Move each player independently; verify no camera-driven movement.
2. Walk through both passages in both directions, including sprinting and backing through. Check all four starting positions for clear sight and movement. Verify the room-one crate, room-two barrier, and room-three crate still block as before.
3. Fire at the targets and a wave enemy from several angles. Confirm the new decoration does not absorb bullets, while the existing walls and cover still do. Confirm enemy damage, attack, and death feedback remains legible against the cooler room palette.
4. Enter Zone A and Zone B. Confirm the amber floor boundary and warning fixtures make each zone readable, both waves spawn, and enemies pursue players through the passages. Check the room-three sample remains reachable and E collects it.
5. Return to the room-one extraction marker with all living players. Confirm its locked and active labels and ten-second countdown are readable, and both clients reach the same result and score. Try replay and return to lobby.
6. Inspect the scene in the Game view on the target Mac at a known resolution and quality level. Record average/low FPS and frame time in a real run, including combat. Check that bloom is restrained and no objective or enemy is obscured. The 60 FPS target is not verified by Editor preview renders.

The two-client gameplay route and actual FPS measurement were not run in the isolated worktree; the local cloud project link and a second live client were unavailable for this pass. Do not mark them passed from the EditMode or preview results.
