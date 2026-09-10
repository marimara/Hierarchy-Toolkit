# 015 — Performance and Compatibility Hardening

Status: PLANNED

## Goal

Validate the completed feature set under realistic large-project usage and isolate fragile Unity Editor integrations.

## Behavior

Project Toolkit remains responsive, visually coherent, and safely degradable across the supported Unity 6 Project Browser presentations.

## Requirements

- Profile hot drawing/binding paths with representative folder and asset counts.
- Establish allocation and timing baselines before optimizing.
- Confirm no per-item AssetDatabase searches, content scans, reflection, preference reads, or avoidable allocations remain.
- Document every use of non-public Unity API behind focused adapters with version guards and a safe disabled fallback.
- Exercise cache invalidation for import, create, delete, move, rename, domain reload, Undo/Redo, settings changes, and package visibility changes.
- Validate interaction and visuals in the manual matrix from `HARNESS.md`.
- Consolidate only proven duplicate infrastructure; do not merge Hierarchy and Project feature coordinators.
- Remove temporary diagnostics and avoid routine logging.

## Acceptance Criteria

- Measured hot paths show no obvious regression against the disabled baseline.
- Repeated redraw does not allocate avoidable per-item collections or perform asset scans.
- All implemented features coexist in supported views.
- Unsupported integrations fail closed without breaking the Project window.
- Targeted and justified regression tests pass.
- Manual compatibility matrix results and remaining limitations are recorded.

## Out of Scope

- New user-facing features, support for pre-Unity-6 versions, and redesign of Unity's Project Browser.
