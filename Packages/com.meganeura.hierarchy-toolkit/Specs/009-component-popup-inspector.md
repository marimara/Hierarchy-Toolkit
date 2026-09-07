# 009 - Component Popup Inspector

Status: PLANNED

## Goal

Allow users to inspect and edit a GameObject component directly from the Hierarchy through a lightweight popup, without requiring normal Inspector navigation.

## Behavior

The Component Minimap from spec 008 becomes interactive.

Alt-clicking a component icon opens a compact popup inspector for that specific component.

The popup displays the component's editable Inspector UI and remains anchored near the Hierarchy interaction when practical.

Only one popup inspector should be active at a time unless the existing architecture strongly favors another safe behavior.

## Requirements

- Reuse the existing Component Minimap icons and right-side layout.
- Alt-click is the interaction for opening the popup.
- Normal click behavior from spec 008 must remain unchanged.
- Do not require selecting the GameObject first.
- Opening the popup must not unexpectedly change the current Hierarchy selection.
- The popup must inspect exactly the component represented by the clicked icon.
- Support built-in Unity components and MonoBehaviour components.
- Use `Editor.CreateEditor` or an equivalent safe Unity Editor API.
- Use Odin rendering only when it integrates cleanly and does not destabilize standard inspectors.
- The popup must support normal serialized property editing.
- Component edits must support Unity Undo/Redo.
- Changes must respect prefab instance override behavior.
- Do not accidentally edit prefab assets when interacting with scene instances.
- Missing or destroyed components must close/fail safely.
- Temporary Editor instances must be disposed correctly.
- Do not leak references after popup close, row recycle, domain reload, scene close, or assembly reload.

## Popup UX

- Keep the popup compact and readable.
- Show the component name/type clearly.
- Provide a close control.
- Allow the popup to remain open while editing.
- Close safely if the inspected component is destroyed or becomes invalid.
- Do not block normal Hierarchy interaction after closing.
- Prefer a size appropriate to the component Inspector rather than a large generic window.
- If the popup cannot fit cleanly near the click position, reposition it within the visible Editor area.
- The popup must be movable by the user.
- Prefer dragging through the popup header/title area.
- Moving the popup must not change Hierarchy selection or edit component values.
- The popup should remain inside the visible Unity Editor desktop area when practical.

## Interaction

- Alt-click on a component minimap icon opens its popup inspector.
- Normal hover tooltip from spec 008 remains available.
- Normal click must not open the popup.
- No component reordering or drag/drop is introduced here.
- No pinning or multiple simultaneous inspector windows are required in this spec.

## Performance

- Do not create Editor instances during normal Hierarchy repaint.
- Create popup/editor state only when the user explicitly opens a component.
- Dispose Editor instances immediately when no longer needed.
- No AssetDatabase searches in hot row rendering paths.
- No polling unless required for safe popup lifetime handling.
- Prefer event-driven invalidation and cleanup.

## Acceptance Criteria

- Alt-clicking a valid component icon opens a popup inspector.
- The popup displays the correct component.
- Standard serialized fields can be edited.
- Undo restores popup edits.
- Redo reapplies popup edits.
- Prefab instance changes behave as valid prefab overrides.
- Opening the popup does not unexpectedly change Hierarchy selection.
- Closing the popup releases temporary Editor resources.
- Destroying the inspected component does not throw or spam the Console.
- Domain/assembly reload does not leave stale popup state.
- Component Minimap layout remains intact.
- Activation Toggle remains functional.
- Existing colors, icons, separators, zebra striping, and hierarchy lines remain functional.
- No package-related Console errors are introduced.
- The popup can be repositioned by dragging its header/title area.

## Out of Scope

- Pinning popup inspectors.
- Multiple simultaneous popups.
- Component reordering.
- Component enable/disable controls.
- Adding or removing components from the popup.
- Component search.
- Global Inspector replacement.
- Settings UI.
- Keyboard shortcuts beyond Alt-click interaction.