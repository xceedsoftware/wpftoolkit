using System;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyItem : Control
    {
        #region Members

        public object Instance { get; private set; }
        public PropertyDescriptor PropertyDescriptor { get; private set; }

        #endregion //Members

        #region Properties

        public string Description
        {
            get { return PropertyDescriptor.Description; }
        }

        public bool IsWriteable
        {
            get { return !IsReadOnly; }
        }

        public bool IsReadOnly
        {
            get { return PropertyDescriptor.IsReadOnly; }
        }

        public Type PropertyType
        {
            get { return PropertyDescriptor.PropertyType; }
        }

        //public string Category
        //{
        //    get { return PropertyDescriptor.Category; }
        //}

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(PropertyItem), new UIPropertyMetadata(string.Empty, new PropertyChangedCallback(OnCategoryChanged), new CoerceValueCallback(OnCoerceCategory)));

        private static object OnCoerceCategory(DependencyObject o, object value)
        {
            PropertyItem propertyItem = o as PropertyItem;
            if (propertyItem != null)
                return propertyItem.OnCoerceCategory((string)value);
            else
                return value;
        }

        private static void OnCategoryChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyItem propertyItem = o as PropertyItem;
            if (propertyItem != null)
                propertyItem.OnCategoryChanged((string)e.OldValue, (string)e.NewValue);
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


        #region Editor

        public static readonly DependencyProperty EditorProperty = DependencyProperty.Register("Editor", typeof(FrameworkElement), typeof(PropertyItem), new UIPropertyMetadata(null, OnEditorChanged));
        public FrameworkElement Editor
        {
            get { return (FrameworkElement)GetValue(EditorProperty); }
            set { SetValue(EditorProperty, value); }
        }

        private static void OnEditorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyItem propertyItem = o as PropertyItem;
            if (propertyItem != null)
                propertyItem.OnEditorChanged((FrameworkElement)e.OldValue, (FrameworkElement)e.NewValue);
        }

        protected virtual void OnEditorChanged(FrameworkElement oldValue, FrameworkElement newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //Editor

        #endregion //Properties

        #region Constructor

        static PropertyItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyItem), new FrameworkPropertyMetadata(typeof(PropertyItem)));
        }

        public PropertyItem(object instance, PropertyDescriptor property)
        {
            Instance = instance;
            PropertyDescriptor = property;
            Name = PropertyDescriptor.Name;
            Category = PropertyDescriptor.Category;
        }

        #endregion //Constructor

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
