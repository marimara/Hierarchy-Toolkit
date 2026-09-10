---
name: unity-project-debug
description: Diagnose an existing Meganeura Project Toolkit bug and apply the smallest in-scope fix. Use for broken Project Browser visuals, navigation, metadata, caching, rules, persistence, or Editor interactions.
---

# Unity Project Debug

Find the root cause before changing behavior.

## Required Context

Read `AGENTS.md`, the implemented feature's spec, the package harness, and only the relevant execution/data path.

## Investigation

Separate confirmed observations from hypotheses. Check as applicable:

- feature registration and duplicate callbacks;
- one-column, two-column, list, or grid context detection;
- item identifier, GUID, and current-path resolution;
- cache population and each invalidation event;
- draw order, clipping, selection overlay, and recycled state;
- focus, rename/search editing, event consumption, and hit regions;
- persistence scope and Undo/Redo;
- asset import, delete, move, and package visibility changes;
- non-public Unity adapter availability for the active Editor version.

Use targeted tests, package-related Console entries, and temporary diagnostics only when they distinguish hypotheses. Remove temporary diagnostics before completion unless intentionally gated by a debug setting.

## Fix Boundary

- Apply the smallest correction that satisfies the active spec.
- Do not redesign unrelated systems or mask the symptom by disabling safeguards.
- Do not introduce polling or broad logging to compensate for missing invalidation.
- Fix only failures caused by or inside the requested Project Toolkit scope.

After the fix, use `unity-project-test` and report root cause, changed files, validation results, remaining manual checks, and blockers.
