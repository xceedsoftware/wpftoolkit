// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a data point used for a scatter series.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplateVisualState(Name = DataPoint.StateCommonNormal, GroupName = DataPoint.GroupCommonStates)]
    [TemplateVisualState(Name = DataPoint.StateCommonMouseOver, GroupName = DataPoint.GroupCommonStates)]
    [TemplateVisualState(Name = DataPoint.StateSelectionUnselected, GroupName = DataPoint.GroupSelectionStates)]
    [TemplateVisualState(Name = DataPoint.StateSelectionSelected, GroupName = DataPoint.GroupSelectionStates)]
    [TemplateVisualState(Name = DataPoint.StateRevealShown, GroupName = DataPoint.GroupRevealStates)]
    [TemplateVisualState(Name = DataPoint.StateRevealHidden, GroupName = DataPoint.GroupRevealStates)]
    public partial class ScatterDataPoint : DataPoint
    {
#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the ScatterDataPoint class.
        /// </summary>
        static ScatterDataPoint()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScatterDataPoint), new FrameworkPropertyMetadata(typeof(ScatterDataPoint)));
        }

#endif
        /// <summary>
        /// Initializes a new instance of the ScatterDataPoint class.
        /// </summary>
        public ScatterDataPoint()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(ScatterDataPoint);
#endif
        }
    }
}