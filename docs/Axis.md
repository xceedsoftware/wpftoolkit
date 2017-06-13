# Axis
_Only available in the Plus Edition_

Derives from Xceed.Wpf.Toolkit.Chart.GridLine

Represents an axis.

## Properties
|| Property || Description
| LabelsType | Gets or sets type of tick labels.
| DateTimeFormatInfo | Gets or sets DateTime format, uses to convert DateTime to labels text in LabelsType.DateTime mode.
| DateTimeFormat | Gets or sets DateTime format. Used from convert DateTime to labels text in LabelsType.DateTime mode.
| Location | Gets start point of axis line.
| ScaleMode | Gets or sets scale mode of the axis.
| GraduationMode | Gets or sets graduation mode of the axis.
| CustomRangeStart | Gets or sets custom start point for manual scale mode.
| CustomRangeEnd | Gets or sets custom end point for manual scale mode.
| TickLabels | Gets list of tick labels.
| TicksCount | Gets or sets the count of ticks.
| TitleMargin | Gets or sets the margin between title label and axis line.
| TickList | Gets collection of ticks.
| GridLines | Gets collection of grid lines.
| ShowArrow | Gets or sets the visibility of the arrow.
| ShowAxis | Gets or sets the visibility of the axis.
| ShowGridLines | Gets or sets the visibility of grid lines.
| ShowTicks | Gets or sets the visibility of ticks.
| ShowTickLabels | Gets or sets the visibility of tick labels.
| ShowAxisLabel | Gets or sets the visibility of the axis label.
| Reversed | Gets or sets indicator of the axis revertion.
| Range | Gets or sets range of the axis.
| GridTemplate | Gets or sets data template for grid lines representation.
| TickTemplate | Gets or sets data template for ticks representation.
| ArrowTemplate | Gets or sets data template for arrow representation.
| LabelTemplate | Gets or sets data template for labels representation.
| Arrow | Gets or sets instance of the axis arrow.
| Title | Gets or sets text for title label.
| AxisTitleTemplate | Gets or sets data template for axis labels representation.
| AxisLabelsLayout | Gets or sets axis labels style of layout.
| GridPoint | Gets layout point used to calculate position of the element. (Inherited from GridLine)
| Orientation | Gets or sets orientation of the grid line. (Inherited from GridLine)
| Info | Gets or Sets binding source object for this primitive. (Inherited from ChartPrimitive)
| IsHighlighted | Gets or Sets, is primitive highlighted or not. (Inherited from ChartPrimitive)
| IsCovered | Gets or Sets is primitive "covered" or not. "Covered" means this primitive is not highlighted, but some other primitive is highlighted.
| DataPoint | Gets DataPoint corresponding to this primitive. (Inherited from ChartPrimitive)

## Methods
|| Method || Description
| GetRealPoint( double pt ) | Converts point in user logic coordinates into point in actual pixels.
| Reset() | Resets layout.
| SetTitlesDirection( DataRange layoutRange, bool layoutReversed ) | Sets direction of the tick labels.
| PerformGraduation() | Performs graduation of the axis.
| PerformCustomRangeGraduation() | Perform graduation for custom range in manual scale mode.

## Events
|| Event || Description
| HighlightEnter | Raised when primitive gets highlighted. (Inheridted from ChartPrimitive)
| HighlightLeave | Raised when primitive gets un-highlighted. (Inherited from ChartPrimtive)

---