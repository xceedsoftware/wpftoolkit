// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Abstract base class for Interpolator converters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An Interpolator is used to project a value from a source range 
    /// [ActualDataMinimum, ActualDataMaximum] to a target range [From, To]. 
    /// The source range can be specified directly by setting the DataMinimum 
    /// and/or DataMaximum properties, or indirectly by setting DataRangeBinding.
    /// When the DataRangeBinding property is set,the TreeMap will evaluate the 
    /// binding for the entire tree, calculating the minimum and maximum values 
    /// automatically. The custom target range and the actual interpolation 
    /// logic is defined by sub-classes of this abstract class.
    /// </para>
    /// </remarks>
    /// <QualityBand>Preview</QualityBand>
    public abstract class Interpolator : FrameworkElement
    {
        /// <summary>
        /// Holds a helper object used to extract values using a property path.
        /// </summary>
        private BindingExtractor _helper;

        /// <summary>
        /// Gets or sets a value telling to which tree nodes the interpolation 
        /// should be applied to. LeafNodesOnly by default.
        /// </summary>
        public InterpolationMode InterpolationMode { get; set; }

        /// <summary>
        /// Gets or sets a value representing the x:Name of the element to which
        /// the interpolated value will be applied.
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Gets or sets a value representing the path to a property which will 
        /// receive the interpolated value.
        /// </summary>
        public string TargetProperty { get; set; }

        #region public double DataMinimum
        /// <summary>
        /// Gets or sets a value representing the fixed minimum value across the 
        /// entire set. If the value is not set directly or is NaN, the 
        /// ActualDataMaximum will be calculated automatically from the data set 
        /// by using the DataRangeBinding property.
        /// </summary>
        public double DataMinimum
        {
            get { return (double)GetValue(DataMinimumProperty); }
            set { SetValue(DataMinimumProperty, value); }
        }

        /// <summary>
        /// Identifies the DataMinimum dependency property.
        /// </summary>
        public static readonly DependencyProperty DataMinimumProperty =
            DependencyProperty.Register(
                "DataMinimum",
                typeof(double),
                typeof(Interpolator),
                new PropertyMetadata(double.NaN));
        #endregion public double DataMinimum

        #region public double DataMaximum
        /// <summary>
        /// Gets or sets a value representing the fixed maximum value across the 
        /// entire set. If the value is not set directly or is NaN, the 
        /// ActualDataMinimum will be calculated automatically from the data set 
        /// by using the DataRangeBinding property.
        /// </summary>
        public double DataMaximum
        {
            get { return (double)GetValue(DataMaximumProperty); }
            set { SetValue(DataMaximumProperty, value); }
        }

        /// <summary>
        /// Identifies the DataMaximum dependency property.
        /// </summary>
        public static readonly DependencyProperty DataMaximumProperty =
            DependencyProperty.Register(
                "DataMaximum",
                typeof(double),
                typeof(Interpolator),
                new PropertyMetadata(double.NaN));
        #endregion public double DataMaximum

        /// <summary>
        /// This fields contains the automatically calculated maximal value in 
        /// the dataset.
        /// </summary>
        private double _actualDataMaximum;

        /// <summary>
        /// Gets the value representing the maximal value in the data set. It is
        /// automatically from the data set by using the DataRangeBinding 
        /// property if DataMaximum is not set. If it is set, DataMaximum is 
        /// returned.
        /// </summary>
        public double ActualDataMaximum
        {
            get
            {
                if (Double.IsNaN(DataMaximum))
                {
                    return _actualDataMaximum;
                }
                else
                {
                    return DataMaximum;
                }
            }

            internal set
            {
                _actualDataMaximum = value;
            }
        }

        /// <summary>
        /// This fields contains the automatically calculated minimal value in 
        /// the dataset.
        /// </summary>
        private double _actualDataMinimum;

        /// <summary>
        /// Gets the value representing the minimal value in the data set. It is
        /// automatically from the data set by using the DataRangeBinding 
        /// property if DataMinimum is not set. If it is set, DataMinimum is 
        /// returned.
        /// </summary>
        public double ActualDataMinimum
        {
            get
            {
                if (Double.IsNaN(DataMinimum))
                {
                    return _actualDataMinimum;
                }
                else
                {
                    return DataMinimum;
                }
            }

            internal set
            {
                _actualDataMinimum = value;
            }
        }

        /// <summary>
        /// Gets or sets a binding to a property which will be examined to retrieve the minimum and maximum range 
        /// values across the entire data set. If this value is null then the DataMinimum and DataMaximum values
        /// need be set manually.
        /// </summary>
        public Binding DataRangeBinding { get; set; }

        /// <summary>
        /// Initializes a new instance of the Interpolator class.
        /// </summary>
        protected Interpolator()
        {
            InterpolationMode = InterpolationMode.LeafNodesOnly;
            ActualDataMinimum = double.PositiveInfinity;
            ActualDataMaximum = double.NegativeInfinity;
            _helper = new BindingExtractor();
        }

        /// <summary>
        /// If the DataRangeBinding property is set then this method updates the minimum/maximum range
        /// of this object by including the value passed in.
        /// </summary>
        /// <param name="data">Object to extract the value from (the Source of the DataRangeBinding).</param>
        internal virtual void IncludeInRange(object data)
        {
            if (DataRangeBinding != null)
            {
                if (!Double.IsNaN(DataMinimum) && !Double.IsNaN(DataMaximum))
                {
                    return;
                }

                IConvertible input = _helper.RetrieveProperty(data, DataRangeBinding) as IConvertible;
                if (input == null)
                {
                    throw new ArgumentException(
                        Properties.Resources.Interpolator_IncludeInRange_DataRangeBindingNotIConvertible);
                }

                double value = input.ToDouble(CultureInfo.InvariantCulture);
                if (Double.IsNaN(DataMinimum) && value < ActualDataMinimum)
                {
                    ActualDataMinimum = value;
                }

                if (Double.IsNaN(DataMaximum) && value > ActualDataMaximum)
                {
                    ActualDataMaximum = value;
                }
            }
        }

        /// <summary>
        /// Called to interpolate the value of the given object between the DataMinimum and DataMaximum
        /// extremes, and to project it in a specific [From, To] range defined. The target range (and
        /// therefore the implementation of this method) is defined in a specific sub-class.
        /// </summary>
        /// <param name="value">Value to interpolate.</param>
        /// <returns>An interpolated value.</returns>
        public abstract object Interpolate(double value);
    }
}
