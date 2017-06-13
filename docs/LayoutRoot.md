# LayoutRoot
Derives from Xceed.Wpf.AvalonDock.Layout.LayoutElement

Represents the root of the layout model.

## Properties
|| Property || Description
| ActiveContent | Gets the active LayoutContent-derived element.  
| BottomSide | Gets or sets the bottom side of the layout root.  
| Children | Gets the child elements of the layout root.  
| ChildrenCount | Gets the number of child elements.  
| FloatingWindows | Gets the floating windows that are in the layout.  
| Hidden | Gets the hidden anchorables in the layout.  
| LastFocusedDocument | Gets the last focused content.  
| LeftSide | Gets or sets the left side of the layout root.  
| Manager | Gets the DockingManager that arranges the panes it contains and handles autohide anchorables and floating windows.  
| Parent | Gets or sets the parent container of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| RightSide | Gets or sets the right side of the layout root.  
| Root | Gets or sets the root of the element. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| RootPanel | Gets or sets the root layout panel.  
| TopSide | Gets or sets the top side of the layout root.  

## Events
|| Event || Description
| ElementAdded | Raised when an element is added to the layout.  
| ElementRemoved | Raised when an element is removed from the layout.  
| PropertyChanged | Raised when a property has changed. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| PropertyChanging | Raised when a property is about to change. (Inherited from Xceed.Wpf.AvalonDock.Layout.LayoutElement)
| Updated | Raised when the layout is updated.  

## Methods
|| Method || Description
| CollectGarbage | Removes any empty containers not directly referenced by other layout items.  
| RemoveChild | Removes the specified child element.  
| ReplaceChild | Replaces on child element with another.  

**Support this project, check out the [Plus Edition](http://wpftoolkit.com).**
---