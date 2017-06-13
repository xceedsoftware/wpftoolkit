# Zoombox
Derives from ContentControl

The Zoombox control provides zoom and pan functionality for its content. It also provides ViewFinder functionality so that the user can identify where they are viewing within the bigger picture. Finally, it provides a view stack and various navigation methods so that the user can navigate back or forward through previous views or zoom to common views.

## Properties
|| Property || Description
| AnimationAccelerationRatio | Gets or sets the acceleration ratio used for scale and pan animations.   
| AnimationDecelerationRatio | Gets or sets the deceleration ratio used for scale and pan animations.   
| AnimationDuration | Gets or sets the duration used for scale and pan animations.   
| AreDragModifiersActive | Gets whether the keys in the DragModifiers collection are currently pressed.   
| AreRelativeZoomModifiersActive | Gets whether the keys in the RelativeZoomModifiers collection are currently pressed.   
| AreZoomModifiersActive | Gets whether the keys in the ZoomModifiers collection are currently pressed.   
| AreZoomToSelectionModifiersActive | Gets whether the keys in the ZoomToSelectionModifiers collection are currently pressed.   
| AutoWrapContentWithViewbox | Gets or sets whether the content of the Zoombox is wrapped in a Viewbox.   
| CurrentView | Gets the current view for the Zoombox.   
| CurrentViewIndex | Gets the index of the current view (see CurrentView) within the Zoombox control's view stack (see the ViewStack).   
| DragModifiers | Gets or sets keys that must be pressed to pan the content by dragging it.   
| DragOnPreview | Gets or sets whether the Zoombox responds to tunneled mouse events to pan the content via a mouse drag.   
| EffectiveViewStackMode | Gets the mode that is currently in effect for the view stack.   
| HasBackStack | Gets whether there are views on the back side of the view stack (see ViewStack).   
| HasForwardStack | Gets whether there are views on the forward side of the view stack (see ViewStack).   
| IsAnimated | Gets or sets whether transitions between views (scale and pan operations) are animated.   
| IsDraggingContent | Gets whether the content is currently being panned via a drag operation.   
| IsSelectingRegion | Gets whether a zoom region is currently being selected.   
| IsUsingScrollbars | Gets or sets a boolean value that specifies whether the Zoombox's scrollbars are enabled to move the content of the Zoombox when it is zoomed.     
| KeepContentInBounds | Gets or sets if we should keep the content in bounds.   
| MaxScale | Gets or sets the maximum scale factor (or zoom level) that can be applied to the content.   
| MinScale | Gets or sets the minimum scale factor (or zoom level) that can be applied to the content.   
| NavigateOnPreview | Gets or sets whether the Zoombox responds to tunneled mouse events (PreviewMouseDown) to navigate through the view stack.   
| PanDistance | Gets or sets how many pixels the content will pan using the PanLeft, PanUp, PanRight, or PanDown Zoombox commands.   
| Position | Gets or sets the top-left point of the content within the Zoombox control.   
| RelativeZoomModifiers | Gets or sets keys that must be pressed to zoom the content relative to the current mouse position using the mouse wheel.   
| Scale | Gets or sets the scale factor (or zoom level) for the content.   
| ViewFinder | Gets a reference to the view finder element within the visual tree of the Zoombox.   
| Viewport | Gets the portion of the content that is visible in the Zoombox. The viewport is expressed in the coordinate space of the content.   
| ViewStack | Gets the view stack for the Zoombox.   
| ViewStackCount | Gets the number of views in the view stack.   
| ViewStackIndex | Gets or sets the index of the selected view within the view stack.   
| ViewStackMode | Gets or sets the mode for the view stack.   
| ViewStackSource | Gets or sets an IEnumerable value representing a collection used to generate a view stack for the Zoombox.   
| ZoomModifiers | Gets or sets keys that must be pressed to zoom the content relative to its ZoomOrigin using the mouse wheel.   
| ZoomOn | Gets or sets what the zoombox should zoom on.   
| ZoomOnPreview | Gets or sets whether the Zoombox responds to tunneled mouse events (PreviewMouseWheel) to zoom the content via the mouse wheel.   
| ZoomOrigin | Gets or sets the relative position within the content around which Zoom operations occur by default.   
| ZoomPercentage | Gets or sets how much the content will zoom when using the ZoomIn or ZoomOut commands or when zooming via the mouse wheel.   
| ZoomToSelectionModifiers | Gets or sets keys that must be pressed to select and zoom to a location of the content using the mouse.   

## Events
|| Event || Description
| AnimationBeginning | Raised when the transition animation is about to begin.   
| AnimationCompleted | Raised when the transition animation completes.   
| CurrentViewChanged | Raised when the CurrentView property changes.  
| ViewStackIndexChanged | Raised when the ViewStackIndex property changes.

## Methods
|| Method || Description
| CenterContent | Centers the content within the Zoombox control without changing its scale.   
| FillToBounds | Scales the content to completely fill the bounds of the Zoombox control.   
| FitToBounds | Scales the content to fit within the bounds of the Zoombox control.   
| GetViewFinderVisibility | Gets the ViewFinderVisibility attached dependency property.   
| GoBack | Navigates to the previous view on the view stack.   
| GoForward | Navigates to the next view on the view stack.   
| GoHome | Navigates to the first view on the view stack.   
| OnApplyTemplate. | Overridden to give us an opportunity to initialize the control.   
| RefocusView | Refocuses the view currently selected within the view stack (the view identified by the ViewStackIndex property).   
| SetViewFinderVisibility | Sets the ViewFinderVisibility attached dependency property.   
| Zoom Overloaded. | Scales the content by a given percentage relative to the point identified by the ZoomOrigin property.   
| ZoomTo Overloaded. | Scales the content to a specific scale value relative to the ZoomOrigin.   

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---