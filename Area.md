# Area
_Only available in the Plus Edition_

Derives from Panel

Used to display charts and a grid with axes and labels. Use the [Series](Series) property to specify the list of DataPoints.

* [Series](Series)
* [Axis](Axis)

## Properties
|| Property || Description
| Title | Gets or sets Area title.
| XAxis | Gets or sets X axis.
| YAxis | Gets or sets Y axis.
| Series | Gets or sets list of Series.
| LayoutBounds | Gets layout bounds for series interior.
| BackgroundTemplate | Gets or sets data template for grid representation.

## Methods
|| Method|| Description
| GetSeriesLayoutBounds() | Calculates layout bounds without tick and tittle labels.
| Invalidate() | Invalidates Area and all it's children.
| GetActualPoint( Point point ) | Converts point in user logic coordinates into actual pixels point.
| GetActualPoint( DataPoint point ) | Converts DataPoint into actual pixels point.
| HighlightSeries( Series series, bool highlighted ) | Sets highlighted state for specified series.

## Events
|| Event || Description
| LegendRefresh | Raised when need legend items refresh.

---