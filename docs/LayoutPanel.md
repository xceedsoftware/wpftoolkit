# LayoutPanel
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>

Represents a panel that arranges child panes (e.g., LayoutAnchorablePane and  LayoutDocumentPane), which in turn contain the actual content (that is, LayoutAnchorable or LayoutDocument elements), using a specified Orientation and adding a resizer between them.

## Properties
|| Property || Description
| Children | Gets the child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| ChildrenCount | Gets the number of child elements. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| DockHeight | Gets or sets the initial height of the dock. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| DockMinHeight | Gets or sets the minimum dock height. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| DockMinWidth | Gets or sets the minimum dock width. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| DockWidth | Gets or sets the initial width of the dock. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| FloatingHeight | Gets or sets the initial height of floating windows. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| FloatingLeft | Gets the initial position of the left side of a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| FloatingTop | Gets the initial position of the topside of a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| FloatingWidth | Gets or sets the initial width of floating windows. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| IsMaximized | Gets whether the element is maximized. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup<ILayoutPanelElement>)
| IsVisible | Gets whether the layout group is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| Orientation | Gets or sets the orientation of the panes the panel contains.  
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
| ComputeVisibility | Determines whether an element is visible. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| GetSchema | Returns null. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| IndexOfChild | Returns the index of the specified child layout element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| InsertChildAt | Inserts the specified child layout element at the specified index. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| MoveChild | Moves a child from an old index to a new index within a pane (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| ReadXml | Overridden. Reads serialized layout information using the specified XmlReader.  
| RemoveChild | Removes the specified child ILayoutElement. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| RemoveChildAt | Removes the child element at the specified index. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| ReplaceChild | Replaces a child ILayoutElement with a new one. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| ReplaceChildAt | Replaces the child element at the specified index with the specified element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutGroup<ILayoutPanelElement>)
| WriteXml | Overridden. Writes serialized layout information using the specified XmlWriter.  

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---