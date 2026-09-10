# 003 — Manual Folder Colors

Status: IN PROGRESS

## Goal

Let artists assign or clear colors on folders directly from the Project window.

## Behavior

A folder with a manual color is clearly tinted in supported Project window presentations. Manual color has higher precedence than automatic rules.

## Requirements

- Use the metadata and cache from spec 002.
- Provide contextual Set Color and Clear Color actions plus an artist-friendly picker.
- When the clicked folder belongs to the current selection, operate on all eligible selected folders; otherwise operate only on the clicked folder.
- Ignore selected non-folder assets without changing them.
- Preserve alpha and distinguish transparent from unset.
- One multi-folder action is one logical Undo step when practical.
- Keep selected and focused item text readable.
- Cooperate with native backgrounds and version-control overlays.
- Support list and grid presentations where Unity exposes a safe drawing surface.
- Never tint the source texture or modify the folder asset itself.

## Acceptance Criteria

- Users can set and clear folder colors without editing code.
- Multi-selection behavior follows the package convention.
- Changes appear immediately and persist.
- Undo and Redo restore the expected states.
- Rename and move preserve the color.
- Selection remains readable in supported views.
- Rendering performs no metadata scan per repaint.

## Visual References

- `Documentation/ProjectToolkit/VisualReferences/vFolders2/02-set-folder-colors.png`
  - Use for: immediate folder-color feedback in tree and grid views, compact access to common color choices, and readability of colored folder silhouettes.
  - Do not copy: exact palette values, popup composition, icons, spacing, branding, or the Alt-click gesture unless separately required by this spec.

The Meganeura implementation must remain visually original. The screenshot does not add icon assignment or palette customization to this spec.

## Out of Scope

- Automatic colors, gradients, palettes, and non-folder asset colors.
