# 008 — Folder Bookmarks

Status: PLANNED

## Goal

Let users bookmark important project folders and navigate to them quickly.

## Behavior

Bookmarks appear in a compact Project Toolkit navigation surface. Activating one reveals and selects its folder in the appropriate Project Browser context.

## Requirements

- Bookmarks are project-scoped personal preferences, not shared team metadata.
- Store folder GUIDs; resolve paths lazily outside repaint.
- Prevent duplicates and handle moved, renamed, deleted, hidden, or package folders safely.
- Provide add, remove, reorder, and clear-invalid workflows without requiring code.
- Preserve the user's current selection until a bookmark is activated.
- Avoid creating a permanent custom Project Browser replacement.
- Do not resolve all bookmarks every repaint or poll continuously.

## Acceptance Criteria

- Users can add, remove, and reorder folder bookmarks.
- Bookmarks survive reload and Editor restart.
- Rename and move preserve bookmarks.
- Activating a valid bookmark reveals the correct folder.
- Invalid bookmarks are clearly represented and removable without Console spam.
- Bookmarks do not create source-control changes for other users.

## Visual References

- `Documentation/ProjectToolkit/VisualReferences/vFolders2/01-navigation-bar.png`
  - Use for: compact bookmark affordances integrated near Project Browser navigation and recognizable folder labels.
  - Do not copy: exact toolbar injection, icons, spacing, branding, or history behavior.

This reference does not require a particular toolbar implementation when Unity 6 cannot support it safely.

## Out of Scope

- Shared bookmark sets, bookmark groups, asset bookmarks, and navigation history.
