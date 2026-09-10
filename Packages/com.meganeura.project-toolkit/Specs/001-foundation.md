# 001 — Foundation

Status: DONE

## Goal

Create an Editor-only, modular foundation that integrates Project Toolkit features into Unity's Project window without replacing native behavior.

## Behavior

Project Toolkit initializes on Editor load, coordinates independently enableable features through one integration layer, and remains inactive in runtime builds.

## Requirements

- Package root is `Packages/com.meganeura.project-toolkit/`.
- Namespace is `Meganeura.ProjectToolkit`.
- All code is Editor-only.
- Introduce a focused feature contract such as `IProjectFeature` only where it reduces duplicated lifecycle or rendering work.
- Centralize registration and Project window callbacks; features must not install competing callbacks when the coordinator can dispatch them.
- Distinguish folders from ordinary assets before folder-only features run.
- Treat Project window item identifiers and paths as transient lookup inputs, not durable metadata identity.
- Support clean initialization and disposal across domain/assembly reload.
- Preserve native selection, rename, ping, double-click/open, drag/drop, context menus, keyboard navigation, and version-control overlays.
- Keep the package functional with all features disabled.
- Do not introduce a dependency on Hierarchy Toolkit.
- Do not create runtime code or modify imported assets.

## Acceptance Criteria

- Unity compiles and loads the package automatically.
- Multiple test features can register without duplicate invocation.
- Assembly reload does not leave stale callbacks.
- Folder and non-folder Project items are identified correctly in supported views.
- Disabling all features leaves native Project behavior intact.
- Runtime assemblies do not reference Project Toolkit.

## Out of Scope

- Persistent metadata.
- Colors, icons, visual decorations, bookmarks, history, shortcuts, and settings UI.
