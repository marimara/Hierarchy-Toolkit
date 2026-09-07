# Hierarchy Toolkit Specs

This directory contains feature specifications for Hierarchy Toolkit.

Package root:

Packages/Hierarchy Toolkit/

Specs define user-visible behavior and feature-specific requirements.

AGENTS.md defines global engineering rules, architecture constraints, performance requirements, testing expectations, and Codex workflow.

If a spec conflicts with AGENTS.md, AGENTS.md wins.

## Status

Each spec should use one of:

- DONE
- IN PROGRESS
- PLANNED

## Required Sections

Each feature spec should contain:

- Goal
- Behavior
- Requirements
- Acceptance Criteria
- Out of Scope

Visual features may also contain:

- Visual Reference
- UX Notes

## Implementation Rules

Codex should implement only the requested spec.

Do not automatically continue to the next numbered spec.

Do not expand scope beyond the specification.

If implementation requires a design decision not covered by the spec, prefer the smallest solution consistent with AGENTS.md.

If a requirement conflicts with the existing architecture, stop and report the conflict instead of silently redesigning the package.

## Definition of Done

A feature is complete when:

1. its acceptance criteria are implemented
2. Unity compilation succeeds
3. relevant targeted EditMode tests pass
4. shared infrastructure tests pass when affected
5. no unrelated project errors are introduced
6. the feature is manually verified when visual behavior cannot be reliably automated

## Current Roadmap

DONE:
001 Foundation
002 Metadata
003 Manual Colors
004 Manual Icons
005 Hierarchy Visuals
006 Separators

PLANNED:
007 Activation Toggle
008 Component Minimap
009 Component Popup Inspector
010 Favorites
011 Navigation
012 Scene Selector
013 Shortcuts
014 Default Parent
015 Automatic Rules
016 Settings
017 Presets
018 Validation
019 Performance and Polish