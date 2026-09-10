# Project Toolkit Validation Harness

This harness defines the minimum verification for changes under `Packages/com.meganeura.project-toolkit/`.

## Level 1 — Import and Compilation

Always import newly created files and compile through Unity MCP.

The task fails when the current change introduces compilation errors. Do not fix unrelated project errors.

For foundation or assembly changes, verify that runtime assemblies do not reference Project Toolkit.

## Level 2 — Targeted EditMode Tests

Run the smallest test batch covering:

- the active feature;
- changed storage, identity, cache, layout, rules, or settings code used by it;
- the acceptance criteria that can be automated.

Prefer behavioral tests over exact pixel assertions. Batch related tests into one Unity test run when possible.

## Level 3 — Impact-Based Regressions

Run only direct consumers of changed shared infrastructure.

Examples:

- metadata store changed → manual colors, manual icons, and automatic rules using it;
- Project window integration changed → every feature registered through that integration;
- navigation adapter changed → bookmarks, history, and folder shortcuts;
- common visual layout changed → visuals, content minimap, and two-line names.

Changing a shared file alone does not justify the complete suite. Identify actual consumers.

If code shared with Hierarchy Toolkit changes, run the directly affected targeted tests in both packages.

## Level 4 — Full Project Toolkit Suite

The full suite is not part of normal feature implementation. Run it only for:

- an explicit user request;
- a release or milestone validation;
- a dedicated regression pass;
- a high-risk architectural change affecting most features;
- evidence of broad breakage found by targeted tests.

If an unrelated failure appears, rerun it once in isolation, report it as pre-existing when confirmed, and do not investigate it without a separate request.

## Level 5 — Console Validation

After compilation and tests, inspect the Unity Console for new Project Toolkit errors, exceptions, repeated warnings, or log spam. Ignore unrelated project issues.

## Level 6 — Manual Project Window Verification

Manually verify behavior that automated tests cannot prove, including as applicable:

- one-column and two-column Project Browser layouts;
- list and grid/icon-size presentations;
- light and dark Editor themes;
- selection, rename, drag/drop, context menus, keyboard navigation, and native overlays;
- narrow panes, clipping, scrolling, and recycled item state;
- behavior in `Assets/` and visible `Packages/` content;
- coexistence of all implemented Project Toolkit decorations;
- domain reload and Editor restart persistence.

Do not claim manual verification unless it was actually observed.

## Efficiency

Normal validation should use one compilation request, one batched targeted test run, directly justified regression tests if needed, and one final Console inspection. Do not poll repeatedly or rerun successful checks.

## Completion Report

Report only:

- files created and modified;
- compilation result;
- targeted tests and result;
- impact-based regression tests and result, if any;
- full suite: not required or executed;
- Console result;
- acceptance criteria verified automatically;
- manual verification completed or still required;
- blockers or important notes.
