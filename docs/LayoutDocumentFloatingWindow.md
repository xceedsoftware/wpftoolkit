# LayoutDocumentFloatingWindow
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutFloatingWindow

Represents a floating window containing one or more documents in the layout model.

## Properties
|| Property || Description
| Children | Overridden. Gets the children of the floating window.
| ChildrenCount | Overridden. Gets the number of children.
| IsValid | Overridden. Gets whether the floating window is valid (whether RootDocument is null).
| Parent | Gets or sets the parent container of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| Root | Gets or sets the root of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| RootDocument | Gets or sets the root document contained in the floating window.

## Events
|| Event || Description
| PropertyChanged | Raised when a property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PropertyChanging | Raised when a property is about to change. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| RootDocumentChanged | Raised when RootDocument has changed.

## Methods
|| Method || Description
| RemoveChild | Overridden. Removes the child element.
| ReplaceChild | Overridden. Replaces a specified child element with a specified new element.

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---