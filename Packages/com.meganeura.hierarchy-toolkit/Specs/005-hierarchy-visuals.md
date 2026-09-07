# \# 005 - Hierarchy Visual Guides

# 

# Status: DONE

# 

# \## Goal

# 

# Improve readability of large and deeply nested Unity Hierarchies without replacing Unity's native tree controls.

# 

# \## Behavior

# 

# Hierarchy Toolkit provides zebra striping and subtle parent/child tree guide lines.

# 

# Unity remains responsible for native foldout triangles.

# 

# Guide lines visually clarify ancestry and sibling relationships.

# 

# \## Requirements

# 

# \- Provide Zebra Striping as an independent feature.

# \- Provide Hierarchy Lines as an independent feature.

# \- Both features must be independently enableable.

# \- Zebra uses subtle alternating row backgrounds.

# \- Manual color overrides take precedence over zebra.

# \- Selection takes precedence over decorative backgrounds.

# \- Hierarchy lines use subtle low-contrast styling.

# \- Draw vertical ancestor continuation lines.

# \- Draw branch connectors for siblings.

# \- Last siblings terminate their branch.

# \- Do not draw custom foldout arrows.

# \- Unity native foldouts indicate expanded/collapsed children.

# \- Leaf objects receive no custom arrow.

# \- Lines must not overlap GameObject labels or icons.

# \- Relationship information must be cached.

# \- Hierarchy structure changes invalidate relevant caches.

# \- No expensive Transform traversal every repaint.

# 

# \## Acceptance Criteria

# 

# \- Zebra rows alternate consistently.

# \- Selected rows remain readable.

# \- Manual colored rows are not overwritten by zebra.

# \- Deep nesting is visually easier to follow.

# \- First/middle/last sibling branches render correctly.

# \- Reparenting updates the lines.

# \- Collapsing/expanding parents updates visible branches.

# \- Leaf objects do not display fake foldout arrows.

# \- Unity's native foldout behavior remains untouched.

# \- Scrolling large hierarchies remains responsive.

# 

# \## Out of Scope

# 

# \- User-configurable visual settings.

# \- Custom line themes.

# \- Animation.

# \- Runtime hierarchy visualization.

