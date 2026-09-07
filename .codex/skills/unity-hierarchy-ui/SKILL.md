# \# Skill: unity-hierarchy-ui

# 

# Use this Skill for Unity Hierarchy visual or interactive features.

# 

# \## Goal

# 

# Add Hierarchy functionality while preserving Unity's native behavior and keeping frequently-called UI paths extremely lightweight.

# 

# \## Preserve Native Behavior

# 

# Do not break:

# 

# \- selection

# \- rename

# \- foldout controls

# \- drag/reorder

# \- prefab indicators

# \- keyboard navigation

# \- row recycling/binding

# 

# Do not simulate native controls when Unity already provides them.

# 

# \## Layout

# 

# Use existing centralized row layout/reservation infrastructure.

# 

# Features must not independently overlap:

# 

# \- GameObject labels

# \- foldouts

# \- icons

# \- warnings

# \- component controls

# \- activation controls

# 

# Support narrow Hierarchy windows gracefully.

# 

# \## Performance

# 

# In repaint, binding, visual generation, and frequently-called UI callbacks:

# 

# Avoid:

# 

# \- LINQ

# \- reflection

# \- AssetDatabase calls

# \- repeated GetComponents

# \- temporary collections

# \- repeated texture/style lookup

# \- repeated GlobalObjectId conversion

# 

# Prefer cached, event-driven data.

# 

# \## UI Toolkit / IMGUI

# 

# Use the existing Hierarchy integration approach.

# 

# Do not introduce a second competing drawing system without a clear architectural reason.

# 

# Use Odin for configuration/editor windows where useful, not for hot Hierarchy row rendering.

# 

# \## Visual Changes

# 

# Visual features should:

# 

# \- remain subtle

# \- preserve text readability

# \- work in light and dark themes

# \- coexist with existing Hierarchy Toolkit features

# \- avoid unnecessary animation

# \- avoid excessive visual noise

# 

# \## Validation

# 

# Test behavior/state automatically where practical.

# 

# Visual correctness that cannot be proven through tests must be reported for manual Unity verification.

