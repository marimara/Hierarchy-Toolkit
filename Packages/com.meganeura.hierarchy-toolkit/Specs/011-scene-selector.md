# \# 011 - Scene Selector

# 

# Status: PLANNED

# 

# \## Goal

# 

# Allow users to switch between project scenes quickly from the Unity Hierarchy and bookmark frequently used scenes for faster access.

# 

# \## Behavior

# 

# The Hierarchy displays a compact scene selector associated with the current scene context.

# 

# Clicking the selector opens a searchable or compact list of available project scenes.

# 

# Users may mark frequently used scenes as favorites.

# 

# Favorite scenes appear in a prioritized section of the selector.

# 

# \## Requirements

# 

# \### Scene discovery

# 

# \- Discover valid Unity scene assets from the project.

# \- Do not perform AssetDatabase scene searches during Hierarchy repaint.

# \- Cache scene discovery results.

# \- Refresh scene cache when relevant project changes occur.

# \- Ignore invalid/missing scene assets gracefully.

# \- Display scene names clearly.

# \- Disambiguate duplicate scene names when necessary using path or folder context.

# 

# \### Scene switching

# 

# \- Selecting a scene loads that scene in the Editor.

# \- For a normal single-scene workflow, opening another scene should follow standard Unity scene switching behavior.

# \- If the current scene has unsaved changes, respect Unity's normal save/discard/cancel workflow.

# \- Do not silently discard unsaved scene changes.

# \- Do not alter build settings automatically.

# \- Do not modify scene assets as part of navigation.

# 

# \### Scene bookmarks

# 

# \- Scene favorites are personal Editor preferences.

# \- Do not store scene bookmarks in scene metadata.

# \- Use stable scene asset identity, preferably GUID-based.

# \- Favorite scenes appear before non-favorites in the selector.

# \- Provide a clear way to add/remove a scene favorite.

# \- Adding the same scene twice must not create duplicates.

# \- Favorites must survive normal Editor/domain reload.

# 

# \### UI

# 

# \- Integrate the selector into the Hierarchy UI.

# \- Keep it compact and visually consistent with Unity.

# \- Show the current scene name.

# \- Favorite scenes should have a visible favorite/star state.

# \- Non-favorite scenes remain accessible.

# \- Provide a tooltip where useful.

# \- Degrade gracefully when the Hierarchy width is narrow.

# 

# \### Multi-scene behavior

# 

# \- Support currently loaded additive scenes safely.

# \- Do not unexpectedly close unrelated additive scenes unless required by standard Unity scene-opening behavior.

# \- If exact replacement behavior is ambiguous in the existing architecture, prefer standard Unity Editor behavior and document the limitation.

# \- Do not introduce custom multi-scene orchestration beyond what this spec requires.

# 

# \### Performance

# 

# \- No AssetDatabase scene enumeration during row repaint.

# \- No polling for scene list changes.

# \- Use project/scene events for cache invalidation.

# \- Avoid repeated allocations in frequently-called Hierarchy UI paths.

# 

# \## Acceptance Criteria

# 

# \- Current scene name is visible through the selector UI.

# \- Clicking the selector shows available project scenes.

# \- Selecting a scene opens the expected scene.

# \- Unsaved scene changes invoke Unity's normal save/discard/cancel behavior.

# \- A scene can be added to favorites.

# \- A scene can be removed from favorites.

# \- Duplicate scene favorites are prevented.

# \- Favorites survive normal Editor/domain reload.

# \- Favorite scenes are visually prioritized.

# \- Deleted/moved scene assets fail safely and refresh correctly.

# \- Existing GameObject bookmarks remain functional.

# \- Existing Hierarchy Toolkit features remain functional.

# \- No package-related Console errors are introduced.

# 

# \## Out of Scope

# 

# \- Modifying Build Settings.

# \- Runtime scene loading.

# \- Scene templates.

# \- Scene creation/deletion.

# \- Scene grouping/folders inside the selector.

# \- Automatic scene opening based on bookmarks.

# \- Workspace/layout presets.

# \- Keyboard shortcuts for scene switching.

