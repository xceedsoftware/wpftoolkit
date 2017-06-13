# LayoutAnchorGroup
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>

Represents an autohidden group of one or more LayoutAnchorable elements that can be anchored to one of the four sides of the DockingManager.

## Properties
|| Property || Description
| Children | Gets the child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| ChildrenCount | Gets the number of child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| IsVisible | Gets whether the layout group is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<LayoutAnchorable>)
| Parent | Gets or sets the parent container of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| Root | Gets or sets the root of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)

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