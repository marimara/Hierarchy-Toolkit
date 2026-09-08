# 012 - Hierarchy Shortcuts

Status: PLANNED

## Goal

Provide fast keyboard shortcuts for common Unity Hierarchy expansion and isolation actions using the GameObject currently under the mouse.

## Behavior

Hierarchy Toolkit adds shortcuts for:

- Expand/Collapse hovered object
- Isolate hovered object

The actions operate on the hovered Hierarchy GameObject when possible.

## Requirements

### Expand / Collapse

- Provide a shortcut to toggle expansion of the hovered GameObject.
- Default shortcut: `E`.
- If the object is expanded, collapse it.
- If the object is collapsed, expand it.
- Only affect Hierarchy expansion state.
- Do not modify scene data.

### Isolate

- Provide a shortcut to isolate the hovered GameObject branch.
- Default shortcut: `Shift+E`.
- Expand the hovered GameObject as needed.
- Collapse unrelated branches.
- Preserve the hovered object's ancestor chain as needed so the branch remains visible.
- Do not hide, deactivate, or reparent scene objects.
- Isolation affects Hierarchy presentation only.

### Hover Targeting

- Shortcuts operate on the GameObject row currently under the mouse in the Hierarchy.
- Do not silently fall back to the currently selected GameObject.
- If no valid Hierarchy GameObject is under the mouse, do nothing.
- Scene header rows and unsupported rows must fail safely.

### Interaction

- Preserve native Unity selection, rename, drag/reorder, and foldout behavior.
- Do not trigger while typing into text fields or search fields.
- Do not trigger during active rename/text editing.
- Use Unity's ShortcutManager or another appropriate modern Unity Editor shortcut API.

### Performance

- Do not poll the Hierarchy continuously.
- Track hover/input using appropriate Editor/UI events.
- Do not traverse the hierarchy during repaint.
- Recursive expansion/collapse work may run only when the shortcut is invoked.

## Acceptance Criteria

- `E` toggles expansion of the hovered GameObject.
- `Shift+E` isolates the hovered branch by collapsing unrelated branches.
- Isolate keeps the target branch visible and usable.
- Shortcuts do nothing safely when no valid GameObject is hovered.
- Typing in search/text fields does not accidentally trigger shortcuts.
- Rename mode does not accidentally trigger shortcuts.
- No scene objects are modified by expand/collapse/isolate.
- Existing GameObject bookmarks continue working.
- Scene Selector continues working.
- Existing Hierarchy Toolkit visual features remain functional.
- No package-related Console errors are introduced.

## Out of Scope

- Focus/Frame Scene View shortcut.
- Default Parent.
- Shortcut customization UI.
- Scene switching shortcuts.
- Activation shortcuts.
- Bookmark shortcuts.
- Runtime shortcuts.