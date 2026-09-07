# \# 004 - Manual Hierarchy Icons

# 

# Status: DONE

# 

# \## Goal

# 

# Allow users to assign custom icons to GameObjects directly in the Unity Hierarchy.

# 

# \## Behavior

# 

# A GameObject may have an explicit manual icon override.

# 

# The icon is displayed using Unity's hierarchy icon area rather than adding unnecessary visual clutter.

# 

# Users can assign or clear icons through Hierarchy Toolkit UI.

# 

# \## Requirements

# 

# \- Store icon references using stable identifiers.

# \- Project assets must not rely on fragile asset paths.

# \- Moving or renaming an icon asset must preserve the reference where possible.

# \- Support Unity built-in icons.

# \- Support appropriate project Texture2D/Sprite assets.

# \- Provide Set Icon and Clear Icon actions.

# \- Provide an artist-friendly icon picker.

# \- Support multi-selection using the same targeting rules as manual colors.

# \- Support Undo/Redo.

# \- Icon lookup must be cached.

# \- Do not load textures repeatedly during Hierarchy repaint.

# \- Deleted/missing icon assets must fail gracefully.

# \- Clearing an icon must preserve color and separator metadata.

# \- Manual icon rendering must coexist with Unity foldouts and hierarchy lines.

# 

# \## Acceptance Criteria

# 

# \- User can assign an icon through UI.

# \- Assigned icon appears in the Hierarchy.

# \- Clear Icon restores normal icon behavior.

# \- Moving or renaming referenced assets does not break stable references.

# \- Deleted icons do not cause console spam or exceptions.

# \- Multi-selection assignment works.

# \- Undo/Redo works.

# \- Colors continue working when an icon is assigned.

# \- Hierarchy remains readable in narrow windows.

# 

# \## Out of Scope

# 

# \- Automatic icon rules.

# \- Component minimap icons.

# \- Shared icon presets.

# \- Runtime icons.

# \- Animated icons.

