# \# Skill: unity-editor-debug

# 

# Use this Skill when debugging an existing Hierarchy Toolkit feature.

# 

# \## Goal

# 

# Find the actual root cause and apply the smallest correct fix without redesigning unrelated systems.

# 

# \## Workflow

# 

# 1\. Read AGENTS.md.

# 2\. Identify the exact failing behavior.

# 3\. Separate confirmed facts from assumptions.

# 4\. Trace only the relevant execution/data path.

# 5\. Inspect the smallest relevant file set.

# 6\. Form the most likely hypotheses.

# 7\. Verify hypotheses using:

# &#x20;  - existing tests

# &#x20;  - Unity Console

# &#x20;  - targeted temporary diagnostics

# &#x20;  - Editor inspection through MCP when useful

# 8\. Identify the root cause.

# 9\. Apply the smallest correct fix.

# 10\. Remove temporary diagnostics unless intentionally preserved behind a debug flag.

# 11\. Run regression tests.

# 12\. Stop.

# 

# \## Debugging Rules

# 

# Do not:

# 

# \- rewrite the architecture because of one bug

# \- modify unrelated features

# \- add broad logging

# \- introduce polling to observe state

# \- disable existing safeguards merely to make the symptom disappear

# 

# Prefer fixing the cause rather than masking the symptom.

# 

# \## When Visual Behavior Fails

# 

# Check, as relevant:

# 

# \- feature registration

# \- row binding

# \- draw/bind order

# \- cache population

# \- cache invalidation

# \- object/entity resolution

# \- UI Toolkit hierarchy

# \- style visibility

# \- clipping

# \- geometry

# \- selection overlays

# \- recycled row state

# \- theme-dependent colors

# 

# \## Output

# 

# Report:

# 

# \- root cause

# \- files modified

# \- fix applied

# \- compilation result

# \- tests/result

# \- whether manual visual verification is still required

