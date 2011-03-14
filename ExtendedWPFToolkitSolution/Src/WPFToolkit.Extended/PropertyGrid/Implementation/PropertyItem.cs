using System;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyItem : Control
    {
        #region Members

        private DependencyPropertyDescriptor _dpDescriptor;

        #endregion //Members

        #region Properties

        #region Category

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(PropertyItem), new UIPropertyMetadata(string.Empty));
        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        #endregion //Category

        public string Description { get { return PropertyDescriptor.Description; } }

        public object Instance { get; private set; }

        /// <summary>
        /// Gets if the property is data bound
        /// </summary>
        public bool IsDataBound
        {
            get
            {
                var dependencyObject = Instance as DependencyObject;
                if (dependencyObject != null && _dpDescriptor != null)
                    return BindingOperations.GetBindingExpressionBase(dependencyObject, _dpDescriptor.DependencyProperty) != null;

                return false;
            }
        }

        public bool IsReadOnly { get { return PropertyDescriptor.IsReadOnly; } }

        #region IsSelected

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(PropertyItem), new UIPropertyMetadata(false, OnIsSelectedChanged));
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static void OnIsSelectedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyItem propertyItem = o as PropertyItem;
            if (propertyItem != null)
                propertyItem.OnIsSelectedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            if (newValue)
                PropertyGrid.SelectedProperty = this;
        }

        #endregion //IsSelected

        public bool IsWriteable { get { return !IsReadOnly; } }

        public PropertyDescriptor PropertyDescriptor { get; private set; }

        public PropertyGrid PropertyGrid { get; private set; }

        public Type PropertyType { get { return PropertyDescriptor.PropertyType; } }

        #region Editor

        public static readonly DependencyProperty EditorProperty = DependencyProperty.Register("Editor", typeof(FrameworkElement), typeof(PropertyItem), new UIPropertyMetadata(null));
        public FrameworkElement Editor
        {
            get { return (FrameworkElement)GetValue(EditorProperty); }
            set { SetValue(EditorProperty, value); }
        }

        #endregion //Editor

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(PropertyItem), new UIPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(OnCoerceValue)));
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static object OnCoerceValue(DependencyObject o, object value)
        {
            PropertyItem propertyItem = o as PropertyItem;
            if (propertyItem != null)
                return propertyItem.OnCoerceValue((object)value);
            else
                return value;
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyItem propertyItem = o as PropertyItem;
            if (propertyItem != null)
                propertyItem.OnValueChanged((object)e.OldValue, (object)e.NewValue);
        }

        protected virtual object OnCoerceValue(object value)
        {
            // TODO: Keep the proposed value within the desired range.
            return value;
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }        

        #endregion //Value

        /// <summary>
        /// Gets the value source.
        /// </summary>
        public BaseValueSource ValueSource
        {
            get
            {
                var dependencyObject = Instance as DependencyObject;
                if (_dpDescriptor != null && dependencyObject != null)
                    return DependencyPropertyHelper.GetValueSource(dependencyObject, _dpDescriptor.DependencyProperty).BaseValueSource;

                return BaseValueSource.Unknown;
            }
        }

        #endregion //Properties

        #region Constructor

        static PropertyItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyItem), new FrameworkPropertyMetadata(typeof(PropertyItem)));
        }

        public PropertyItem(object instance, PropertyDescriptor property, PropertyGrid propertyGrid)
        {
            Instance = instance;
            PropertyDescriptor = property;
            Name = PropertyDescriptor.Name;
            Category = PropertyDescriptor.Category;
            PropertyGrid = propertyGrid;

            _dpDescriptor = DependencyPropertyDescriptor.FromProperty(property);
        }

        #endregion //Constructor

        #region Base Class Overrides

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;

            if (Editor != null)
                Editor.Focus();

            e.Handled = true;
        }

        #endregion //Base Class Overrides
    }
}
