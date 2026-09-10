# 006 — Automatic Folder Icons

Status: PLANNED

## Goal

Assign useful folder icons automatically from folder name, location, or contents while keeping manual choice authoritative.

## Behavior

Folders without a manual icon may receive a cached automatic icon selected by deterministic built-in inference.

## Requirements

- Precedence is manual icon, explicit automatic rule, built-in inference, then native icon.
- Begin with a small, original, configurable mapping for common Unity folder roles.
- Content inference must be bounded, cached, and refreshed from relevant project/import changes.
- Never enumerate folder contents during item repaint.
- Define deterministic handling for mixed-content and empty folders.
- Allow users to disable automatic inference without deleting manual assignments.
- Package and special folders that cannot be inspected must fall back safely.
- Reuse the icon resolution and rendering path from manual icons.

## Acceptance Criteria

- Common supported folders receive the expected inferred icon.
- Manual icons always win.
- Empty, mixed, moved, and deleted folders behave deterministically.
- Relevant asset changes invalidate only necessary cached results where practical.
- No repeated folder enumeration or AssetDatabase search occurs during repaint.

## Out of Scope

- General user rule editor and public API, delivered by spec 012.
- External icon downloads.
