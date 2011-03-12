using System;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyItem : Control
    {
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
        }

        #endregion //Constructor

        #region Base Class Overrides

        protected override void OnPreviewMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        #endregion //Base Class Overrides
    }
}
