# 009 — Navigation History

Status: PLANNED

## Goal

Provide browser-like Back and Forward navigation for recently visited Project folders.

## Behavior

Folder navigation records a bounded history. Back and Forward restore prior valid folder locations without recording recursive duplicate entries.

## Requirements

- Track stable folder GUIDs and maintain a configurable bounded history.
- Record meaningful user navigation, not selection noise or Toolkit-driven restoration.
- Navigating after going Back clears the obsolete Forward branch.
- Consecutive duplicate locations are collapsed.
- Invalid entries are skipped safely.
- History is project-scoped and user-local.
- Provide compact controls with disabled states and tooltips.
- Reuse the folder reveal/navigation adapter used by bookmarks.
- Use events where available; any unavoidable observation must be low-frequency, bounded, and stop when no Project Browser is relevant.

## Acceptance Criteria

- Back and Forward follow expected browser semantics.
- Toolkit navigation does not create loops or duplicate entries.
- Deleted and moved folders behave safely.
- Controls accurately reflect availability.
- History survives domain reload; Editor-restart persistence is configurable or documented by the implementation.

## Visual References

- `Documentation/ProjectToolkit/VisualReferences/vFolders2/01-navigation-bar.png`
  - Use for: familiar Back/Forward placement, compact disabled states, and coexistence with folder bookmarks.
  - Do not copy: exact controls, toolbar structure, styling, branding, or undocumented navigation semantics.

Browser-style behavior in the written requirements takes precedence over anything inferred from the still image.

## Out of Scope

- Cross-project history, file-open history, and custom tabs.
