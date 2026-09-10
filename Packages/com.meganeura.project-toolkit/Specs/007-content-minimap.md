# 007 — Content Minimap

Status: PLANNED

## Goal

Show a compact summary of the asset types contained in a folder.

## Behavior

Supported folder rows display a small, non-intrusive set of type indicators. Hovering an indicator identifies the represented type.

## Requirements

- Summaries are computed outside repaint and cached by folder GUID.
- Use bounded direct-child inspection initially; recursive scanning is out of scope unless later measured and specified.
- Group assets by useful editor-facing categories rather than every concrete type.
- Define deterministic ordering and a maximum visible indicator count.
- Preserve folder names and native controls when width is limited; reduce or hide the minimap first.
- Missing, importing, or unsupported assets must not throw.
- Asset import, move, delete, and project-change events invalidate affected summaries.
- Avoid reflection, LINQ, temporary collections, and texture lookup in hot paths.

## Acceptance Criteria

- Supported folders show correct direct-content categories.
- Empty folders show no misleading indicators.
- Tooltips identify categories.
- Narrow views do not overlap labels or native controls.
- Asset changes refresh the summary without continuous polling.
- No folder scan occurs per repaint.

## Visual References

- `Documentation/ProjectToolkit/VisualReferences/vFolders2/07-content-minimap.png`
  - Use for: compact right-aligned indicators, restrained density, stable ordering, and preserving the folder label.
  - Do not copy: third-party icon artwork, exact colors, category set, spacing, or number of indicators.

The screenshot shows presentation only. Direct-child scope, categories, caching, overflow, and interaction are governed exclusively by this spec.

## Out of Scope

- Asset counts, recursive analytics, clicking indicators, validation warnings, and popup browsers.
