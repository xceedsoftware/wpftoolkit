# LayoutDocumentPane
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>

Represents a layout element that contains a collection of LayoutDocument obiects.

## Properties
|| Property || Description
| Children | Gets the child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| ChildrenCount | Gets the number of child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| ChildrenSorted | Gets whether the child documents are sorted.
| DockHeight | Gets or sets the initial height of the dock. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| DockMinHeight | Gets or sets the minimum dock height. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| DockMinWidth | Gets or sets the minimum dock width. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| DockWidth | Gets or sets the initial width of the dock. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| FloatingHeight | Gets or sets the initial height of floating windows. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| FloatingLeft | Gets the initial position of the left side of a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| FloatingTop | Gets the initial position of the topside of a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| FloatingWidth | Gets or sets the initial width of floating windows. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| IsMaximized | Gets whether the element is maximized. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<LayoutContent>)
| IsVisible | Gets whether the layout group is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
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
| ComputeVisibility | Determines whether an element is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| GetSchema | Returns null. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| IndexOf | Gets the index of the specified child content.
| IndexOfChild | Returns the index of the specified child layout element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| InsertChildAt | Inserts the specified child layout element at the specified index. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| MoveChild | Moves a child from an old index to a new index within a pane. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| ReadXml | Overridden. Reads serialized layout information using the specified XmlReader.  
| RemoveChild | Removes the specified child ILayoutElement. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| RemoveChildAt | Removes the child element at the specified index. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| ReplaceChild | Replaces a child ILayoutElement with a new one. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| ReplaceChildAt | Replaces the child element at the specified index with the specified element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutContent>)
| WriteXml | Overridden. Writes serialized layout information using the specified XmlReader.

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---