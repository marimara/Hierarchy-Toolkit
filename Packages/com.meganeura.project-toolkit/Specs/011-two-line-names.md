# 011 — Two-Line Names

Status: PLANNED

## Goal

Reduce truncation of long asset and folder names in supported grid views.

## Behavior

When enabled and space permits, long labels may use a second line while preserving native interaction and legibility.

## Requirements

- Apply only in Project Browser presentations where a reliable, safe label layout is available.
- Preserve selection, rename editing, drag/drop, double-click, and keyboard behavior.
- Never draw a second label over Unity's active rename field.
- Use deterministic truncation/wrapping and cached GUI content/styles.
- Respect zoom/icon size, pane width, and light/dark themes.
- Clear recycled visual state when an item no longer needs a second line.
- Disable cleanly on unsupported Unity variants rather than relying on brittle assumptions.

## Acceptance Criteria

- Supported long names gain useful readable space.
- Short names and list views are not unnecessarily altered.
- Rename and native interactions remain correct.
- Zoom changes and recycled items do not leave stale labels.
- Disabling restores native presentation immediately.

## Out of Scope

- Renaming assets, custom fonts, arbitrary row-height redesign, and replacing the grid.
