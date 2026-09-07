# Hierarchy Toolkit

Unity Editor-only internal package for improving the Unity Hierarchy workflow for programmers, designers, and artists.

The goal is not merely to reproduce vHierarchy-style functionality, but to build the best possible internal Hierarchy workflow for our team.

---

## Goals

Provide:

- GameObject colors
- custom icons
- automatic icons
- automatic colors
- hierarchy separators
- hierarchy lines
- zebra striping
- activation toggle
- component minimap
- component popup inspector
- favorites / bookmarks
- scene navigation
- hierarchy shortcuts
- default parent
- hierarchy organization presets
- validation warnings
- configurable rules
- artist-friendly settings
- fast navigation for large scenes

Additional features may be introduced when they clearly improve the team's Unity workflow.

---

## Architecture

Namespace:

Meganeura.HierarchyToolkit

Package root:

Packages/com.meganeura.hierarchy-toolkit/

Preferred structure:

Editor/
  Core/
  Features/
  Data/
  Settings/
  UI/
  Rules/
  Validation/
  Utilities/
  Tests/

Runtime code should NOT be created unless explicitly required.

The package should remain Editor-only whenever possible.

---

## Primary Dependencies

The project always contains:

- Unity Editor API
- Odin Inspector / Odin Serializer
- DOTween Pro

These dependencies are available and may be used.

### Odin

Odin Inspector is an official dependency of this internal package.

Prefer Odin when it meaningfully reduces boilerplate or improves editor UX.

Odin SHOULD be used for:

- settings interfaces
- configuration panels
- serialized rule lists
- reorderable collections
- dropdowns
- search interfaces
- tables
- previews
- validation interfaces
- custom configuration assets
- editor windows
- complex inspector layouts
- artist-facing tooling
- polymorphic serialized configuration when useful

Do NOT reimplement standard editor UI manually when Odin already provides a better and reliable solution.

However, avoid expensive Odin GUI operations directly inside:

EditorApplication.hierarchyWindowItemOnGUI

The Hierarchy drawing loop must remain lightweight.

Heavy configuration UI should live in dedicated Odin EditorWindows, inspectors, popup windows, or configuration assets.

### Odin Serializer

Odin Serializer may be used where Unity serialization is insufficient, especially for editor configuration or polymorphic data.

Do not introduce unnecessarily complex serialization when standard Unity serialization already solves the problem cleanly.

### DOTween

DOTween Pro is available but should only be used when animation meaningfully improves UX.

Do NOT use DOTween for:

- basic Hierarchy drawing
- simple state transitions that do not need animation
- anything that causes continuous unnecessary Editor repainting

Animation must never compromise Editor responsiveness.

---

## Core Principles

### 1. Performance First

The Unity Hierarchy can redraw extremely frequently.

Code executed through:

EditorApplication.hierarchyWindowItemOnGUI

must be extremely cheap.

Avoid:

- LINQ
- reflection per frame
- GetComponents repeatedly
- AssetDatabase searches per draw
- string allocations
- temporary collections
- new GUIContent allocations
- repeated texture lookup
- repeated type discovery
- repeated GlobalObjectId conversion when avoidable

Prefer:

- caches
- dirty flags
- event-driven updates
- reusable GUIContent
- cached GUIStyle
- cached component information
- cached textures
- cached type metadata

Invalidate caches only when relevant Editor state changes.

---

### 2. Event Driven

Do not poll Editor state when an appropriate Unity Editor event exists.

Prefer events such as:

- EditorApplication.hierarchyChanged
- Selection.selectionChanged
- Undo.undoRedoPerformed
- EditorSceneManager.sceneOpened
- EditorSceneManager.sceneClosed
- EditorSceneManager.sceneSaved
- EditorApplication.projectChanged

Only use EditorApplication.update when there is no cleaner alternative.

---

### 3. Modular Features

Features must be independently enableable.

Prefer a modular feature architecture such as:

IHierarchyFeature

Each feature should own its:

- settings
- drawing
- interaction
- cache logic

when practical.

Avoid giant manager classes.

The central hierarchy system should coordinate features rather than implement all feature logic itself.

---

### 4. No Scene Pollution

Never add MonoBehaviours, hidden GameObjects, or helper components to scenes merely to store Hierarchy Toolkit metadata.

Hierarchy metadata must live outside scene object components.

Use stable Unity object identification where necessary.

Prefer GlobalObjectId or another appropriate stable identifier instead of relying exclusively on InstanceID.

---

### 5. Undo Support

Every user action that changes:

- scene state
- GameObjects
- parent relationships
- activation state
- project assets
- serialized configuration

must support Unity Undo whenever applicable.

Use appropriate Unity APIs such as:

Undo.RecordObject
Undo.RegisterCompleteObjectUndo
Undo.SetTransformParent
Undo.RegisterCreatedObjectUndo

Do not implement editor actions that cannot reasonably be undone when Unity supports Undo for that operation.

---

### 6. Multi-Selection

Where sensible, Hierarchy actions should support multiple selected GameObjects.

Examples:

- assign color
- assign icon
- toggle active state
- clear metadata
- apply presets
- validation actions

Do not assume only one GameObject is selected unless the feature inherently requires one.

---

### 7. Prefab Safety

Features must behave correctly with:

- prefab assets
- prefab instances
- nested prefabs
- prefab overrides
- Prefab Mode

Do not accidentally modify prefab assets when the user intends to modify scene instances.

Use Unity prefab APIs where appropriate.

---

### 8. Artist-Friendly UX

The package is intended for programmers AND artists.

Prefer:

- visual controls
- icons
- tooltips
- context menus
- searchable lists
- presets
- sensible defaults
- clear terminology

Avoid requiring artists to manually edit configuration files or code.

Advanced options may exist but should not clutter the normal workflow.

---

### 9. Minimal Clicks

Workflow speed is a primary design goal.

Whenever practical:

- actions should work directly from the Hierarchy
- avoid forcing object selection before an action
- avoid unnecessary confirmation dialogs
- provide contextual controls
- use shortcuts for frequent operations
- preserve current selection unless changing it is part of the action

---

## Data Storage

Separate data into appropriate scopes.

### Shared Project Data

Examples:

- color definitions
- icon presets
- automatic rules
- team presets
- shared validation settings
- project-wide behavior

Store in project assets suitable for source control.

### Scene/Object Metadata

Examples:

- manually assigned GameObject color
- manually assigned GameObject icon
- separator configuration

Store using stable object identification.

Metadata must survive normal Editor reloads.

Avoid designs that generate unnecessary Git conflicts.

### Personal User Preferences

Examples:

- zebra striping enabled
- icon size
- visual density
- personal bookmarks
- shortcut preferences
- popup behavior

Prefer EditorPrefs or another appropriate user-local mechanism when settings should not be committed to source control.

---

## Automatic Rules

The architecture should support automatic rules.

Examples:

- Component type → icon
- Component type → color
- GameObject name → icon
- GameObject name → color
- Layer → visual style
- Tag → visual style
- validation condition → warning icon

Manual overrides should take precedence over automatic rules unless a feature explicitly defines otherwise.

Rule evaluation must be cached.

Do not evaluate expensive rules every Hierarchy repaint.

---

## Component Minimap

Component Minimap must be designed with performance in mind.

Do not call GetComponents every repaint.

Cache component information and invalidate it when necessary.

Component icons should:

- provide tooltips
- support configurable visibility
- remain visually compact
- avoid hiding the GameObject name
- behave predictably on narrow Hierarchy windows

Custom MonoBehaviour icons should be handled consistently.

---

## Component Popup Inspector

Popup component inspectors may use:

Editor.CreateEditor

and standard/Odin inspector rendering where appropriate.

Popup inspectors must:

- properly dispose temporary Editor instances
- support Undo
- avoid leaking references
- avoid creating persistent Editor objects unnecessarily

---

## Validation System

Validation warnings should be extensible.

Potential validations include:

- Missing Script
- missing serialized reference
- missing material
- invalid prefab state
- disabled renderer
- unexpected object configuration

Validation must NOT scan the entire project every Hierarchy repaint.

Use cached results and explicit/event-driven refresh.

Warnings should provide useful tooltips explaining the problem.

When appropriate, clicking a warning may provide a way to locate or fix the issue.

---

## Settings

Use Odin for the main settings experience.

Settings should be divided logically, for example:

General
Appearance
Colors
Icons
Component Minimap
Navigation
Shortcuts
Automatic Rules
Validation
Advanced

Features must be independently enableable.

Prefer clear defaults that require little or no initial setup.

---

## UI Design

The Hierarchy must remain visually readable.

Avoid excessive visual noise.

Features should cooperate when drawing into the same row.

Create a centralized layout system for right-side controls rather than allowing features to independently overlap each other.

Reserve drawing regions where practical.

Example conceptual row:

[Hierarchy indentation] [Icon] GameObject Name [Warnings] [Components] [Active]

Feature rendering should degrade gracefully when the Hierarchy window becomes narrow.

---

## Caching

Prefer explicit caches for expensive data.

Potential caches:

- GUIContent
- GUIStyle
- textures/icons
- component arrays
- component icon mappings
- reflection metadata
- rule evaluation
- validation results
- GlobalObjectId mappings

Caches must have clear invalidation rules.

Do not create caches that silently become stale indefinitely.

---

## Logging

Avoid console spam.

Do not log routine Editor operations.

Errors should explain:

- what failed
- which object or feature was involved
- whether user action is required

Debug logging must be optional.

---

## Compatibility

Target:

Unity 6.x

Primary development environment is the Unity version currently used by the project.

Do not add compatibility code for old Unity versions unless explicitly requested.

Prefer modern Unity 6 APIs when appropriate.

---

## Coding Style

Prefer:

- focused classes
- meaningful names
- small responsibilities
- explicit ownership of caches
- clear feature boundaries

One feature per class where practical.

Prefer:

IHierarchyFeature

when appropriate.

Avoid:

- giant managers
- global mutable state without justification
- unnecessary static state
- duplicated GUI logic
- duplicated caching systems

Public APIs should have XML documentation.

Internal implementation does not require excessive documentation when the code is self-explanatory.

---

## Testing

Prefer EditMode tests.

Tests should focus on logic that can reliably be automated.

Examples:

- metadata storage
- rule precedence
- cache invalidation
- settings serialization
- multi-selection operations
- validation rules

Do not create fragile tests that depend heavily on exact IMGUI pixel output.

---

## MCP / Codex Rules

Never edit MCP for Unity source.

Never modify unrelated project code.

Never make broad project-wide changes unless explicitly requested.

When implementing a task:

1. Read AGENTS.md.
2. Inspect only files relevant to the requested task.
3. Reuse existing architecture.
4. Do not redesign working systems unless required.
5. Implement only the requested feature or step.
6. Compile.
7. Fix errors caused by the current task.
8. Run relevant targeted tests when they exist.
9. Stop when the requested task is complete.

Do not continue implementing future roadmap items without being asked.

---

## Codex Token Efficiency

Avoid exploring unrelated project directories.

For Hierarchy Toolkit tasks, prioritize files inside:

Packages/com.meganeura.hierarchy-toolkit/

Only inspect external files when required to understand an explicit integration.

Do not repeatedly reread unchanged files unless necessary.

Do not provide long implementation explanations after completing tasks.

Final reports should normally contain only:

- files created
- files modified
- compilation result
- tests executed
- blockers or important implementation notes

---

## Repository Exploration Limits

Do not inspect or search these directories unless explicitly required:

- Library/
- Temp/
- Logs/
- obj/
- UserSettings/
- Packages unrelated to Hierarchy Toolkit

Do not perform broad repository-wide searches when the relevant package path is already known.

For normal C# creation/editing, use direct file operations when available.
Use Unity MCP primarily for:
- compilation
- console errors
- EditMode tests
- Unity-specific validation
- asset/database operations that require the Editor

---

## Important

Before implementing any feature, prioritize in this order:

1. correctness
2. Editor stability
3. performance
4. artist usability
5. maintainability
6. visual polish

Do not sacrifice Hierarchy responsiveness for cosmetic functionality.

When multiple implementation approaches are valid, prefer the one that produces the best long-term internal tool rather than the one with the fewest lines of code.