# 002 — Folder Metadata

Status: DONE

## Goal

Persist Project Toolkit folder customization safely using stable asset identity without modifying folder contents or `.meta` files.

## Behavior

Supported folders can hold optional manual color and icon assignments that survive moves, renames, reloads, and Editor restarts.

## Requirements

- Identify project folders durably by asset GUID, never by path alone.
- Resolve current paths outside hot rendering callbacks and cache the result.
- Store team-visible folder customizations in one source-control-friendly project data store outside the package source.
- Store only portable identifiers for custom assets; do not serialize transient instance IDs.
- Distinguish an unset field from an explicitly transparent color or explicit default icon.
- Remove or safely ignore orphaned records for deleted or unavailable folders through an explicit maintenance path, not repaint-time mutation.
- Moves and renames must not lose metadata.
- Package folders and immutable/read-only locations must fail safely according to the operation being attempted.
- Configuration changes support Undo/Redo where Unity can provide meaningful Undo.
- Maintain a cached read model with explicit invalidation after project, Undo/Redo, or data-store changes.
- Do not write during Project window rendering.

## Acceptance Criteria

- Metadata survives reload and Editor restart.
- A renamed or moved folder keeps its metadata.
- Deleted folders cause no exception or Console spam.
- Transparent color differs from no override.
- Cache invalidation makes edits visible immediately.
- No AssetDatabase search or data-store write occurs per item repaint.

## Out of Scope

- Rendering colors or icons.
- Automatic rules.
- Personal bookmarks and appearance preferences.
- Import/export.
