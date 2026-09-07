# \# Skill: unity-editor-test

# 

# Use this Skill when adding or extending tests for Hierarchy Toolkit.

# 

# \## Test Type

# 

# Prefer EditMode tests.

# 

# Test behavior and state rather than exact rendered pixels.

# 

# \## Priorities

# 

# When relevant, cover:

# 

# \- metadata persistence

# \- Undo/Redo

# \- multi-selection

# \- cache invalidation

# \- rule precedence

# \- prefab safety

# \- stable object identity

# \- supported/unsupported object filtering

# \- scene reload behavior

# \- shared infrastructure regression

# 

# \## UI Features

# 

# For visual features, test:

# 

# \- data/state used by rendering

# \- layout calculations

# \- precedence rules

# \- hierarchy relationship calculations

# \- cache behavior

# 

# Do not create tests that depend on exact:

# \- pixel output

# \- absolute screen coordinates

# \- Unity skin visuals

# \- window size unless behavior specifically depends on it

# 

# Manual visual verification may still be required.

# 

# \## Isolation

# 

# Tests must:

# 

# \- clean up created scenes/assets

# \- avoid modifying unrelated project assets

# \- avoid depending on project-specific gameplay systems

# \- be deterministic

# \- not rely on test execution order

# 

# \## Harness

# 

# After targeted tests:

# 

# \- compile

# \- inspect package-related Console errors

# \- run the full Hierarchy Toolkit suite when shared infrastructure changed

# 

# \## Output

# 

# Report:

# 

# \- tests created/modified

# \- tests executed

# \- pass/fail result

# \- coverage gaps requiring manual verification

