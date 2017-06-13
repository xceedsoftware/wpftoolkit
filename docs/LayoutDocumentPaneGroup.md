# LayoutDocumentPaneGroup
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>

Represents an element in the layout model that can contain and organize multiple LayoutDocumentPane elements, which in turn contain LayoutDocument elements.

## Properties
|| Property || Description
| Children | Gets the child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| ChildrenCount | Gets the number of child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| DockHeight | Gets or sets the initial height of the dock. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| DockMinHeight | Gets or sets the minimum dock height. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| DockMinWidth | Gets or sets the minimum dock width. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| DockWidth | Gets or sets the initial width of the dock. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| FloatingHeight | Gets or sets the initial height of floating windows. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| FloatingLeft | Gets the initial position of the left side of a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| FloatingTop | Gets the initial position of the topside of a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| FloatingWidth | Gets or sets the initial width of floating windows. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| IsMaximized | Gets whether the element is maximized. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutDocumentPane>)
| IsVisible | Gets whether the layout group is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| Orientation | Gets or sets the orientation of the pane group.
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
| ComputeVisibility | Determines whether an element is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| GetSchema | Returns null. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| IndexOfChild | Returns the index of the specified child layout element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| InsertChildAt | Inserts the specified child layout element at the specified index. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| MoveChild | Moves a child from an old index to a new index within a pane. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| ReadXml | Overridden. Reads serialized layout information using the specified XmlReader.
| RemoveChild | Removes the specified child ILayoutElement. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| RemoveChildAt | Removes the child element at the specified index. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| ReplaceChild | Replaces a child ILayoutElement with a new one. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| ReplaceChildAt | Replaces the child element at the specified index with the specified element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutDocumentPane>)
| WriteXml | Overridden. Reads serialized layout information using the specified XmlWriter.

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---