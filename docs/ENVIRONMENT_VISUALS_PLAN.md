# M8A environment and lighting plan

The current `FirstPersonTest` level uses an abandoned research facility direction. This decision is specific to this map; future levels can have other themes (D-022). The owner approved the two CC0 asset sources before import.

## Baseline and constraints

- Three adjacent 10 m by 10 m rooms share two 3 m wide, 2.4 m high passages. Room one has the player spawn and extraction point; room two has Danger Zone A; room three has Danger Zone B and the research sample.
- `Three Room Graybox` owns a saved `NavMeshSurface` baked from child colliders. The scene has one directional light, flat ambient light, no fog, and no scene Volume. The PC URP renderer has SSAO enabled. There are no imported environment models or material textures.
- The enemy prefab is a capsule with a separate hit collider and facing marker. `EnemyFeedback` controls its damage, attack, and death color through a material property block.
- Keep every passage, player and enemy spawn, zone trigger, mission objective, target, and extraction trigger in place. Decorative meshes should not affect navigation or shot blocking unless intentionally replacing a validated collision surface.

## Visual slice

1. Build room one as a research facility entry and decontamination bay with modular wall and floor pieces, an open doorframe, restrained structural detail, and a clearly marked extraction area.
2. Establish a reusable URP material palette: neutral painted panels, dark structural metal, and limited cyan for safe/navigation information and amber for warnings. Give enemies a contrasting silhouette and color. Avoid changing their collision or combat components.
3. Tune the scene's base light and a small number of practical lights. Test a subtle Volume only after confirming the player camera renders post-processing and the PC/Mobile quality assets behave as intended. Keep objectives, enemies, and doorways legible during combat.
4. Verify room-one navigation, spawn clearances, shooting, and extraction. Compare Game-view captures before extending the modular language to rooms two and three.

## Extension and checks

- Room two: containment/security cues, amber warning fixtures and hazard floor markings around Zone A, without changing the zone trigger.
- Room three: blue lab identity, distinct sample station, warning cues around Zone B, and an unobstructed route to the sample.
- Rebuild the saved NavMesh only if collision geometry changes. Confirm complete room-to-room paths and valid wave spawn points. Keep decorative meshes outside the `NavMeshSurface` collection hierarchy or remove their colliders.
- Import only selected approved files and their `.meta` files. Record each source, license, and in-project path in `ASSET_LICENSES.md` before delivery.
- Run Unity import/compile and focused EditMode checks, then manually test movement, combat, both waves, sample collection, extraction, and shared environment visibility with two peers. Capture frame time on the macOS development machine before reporting any 60 FPS result.

The room-one visual slice was built and rendered first. Its saved NavMesh routes and collision separation were checked before extending the visual system to rooms two and three. Editor preview images were then inspected for all three rooms and extraction. Runtime movement, combat, and network play still require the manual checklist in [ENVIRONMENT_VISUALS_TESTING.md](ENVIRONMENT_VISUALS_TESTING.md).
