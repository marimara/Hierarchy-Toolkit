# 014 — Feature Settings and Toggles

Status: PLANNED

## Goal

Provide one discoverable control surface for enabling, disabling, and configuring implemented Project Toolkit features.

## Behavior

`Tools > Project Toolkit` exposes a master toggle and checkable entries for features and shortcuts that actually exist. A focused settings UI exposes their options.

## Requirements

- Do not show placeholders for unimplemented roadmap items.
- Disabling a feature stops its drawing/interaction work without deleting metadata or preferences.
- Re-enabling restores previous state.
- Master disable preserves individual choices.
- Feature states are project-scoped personal preferences unless a setting is explicitly team-owned.
- Centralize cached enabled state and change notifications; do not scatter preference reads through repaint code.
- Menu checkmarks and settings UI remain synchronized.
- Use Odin for the settings experience when it materially improves search, tables, previews, or rule editing, but not in Project item rendering.
- Clearly distinguish personal settings from source-controlled team configuration.

## Acceptance Criteria

- The menu and settings UI expose all and only implemented features.
- Toggles update behavior immediately and survive restart.
- Disabling does not delete colors, icons, rules, bookmarks, or history.
- Master re-enable restores prior individual choices.
- Disabled features avoid unnecessary hot-path work.
- Settings do not dirty scenes or modify folder assets.

## Out of Scope

- Shortcut rebinding, user accounts, cloud sync, and runtime settings.
