# WindowControl
Derives from ContentControl

The control used to build the [ChildWindow](ChildWindow) and [MessageBox](MessageBox). It contains the base data for a window.

## Properties
|| Property || Description
| Caption | Gets or sets the Title of the Window.
| CaptionForeground | Gets or sets the foreground of the title of the window.
| CaptionShadowBrush | Gets or sets the glow effect brush that highlights the window title.
| CaptionIcon | Gets or sets the Image to use near the window title.
| CloseButtonStyle | Gets or sets the style for the Close Button.
| CloseButtonVisibility | Gets or sets if the Close button is visible in the window.
| IsActive | Gets or sets if the window has the focus.
| Left | Gets or sets the position in X of the top left corner of the window in pixel.
| Top | Gets or sets the position in Y of the top left corner of the window in pixel.
| ResizeMode | Gets or sets the mode for resizing (NoResize, CanMinimize, CanResize).
| WindowBackground | Gets or sets the Background for the Window chrome border.
| WindowBorderBrush | Gets or sets the Border Brush for the window.
| WindowBorderThickness | Gets or sets the Thickness for the window.
| WindowInactiveBackground | Gets or sets the Background for the window chrome border when window is inactive.
| WindowOpacity | Gets or sets the Opacity for the Window chrome border.
| WindowStyle | Gets or sets the style for the Window (None, SingleBorderWindow, ThreeDBorderWindow or ToolWindow).
| WindowThickness | Gets or sets the Thickness for the Window chrome Border.

## Events
|| Event || Description
| HeaderMouseLeftButtonClicked | Raised when the header is clicked using the left mouse button.
| HeaderMouseLeftButtonDoubleClicked | Raised when the header is double-clicked with the left mouse button.
| HeaderMouseRightButtonClicked | Raised when the header is clicked using the right mouse button.
| HeaderDragDelta | Raised when the Window is dragged.
| HeaderIconClicked | Raised when the header icon is clicked.
| HeaderIconDoubleClicked | Raised when the header icon is double-clicked.
| CloseButtonClicked | Raised when the Close button is clicked.

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---