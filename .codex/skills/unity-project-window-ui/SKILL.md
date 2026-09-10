---
name: unity-project-window-ui
description: Build or modify visual and interactive behavior inside Unity's Project Browser for Project Toolkit. Use for item drawing, folder controls, navigation, Project window shortcuts, and layout-sensitive behavior.
---

# Unity Project Window UI

Add Project Browser behavior without replacing or destabilizing Unity's native window.

## Native Behavior to Preserve

- selection and multi-selection;
- rename fields;
- single/double-click and asset opening;
- drag/drop and folder drop targets;
- context menus;
- keyboard navigation and search input;
- folder disclosure state;
- native icons, badges, and version-control overlays;
- row/item recycling where used by Unity.

## View Context

Before implementing, identify which contexts the active spec supports:

- one-column folder tree;
- two-column folder tree and asset area;
- list view;
- grid view and changing icon sizes;
- `Assets/` versus visible `Packages/` content.

Skip an unsupported context safely. Do not infer semantics solely from rectangle dimensions if a more reliable context signal exists.

## Hot Path

Project item callbacks may execute for many visible assets. In drawing or binding code, do not perform:

- AssetDatabase searches or folder enumeration;
- GUID/path conversion that could have been cached;
- preference or configuration deserialization;
- reflection;
- LINQ;
- texture/style discovery;
- transient collection or repeated GUI content allocation;
- configuration mutation.

Build cached read models from project/import, Undo/Redo, and settings events. Every cache must declare ownership and invalidation.

## Drawing and Input

- Use one coordinated integration path and deterministic draw order.
- Keep selection and text contrast readable in light and dark themes.
- Clear all state applied to recycled elements.
- Let native controls consume input first unless the active spec defines a precise Toolkit hit area.
- Do not intercept global keys; validate Project Browser focus and text-editing state.
- Isolate non-public Unity integration behind a small adapter with a safe disabled fallback.
- Use Odin for configuration surfaces when useful, never for per-item rendering.

## Validation

Use `unity-project-test` and the manual Project Browser matrix in the package harness. Do not claim visual or interactive verification without observing it in Unity.
