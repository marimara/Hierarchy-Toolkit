# \# Hierarchy Toolkit Validation Harness

# 

# The validation harness defines how Hierarchy Toolkit changes are verified.

# 

# \## Level 1 - Compilation

# 

# Compile the Unity project through Unity MCP.

# 

# The task fails if the current change introduces compilation errors.

# 

# Do not fix unrelated project compilation problems.

# 

# \## Level 2 - Targeted Tests

# 

# Run EditMode tests directly related to the active feature or bug.

# 

# Examples:

# 

# \- metadata feature → metadata tests

# \- separator feature → separator tests

# \- activation toggle → activation tests

# 

# \## Level 3 - Full Toolkit Suite

# 

# Run the complete Hierarchy Toolkit EditMode test suite when:

# 

# \- shared infrastructure changed

# \- hierarchy binding changed

# \- metadata storage changed

# \- cache infrastructure changed

# \- centralized layout changed

# \- feature registration/bootstrap changed

# 

# \## Level 4 - Console Validation

# 

# Inspect the Unity Console after compilation and tests.

# 

# The change must not introduce package-related:

# 

# \- errors

# \- exceptions

# \- repeated warnings

# \- console spam

# 

# Do not attempt to fix unrelated project issues.

# 

# \## Level 5 - Manual Verification

# 

# Visual and interactive behavior that cannot be reliably proven through automated tests requires manual Unity Editor verification.

# 

# Examples:

# 

# \- visual alignment

# \- text readability

# \- drag/reorder interaction

# \- popup placement

# \- narrow Hierarchy behavior

# \- visual coexistence with existing features

# 

# Do not claim manual verification was completed unless the behavior was actually observed.

# 

# \## Completion

# 

# A task is complete when all applicable levels pass.

# 

# The final report must state:

# 

# \- compilation result

# \- targeted tests executed

# \- full suite executed or not required

# \- Console result

# \- manual verification required or completed

# \- remaining blockers

