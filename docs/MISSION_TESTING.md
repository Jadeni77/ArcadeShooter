# Milestone 6 macOS mission test

Use two separate macOS players, one host and one joiner, against the same linked Unity Dashboard project. Follow the Session/Relay setup in [MULTIPLAYER_TESTING.md](MULTIPLAYER_TESTING.md). The first scene in the build must be `MainMenu`; `FirstPersonTest` must also be included. The host shares the private lobby code with the joiner and starts the mission. This checklist records work to perform; it does not claim the two-client test has passed.

## Full success run

1. Host a session and join from the second player. Confirm both appear in the lobby; only the host has **Start Game**. Start once. Both should load the same three-room map with their own camera and freely controlled WASD/mouse movement.
2. Confirm the mission HUD on both players lists **Clear Danger Zone A**, **Clear Danger Zone B**, and **Collect the research sample** as incomplete. The extraction marker in room one should say locked.
3. Enter Zone A in room two, defeat its three spawned enemies, and confirm its objective changes to Done on both players. Re-enter; it must remain complete and spawn no additional wave.
4. Clear both waves in Zone B in room three (two enemies, then four after the delay). Confirm the second objective changes to Done on both players. The original room-one enemy and teal training targets are not zone objectives.
5. Find the amber sample in room three. Move one living player near it; the E prompt appears. Press E once. Both players must see the sample disappear and objective three complete. A distant player pressing E must not collect it; pressing E again must not add score or progress.
6. Confirm the extraction marker in room one turns green, and both HUDs show its location/status. Enter with one player; the counter should show one of two inside and no countdown. Bring the second player inside; verify the ten-second countdown on both displays. Move either one out; the timer must cancel. Re-enter and wait ten full seconds.
7. Both players should see **Mission Success**, the same mission duration, team score and rank, and the same two player rows. Compare each player's kills, accepted shots, confirmed enemy hits, accuracy, direct sample objective count, and personal score. Shooting and movement must no longer change statistics after results appear.
8. Host clicks **Replay Mission**. Both players should reload the map without a new join code. Health, ammunition, enemies, danger zones, sample, locked extraction, score, timer, and spawn positions must match a fresh mission. Complete or fail it again, then use **Return to Lobby**. Both players should return to the connected lobby. Host starts once more to confirm another mission can begin.

## Failure and disconnect cases

- In a separate run, let every connected player die. Both should see Mission Failed; shots and enemy attacks must not change the result. The host can return everyone to the lobby; an individual player can leave the session.
- While extraction is counting down, disconnect one required player. The remaining living player should become the required roster and must remain inside for a fresh countdown. If no living player remains, the mission fails. Disconnect the host separately; the joiner should return to the menu through the existing shutdown path.
- If a player dies while the other lives, the dead player is excluded from extraction. No revive exists. Verify the mission can still succeed with the survivor.
- Test with a client whose connection is delayed or lost during scene load; only clients that completed loading should receive a spawned avatar. Verify no duplicate `NetworkManager`, UI, player, or wave enemies on lobby return and replay.

## Score rules and verification

The server records one shot when it accepts a rifle request and spends one authoritative round. Only damage to a living enemy counts as a hit. The lethal hit adds one kill to that shooter and cannot be credited again. Teammate damage and training targets add no score. Accuracy is `100 × enemy hits / accepted shots`, or `0%` with zero shots. Individual score is `100 × kills + 10 × enemy hits`. Team score is the sum of individual scores plus `500 × completed shared objectives` once, with no default success bonus. The player who collects the sample gets one individual objective count; zone clears contribute to the shared team count. Failure always ranks C. Success ranks B at 1,800 points, A at 2,400, and S at 2,800; lower success scores rank C. These values are serialized on the scene's Mission Control component and may be tuned there.

Run `ArcadeShooter > Validation > Check Mission Scoring` and `Check Mission Progress and Extraction` in the Unity Editor, then use `Build macOS Development Player` to build the enabled scenes. Record both players' console logs and any errors or warnings. The live two-client run is the acceptance gate for shared state, replay, and disconnect behavior.
