# AnimatedTimelinePanel
_Only available in the Plus Edition_

Derives from AnimationPanel

Defines an area where items are positioned on a timeline.

![](AnimatedTimelinePanel_animatedtimelinepanel.jpg)

## Properties
|| Property || Description
| * | All the Properties from [Canvas](Canvas) Panel
| BeginDate | Gets or sets the date corresponding to the beginning of the TimelinePanel.
| CanHorizontallyScroll | Gets or sets if the horizontal scrolling is enabled.
| CanVerticallyScroll | Gets or sets if the vertical scrolling is enabled.
| Date (attached) | Gets or sets the Date of a children.
| DateEnd (attached) | Gets or sets the end Date of a children.
| EndDate | Gets or sets the date corresponding to the end of the AnimatedTimelinePanel.
| ExtendHeight | Gets the Height extended for the panel.
| ExtendWidth | Gets the Width extended for the panel.
| HorizontalOffset | Gets the horizontal offset for the panel.
| KeepOriginalOrderForOverlap | Gets or sets if the orginal order should be kept when overlapping occurs.
| Orientation | Gets or sets the orientation (Vertical/Horizontal) of the Panel.
| OverlapBehavior | Gets or sets the OverlapBehavior dependency property.
| ScrollOwner | Gets or sets the ScrollViewer.
| UnitSize | Gets or sets the size of one unit.
| UnitTimeSpan | Gets or sets the TimeSpan for one unit.
| VerticalOffset | Gets the Vertical offset.
| ViewportHeight | Gets the height of the viewport.
| ViewportWidth | Gets the width of the viewport.

## Events
|| Event || Description
| * | All the Events from [Canvas](Canvas) Panel

## Methods
|| Method || Description
| LineDown() | Sets Vertical offset down by one.
| LineLeft() | Sets Horizontal offset left by one.
| LineUp() | Sets Vertical offset up by one.
| LineRight() | Sets Horizontal offset right by one.
| MakeVisible( Visual visual, Rect rectangle ) | show a children of the panel.
| MouseWheelDown() | Sets Vertical offset down by SystemParameters.WheelScrollLines.
| MouseWheelLeft() | Sets Horizontal offset left by SystemParameters.WheelScrollLines.
| MouseWheelUp() | Sets Vertical offset up by SystemParameters.WheelScrollLines.
| MouseWheelRight() | Sets Horizontal offset right by SystemParameters.WheelScrollLines.
| PageDown() | Sets Vertical offset down by page.
| PageLeft() | Sets Horizontal offset left by page.
| PageUp() | Sets Vertical offset up by page.
| PageRight() | Sets Horizontal offset right by page.
| SetHorizontalOffset( double offset ) | Sets the Horizontal offset.
| SetVerticalOffset( double offset ) | Sets the Vertical offset.
---