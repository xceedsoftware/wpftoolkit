// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows.Data;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Helper class which can extract the value from a source object using a binding path. It 
    /// creates a Binding object based on the path, and calls SetBinding to a temporary 
    /// FrameworkElement (base class) to extract the value.
    /// </summary>
    internal class BindingExtractor : FrameworkElement
    {
        #region public object Value
        /// <summary>
        /// Gets or sets a generic Value which will be the target of the binding.
        /// </summary>
        private object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(object),
                typeof(BindingExtractor),
                null);
        #endregion

        /// <summary>
        /// Returns the value of the given Binding when applied on the given object instance.
        /// It does that by making a copy of the binding, setting its source to be the object
        /// instance and the target to be the member Value property.
        /// </summary>
        /// <param name="instance">Object instance containing the property.</param>
        /// <param name="valueBinding">Binding to the property to be retrieved.</param>
        /// <returns>The value of the binding.</returns>
        public object RetrieveProperty(object instance, Binding valueBinding)
        {
            // We need to make a new instance each time because you can't change 
            // the Source of a Binding once it has been set.
            Binding binding = new Binding()
            {
                BindsDirectlyToSource = valueBinding.BindsDirectlyToSource,
                Converter = valueBinding.Converter,
                ConverterCulture = valueBinding.ConverterCulture,
                ConverterParameter = valueBinding.ConverterParameter,
                Mode = valueBinding.Mode,
                NotifyOnValidationError = valueBinding.NotifyOnValidationError,
                Path = valueBinding.Path,
                Source = instance,
                UpdateSourceTrigger = valueBinding.UpdateSourceTrigger,
                ValidatesOnExceptions = valueBinding.ValidatesOnExceptions,
#if !NO_COMPLETE_BINDING_PROPERTY_LIST
                FallbackValue = valueBinding.FallbackValue,
                StringFormat = valueBinding.StringFormat,
                TargetNullValue = valueBinding.TargetNullValue,
                ValidatesOnDataErrors = valueBinding.ValidatesOnDataErrors,
#endif
#if !NO_VALIDATESONNOTIFYDATAERRORS
                ValidatesOnNotifyDataErrors = valueBinding.ValidatesOnNotifyDataErrors,
#endif
            };

            SetBinding(BindingExtractor.ValueProperty, binding);

            // Now this dependency property has been updated.
            return Value;
        }
    }
}
