# Skill: unity-editor-feature

Use this Skill when implementing a new Unity Editor feature for Hierarchy Toolkit.

## Inputs

- AGENTS.md
- the active feature spec
- existing relevant package files

## Workflow

1. Read AGENTS.md.
2. Read the active spec completely.
3. Identify the smallest existing architecture surface needed for the feature.
4. Inspect only relevant files.
5. Reuse established patterns before introducing new abstractions.
6. Implement only the active spec.
7. Preserve:
   - Undo/Redo
   - multi-selection conventions
   - prefab safety
   - Editor stability
   - Hierarchy performance
8. Add or update targeted EditMode tests.
9. Run the validation harness.
10. Stop when the active spec is complete.

## Rules

- Do not implement future specs.
- Do not redesign working systems unless the active spec requires it.
- Do not add runtime code unless explicitly required.
- Do not modify unrelated project code.
- Prefer existing shared infrastructure over feature-specific duplication.
- Avoid speculative abstractions for features that do not yet exist.

## Output

Report only:

- files created
- files modified
- compilation result
- tests executed/result
- acceptance criteria verified automatically
- manual verification still required
- blockers