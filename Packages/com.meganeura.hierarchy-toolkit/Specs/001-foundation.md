# \# 001 - Foundation

# 

# Status: DONE

# 

# \## Goal

# 

# Provide the editor-only architectural foundation used by all Hierarchy Toolkit features.

# 

# The foundation must support modular hierarchy features without modifying runtime game behavior.

# 

# \## Behavior

# 

# Hierarchy Toolkit initializes automatically when the Unity Editor loads.

# 

# Features are registered through a shared hierarchy drawing/integration system.

# 

# Each feature may participate independently in hierarchy rendering or interaction.

# 

# The package must remain invisible to runtime builds.

# 

# \## Requirements

# 

# \- Package root is `Packages/Hierarchy Toolkit/`.

# \- Code is Editor-only.

# \- Namespace is `Meganeura.HierarchyToolkit`.

# \- Use modular features based on `IHierarchyFeature` where appropriate.

# \- A central bootstrap owns package initialization.

# \- A central hierarchy integration layer coordinates features.

# \- Features must not independently create competing hierarchy callbacks when centralized integration is available.

# \- Unity 6 hierarchy APIs should be used.

# \- Existing Unity Hierarchy selection, rename, drag/reorder, and foldout behavior must remain intact.

# \- Shared systems must support proper disposal during assembly reload and Editor shutdown.

# \- Do not add runtime MonoBehaviours.

# \- Do not add hidden GameObjects to scenes.

# 

# \## Acceptance Criteria

# 

# \- Package loads without compilation errors.

# \- Hierarchy Toolkit initializes automatically.

# \- Multiple features can be registered.

# \- Duplicate feature registration does not create duplicate rendering.

# \- Assembly reload does not leave stale Editor callbacks.

# \- Unity Hierarchy remains usable with all features disabled.

# \- Runtime assemblies do not depend on Hierarchy Toolkit.

# \- Existing project code outside the package is not required for initialization.

# 

# \## Out of Scope

# 

# \- Feature-specific metadata.

# \- Colors.

# \- Icons.

# \- Separators.

# \- Navigation.

# \- Component minimap.

# \- Settings UI.

