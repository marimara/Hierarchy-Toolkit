# \# Hierarchy Toolkit Validation Harness

# 

# The validation harness defines the minimum verification required for Hierarchy Toolkit changes.

# 

# The goal is to validate changes reliably without running unnecessary tests.

# 

# \## Level 1 - Compilation

# 

# Always compile the Unity project through Unity MCP.

# 

# The task fails if the current change introduces compilation errors.

# 

# Do not fix unrelated project compilation problems.

# 

# \## Level 2 - Targeted Tests

# 

# Always run only the EditMode tests directly related to:

# 

# \- the active feature

# \- changed shared utilities directly used by that feature

# \- regressions explicitly associated with the change

# 

# Examples:

# 

# \- activation toggle → activation tests

# \- separator → separator tests

# \- metadata change → metadata tests

# \- layout change used by activation toggle → activation + layout tests

# 

# Do not automatically run unrelated feature tests.

# 

# \## Level 3 - Impact-Based Regression Tests

# 

# If shared infrastructure changed, identify only the features directly affected by that shared code.

# 

# Run targeted regression tests for those direct dependents.

# 

# Examples:

# 

# \- centralized row layout changed

# &#x20; → test features using right-side layout reservations

# 

# \- metadata store changed

# &#x20; → test metadata-dependent features

# 

# \- hierarchy binding changed

# &#x20; → test features bound through that path

# 

# Do not run the complete Toolkit suite merely because a shared file was touched.

# 

# \## Level 4 - Full Toolkit Suite

# 

# The full Hierarchy Toolkit EditMode suite is NOT part of normal feature implementation.

# 

# Run the full suite only when:

# 

# \- explicitly requested

# \- preparing a release or milestone

# \- performing a dedicated regression pass

# \- a high-risk architectural change affects most Toolkit features

# \- targeted regression tests reveal evidence of broader breakage

# 

# If the full suite reveals an unrelated failure:

# 

# 1\. rerun that failing test once in isolation

# 2\. if it reproduces and is unrelated to the current change, report it

# 3\. do not investigate or fix it unless explicitly requested

# 4\. stop unrelated debugging

# 

# \## Level 5 - Console Validation

# 

# Inspect the Unity Console after compilation and targeted tests.

# 

# The change must not introduce package-related:

# 

# \- errors

# \- exceptions

# \- repeated warnings

# \- console spam

# 

# Do not investigate unrelated project issues.

# 

# \## Level 6 - Manual Verification

# 

# Visual and interactive behavior that cannot be reliably proven through automated tests requires manual Unity Editor verification.

# 

# Examples:

# 

# \- visual alignment

# \- mouse interaction

# \- text readability

# \- drag/reorder

# \- popup placement

# \- narrow Hierarchy behavior

# \- coexistence with existing visual features

# 

# Do not claim manual verification was completed unless it was actually observed.

# 

# \## Completion

# 

# Normal feature work requires:

# 

# 1\. compilation

# 2\. targeted tests

# 3\. impact-based regression tests only when justified

# 4\. Console validation

# 5\. manual verification when applicable

# 

# The full Toolkit suite is optional unless explicitly required by Level 4.

# 

# \## Final Report

# 

# State only:

# 

# \- compilation result

# \- targeted tests executed

# \- impact-based regression tests executed, if any

# \- full suite: not required / executed

# \- Console result

# \- manual verification required or completed

# \- blockers

