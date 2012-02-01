using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup.Primitives;
using Microsoft.Windows.Controls.PropertyGrid.Commands;
using Microsoft.Windows.Controls.PropertyGrid.Attributes;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyItem : Control
    {
        #region Members

        private DependencyPropertyDescriptor _dpDescriptor;
        private MarkupObject _markupObject;

        #endregion //Members

        #region Properties

        public string BindingPath { get; private set; }

        #region Category

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(PropertyItem), new UIPropertyMetadata(string.Empty));
        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        #endregion //Category

        #region Description

        public string Description
        {
            get { return PropertyDescriptor.Description; }
        }

        #endregion //Description

        #region DisplayName

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(PropertyItem), new UIPropertyMetadata(null));
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        #endregion //DisplayName

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
            if (oldValue != null)
                oldValue.DataContext = null;

            if (newValue != null)
                newValue.DataContext = this;
        }

        #endregion //Editor

        #region Instance

        private object _instance;
        public object Instance
        {
            get { return _instance; }
            private set
            {
                _instance = value;
                _markupObject = MarkupWriter.GetMarkupObjectFor(_instance);
            }
        }

        #endregion //Instance

        #region IsDataBound

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

        #endregion //IsDataBound

        #region IsDynamicResource

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

        #endregion //IsDynamicResource

        #region IsExpanded

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(PropertyItem), new UIPropertyMetadata(false, OnIsExpandedChanged));
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        private static void OnIsExpandedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyItem propertyItem = o as PropertyItem;
            if (propertyItem != null)
                propertyItem.OnIsExpandedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsExpandedChanged(bool oldValue, bool newValue)
        {
            if (newValue && (Properties == null || Properties.Count == 0))
            {
                GetChildProperties();
            }
        }

        #endregion IsExpanded

        #region HasChildProperties

        public static readonly DependencyProperty HasChildPropertiesProperty = DependencyProperty.Register("HasChildProperties", typeof(bool), typeof(PropertyItem), new UIPropertyMetadata(false));
        public bool HasChildProperties
        {
            get { return (bool)GetValue(HasChildPropertiesProperty); }
            set { SetValue(HasChildPropertiesProperty, value); }
        }

        #endregion HasChildProperties

        #region HasResourceApplied

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

        #endregion //HasResourceApplied

        public bool IsReadOnly { get; private set; }

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

        #region Level

        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(PropertyItem), new UIPropertyMetadata(0));
        public int Level
        {
            get { return (int)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        #endregion //Level

        #region Properties

        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(PropertyItemCollection), typeof(PropertyItem), new UIPropertyMetadata(null));
        public PropertyItemCollection Properties
        {
            get { return (PropertyItemCollection)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        #endregion //Properties

        #region PropertyDescriptor

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
                _dpDescriptor = DependencyPropertyDescriptor.FromProperty(_propertyDescriptor);
            }
        }

        #endregion //PropertyDescriptor

        public PropertyGrid PropertyGrid { get; private set; }

        public int PropertyOrder { get; set; } //maybe make a DP

        #region PropertyType

        public Type PropertyType
        {
            get { return PropertyDescriptor.PropertyType; }
        }

        #endregion //PropertyType

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
            if (IsInitialized)
            {
                PropertyGrid.RaiseEvent(new PropertyValueChangedEventArgs(PropertyGrid.PropertyValueChangedEvent, this, oldValue, newValue));
            }
        }

        #endregion //Value

        #region ValueSource

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

        #endregion //ValueSource

        #endregion //Properties

        #region Constructors

        static PropertyItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyItem), new FrameworkPropertyMetadata(typeof(PropertyItem)));
        }

        public PropertyItem(object instance, PropertyDescriptor property, PropertyGrid propertyGrid, string bindingPath)
        {
            PropertyDescriptor = property;
            PropertyGrid = propertyGrid;
            Instance = instance;
            BindingPath = bindingPath;

            SetPropertyDescriptorProperties();
            ResolveExpandableObject();
            ResolvePropertyOrder();

            CommandBindings.Add(new CommandBinding(PropertyItemCommands.ResetValue, ExecuteResetValueCommand, CanExecuteResetValueCommand));
            AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(PropertyItem_PreviewMouseDown), true);
        }

        #endregion //Constructors

        #region Event Handlers

        void PropertyItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsSelected = true;
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

        #region Methods

        private void GetChildProperties()
        {
            if (Value == null)
                return;

            var propertyItems = new List<PropertyItem>();

            try
            {
                PropertyDescriptorCollection descriptors = PropertyGridUtilities.GetPropertyDescriptors(Value);

                foreach (PropertyDescriptor descriptor in descriptors)
                {
                    if (descriptor.IsBrowsable)
                        propertyItems.Add(PropertyGridUtilities.CreatePropertyItem(descriptor, Instance, PropertyGrid, String.Format("{0}.{1}", BindingPath, descriptor.Name), Level + 1));
                }
            }
            catch (Exception ex)
            {
                //TODO: handle this some how
            }

            Properties = PropertyGridUtilities.GetAlphabetizedProperties(propertyItems);
        }

        private void ResolveExpandableObject()
        {
            var attribute = PropertyGridUtilities.GetAttribute<ExpandableObjectAttribute>(PropertyDescriptor);
            if (attribute != null)
            {
                HasChildProperties = true;
                IsReadOnly = true;
            }
        }

        private void ResolvePropertyOrder()
        {
            var attrs = PropertyDescriptor.Attributes.OfType<PropertyOrderAttribute>();
            if (attrs.Any())
                PropertyOrder = attrs.First().Order;
            else
                PropertyOrder = 0;
        }

        private void SetPropertyDescriptorProperties()
        {
            Name = PropertyDescriptor.Name;
            DisplayName = PropertyDescriptor.DisplayName;
            Category = PropertyDescriptor.Category;
            IsReadOnly = PropertyDescriptor.IsReadOnly;
        }

        #endregion //Methods
    }
}
