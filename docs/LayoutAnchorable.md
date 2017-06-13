# LayoutAnchorable
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutContent

Represents an anchorable in the layout model.

## Properties
|| Property || Description
| AutoHideHeight | Gets or sets the height to use when auto-hidden anchorables are shown for the first time.
| AutoHideMinHeight |
| AutoHideMinWidth | Gets or sets the width to use when auto-hidden anchorables are shown for the first time.
| AutoHideWidth |
| CanAutoHide | Gets or sets whether an anchorable can be autohidden.
| CanClose | Gets or sets whether the content can be closed definitively (removed from the layout and not just hidden). (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| CanFloat | Gets whether the content can be moved to a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| CanHide | Gets or sets whether an anchorable can be hidden.
| Content | Gets or sets the content of the LayoutContent instance. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| ContentId | Gets or sets the ID of the content, which is used to identify the content during serialization/deserialization. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| FloatingHeight | Gets or sets the height that will be initially used when the content is dragged and then displayed in a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| FloatingLeft | Gets or sets the left edge of a floating window that will contain this content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| FloatingTop | Gets or sets the top edge of a floating window that will contain this content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| FloatingWidth | Gets or sets the width that will be initially used when the content is dragged and then displayed in a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IconSource | Gets the icon source of the content (displayed next on the tab). (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsActive | Gets whether the content is active. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsAutoHidden | Gets whether the anchorable is anchored to a border in an autohidden state.
| IsFloating | Gets or sets whether the content is in a floating window. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsHidden | Gets whether the anchorable can be hidden.
| IsLastFocusedDocument | Gets whether the content is the last focused document. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsMaximized | Gets or sets whether a content element is maximized. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsSelected | Gets or sets whether a content element is selected. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsVisible | Gets or sets whether the anchorable is visible.
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
| Hiding | Raised when the anchorable has been hidden (usually by the end-user clicking on the "X" button).
| IsActiveChanged | Raised when the IsActive property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsSelectedChanged | Raised when the IsSelected property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| IsVisibleChanged | Raised when the IsVisible property changes.
| PropertyChanged | Raised when a property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PropertyChanging | Raised when a property is about to change. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)

## Methods
|| Method || Description
| AddToLayout | Add the anchorable to a DockingManager layout.
| CompareTo | Compares the content of the current instance with the content of the specified object. If it the content cannot be compared, the titles are compared. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Dock | Re-dock the content to its previous container. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| DockAsDocument | Dock the content as document. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Float | Programmatically creates a floating window of the content. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| GetSchema | Returns null. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutContent)
| Hide | Hide this content and add it to the ILayoutRoot.Hidden collection of parent root.
| ReadXml | Overridden. Reads serialized layout information using the specified XmlReader.
| Show | Shows the content if it was previously hidden.
| ToggleAutoHide | Toggles autohide state.
| WriteXml | Overridden. Writes serialized layout information using the specified XmlWriter.

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---