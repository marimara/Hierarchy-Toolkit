# 010 — Folder Shortcuts

Status: PLANNED

## Goal

Speed up common Project folder navigation with opt-in keyboard shortcuts.

## Behavior

When the Project Browser has the relevant focus, users can expand or collapse a folder, isolate a folder branch, and collapse supported branches through shortcuts.

## Requirements

- Default bindings follow the product plan: `E` expand/collapse, `Shift+E` isolate, and `Ctrl+Shift+E` collapse all, subject to conflict validation in Unity 6.
- Execute only with a valid focused Project Browser context and eligible folder target.
- Do not fire while renaming, typing in search, editing text, or another control consumes the key.
- Preserve selection where the action does not inherently navigate.
- Prefer public Unity command/shortcut facilities; isolate any reflected/internal integration behind a replaceable adapter and fail closed.
- Each shortcut can be disabled independently from its underlying feature.
- Avoid global key interception.

## Acceptance Criteria

- Each shortcut performs its defined action in supported contexts.
- Shortcuts do nothing while editing text or outside the Project Browser.
- Isolate and collapse operations do not modify assets.
- Unsupported Unity layouts fail safely.
- Disabling a shortcut affects only that binding.

## Out of Scope

- Arbitrary key rebinding and custom Project Browser tabs.
