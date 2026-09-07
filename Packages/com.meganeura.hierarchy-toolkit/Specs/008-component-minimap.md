# 008 - Component Minimap

Status: PLANNED

## Goal

Display compact component icons on the right side of Unity Hierarchy rows so users can quickly understand a GameObject's composition without selecting it.

## Behavior

Eligible GameObject rows display a compact horizontal set of component icons on the right side of the Hierarchy.

Each icon represents one component type attached to the GameObject.

The minimap is informational in this spec.

Hovering an icon shows a tooltip with the component type name.

## Requirements

- Use the existing centralized right-side row layout infrastructure.
- Do not overlap the Activation Toggle.
- Do not overlap native Unity row controls or GameObject labels.
- Degrade gracefully in narrow Hierarchy windows.
- Preserve Unity selection, rename, foldout, drag/reorder, and keyboard behavior.

### Component collection

- Do not call `GetComponents` every repaint.
- Cache component information per supported GameObject.
- Rebuild or invalidate cached component data only when relevant Editor state changes.
- Missing Script entries must fail safely and must not throw.
- Ignore `Transform` by default to reduce visual noise.
- Display other attached component types in component order unless an existing architecture requirement suggests otherwise.
- Multiple components of the same type may be represented once in this initial version.

### Icons

- Prefer Unity's native component icons when available.
- Custom MonoBehaviour components should use their Unity script/component icon when available.
- Missing icon lookup must degrade gracefully.
- Cache resolved textures/icons.
- Do not perform AssetDatabase searches in hot Hierarchy paths.

### Interaction

- Icons use `PickingMode.Ignore` unless interaction is explicitly required for tooltips.
- Hovering an icon shows the component type name.
- This spec does not open inspectors or modify components when clicked.

### Layout

- Component icons appear before the Activation Toggle in the reserved right-side area.
- Icon size and spacing should remain compact and consistent.
- If insufficient width is available:
  - preserve native GameObject label readability first
  - preserve Activation Toggle next
  - reduce or hide minimap icons as necessary
- Hidden icons must not create overlap or horizontal visual corruption.

### Performance

- No repeated `GetComponents` calls during repaint/binding.
- No AssetDatabase search during row rendering.
- No reflection in hot paths.
- Avoid per-row temporary collections and avoidable allocations.
- Use event-driven or explicit cache invalidation.

## Acceptance Criteria

- Supported GameObjects display icons for attached non-Transform components.
- Icons correspond to the expected component types.
- Tooltips show component names.
- Missing Script does not throw or spam the Console.
- Duplicate component types do not create unnecessary duplicate icons in this initial version.
- Activation Toggle remains visible and functional.
- Manual colors, manual icons, separators, zebra striping, and hierarchy lines remain functional.
- Narrow Hierarchy windows do not produce overlapping controls.
- Component data is cached rather than collected every repaint.
- No package-related Console errors are introduced.

## Visual Reference

See attached reference image.

Use the image only for:
- compact icon presentation
- spacing
- right-side placement
- overall visual density

Do not implement the popup inspector or Alt-click interaction shown in the reference.
Those belong to a future spec.

## Out of Scope

- Component popup inspector.
- Clicking icons to edit components.
- Dragging/reordering components.
- Component enable/disable controls.
- User-configurable component visibility.
- Component icon presets.
- Validation warnings.
- Settings UI.