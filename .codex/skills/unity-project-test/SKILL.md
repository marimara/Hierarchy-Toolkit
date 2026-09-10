---
name: unity-project-test
description: Create and run targeted Unity EditMode tests for Meganeura Project Toolkit using its validation harness. Use for Project Toolkit test work and validation, not broad project regression runs.
---

# Unity Project Test

Follow `Packages/com.meganeura.project-toolkit/Specs/HARNESS.md`.

## Test Priorities

Test behavior and state rather than exact pixels. Cover the relevant subset of:

- GUID identity across folder rename and move;
- persistence and orphan handling;
- manual-versus-automatic precedence;
- deterministic rule ordering;
- Undo/Redo and multi-selection filtering;
- cache population and event-driven invalidation;
- content category calculation;
- bookmark/history semantics;
- enabled-state behavior;
- layout calculations and supported-context classification;
- safe fallback when Unity integration is unavailable.

## Isolation

- Create temporary assets only under a dedicated test folder.
- Resolve and validate that exact test path before cleanup.
- Clean up created assets and preferences even when a test fails.
- Do not alter existing project folders, package metadata, or gameplay assets.
- Do not depend on test order, current Project Browser selection, or a particular skin.
- Use tests for state/layout decisions and reserve visual correctness for manual verification.

## Execution

- Import new files and compile through Unity MCP.
- Batch the active feature's targeted EditMode tests where possible.
- Run direct regressions only when shared infrastructure changed.
- Run the complete Project Toolkit suite only when harness Level 4 applies.
- If code shared with Hierarchy Toolkit changed, run only identified direct dependents in both packages.
- Inspect the Console once after validation and do not rerun successful checks.

Report the exact test scope and result, full-suite decision, Console result, automated acceptance coverage, manual gaps, and blockers.
