---
name: unity-project-feature
description: Implement a feature in the Meganeura Project Toolkit package from its active numbered spec. Use for new Project Toolkit feature work, not Hierarchy Toolkit changes or unspecced product design.
---

# Unity Project Feature

Implement one active Project Toolkit spec without pulling later roadmap work forward.

## Required Context

Read, in order:

1. the project-root `AGENTS.md`;
2. `Packages/com.meganeura.project-toolkit/Specs/README.md`;
3. the complete active spec;
4. `Packages/com.meganeura.project-toolkit/Specs/HARNESS.md`;
5. only the existing package files directly relevant to the feature.

Also use `unity-project-window-ui` when the change draws in or intercepts interaction from the Project Browser. Use `unity-project-test` for its test and validation work.

## Workflow

- Map every acceptance criterion to implementation or an explicitly reported manual check.
- Reuse established Project Toolkit patterns before adding an abstraction.
- Keep the feature independently enableable where practical.
- Keep durable folder identity GUID-based and cached.
- Separate team configuration, personal preferences, and transient caches.
- Preserve native Project Browser behavior and avoid writes during drawing.
- Add or update the smallest reliable EditMode test set.
- Run the package harness and stop when the active spec is complete.

## Boundaries

- Do not implement the next spec.
- Do not make Project Toolkit depend on Hierarchy Toolkit.
- Do not extract a shared package speculatively. Extraction requires two real consumers and must be in scope.
- Do not modify imported assets, folder `.meta` files, or runtime assemblies to store Toolkit state.
- Do not copy source, names, artwork, or icon libraries from third-party Unity assets.

## Completion

Report only the fields required by the Project Toolkit harness, including manual verification still required and blockers.
