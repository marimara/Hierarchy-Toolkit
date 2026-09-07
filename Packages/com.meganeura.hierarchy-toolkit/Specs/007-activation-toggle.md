# \# 007 - Activation Toggle

# 

# Status: PLANNED

# 

# \## Goal

# 

# Allow users to activate or deactivate scene GameObjects directly from the Unity Hierarchy with minimal clicks.

# 

# \## Behavior

# 

# Eligible GameObject rows display a compact activation toggle on the right side of the Hierarchy.

# 

# The toggle represents the GameObject's `activeSelf` state.

# 

# Clicking the control changes `activeSelf` without requiring the GameObject to be selected first.

# 

# If the clicked GameObject belongs to the current multi-selection, the same target state is applied to all eligible selected GameObjects.

# 

# \## Requirements

# 

# \- Use `GameObject.SetActive` / `activeSelf`.

# \- Do not store activation state in Hierarchy Toolkit metadata.

# \- Display a compact right-side activation control.

# \- Active and inactive states must be visually distinguishable.

# \- Provide a tooltip explaining the control.

# \- Clicking the control must not unexpectedly change the current selection.

# \- The control must not replace Unity's normal row selection behavior.

# 

# \### Multi-selection

# 

# If the clicked GameObject belongs to the current selection:

# 

# \- determine the target state from the clicked object's current `activeSelf`

# \- apply the inverse of that state to all eligible selected GameObjects

# \- use one logical Undo operation when practical

# 

# If the clicked object is not part of the current selection:

# 

# \- affect only the clicked GameObject

# 

# Mixed active/inactive selections therefore become one consistent state based on the clicked object's toggle action.

# 

# \### Unity behavior

# 

# \- Support Undo/Redo.

# \- Support prefab instances safely.

# \- Do not modify prefab assets when operating on scene instances.

# \- Display the object's `activeSelf`, not `activeInHierarchy`.

# \- A child with `activeSelf == true` under an inactive parent must still show its own activeSelf state correctly.

# 

# \### Layout

# 

# \- Reserve right-side row space through shared layout infrastructure.

# \- Keep the activation control at the right side of the row.

# \- Do not overlap existing or future right-side controls.

# \- Reserve compatibility with future Component Minimap and Validation controls.

# \- Degrade gracefully when the Hierarchy window is narrow.

# \- If there is insufficient width, preserve the GameObject label and native controls before decorative/right-side controls.

# 

# \### Performance

# 

# \- No AssetDatabase calls during row rendering.

# \- No metadata lookup for normal activation rendering.

# \- No repeated GetComponents calls.

# \- No avoidable per-row allocations in hot paths.

# \- Prefer existing Hierarchy binding/layout infrastructure.

# 

# \## Acceptance Criteria

# 

# \- Toggle appears on supported scene GameObjects.

# \- Toggle visually reflects `activeSelf`.

# \- Clicking the toggle changes `activeSelf`.

# \- Clicking the toggle does not unexpectedly change selection.

# \- Clicking an unselected object affects only that object.

# \- Clicking an object inside the current multi-selection affects all eligible selected objects.

# \- Mixed selections resolve to one consistent target state.

# \- Undo restores all affected objects.

# \- Redo reapplies the operation.

# \- Prefab instances remain valid.

# \- Child activeSelf state is represented correctly under inactive parents.

# \- Manual colors continue to render correctly.

# \- Manual icons continue to render correctly.

# \- Separators remain functional.

# \- Zebra striping and hierarchy lines remain functional.

# \- Narrow Hierarchy layouts do not produce overlapping controls.

# \- No package-related Console errors are introduced.

# 

# \## Out of Scope

# 

# \- Keyboard shortcut for activation.

# \- GameObject locking.

# \- Scene visibility toggle.

# \- Runtime controls.

# \- Activation animations.

# \- Settings UI.

# \- Component Minimap.

# \- Validation warnings.

