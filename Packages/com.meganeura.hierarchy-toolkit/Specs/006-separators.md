# \# 006 - Hierarchy Separators

# 

# Status: DONE

# 

# \## Goal

# 

# Allow users to organize large scenes into clearly readable visual sections inside the Unity Hierarchy.

# 

# \## Behavior

# 

# A normal scene GameObject can be marked as a Hierarchy Separator.

# 

# The separator row behaves as a visual section header while remaining a real selectable and reorderable GameObject.

# 

# \## Requirements

# 

# \- Separator state is stored in scene metadata.

# \- Do not add components to separator GameObjects.

# \- Separator supports:

# &#x20; - display text override

# &#x20; - optional background color

# &#x20; - optional text color

# &#x20; - bold toggle

# \- Empty display text falls back to GameObject.name.

# \- Provide:

# &#x20; - Mark as Separator

# &#x20; - Edit Separator

# &#x20; - Clear Separator

# \- Edit Separator uses an artist-friendly Odin window.

# \- Mark/Clear support multi-selection.

# \- Edit operates on one object.

# \- Changes support Undo/Redo.

# \- Separator rows remain selectable.

# \- Normal Unity rename behavior must remain intact.

# \- Drag/reorder must remain intact.

# \- Native foldout behavior must remain intact.

# \- Hierarchy lines should not visually cross separator rows.

# \- Manual colors and separator styling must follow a deterministic precedence.

# \- Clearing separator metadata must preserve color and icon customization.

# 

# \## Acceptance Criteria

# 

# \- GameObject can be marked as a separator.

# \- Separator background renders visibly.

# \- Custom display text renders visibly.

# \- Empty display text shows the GameObject name.

# \- Custom text color works.

# \- Bold toggle works.

# \- Edit window reloads existing separator values.

# \- Clear Separator restores normal row appearance.

# \- Undo/Redo works.

# \- Multi-selection Mark/Clear works.

# \- Existing colors and icons remain intact.

# 

# \## Out of Scope

# 

# \- Separator presets.

# \- Decorative textures.

# \- Automatic separator creation.

# \- Runtime behavior.

# \- Full settings UI.

