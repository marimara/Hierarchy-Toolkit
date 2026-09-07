# Hierarchy Toolkit

Unity Editor-only internal package for improving the Unity Hierarchy workflow for programmers, designers, and artists.

Package root:

Packages/com.meganeura.hierarchy-toolkit/

Namespace:

Meganeura.HierarchyToolkit

Feature behavior and roadmap are defined in:

Packages/com.meganeura.hierarchy-toolkit/Specs/

The goal is to build a fast, maintainable, artist-friendly internal Hierarchy workflow without replacing or destabilizing Unity's native editor behavior.

---

## Architecture

Preferred package structure:

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

Specs/

Runtime code should NOT be created unless explicitly required.

The package should remain Editor-only whenever possible.

Prefer modular features based on:

IHierarchyFeature

The central hierarchy system should coordinate features rather than implement all feature logic itself.

Avoid giant manager classes.

Features should be independently enableable whenever practical.

---

## Dependencies

The project always contains:

- Unity Editor API
- Odin Inspector
- Odin Serializer
- DOTween Pro

### Odin

Odin Inspector is an official dependency of this internal package.

Use Odin when it meaningfully reduces boilerplate or improves editor UX.

Prefer Odin for:

- settings windows
- configuration panels
- rule editors
- searchable lists
- tables
- previews
- inspectors
- popup editors
- artist-facing management tools
- polymorphic serialized configuration when useful

Do not use expensive Odin rendering inside frequently-called Hierarchy row rendering or binding paths.

The Hierarchy drawing path must remain lightweight.

### Odin Serializer

Use Odin Serializer where Unity serialization is insufficient.

Do not introduce unnecessary serialization complexity when standard Unity serialization already solves the problem cleanly.

### DOTween

DOTween Pro is available but should only be used when animation meaningfully improves UX.

Do not use DOTween for:

- basic Hierarchy drawing
- unnecessary state transitions
- continuously repainted decorative animation

Animation must never compromise Editor responsiveness.

---

## Performance

The Unity Hierarchy can redraw and rebind rows very frequently.

Code executed in:

- hierarchyWindowItemByEntityIdOnGUI
- Hierarchy row binding callbacks
- generateVisualContent
- GeometryChanged callbacks
- frequently invoked UI Toolkit refresh paths

must remain extremely cheap.

Avoid in hot paths:

- LINQ
- reflection
- repeated GetComponents
- AssetDatabase searches
- repeated GlobalObjectId conversion
- string allocations
- temporary collections
- new GUIContent allocations
- repeated texture lookup
- repeated type discovery

Prefer:

- caches
- dirty flags
- event-driven invalidation
- reusable GUIContent
- cached GUIStyle
- cached textures
- cached component data
- cached type metadata
- cached rule evaluation

Caches must have clear invalidation rules.

Do not create caches that silently remain stale.

---

## Event-Driven Design

Do not poll Editor state when an appropriate event exists.

Prefer events such as:

- EditorApplication.hierarchyChanged
- Selection.selectionChanged
- Undo.undoRedoPerformed
- EditorSceneManager.sceneOpened
- EditorSceneManager.sceneClosed
- EditorSceneManager.sceneSaved
- EditorApplication.projectChanged

Use EditorApplication.update only when there is no cleaner alternative.

---

## Scene Safety

Never add MonoBehaviours, hidden GameObjects, or helper components to scenes merely to store Hierarchy Toolkit metadata.

Hierarchy metadata must live outside scene object components.

Use stable object identification where necessary.

Prefer GlobalObjectId or another appropriate stable identifier instead of relying exclusively on InstanceID.

---

## Undo

Every user action that changes:

- scene state
- GameObjects
- parent relationships
- activation state
- project assets
- serialized configuration

must support Unity Undo whenever applicable.

Use appropriate APIs such as:

- Undo.RecordObject
- Undo.RegisterCompleteObjectUndo
- Undo.SetTransformParent
- Undo.RegisterCreatedObjectUndo

One user action should behave as one logical Undo step when practical.

---

## Multi-Selection

Where sensible, Hierarchy actions should support multiple selected GameObjects.

Examples include:

- assigning colors
- assigning icons
- toggling active state
- clearing metadata
- applying presets
- validation actions

Do not assume only one GameObject is selected unless the feature inherently requires it.

When an action is triggered from a clicked object:

- if that object belongs to the current selection, operate on eligible selected objects
- otherwise operate only on the clicked object

unless the active spec defines different behavior.

---

## Prefab Safety

Features must behave safely with:

- prefab assets
- prefab instances
- nested prefabs
- prefab overrides
- Prefab Mode

Do not accidentally modify prefab assets when the user intends to modify scene instances.

Use Unity prefab APIs where appropriate.

If a feature does not support a prefab context yet, fail safely and follow the active spec.

---

## Artist UX

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

Avoid requiring artists to manually edit code or configuration files.

Advanced options may exist but should not clutter the normal workflow.

Workflow speed is a design goal.

Whenever practical:

- actions should work directly from the Hierarchy
- avoid unnecessary selection changes
- avoid unnecessary confirmation dialogs
- preserve current selection
- use contextual controls
- support shortcuts for frequent operations

---

## Data Storage

Separate data according to scope.

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

- manual GameObject color
- manual GameObject icon
- separator configuration

Use stable object identification.

Metadata must survive normal Editor reloads.

Avoid designs that create unnecessary Git conflicts.

### Personal User Preferences

Examples:

- zebra striping enabled
- icon size
- visual density
- personal bookmarks
- shortcut preferences
- popup behavior

Prefer EditorPrefs or another appropriate user-local mechanism when settings should not be committed.

---

## UI Design

The Hierarchy must remain visually readable.

Avoid excessive visual noise.

Features drawing into the same row must cooperate.

Use centralized layout/reservation logic for right-side controls rather than allowing features to overlap independently.

Feature rendering should degrade gracefully when the Hierarchy window becomes narrow.

Conceptual row:

[Indentation] [Icon] GameObject Name [Warnings] [Components] [Active]

Preserve native Unity behavior for:

- selection
- rename
- foldouts
- drag/reorder
- prefab indicators

unless the active spec explicitly requires otherwise.

---

## Logging

Avoid console spam.

Do not log routine Editor operations.

Errors should explain:

- what failed
- which feature or object was involved
- whether user action is required

Debug logging must be optional.

Temporary diagnostics added during debugging should be removed before completion unless intentionally kept behind a debug flag.

---

## Compatibility

Target:

Unity 6.x

Primary development environment is the Unity version currently used by the project.

Prefer modern Unity 6 APIs.

Do not add compatibility code for old Unity versions unless explicitly requested.

---

## Coding Style

Prefer:

- focused classes
- meaningful names
- small responsibilities
- explicit cache ownership
- clear feature boundaries
- reuse of existing architecture

Avoid:

- giant managers
- unnecessary global mutable state
- unnecessary static state
- duplicated GUI logic
- duplicated caching systems
- speculative abstractions

One feature per class where practical.

Public APIs should have XML documentation.

Internal implementation does not require excessive comments when the code is self-explanatory.

---

## Testing

Prefer EditMode tests.

Tests should focus on behavior and state that can be reliably automated.

Relevant areas include:

- metadata persistence
- rule precedence
- cache invalidation
- settings serialization
- Undo/Redo
- multi-selection
- prefab safety
- validation logic

Do not create fragile tests that depend on exact pixel output or Editor layout details.

Visual and interactive behavior should be manually verified when automated tests cannot prove correctness.

Prefer the smallest test set that provides reasonable confidence in the current change.

Do not run broad regression suites by default.

---

## Specs

Feature behavior is defined in:

Packages/com.meganeura.hierarchy-toolkit/Specs/

For implementation tasks:

- the active spec is the source of truth for feature-specific behavior
- AGENTS.md defines global engineering constraints
- do not expand scope beyond the active spec
- do not automatically continue to the next spec
- if the spec conflicts with AGENTS.md, AGENTS.md wins
- if implementation requires changing the spec, stop and report the conflict

Specs should normally contain:

- Goal
- Behavior
- Requirements
- Acceptance Criteria
- Out of Scope

Visual features may also contain:

- Visual Reference
- UX Notes

---

## Skills

Use relevant Codex Skills when available.

Skills define reusable execution workflows and do not replace AGENTS.md or Specs.

Preferred Skills:

- unity-editor-feature — implementing new Editor features
- unity-hierarchy-ui — Hierarchy visual and interactive work
- unity-editor-debug — debugging existing tooling
- unity-editor-test — creating or extending tests

AGENTS.md defines global engineering rules.

Specs define feature behavior and acceptance criteria.

Skills define reusable implementation, debugging, and testing workflows.

---

## MCP / Codex Rules

Never edit MCP for Unity source.

Never modify unrelated project code.

Never make broad project-wide changes unless explicitly requested.

For Hierarchy Toolkit tasks, prioritize files inside:

Packages/com.meganeura.hierarchy-toolkit/

Do not inspect unrelated project directories unless necessary.

Do not inspect or search these directories unless explicitly required:

- Library/
- Temp/
- Logs/
- obj/
- UserSettings/
- Packages unrelated to Hierarchy Toolkit

Do not perform broad repository-wide searches when the relevant package path is already known.

Use direct file operations for normal C# creation/editing when available.

Use Unity MCP primarily for:

- compilation
- console errors
- EditMode tests
- Unity-specific validation
- asset/database operations that require the Editor

Do not repeatedly reread unchanged files unless necessary.

Do not provide long implementation explanations after completing tasks.

---

## Implementation Workflow

When implementing a task:

1. Follow AGENTS.md.
2. Read the active spec completely.
3. Use relevant Skills when available.
4. Inspect only files relevant to the task.
5. Reuse existing architecture.
6. Do not redesign working systems unless required.
7. Implement only the active spec.
8. Compile.
9. Fix only errors caused by the current task.
10. Run targeted tests for the active feature.
11. Run only directly impacted regression tests when shared infrastructure changed.
12. Inspect package-related Console errors.
13. Report manual verification when visual behavior cannot be proven automatically.
14. Stop when the active spec is complete.

Do not automatically run the full Hierarchy Toolkit suite.

---

## Definition of Done

A feature is complete only when:

1. all acceptance criteria from the active spec are implemented
2. Unity compilation succeeds
3. relevant targeted EditMode tests pass
4. directly impacted regression tests pass when required
5. no package-related Console errors remain
6. unrelated project errors are not modified or "fixed"
7. visual or interactive behavior is manually verified when automated tests cannot prove it
8. the final report states:
   - compilation result
   - targeted tests executed
   - impact-based regression tests executed, if any
   - acceptance criteria verified automatically
   - manual verification still required
   - remaining blockers

The full Hierarchy Toolkit suite is not required for normal feature work.

---

## Validation Harness

Validation procedure is defined in:

Packages/com.meganeura.hierarchy-toolkit/Specs/HARNESS.md

Default validation is:

- compile
- targeted tests
- impact-based regression tests only when justified
- Console validation
- manual verification when required

Do not run the full Hierarchy Toolkit suite during normal feature work unless HARNESS.md explicitly calls for it.

Prefer the smallest test set that provides reasonable confidence in the current change.

Do not claim visual verification unless it was actually performed.

---

## Full Suite Policy

The full Hierarchy Toolkit EditMode suite is reserved for:

- explicit user requests
- release or milestone validation
- dedicated regression passes
- high-risk architectural changes affecting most Toolkit features
- cases where targeted tests reveal evidence of broader breakage

If a full-suite run reveals a failure unrelated to the current task:

1. rerun the failing test once in isolation
2. if it reproduces and the changed files are unrelated, report it as pre-existing
3. do not investigate or fix it unless explicitly requested
4. stop unrelated debugging

---

## Final Report

Final reports should normally contain only:

- files created
- files modified
- compilation result
- targeted tests executed/result
- impacted regression tests executed/result, if any
- full suite: not required / executed
- Console result
- acceptance criteria verified automatically
- manual verification still required
- blockers or important notes

---

## Priorities

When multiple implementation approaches are valid, prioritize:

1. correctness
2. Editor stability
3. performance
4. artist usability
5. maintainability
6. visual polish

Do not sacrifice Hierarchy responsiveness for cosmetic functionality.

Prefer the best long-term internal-tool design over the fewest lines of code.