# LayoutContent
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutElement

Base class of the LayoutAnchorable and LayoutDocument classes.

## Properties
|| Property || Description
| CanClose | Gets or sets whether the content can be closed definitively (removed from the layout and not just hidden).
| CanFloat | Gets whether the content can be moved to a floating window.
| Content | Gets or sets the content of the LayoutContent instance.
| ContentId | Gets or sets the ID of the content, which is used to identify the content during serialization/deserialization.
| FloatingHeight | Gets or sets the height that will be initially used when the content is dragged and then displayed in a floating window.
| FloatingLeft | Gets or sets the left edge of a floating window that will contain this content.
| FloatingTop | Gets or sets the top edge of a floating window that will contain this content.
| FloatingWidth | Gets or sets the width that will be initially used when the content is dragged and then displayed in a floating window.
| IconSource | Gets the icon source of the content (displayed next on the tab).
| IsActive | Gets whether the content is active.
| IsEnabled | Gets or sets if the LayoutDocument or LayoutAnchorable is Enabled. Default is True.
| IsFloating | Gets or sets whether the content is in a floating window.
| IsLastFocusedDocument | Gets whether the content is the last focused document.
| IsMaximized | Gets or sets whether a content element is maximized.
| IsSelected | Gets or sets whether a content element is selected.
| LastActivationTimeStamp | Gets or sets the date and time of the last activation of the content.
| Parent | Gets or sets the parent container of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PreviousContainerIndex | Gets or sets the index of the previous container.
| Root | Gets or sets the root of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| Title | Gets or sets the title of the content.
| ToolTip | Gets or sets the tooltip of the content.

## Events
|| Event || Description
| Closed | Raised when the content is closed (i.e., removed definitively from the layout).
| Closing | Raised when the content is about to be closed (i.e. removed definitively from the layout).
| IsActiveChanged | Raised when the IsActive property has changed.
| IsSelectedChanged | Raised when the IsSelected property has changed.
| PropertyChanged | Raised when a property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PropertyChanging | Raised when a property is about to change. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)

## Methods
|| Method || Description
| Close | Close the content.  
| CompareTo | Compares the content of the current instance with the content of the specified object. If it the content cannot be compared, the titles are compared.  
| Dock | Re-dock the content to its previous container  
| DockAsDocument | Dock the content as document  
| Float | Programmatically creates a floating window of the content.  
| GetSchema | Returns null.  
| ReadXml | Reads serialized layout information using the specified XmlReader.  
| WriteXml | Writes serialized layout information using the specified XmlWriter.  

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---