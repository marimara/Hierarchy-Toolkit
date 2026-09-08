# 015 - Feature Menu and Toggles

Status: PLANNED

## Goal

Provide a central Unity Editor menu for enabling and disabling Hierarchy Toolkit features and shortcuts.

The menu should make the current package capabilities easy to discover and individually configurable.

## Behavior

Add a Unity Editor menu at:

Tools > Hierarchy Toolkit

The menu exposes checkable feature and shortcut entries.

Toggling an entry immediately enables or disables that behavior without deleting its stored data.

## Feature Menu

Provide checkable entries for currently implemented user-facing features:

- Quick Style Palette
- Scene Selector
- Component Minimap
- Component Popup Inspector
- Activation Toggle
- Hierarchy Lines
- Zebra Striping
- Manual Colors
- Manual Icons
- Separators
- GameObject Bookmarks
- Default Parent

Only expose features that actually exist in the package.

Do not add placeholders for future specs.

## Shortcut Menu

Provide a separate Shortcuts section with checkable entries for currently implemented Hierarchy Toolkit shortcuts:

- `D` - Toggle Default Parent
- `E` - Expand / Collapse Hovered
- `Shift+E` - Isolate Hovered Branch

Disabling a shortcut prevents only that shortcut from executing.

It must not disable the underlying feature.

Example:

Disabling the `D` shortcut must not disable Default Parent context-menu functionality.

## Toggle Semantics

Disabling a feature:

- stops its Hierarchy rendering and/or interaction
- does not delete metadata
- does not clear colors, icons, bookmarks, presets, or Default Parent state
- does not alter scene objects

Re-enabling the feature restores the existing stored state immediately.

## Master Toggle

Provide:

`Disable Hierarchy Toolkit`

This acts as a master enable/disable control.

When globally disabled:

- Hierarchy Toolkit visual decorations are not rendered
- Hierarchy Toolkit shortcuts do not execute
- custom Hierarchy interactions are disabled
- package data remains untouched

Re-enabling restores the user's previous individual feature settings.

The master toggle must not overwrite individual feature preferences.

## Persistence

Feature and shortcut enabled states are user/editor preferences.

They should:

- persist across Unity restarts
- be scoped appropriately to the project
- not dirty scenes
- not create scene metadata
- not modify GameObjects
- not require source-control changes for another developer's personal preferences

Use an appropriate Editor preference/configuration mechanism consistent with the package architecture.

## Default State

Existing features and shortcuts are enabled by default.

Introducing this spec must preserve the package's current behavior unless the user explicitly disables something.

## Menu State

Unity menu checkmarks must accurately reflect current enabled states.

Checkmarks must update immediately when a setting changes.

Changing a setting through another future Settings UI must also remain synchronized with these menu entries.

## Architecture

Do not scatter independent EditorPrefs checks throughout every hot rendering method.

Introduce a small centralized feature-preference/settings layer.

Existing feature classes should consume the centralized enabled state through appropriate properties/events.

Where a feature already has an `Enabled` concept, reuse it instead of creating parallel state.

Feature changes should trigger only the repaint/cache invalidation required by that feature.

## Interaction Safety

Disabling one feature must not unintentionally disable unrelated features.

Examples:

- disabling Hierarchy Lines must not disable Zebra Striping
- disabling Manual Colors must not delete colors
- disabling Component Minimap must not affect Activation Toggle layout incorrectly
- disabling Quick Style Palette must not disable Manual Color/Icon context menus
- disabling Default Parent must not delete the configured Default Parent

## Performance

- no preference lookup on every repaint where a cached value is sufficient
- no AssetDatabase work in Hierarchy repaint
- settings changes should be event-driven
- disabling a feature should avoid its unnecessary hot-path work where practical

## Acceptance Criteria

- `Tools > Hierarchy Toolkit` exists.
- Implemented features appear as checkable menu entries.
- Implemented shortcuts appear in a separate checkable section.
- Toggling a feature immediately changes its behavior.
- Toggling a shortcut affects only that shortcut.
- Disabled features retain their existing stored data.
- Re-enabling restores previous visual/data state.
- Preferences survive Unity restart.
- Preferences do not dirty scenes.
- Master disable turns off Hierarchy Toolkit behavior without deleting settings.
- Master re-enable restores previous individual states.
- Menu checkmarks stay synchronized with preference state.
- Existing Hierarchy Toolkit features continue working when enabled.
- No package-related Console errors are introduced.

## Out of Scope

- Shortcut key rebinding.
- Automatic Rules.
- Presets.
- Import/export settings.
- Runtime configuration.
- Per-scene enable/disable profiles.
- Per-user visual themes.