using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Windows.Controls.PropertyGrid.Commands;
using System.Windows.Markup.Primitives;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyItem : Control
    {
        #region Members

        private DependencyPropertyDescriptor _dpDescriptor;
        private MarkupObject _markupObject;

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

        #region DisplayName

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(PropertyItem), new UIPropertyMetadata(null));
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }        

        #endregion //DisplayName

        #region Editor

        public static readonly DependencyProperty EditorProperty = DependencyProperty.Register("Editor", typeof(FrameworkElement), typeof(PropertyItem), new UIPropertyMetadata(null));
        public FrameworkElement Editor
        {
            get { return (FrameworkElement)GetValue(EditorProperty); }
            set { SetValue(EditorProperty, value); }
        }

        #endregion //Editor

        private object _instance;
        public object Instance
        {
            get
            {
                return _instance;
            }
            private set
            {
                _instance = value;
                _markupObject = MarkupWriter.GetMarkupObjectFor(_instance);
            }
        }

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

        public bool IsDynamicResource
        {
            get
            {
                var markupProperty = _markupObject.Properties.Where(p => p.Name == PropertyDescriptor.Name).FirstOrDefault();
                if (markupProperty != null)
                    return markupProperty.Value is DynamicResourceExtension;
                return false;
            }
        }

        public bool HasResourceApplied
        {
            //TODO: need to find a better way to determine if a StaticResource has been applied to any property not just a style
            get
            {
                var markupProperty = _markupObject.Properties.Where(p => p.Name == PropertyDescriptor.Name).FirstOrDefault();
                if (markupProperty != null)
                    return markupProperty.Value is Style;

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

        private PropertyDescriptor _propertyDescriptor;
        public PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return _propertyDescriptor;
            }
            private set
            {
                _propertyDescriptor = value;
                Name = _propertyDescriptor.Name;
                DisplayName = _propertyDescriptor.DisplayName;
                Category = _propertyDescriptor.Category;
                _dpDescriptor = DependencyPropertyDescriptor.FromProperty(_propertyDescriptor);
            }
        }

        public PropertyGrid PropertyGrid { get; private set; }

        public Type PropertyType { get { return PropertyDescriptor.PropertyType; } }

        public ICommand ResetValueCommand { get; private set; }

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(PropertyItem), new UIPropertyMetadata(null, OnValueChanged));
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyItem propertyItem = o as PropertyItem;
            if (propertyItem != null)
                propertyItem.OnValueChanged((object)e.OldValue, (object)e.NewValue);
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

            PropertyDescriptor = property;
            PropertyGrid = propertyGrid;
            Instance = instance;

            CommandBindings.Add(new CommandBinding(PropertyItemCommands.ResetValue, ExecuteResetValueCommand, CanExecuteResetValueCommand));

            AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(PropertyItem_MouseDown), true);
            AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(PropertyItem_PreviewMouseDown), true);
        }

        #endregion //Constructor

        #region Event Handlers

        void PropertyItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Editor != null)
                Editor.Focus();

            e.Handled = true;
        }

        void PropertyItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsSelected = true;

            //if it is a comboBox then the selection will not take when Focus is called
            if (!(e.Source is ComboBox))
                Focus();
        }

        #endregion  //Event Handlers

        #region Commands

        private void ExecuteResetValueCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (PropertyDescriptor.CanResetValue(Instance))
                PropertyDescriptor.ResetValue(Instance);

            //TODO: notify UI that the ValueSource may have changed to update the icon
        }

        private void CanExecuteResetValueCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            bool canExecute = false;

            if (PropertyDescriptor.CanResetValue(Instance) && !PropertyDescriptor.IsReadOnly)
            {
                canExecute = true;
            }

            e.CanExecute = canExecute;
        }

        #endregion //Commands
    }
}
