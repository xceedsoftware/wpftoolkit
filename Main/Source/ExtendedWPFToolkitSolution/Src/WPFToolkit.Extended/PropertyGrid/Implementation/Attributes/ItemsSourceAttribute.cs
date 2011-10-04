using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Attributes
{
    public class ItemsSourceAttribute : Attribute
    {
        public Type Type { get; set; }

        public ItemsSourceAttribute(Type type)
        {
            var valueSourceInterface = type.GetInterface("Microsoft.Windows.Controls.PropertyGrid.Attributes.IItemsSource");
            if (valueSourceInterface == null)
                throw new ArgumentException("Type must implement the IItemsSource interface.", "type");

            Type = type;
        }
    }
}