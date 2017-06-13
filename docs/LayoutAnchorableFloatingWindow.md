# LayoutAnchorableFloatingWindow
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutFloatingWindow

Represents a floating window containing one or more anchorables in the layout model.

## Properties
|| Property || Description
| Children | Overridden. Gets the children of the floating window.
| ChildrenCount | Overridden. Gets the number of children.
| IsSinglePane | Gets whether the floating window contains a single pane.
| IsValid | Overridden. Gets whether the floating window is valid (whether RootPanel is null).
| IsVisible | Gets if the element is visible.
| Parent | Gets or sets the parent container of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| Root | Gets or sets the root of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| RootPanel | Gets or sets the root panel contained in the floating window.
| SinglePane | Gets the pane contained in a floating window that contains only one pane.

## Events
|| Event || Description
| IsVisibleChanged | Raised when IsVisible has changed.
| PropertyChanged | Raised when a property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PropertyChanging | Raised when a property is about to change. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)

## Methods
|| Method || Description
| RemoveChild | Overridden. Removes the child element.
| ReplaceChild | Overridden. Replaces a specified child element with a specified new element.

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---