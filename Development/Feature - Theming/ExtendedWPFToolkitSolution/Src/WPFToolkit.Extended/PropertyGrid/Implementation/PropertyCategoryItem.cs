using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyCategoryItem : Control
    {
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(PropertyCategoryItem), new UIPropertyMetadata(String.Empty, new PropertyChangedCallback(OnCategoryChanged), new CoerceValueCallback(OnCoerceCategory)));

        private static object OnCoerceCategory(DependencyObject o, object value)
        {
            PropertyCategoryItem propertyCategoryItem = o as PropertyCategoryItem;
            if (propertyCategoryItem != null)
                return propertyCategoryItem.OnCoerceCategory((string)value);
            else
                return value;
        }

        private static void OnCategoryChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyCategoryItem propertyCategoryItem = o as PropertyCategoryItem;
            if (propertyCategoryItem != null)
                propertyCategoryItem.OnCategoryChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual string OnCoerceCategory(string value)
        {
            // TODO: Keep the proposed value within the desired range.
            return value;
        }

        protected virtual void OnCategoryChanged(string oldValue, string newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        public string Category
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (string)GetValue(CategoryProperty);
            }
            set
            {
                SetValue(CategoryProperty, value);
            }
        }


        private List<PropertyItem> _Properties = new List<PropertyItem>();
        public List<PropertyItem> Properties
        {
            get
            {
                return _Properties;
            }
            set
            {
                _Properties = value;
            }
        }


        static PropertyCategoryItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyCategoryItem), new FrameworkPropertyMetadata(typeof(PropertyCategoryItem)));
        }
    }
}
