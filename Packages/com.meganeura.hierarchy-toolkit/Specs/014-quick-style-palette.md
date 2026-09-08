# 014 - Quick Style Palette

Status: PLANNED

## Goal

Provide a fast compact palette for manually styling Hierarchy GameObjects with reusable colors and icons.

The palette should make common manual styling actions substantially faster than navigating nested Unity context menus.

## Behavior

Alt-clicking a valid GameObject row in the Hierarchy opens a compact style palette near the clicked row.

The palette provides:

- reusable color presets
- reusable icon choices
- clear color
- clear icon
- recursive color application
- access to custom preset editing

This feature extends the existing Manual Colors and Manual Icons systems.

It must reuse existing metadata rather than introducing a second styling system.

## Open Palette

- Alt + left click on a valid Hierarchy GameObject row opens the Quick Style Palette.
- The palette appears near the clicked row/pointer.
- Only one palette may be open at a time.
- Alt-clicking another GameObject updates the target.
- Clicking elsewhere closes the palette where appropriate.
- Unsupported rows and scene headers do nothing.
- Do not interfere with rename, drag/reorder, foldouts, or native selection behavior.

## Color Presets

- Display a compact horizontal row of predefined color swatches.
- Clicking a swatch applies that color using the existing Manual Colors system.
- Include a clear-color action represented by a compact `X`.
- Colors are project-configurable presets.
- User-added colors persist across Editor sessions.
- Presets should be easy to reorder/edit later through configuration UI.

Initial built-in palette may provide a useful small set such as:

- neutral/dark
- red
- yellow/gold
- green
- teal
- cyan
- blue
- purple
- pink

Exact colors are configuration data, not hard-coded rendering logic.

## Alpha Gradient

Manual row colors should support the visual treatment shown in the reference:

- strong/opaque color toward the left side of the row
- progressively reduced alpha toward the right side
- smooth horizontal fade to transparent

The fade should integrate with the existing row background rendering rather than overlaying a separate opaque rectangle.

Selection must remain clearly visible.

The gradient must work in both Unity light and dark themes.

Do not change the stored manual color solely to achieve the gradient.

The stored color remains the source color; alpha interpolation is a rendering concern.

## Gradient Configuration

Provide a simple configurable gradient mode for manual colors.

Initial implementation may use:

- start alpha: 1.0
- end alpha: 0.0

The implementation should allow these values to become configurable later without redesigning the rendering architecture.

## Recursive Color Application

Holding Alt while choosing a color preset applies that manual color to:

- the target GameObject
- all descendant GameObjects

All eligible descendants receive the same manual color.

Recursive application must use the existing Manual Colors metadata system.

Do not modify GameObject names, components, hierarchy structure, or scene content.

### Clearing Recursively

Holding Alt while activating Clear Color clears manual color overrides from:

- the target
- all descendants

Automatic/future styling may become visible again after a manual override is cleared.

## Recursive Safety

- Traverse descendants only when the user explicitly invokes recursive styling.
- Do not perform descendant traversal during repaint.
- Support Undo/Redo as one logical user operation.
- A single Undo should revert the recursive styling operation where practical.
- Unsupported objects should be skipped safely.
- Multi-scene boundaries must be respected.

## Icons

- Display a compact grid of commonly useful icon presets.
- Reuse the existing Manual Icons implementation and icon references.
- Clicking an icon applies it to the palette target.
- Include a clear-icon action represented by `X`.
- Native Unity icons may be used.
- Existing custom icon support must remain available.

The palette should visually resemble the attached reference in density and hierarchy without copying unrelated asset-specific artwork or behavior.

## Custom Icons

- Provide compact access to selecting additional supported icons.
- A `+` action may open the existing/full icon picker or future preset editor.
- Do not duplicate the existing icon storage implementation.

## Custom Color Presets

- Provide a compact `+` or equivalent action for adding/editing reusable color presets.
- Use Unity ColorField or an appropriate small editor popup.
- Added presets persist as project configuration.
- Editing presets must not automatically recolor existing GameObjects; presets are choices, not linked style assets.

## Existing Context Menus

Existing Hierarchy Toolkit context-menu actions for:

- Color
- Icon

must remain functional.

The Quick Style Palette is an additional faster interaction, not a replacement that removes existing access.

## Multi-selection

If the clicked GameObject belongs to the current selection, normal Manual Color / Manual Icon multi-selection conventions may be reused where predictable.

Recursive Alt application applies to the explicitly targeted object's descendant hierarchy and must not unexpectedly recurse through unrelated selected roots.

Avoid ambiguous combinations of multi-selection + recursion.

## Visual Design

The palette should be:

- compact
- dark/light theme compatible
- visually dense but readable
- close to the clicked Hierarchy row
- fast to dismiss
- free of unnecessary labels

Prefer recognizable swatches/icons over text-heavy controls.

Do not use Odin inside the hot Hierarchy row rendering path.

Odin may be used for non-hot configuration UI if useful.

## Row Rendering Integration

Reuse the existing Manual Colors renderer.

The alpha gradient must not interfere with:

- Zebra Striping
- selection
- Separators
- Hierarchy Lines
- Manual Icons
- Activation Toggle
- Component Minimap
- Default Parent indicator
- Scene Selector
- native prefab indicators

Separators keep their existing dedicated background behavior unless explicitly styled by their own system.

## Undo / Redo

- Single color assignment supports Undo/Redo.
- Single icon assignment supports Undo/Redo.
- Recursive color assignment supports Undo/Redo as one logical operation where practical.
- Recursive clear supports Undo/Redo.
- Editing color presets should support appropriate project-setting Undo where practical.

## Performance

- Do not rebuild the palette during Hierarchy repaint.
- Do not enumerate descendants except when recursive application is explicitly invoked.
- Do not perform AssetDatabase searches in row repaint.
- Do not perform icon discovery in row repaint.
- Cache reusable preset/icon data where appropriate.

## Acceptance Criteria

- Alt-clicking a valid GameObject row opens the Quick Style Palette.
- Color presets can be applied with one click.
- Manual colors render with the intended horizontal alpha fade.
- Clear Color works.
- Holding Alt while applying a color applies it recursively to descendants.
- Holding Alt while clearing a color clears descendants.
- Recursive operations support Undo/Redo.
- Icon presets can be applied with one click.
- Clear Icon works.
- Existing custom/native icon support remains functional.
- Custom color presets can be stored and reused.
- Existing Color and Icon context-menu actions continue working.
- Selection remains readable over colored rows.
- Existing Hierarchy Toolkit features remain functional.
- No package-related Console errors are introduced.

### Icon Library

- The `+` button in the Quick Style Palette icon section opens a dedicated Icon Library window.
- The Icon Library displays the available built-in Unity Editor icons in a searchable grid.
- Built-in icons are discovered/cached outside Hierarchy repaint and are not duplicated as package assets.
- Clicking an icon applies it to the current target through the existing Manual Icons system.
- The window must be scrollable and searchable by icon name.
- The Icon Library may remain open while browsing/applying icons where practical.

### Custom Icons

- The Icon Library contains its own `+` action for adding a custom project Sprite.
- Adding a Sprite makes it available inside the Icon Library for future use.
- Custom icon entries persist across Editor sessions as project-level configuration.
- Do not copy or duplicate the source Sprite asset unless technically necessary.
- Prevent duplicate custom entries.
- Missing/deleted Sprite references must fail safely and be removable.
- Built-in Unity icons and custom project icons should coexist in the same browser, with a clear distinction where useful.
- Selecting a custom icon applies it through the existing Manual Icons system.

### Palette Close

- The Quick Style Palette must provide an explicit close button in its window/header.
- The close control is separate from styling reset actions.
- The `X` inside the color section continues to mean Clear/Reset Color.
- The `X` inside the icon section continues to mean Clear/Reset Icon.
- Closing the palette must not change any color or icon state.

### Interaction Priority with Component Minimap

Alt-click interactions must respect the element actually under the pointer.

If Alt-click occurs over a Component Minimap icon:

- open only the Component Popup Inspector
- do not also open the Quick Style Palette
- do not change color/icon styling
- do not let the row-level Alt-click handler consume or duplicate the minimap interaction

The Quick Style Palette should open only when Alt-click targets the normal GameObject row area rather than an interactive Hierarchy Toolkit control.

Component Minimap interaction has priority over the row-level Quick Style Palette interaction.

Use the actual UI Toolkit picked element / event target or ancestor relationship to distinguish these cases.

Do not rely only on the GameObject row being hovered.

## Out of Scope

- Automatic styling rules.
- Rules based on Component/Tag/Layer.
- Recursive icon application.
- Runtime styling.
- Linked/live color styles.
- Full theme/preset system.
- Automatic hierarchy organization.