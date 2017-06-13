# Series
_Only available in the Plus Edition_

Derives from DependencyObject

Displays a list of DataPoint primitives in an Area of a Chart.

## Properties
|| Property || Description
| ShowHintLabels | Gets or Sets, show hint labels for series elements or not.
| ShowPointsInLegend | If true, DataPoints from this Series are displayed in legend.
| IsOwnerHighlight | Gets or Sets, is this Series performing highlight of it's child elements manually or not.
| MarkerTemplate | Gets or Sets DataTemplate for Markers.
| Template | Gets or Sets DataTemplate for this Series elements.
| HintLabelTemplate | Gets or sets hint label template for elements of this series.
| HintLineTemplate | Gets or sets hint line template for elements of this series.
| HintLineLength | Gets or sets hint line length.
| Title | Gets or Sets Series Title. Displayed in LegendItem
| Spacing | Gets or Sets Spacing between series elements. this value is % relative to element width. Example: if for Column layout type spacing is 20, then column width is 80.
| Layout | Gets or Sets LayoutEngine for this Series, that will layout this Series.
| DefaultInterior | Gets or Sets default interior color for series elements.
| Area | Gets parent Area
| DataPointBindings | Gets list of BindingInfo objects. Add BindingInfo objects to this list to set up binding for DataPoints.
| DataPointsSource | Gets or Sets binding source for DataPoints.
| DataPoints | Gets or sets list of DataPoint.
| LayoutPrimitives | Gets list of ChartPrimitive for layout elements.
| LayoutHints | Gets list of ChartPrimitive for layout hints.

## Methods
|| Method || Description
| ApplyDefaultTemplate() | Applies default DataTemplates for series elements.
| GetSortedPoints() | Gets array of DataPoints contained in this series, sorted if needed.
| Reset() | Clears list of chart primitives.

---