# 010 - GameObject Bookmarks

Status: PLANNED

## Goal

Allow users to bookmark important scene GameObjects and reveal them instantly from the Unity Hierarchy.

## Behavior

Users can bookmark supported scene GameObjects.

Bookmarked objects are exposed through a compact navigation area integrated into the Hierarchy.

Clicking a bookmark reveals the corresponding GameObject in the Hierarchy without requiring the user to manually search or expand its parent hierarchy.

Bookmarks are personal Editor workflow data.

## Requirements

### Storage

- Bookmarks are user-local preferences.
- Do not store bookmarks in scene metadata.
- Do not modify scene GameObjects.
- Use stable Unity object identity where possible.
- Bookmarks should survive normal Editor/domain reload.
- Missing or deleted objects must fail safely.
- Duplicate bookmarks are not allowed.

### Bookmark Management

- Provide a Hierarchy Toolkit action to add a GameObject bookmark.
- Provide a way to remove an existing bookmark.
- Support adding/removing multiple eligible selected GameObjects when practical.
- Bookmarks should use the GameObject's current name for display/tooltips.

### Navigation Bar

- Integrate bookmark access into the Hierarchy rather than requiring a separate Editor window.
- Keep the UI compact and visually consistent with Unity.
- Avoid excessive clutter when several bookmarks exist.
- A bookmark may be represented by an icon, compact control, menu entry, or equivalent UI suitable for available width.
- Provide a tooltip identifying the bookmarked object.

### Reveal Behavior

Clicking a valid bookmark should:

- locate the corresponding GameObject
- reveal it in the Hierarchy
- expand ancestor objects as necessary
- scroll the Hierarchy to the object
- select or clearly highlight the object
- preserve normal Unity Hierarchy behavior afterward

The user should not need to manually expand the object's parents.

### Scene Handling

- Bookmarks may reference GameObjects in currently loaded scenes.
- If the bookmarked object's scene is not loaded, fail safely.
- Do not automatically open unloaded scenes in this spec.
- Scene loading and scene bookmarks belong to spec 011.

### Performance

- Do not resolve every bookmark every Hierarchy repaint.
- Cache bookmark resolution where useful.
- Refresh using relevant Editor events.
- Avoid unnecessary AssetDatabase work.
- Do not poll continuously.

## Acceptance Criteria

- A supported GameObject can be bookmarked.
- Duplicate bookmarks are prevented.
- A bookmark can be removed.
- Bookmarks survive normal Editor/domain reload.
- Clicking a loaded bookmark reveals the correct GameObject.
- Hidden ancestors are expanded as necessary.
- The Hierarchy scrolls to the bookmarked object.
- The object is selected or clearly highlighted.
- Deleted objects do not throw or spam the Console.
- Existing Hierarchy Toolkit features remain functional.
- No package-related Console errors are introduced.

## Out of Scope

- Scene bookmarks.
- Scene switching.
- Automatically opening unloaded scenes.
- Bookmark folders/groups.
- Shared/team bookmarks.
- Runtime bookmarks.
- Search/filter window.