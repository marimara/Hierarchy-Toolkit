# Project Toolkit Specs

This directory defines the behavior and implementation order for Project Toolkit.

Package root: `Packages/com.meganeura.project-toolkit/`

Namespace: `Meganeura.ProjectToolkit`

`AGENTS.md` supplies project-wide engineering constraints. The active spec is the source of truth for feature behavior. If they conflict, `AGENTS.md` wins and implementation must stop for review.

## Status

Every spec uses one status:

- `DONE`
- `IN PROGRESS`
- `PLANNED`

## Implementation Rules

- Implement only the explicitly requested spec.
- Read the complete active spec before editing code.
- Do not automatically continue to the next numbered spec.
- Do not add future-spec behavior as scaffolding.
- Prefer the smallest design compatible with later roadmap items.
- Reuse a Hierarchy Toolkit implementation only when its abstraction is genuinely independent of Hierarchy objects and rendering.
- Do not make either feature package depend directly on the other.
- Extract shared code into a separate shared package only after at least two real consumers exist and the extraction is part of the active task.
- Do not copy source code, artwork, icon libraries, names, or other protected assets from third-party Unity packages.

## Visual References

Development-only references live in `Documentation/ProjectToolkit/VisualReferences/`, outside the Unity package. Read its `README.md` before using them.

- A spec must name every image relevant to it and state which visual or interaction qualities may inform the implementation.
- Images are supporting evidence, not additional requirements and not permission to implement visible features outside the active spec.
- The written spec wins whenever a screenshot is ambiguous or shows functionality outside scope.
- Third-party branding, artwork, icons, palette values, text, and exact visual composition must not be copied.
- Prompts should load only the images linked by the active spec, not the complete reference library.

## Roadmap

| Spec | Feature | Depends on | Status |
|---|---|---|---|
| 001 | Foundation | — | DONE |
| 002 | Folder metadata | 001 | DONE |
| 003 | Manual folder colors | 002 | IN PROGRESS |
| 004 | Manual folder icons | 002 | PLANNED |
| 005 | Project window visuals | 001 | PLANNED |
| 006 | Automatic folder icons | 002, 004 | PLANNED |
| 007 | Content minimap | 001 | PLANNED |
| 008 | Folder bookmarks | 001 | PLANNED |
| 009 | Navigation history | 008 | PLANNED |
| 010 | Folder shortcuts | 001 | PLANNED |
| 011 | Two-line names | 001 | PLANNED |
| 012 | Automatic rules and API | 002–007 | PLANNED |
| 013 | Palettes and transfer | 003, 004, 012 | PLANNED |
| 014 | Feature settings and toggles | implemented features | PLANNED |
| 015 | Performance and compatibility hardening | all implemented features | PLANNED |

The numbering is the recommended implementation order, not permission to continue automatically.

## Definition of Done

A spec is complete only when its acceptance criteria pass, Unity compiles, targeted EditMode tests pass, justified direct regressions pass, the Console has no new package-related errors, and any remaining manual visual checks are reported truthfully.

Follow `HARNESS.md` for validation.
