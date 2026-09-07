# 003 - Manual Hierarchy Colors

Status: DONE

## Goal

Allow artists and developers to assign custom background colors to individual Hierarchy GameObjects.

## Behavior

A GameObject may have an explicit manual hierarchy color.

The color appears directly behind its Hierarchy row.

Users assign or clear colors through Hierarchy Toolkit UI.

Manual color overrides take precedence over automatic visual styling.

## Requirements

- Store manual color in scene metadata.
- Preserve alpha.
- Explicit transparent color must remain distinguishable from no color override.
- Provide Set Color and Clear Color actions.
- Provide an artist-friendly color picker.
- Support multi-selection.
- When the clicked object belongs to the current selection, operate on eligible selected objects.
- Otherwise operate only on the clicked object.
- Support objects from multiple saved scenes in one operation.
- Changes support Undo/Redo.
- Manual color lookup must use cached metadata.
- Do not call AssetDatabase per Hierarchy repaint.
- Selection highlighting must remain readable.
- Zebra striping must not override manual colors.
- Hierarchy interaction must remain native.

## Acceptance Criteria

- User can assign a color without writing code.
- Color appears immediately in the Hierarchy.
- Color persists after scene reopen.
- Color persists after Editor restart.
- Clear Color removes only the color override.
- Multiple selected objects can receive the same color.
- Undo restores the previous color state.
- Redo reapplies it.
- Selected objects remain clearly visible.
- Color rendering introduces no obvious Hierarchy flicker or slowdown.

## Out of Scope

- Automatic color rules.
- Color presets.
- Gradient rows.
- Animation.
- Runtime coloring.