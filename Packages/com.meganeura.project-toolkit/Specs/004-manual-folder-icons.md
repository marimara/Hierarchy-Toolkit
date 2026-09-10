# 004 — Manual Folder Icons

Status: PLANNED

## Goal

Let artists assign or clear a recognizable icon on folders from the Project window.

## Behavior

A manual icon decorates or replaces the folder's Toolkit-controlled icon area without obscuring essential native state. Manual icons override automatic icons.

## Requirements

- Use the metadata and cache from spec 002.
- Provide contextual Set Icon and Clear Icon actions and a searchable picker.
- Support Unity built-in icons and user-provided project textures through stable identifiers.
- Do not redistribute third-party icon libraries or copy another asset's visual set.
- Follow the same clicked-item and multi-selection convention as manual colors.
- Cache resolved textures; no repeated icon lookup or asset loading in repaint.
- Missing or deleted custom textures degrade to the native folder icon and remain repairable.
- Preserve native open/closed folder cues and overlays when they convey state.
- One multi-folder action is one logical Undo step when practical.

## Acceptance Criteria

- Users can set and clear icons without code.
- Assignments persist across reload, rename, and move.
- Multi-selection, Undo, and Redo behave correctly.
- Missing custom textures do not throw or spam the Console.
- List and grid presentations remain readable.
- Manual icons take precedence over automatic icons.

## Visual References

- `Documentation/ProjectToolkit/VisualReferences/vFolders2/03-set-folder-icons.png`
  - Use for: legible icon overlays at tree and grid sizes, consistent placement, and quick visual recognition.
  - Do not copy: included icon artwork, exact picker layout, palette, branding, or the Alt-click gesture unless separately required by this spec.

The reference does not authorize bundling its icon set. Use Unity-provided icons or properly licensed user assets through the mechanisms defined by this spec.

## Out of Scope

- Automatic icon inference, downloadable icon packs, icon authoring, and import/export.
