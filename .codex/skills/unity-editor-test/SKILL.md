---
name: unity-editor-test
description: Create and validate targeted EditMode tests for Hierarchy Toolkit features, following the project validation harness.
---

# Skill: unity-editor-test

Use this Skill when adding or extending tests for Hierarchy Toolkit.

## Test Type

Prefer EditMode tests. Test behavior and state rather than exact rendered pixels.

## Priorities

When relevant, cover:

- metadata persistence
- Undo/Redo
- multi-selection
- cache invalidation
- rule precedence
- prefab safety
- stable object identity
- supported/unsupported object filtering
- scene reload behavior
- shared infrastructure regression

## UI Features

For visual features, test:

- data/state used by rendering
- layout calculations
- precedence rules
- hierarchy relationship calculations
- cache behavior

Do not create tests that depend on exact pixel output, absolute screen coordinates, Unity skin visuals, or window size unless behavior specifically depends on it.

Manual visual verification may still be required. Do not claim it was completed unless actually observed.

## Isolation

Tests must:

- clean up created scenes/assets
- avoid modifying unrelated project assets
- avoid depending on project-specific gameplay systems
- be deterministic
- not rely on test execution order

## Harness

Follow AGENTS.md and `Packages/com.meganeura.hierarchy-toolkit/Specs/HARNESS.md` (paths relative to the project root).

- Compile through Unity MCP before running tests; import newly created files before requesting compilation.
- Run targeted EditMode tests for the active feature and changed shared utilities, following Level 2.
- When shared infrastructure changes, run only directly impacted regression tests under Level 3. A shared-file change alone does not justify the full suite.
- Run the full Hierarchy Toolkit suite only when Level 4 applies.
- Inspect package-related Console errors, exceptions, and repeated warnings after validation.
- Report required manual verification and remaining blockers.

## Efficient Validation

- Group related implementation and test edits, then review them before compilation and validation.
- After a correction, recompile changed code and rerun only tests affected by that correction or failure. Preserve valid results for unchanged behavior.
- Do not repeat successful checks or reread unchanged files without a concrete reason.
- Efficiency does not waive acceptance criteria or required harness checks.

## Output

Report concisely:

- tests created/modified
- compilation result
- targeted tests executed/result
- impacted regression tests executed/result, if any
- full suite: not required / executed
- Console result
- acceptance criteria verified automatically
- coverage gaps requiring manual verification
- blockers
