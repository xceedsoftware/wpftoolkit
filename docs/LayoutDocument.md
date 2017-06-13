# LayoutDocument
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutContent

Represents a document in the layout model

## Properties
|| Property || Description
| CanClose | Gets or sets whether the content can be closed definitively (removed from the layout and not just hidden). (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| CanFloat | Gets whether the content can be moved to a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Content | Gets or sets the content of the LayoutContent instance. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| ContentId | Gets or sets the ID of the content, which is used to identify the content during serialization/deserialization. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Description | Gets or sets the document's description.
| FloatingHeight | Gets or sets the height that will be initially used when the content is dragged and then displayed in a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| FloatingLeft | Gets or sets the left edge of a floating window that will contain this content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| FloatingTop | Gets or sets the top edge of a floating window that will contain this content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| FloatingWidth | Gets or sets the width that will be initially used when the content is dragged and then displayed in a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IconSource | Gets the icon source of the content (displayed next on the tab). (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsActive | Gets whether the content is active. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsFloating | Gets or sets whether the content is in a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsLastFocusedDocument | Gets whether the content is the last focused document. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsMaximized | Gets or sets whether a content element is maximized. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsSelected | Gets or sets whether a content element is selected. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsVisible | Gets or sets whether the document is visible.  
| LastActivationTimeStamp | Gets or sets the date and time of the last activation of the content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Parent | Gets or sets the parent container of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PreviousContainerIndex | Gets or sets the index of the previous container. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Root | Gets or sets the root of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| Title | Gets or sets the title of the content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| ToolTip | Gets or sets the tooltip of the content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)

## Events
|| Event || Description
| Closed | Raised when the content is closed (i.e., removed definitively from the layout). (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Closing | Raised when the content is about to be closed (i.e. removed definitively from the layout). (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsActiveChanged | Raised when the IsActive property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsSelectedChanged | Raised when the IsSelected property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| PropertyChanged | Raised when a property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PropertyChanging | Raised when a property is about to change. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)

## Methods
|| Method || Description
| CompareTo | Compares the content of the current instance with the content of the specified object. If it the content cannot be compared, the titles are compared. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Dock | Re-dock the content to its previous container. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| DockAsDocument | Dock the content as document. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Float | Programmatically creates a floating window of the content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| GetSchema | Returns null. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| ReadXml | Overridden. Reads serialized layout information using the specified XmlReader.
| WriteXml | Overridden. Writes serialized layout information using the specified XmlWriter.

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---