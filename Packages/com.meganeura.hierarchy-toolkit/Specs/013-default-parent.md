# \# 013 - Default Parent

# 

# Status: PLANNED

# 

# \## Goal

# 

# Allow users to quickly designate one scene GameObject as the Default Parent for newly added scene objects and prefabs.

# 

# The current Default Parent must be immediately visible in the Hierarchy and easy to change or clear.

# 

# \## Behavior

# 

# Hierarchy Toolkit allows one valid GameObject to be marked as the Default Parent.

# 

# Supported newly added objects are automatically parented under that GameObject when doing so is safe and does not override explicit user intent.

# 

# The Default Parent is an Editor-only organizational tool and must not affect runtime behavior.

# 

# \## Requirements

# 

# \### Set / Clear Default Parent

# 

# \- Provide a keyboard shortcut to toggle the hovered GameObject as Default Parent.

# \- Default shortcut: `D`.

# \- The shortcut operates on the valid GameObject row currently under the mouse in the Hierarchy.

# \- If the hovered GameObject is not the current Default Parent, pressing `D` sets it as the new Default Parent.

# \- If the hovered GameObject is already the current Default Parent, pressing `D` clears it.

# \- Setting a new Default Parent replaces the previous one.

# \- Only one Default Parent may be active per supported scene scope.

# \- Also provide a Hierarchy context-menu action to set or clear the Default Parent.

# 

# \### Hover Targeting

# 

# \- The shortcut targets the GameObject row physically under the mouse.

# \- Do not silently fall back to `Selection.activeGameObject`.

# \- If no valid GameObject row is hovered, do nothing.

# \- Scene header rows and unsupported hierarchy rows must fail safely.

# \- Do not trigger while renaming or editing text/search fields.

# 

# \### Visual Indicator

# 

# \- The current Default Parent must be visually identifiable directly in the Hierarchy.

# \- Display a subtle text indicator beside the GameObject name:

# &#x20; `Default parent`

# \- The indicator should follow the visual intent of the attached reference:

# &#x20; - secondary/subtle text

# &#x20; - clearly readable

# &#x20; - does not compete with the GameObject name

# &#x20; - does not interfere with selection, rename, foldouts, icons, drag/reorder, component minimap, or activation toggle

# \- Do not modify the actual GameObject name.

# \- Do not store the label as scene content.

# \- The indicator must update immediately when the Default Parent changes or is cleared.

# 

# \### Valid Parent

# 

# \- Default Parent must be a real scene GameObject.

# \- A visual Separator alone is not a parenting target unless it also corresponds to a real valid GameObject.

# \- Do not create hidden helper GameObjects or components.

# \- Do not add a marker component to the Default Parent.

# 

# \### Scope / Persistence

# 

# \- The Default Parent belongs to its scene.

# \- Use the package's existing scene-metadata architecture where appropriate.

# \- Store the target using a stable object identity consistent with existing package metadata.

# \- The reference must survive hierarchy reordering and normal scene reloads.

# \- If the target no longer exists, fail safely and clear/ignore the stale reference.

# \- Do not incorrectly apply a Default Parent from one scene to objects in another scene.

# 

# \### Automatic Parenting

# 

# \- Supported newly added objects should be parented under the active Default Parent.

# \- This includes prefabs dragged into the scene where compatible with Unity's normal Editor workflow.

# \- Do not unexpectedly reparent pre-existing objects.

# \- Explicit user parenting must take priority over Default Parent behavior.

# \- Dragging/reordering an existing GameObject in the Hierarchy must continue to behave normally.

# \- Do not override an explicit destination parent chosen by the user.

# 

# \### Transform Behavior

# 

# \- Preserve the newly added object's intended world placement.

# \- Automatic parenting must not unexpectedly move, rotate, or scale the object in world space.

# \- Follow Unity's normal prefab/object placement semantics where needed.

# 

# \### Scene Safety

# 

# \- Target and Default Parent must belong to the same valid scene.

# \- Never parent the Default Parent to itself.

# \- Never create cyclic Transform relationships.

# \- Handle scene load, unload, close, and reload safely.

# \- Multi-scene editing must remain conservative.

# 

# \### Prefab Safety

# 

# \- Preserve prefab instance integrity.

# \- Do not create invalid prefab modifications.

# \- Do not interfere with nested prefab structure.

# \- Prefab Stage must fail safely unless normal Unity behavior explicitly supports the operation.

# 

# \### Undo / Redo

# 

# \- Automatic parenting must integrate correctly with Unity Undo/Redo.

# \- Changing or clearing Default Parent metadata should use the package's existing metadata Undo conventions where applicable.

# \- Undoing object creation/parenting must not leave stale Default Parent state.

# 

# \### Performance

# 

# \- Do not poll for new objects every frame.

# \- Use appropriate Unity Editor hierarchy/object change events.

# \- Do not scan the entire scene during repaint.

# \- Do not perform object discovery or Transform traversal in hot Hierarchy rendering paths.

# \- Cache/refresh Default Parent state using the package's normal event-driven patterns.

# 

# \## Acceptance Criteria

# 

# \- Hovering a valid GameObject and pressing `D` sets it as Default Parent.

# \- Pressing `D` again on the active Default Parent clears it.

# \- Pressing `D` on another GameObject replaces the previous Default Parent.

# \- Context-menu set/clear actions work.

# \- The active GameObject shows a subtle `Default parent` indicator beside its name.

# \- The indicator disappears immediately when cleared or replaced.

# \- The GameObject's actual name remains unchanged.

# \- Supported newly created/added objects are parented under the active Default Parent.

# \- Supported prefabs dragged into the scene are parented under it when safe.

# \- Existing objects are not unexpectedly reparented.

# \- Explicit user parenting remains authoritative.

# \- World placement is preserved.

# \- Invalid/deleted Default Parent references fail safely.

# \- Different scenes do not incorrectly share their Default Parent.

# \- Undo/Redo behaves correctly.

# \- Existing Hierarchy Toolkit features remain functional.

# \- No package-related Console errors are introduced.

# 

# \## Out of Scope

# 

# \- Multiple simultaneous Default Parents in the same scene.

# \- Default Parent rules by object/component type.

# \- Automatic grouping presets.

# \- Runtime parenting.

# \- Scene visibility behavior.

# \- Automatic renaming.

# \- Replacing Separators with parenting containers.

# \- Shortcut customization UI.

