# LayoutAnchorablePane
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>

Represents a layout element that contains a collection of LayoutAnchorable obiects.

## Properties
|| Property || Description
| CanClose | Gets whether the pane can be closed.
| CanHide | Gets whether the pane can be hidden.
| Children | Gets the child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| ChildrenCount | Gets the number of child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| DockHeight | Gets or sets the initial height of the dock. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| DockMinHeight | Gets or sets the minimum dock height. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| DockMinWidth | Gets or sets the minimum dock width. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| DockWidth | Gets or sets the initial width of the dock. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| FloatingHeight | Gets or sets the initial height of floating windows. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| FloatingLeft | Gets the initial position of the left side of a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| FloatingTop | Gets the initial position of the topside of a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| FloatingWidth | Gets or sets the initial width of floating windows. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| IsDirectlyHostedInFloatingWindow | Gets whether the pane is hosted directly in a floating window (not in a LayoutAnchorablePaneGroup).
| IsHostedInFloatingWindow | Gets whether the pane is hosted in a floating window.
| IsMaximized | Gets whether the element is maximized. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutAnchorable>)
| IsVisible | Gets whether the layout group is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| Name | Gets the name of the pane.
| Parent | Gets or sets the parent container of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| Root | Gets or sets the root of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| SelectedContent | Gets the selected content in the pane.
| SelectedContentIndex | Gets or sets the index of the selected content in the pane.

## Events
|| Event || Description
| ChildrenCollectionChanged | Raised when the child collection changes. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroupBase)
| ChildrenTreeChanged | Raised when the children tree changes. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroupBase)
| PropertyChanged | Raised when a property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PropertyChanging | Raised when a property is about to change. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)

## Methods
|| Method || Description
| ComputeVisibility | Determines whether an element is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| GetSchema | Returns null. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| IndexOf | Gets the index of the specified child content.  
| IndexOfChild | Returns the index of the specified child layout element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| InsertChildAt | Inserts the specified child layout element at the specified index. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| MoveChild | Moves a child from an old index to a new index within a pane (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| ReadXml | Overridden. Reads serialized layout information using the specified XmlReader.  
| RemoveChild | Removes the specified child ILayoutElement. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| RemoveChildAt | Removes the child element at the specified index. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| ReplaceChild | Replaces a child ILayoutElement with a new one. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| ReplaceChildAt | Replaces the child element at the specified index with the specified element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| WriteXml | Overridden. Writes serialized layout information using the specified XmlWriter.  

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---