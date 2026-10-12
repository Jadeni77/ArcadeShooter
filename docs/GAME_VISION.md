# Game vision

## Pitch

A 3D online cooperative first-person shooter for 2–4 players that combines responsive, satisfying arcade shooting with the freedom to explore a map. Players choose a mission, enter together, encounter danger as they move through the environment, complete an objective, and extract. The game must never move players along a fixed route or take over their cameras.

## Confirmed experience

- **Player agency:** Each player moves independently with WASD, mouse look, sprint, and jump. Players can stop, backtrack, explore, and choose how to approach a fight.
- **Cooperation:** Players share a mission and survive encounters together. The design should give them reasons to coordinate, cover each other, and recover from mistakes.
- **Arcade combat feel:** Shooting should be immediate and readable. Gunfire, reloads, hits, critical hits, enemy reactions, impact effects, and sound should reinforce one another.
- **Pacing through place:** Entering a location may activate a danger zone. A zone can warn players, start waves or ambushes, require defense, or introduce a boss. Players retain movement and camera control throughout.
- **Clear outcomes:** Objectives and extraction give a mission a beginning, middle, and end. Rewards and deeper progression can grow later.

The arcade reference describes the desired feel of shooting. This is **not a rail shooter**.

## Core loop

1. Form or join an online group and select a mission.
2. Enter an explorable 3D map together and move freely.
3. Discover locations, trigger danger zones, and fight enemies.
4. Complete the mission objective and reach extraction.
5. Receive a result and any rewards, then choose whether to play again.

The first prototype may have only one selectable mission and a minimal result screen. It should still demonstrate the full loop without requiring a progression economy.

## Combat and presentation direction

Support gunshots and reload sounds; hit and critical-hit confirmation; enemy attack, damage, and death sounds; environmental audio; and music that changes when a danger-zone encounter starts or ends. Use muzzle flashes, impacts, and enemy damage feedback. Start with placeholders and replace them only when an asset has a recorded source and license in [ASSET_LICENSES.md](ASSET_LICENSES.md).

## First playable prototype

One small explorable map, one first-person player controller, one working gun, one enemy type, one danger zone that spawns enemies, basic health and damage, a simple mission objective and extraction, and **two-player online play**. A host can select the single available mission. The broader game targets 2–4 players; validating 3–4 players follows the first two-player slice.

## Long-term possibilities, not initial commitments

More themed maps and chapters, enemy types and bosses, unlockable weapons and equipment, player levels, chapter scores and rankings, randomized encounters, difficulty modes, hidden objectives, achievements, and content updates. Build reusable mission, weapon, enemy, and encounter definitions as these features become necessary; avoid building a full framework before the prototype proves the loop.
