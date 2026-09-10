# 012 — Automatic Rules and API

Status: PLANNED

## Goal

Let teams define deterministic automatic styling rules and let editor tooling query or assign Project Toolkit styling through a small public API.

## Behavior

Rules may match folder name, GUID/path scope, or cached content characteristics and apply icon/color styling when no higher-priority manual assignment exists.

## Requirements

- Precedence is manual assignment, ordered user rule, built-in inference, then default.
- Rule order is explicit, stable, and artist-editable.
- Matchers and results are serializable shared project data.
- Rule evaluation is compiled/cached outside repaint and invalidated by relevant configuration or project changes.
- Avoid regex unless explicitly selected; invalid patterns fail visibly without breaking other rules.
- Provide preview/test feedback in the rule editor.
- Public API supports setting, clearing, and resolving folder styling by GUID or validated asset path.
- Public API has XML documentation and does not expose internal rendering types.
- API operations trigger the same persistence, Undo, and invalidation paths as UI actions.

## Acceptance Criteria

- Ordered rules produce deterministic results.
- Manual assignments override rules.
- Invalid rules do not throw during Project drawing.
- Editing rules updates affected folders without restart.
- Public API and UI produce equivalent stored state.
- No rule parsing or full-rule scan occurs per repaint.

## Out of Scope

- Runtime API, arbitrary user code execution, cloud rule sharing, and asset relocation.
