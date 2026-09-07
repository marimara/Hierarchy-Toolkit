# 002 - Scene Object Metadata

Status: DONE

## Goal

Persist Hierarchy Toolkit customization for scene GameObjects without adding components or helper objects to scenes.

## Behavior

Each supported saved scene has a canonical metadata store.

GameObject customization is associated with stable Unity object identity.

Metadata survives normal scene reloads, domain reloads, and Editor restarts.

## Requirements

- Use stable object identification based on Unity GlobalObjectId.
- Store metadata outside scene GameObjects.
- Use one deterministic metadata store per saved scene.
- Scene store resolution must be based on the scene GUID.
- Renaming or moving a scene while preserving its `.meta` GUID must preserve store resolution.
- Metadata supports independent feature overrides.
- Current metadata includes manual color, manual icon, and separator information.
- Removing one customization must preserve unrelated metadata.
- Empty metadata entries should be removable.
- Changes must support Undo/Redo where applicable.
- Store changes must notify dependent caches.
- Unsupported objects must fail safely.
- Prefab assets are not scene metadata targets.
- Prefab Mode support is not currently required.

## Acceptance Criteria

- Metadata can be created for a supported scene GameObject.
- The same GameObject can be retrieved later using stable identity.
- Scene close/reopen preserves customization.
- Editor reload preserves customization.
- Clearing color preserves icon and separator metadata.
- Clearing icon preserves color and separator metadata.
- Clearing separator preserves color and icon metadata.
- Empty entries can be removed.
- Undo/Redo restores metadata changes.
- A saved scene always resolves to the same canonical store.

## Out of Scope

- Save As metadata migration.
- Cross-scene GameObject migration.
- Prefab Mode customization.
- Runtime metadata.
- Cloud synchronization.