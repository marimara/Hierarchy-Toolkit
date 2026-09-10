# 005 — Project Window Visuals

Status: PLANNED

## Goal

Improve Project window readability with subtle, optional visual structure.

## Behavior

Users may independently enable supported hierarchy lines, zebra striping, compact/minimal presentation, and background treatment without changing assets.

## Requirements

- Each visual is independently enableable and defaults to a conservative appearance.
- Work in supported one-column, two-column, list, and grid contexts; skip a context safely when reliable geometry is unavailable.
- Preserve labels, selection, rename fields, folder disclosure controls, drag/drop targets, and native overlays.
- Manual folder colors take precedence over generic row backgrounds.
- Use cached styles, colors, and textures.
- Avoid allocations and AssetDatabase work in hot drawing paths.
- Degrade gracefully in narrow panes and both Editor themes.
- Store appearance choices as project-scoped personal preferences.
- Do not simulate or replace Unity's folder tree.

## Acceptance Criteria

- Each implemented visual can be toggled independently.
- Disabled visuals perform no avoidable drawing work.
- Manual colors remain visible and selection remains readable.
- Native interaction continues to work.
- No visible stale decoration remains on recycled or changed items.

## Out of Scope

- Manual assignments, automatic rules, content minimap, and two-line names.
