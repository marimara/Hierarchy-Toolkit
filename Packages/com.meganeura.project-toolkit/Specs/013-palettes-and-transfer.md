# 013 — Palettes and Transfer

Status: PLANNED

## Goal

Make repeated styling fast and allow intentional reuse of palettes and rules across projects.

## Behavior

Users can manage named colors and icons, apply them quickly, and export/import portable Toolkit configuration.

## Requirements

- Provide searchable, artist-friendly palette management with previews.
- Built-in choices remain distinguishable from user-created entries.
- Imported custom icons resolve through portable asset GUID/path information and report missing dependencies.
- Export contains only Project Toolkit configuration selected by the user; never export project assets implicitly.
- Import validates version and schema before mutation and presents conflicts.
- Import supports Undo or creates a recoverable backup before replacement/merge.
- Do not silently overwrite existing rules, names, or assignments.
- Preserve stable identifiers where valid and remap only through explicit user choices.

## Acceptance Criteria

- Users can create, edit, remove, search, and apply palette entries.
- Export/import round-trips supported configuration.
- Missing icon dependencies and conflicts are reported clearly.
- Cancelled or invalid imports leave current configuration unchanged.
- No third-party icon artwork is bundled without an appropriate license.

## Visual References

- `Documentation/ProjectToolkit/VisualReferences/vFolders2/10-customize-palette.png`
  - Use for: discoverable palette editing, grouped enablement, previews, search, and handling a large icon catalog.
- `Documentation/ProjectToolkit/VisualReferences/vFolders2/02-set-folder-colors.png`
  - Use for: compact application of frequently used colors.
- `Documentation/ProjectToolkit/VisualReferences/vFolders2/03-set-folder-icons.png`
  - Use for: compact application of frequently used icons.

Do not copy the icon library, exact palette, Inspector layout, grouping, branding, or control composition. The reference does not expand import/export behavior beyond this spec.

## Out of Scope

- Online galleries, asset downloads, automatic cross-project syncing, and theme generation.
