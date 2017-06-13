# DockingManager
Derives from Control

The core control of AvalonDock.

## Properties
|| Property || Description
| ActiveContent | Gets or sets the currently active content.
| AllowMixedOrientation | Gets or sets whether the docking manager should allow mixed orientation for document panes.
| AnchorableContextMenu | Gets or sets the context menu to display for anchorables.
| AnchorableHeaderTemplate | Gets or sets the data template to use for the headers of anchorables.
| AnchorableHeaderTemplateSelector | Gets or sets the selector to use when selecting the data template for the headers of anchorables.
| AnchorablePaneControlStyle | Gets or sets the style to apply to LayoutAnchorablePaneControl.
| AnchorablePaneTemplate | Gets or sets the ControlTemplate used to render LayoutAnchorablePaneControl.
| AnchorablesSource | Gets or sets the source collection of LayoutAnchorable objects.
| AnchorableTitleTemplate | Gets or sets the data template to use for anchorable titles.
| AnchorableTitleTemplateSelector | Gets or sets the selector to use when selecting the data template for anchorable titles.
| AnchorGroupTemplate | Gets or sets the ControlTemplate used to render the LayoutAnchorGroupControl.
| AnchorSideTemplate | Gets or sets the ControlTemplate used to render LayoutAnchorSideControl.
| AnchorTemplate | Gets or sets the ControlTemplate used to render LayoutAnchorControl.
| AutoHideWindow | Gets the currently shown autohide window.
| BottomSidePanel | Gets or sets the bottom side panel control.
| DocumentContextMenu | Gets or sets the context menu to show for documents.
| DocumentHeaderTemplate | Gets or sets the data template to use for document headers.
| DocumentHeaderTemplateSelector | Gets or sets the template selector that is used when selecting the data template for document headers.
| DocumentPaneControlStyle | Gets or sets the style of LayoutDocumentPaneControl.
| DocumentPaneMenuItemHeaderTemplate | Gets or sets the DataTemplate to use when creating menu items in dropdowns on document panes.
| DocumentPaneMenuItemHeaderTemplateSelector | Gets or sets the data template selector to use for the menu items shown when the user selects the LayoutDocumentPaneControl's document switch context menu.
| DocumentPaneTemplate | Gets or sets the ControlTemplate used to render LayoutDocumentPaneControl.
| DocumentsSource | Gets or sets the source collection of LayoutDocument objects.
| DocumentTitleTemplate | Gets or sets the data template to use for document titles.
| DocumentTitleTemplateSelector | Gets or sets the data template selector to use when creating the data template for the title.
| FloatingWindows | Gets the floating windows.
| GridSplitterHeight | Gets or sets the height of grid splitters.
| GridSplitterWidth | Gets or sets the width of grid splitters.
| IconContentTemplate | Gets or sets the data template to use on the icon extracted from the layout model.
| IconContentTemplateSelector | Gets or sets the data template selector to use when selecting the datatamplate for content icons.
| Layout | Gets or sets the root of the layout tree.
| LayoutItemContainerStyle | Gets or sets the style to apply to LayoutDocumentItem objects.
| LayoutItemContainerStyleSelector | Gets or sets the style selector of LayoutDocumentItemStyle.
| LayoutItemTemplate | Gets or sets the template used to render anchorable and document content.
| LayoutItemTemplateSelector | Gets or sets the template selector to use for anchorable and document templates.
| LayoutRootPanel | Gets or sets the layout panel control which is attached to the Layout.Root property.
| LayoutUpdateStrategy | Gets or sets the strategy class to call when AvalonDock needs to position an anchorable inside an existing layout model.
| LeftSidePanel | Gets or sets the left side panel control.
| RightSidePanel | Gets or sets the right side panel control.
| ShowSystemMenu | Gets or sets whether floating windows should show the system menu when a custom context menu is not defined.
| Theme | Gets or sets the theme to use for AvalonDock controls.
| TopSidePanel | Gets or sets the top side panel control.

## Events
|| Event || Description
| ActiveContentChanged | Raised when ActiveContent changes.
| DocumentClosed | Raised after a document is closed.
| DocumentClosing | Raised when a document is about to be closed.
| LayoutChanged | Raised when Layout changes.
| LayoutChanging | Raised when Layout is about to be changed.

## Methods
|| Method || Description
| GetLayoutItemFromModel | Returns the LayoutItem wrapper for the content passed as argument.
| OnApplyTemplate | Overridden. Invoked whenever application code or internal processes call ApplyTemplate, setting up AutoHideWindow.

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---