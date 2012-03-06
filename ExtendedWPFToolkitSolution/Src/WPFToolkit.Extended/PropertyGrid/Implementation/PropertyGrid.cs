using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Windows.Controls.PropertyGrid.Commands;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyGrid : Control
    {
        #region Members

        private Thumb _dragThumb;
        private List<PropertyItem> _propertyItemsCache;

        #endregion //Members

        #region Properties

        #region AdvancedOptionsMenu

        public static readonly DependencyProperty AdvancedOptionsMenuProperty = DependencyProperty.Register("AdvancedOptionsMenu", typeof(ContextMenu), typeof(PropertyGrid), new UIPropertyMetadata(null));
        public ContextMenu AdvancedOptionsMenu
        {
            get { return (ContextMenu)GetValue(AdvancedOptionsMenuProperty); }
            set { SetValue(AdvancedOptionsMenuProperty, value); }
        }

        #endregion //AdvancedOptionsMenu

        #region AutoGenerateProperties

        public static readonly DependencyProperty AutoGeneratePropertiesProperty = DependencyProperty.Register("AutoGenerateProperties", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));
        public bool AutoGenerateProperties
        {
            get { return (bool)GetValue(AutoGeneratePropertiesProperty); }
            set { SetValue(AutoGeneratePropertiesProperty, value); }
        }

        #endregion //AutoGenerateProperties

        #region DisplaySummary

        public static readonly DependencyProperty DisplaySummaryProperty = DependencyProperty.Register("DisplaySummary", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));
        public bool DisplaySummary
        {
            get { return (bool)GetValue(DisplaySummaryProperty); }
            set { SetValue(DisplaySummaryProperty, value); }
        }

        #endregion //DisplaySummary

        #region EditorDefinitions

        public static readonly DependencyProperty EditorDefinitionsProperty = DependencyProperty.Register("EditorDefinitions", typeof(EditorDefinitionCollection), typeof(PropertyGrid), new UIPropertyMetadata(null));
        public EditorDefinitionCollection EditorDefinitions
        {
            get { return (EditorDefinitionCollection)GetValue(EditorDefinitionsProperty); }
            set { SetValue(EditorDefinitionsProperty, value); }
        }

        #endregion //EditorDefinitions

        #region Filter

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata(null, OnFilterChanged));
        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        private static void OnFilterChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnFilterChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFilterChanged(string oldValue, string newValue)
        {
            if (Properties != null)
                Properties.Filter(newValue);
        }

        #endregion //Filter

        #region FilterWatermark

        public static readonly DependencyProperty FilterWatermarkProperty = DependencyProperty.Register("FilterWatermark", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata("Search"));
        public string FilterWatermark
        {
            get { return (string)GetValue(FilterWatermarkProperty); }
            set { SetValue(FilterWatermarkProperty, value); }
        }

        #endregion //FilterWatermark

        #region IsCategorized

        public static readonly DependencyProperty IsCategorizedProperty = DependencyProperty.Register("IsCategorized", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true, OnIsCategorizedChanged));
        public bool IsCategorized
        {
            get { return (bool)GetValue(IsCategorizedProperty); }
            set { SetValue(IsCategorizedProperty, value); }
        }

        private static void OnIsCategorizedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnIsCategorizedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsCategorizedChanged(bool oldValue, bool newValue)
        {
            InitializePropertyGrid(newValue);
        }

        #endregion //IsCategorized

        #region NameColumnWidth

        public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register("NameColumnWidth", typeof(double), typeof(PropertyGrid), new UIPropertyMetadata(150.0));
        public double NameColumnWidth
        {
            get { return (double)GetValue(NameColumnWidthProperty); }
            set { SetValue(NameColumnWidthProperty, value); }
        }

        #endregion //NameColumnWidth

        #region Properties

        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(PropertyItemCollection), typeof(PropertyGrid), new UIPropertyMetadata(null));
        public PropertyItemCollection Properties
        {
            get { return (PropertyItemCollection)GetValue(PropertiesProperty); }
            private set { SetValue(PropertiesProperty, value); }
        }

        #endregion //Properties

        #region PropertyDefinitions

        public static readonly DependencyProperty PropertyDefinitionsProperty = DependencyProperty.Register("PropertyDefinitions", typeof(PropertyDefinitionCollection), typeof(PropertyGrid), new UIPropertyMetadata(null));
        public PropertyDefinitionCollection PropertyDefinitions
        {
            get { return (PropertyDefinitionCollection)GetValue(PropertyDefinitionsProperty); }
            set { SetValue(PropertyDefinitionsProperty, value); }
        }

        #endregion //PropertyDefinitions

        #region SelectedObject

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register("SelectedObject", typeof(object), typeof(PropertyGrid), new UIPropertyMetadata(null, OnSelectedObjectChanged));
        public object SelectedObject
        {
            get { return (object)GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        private static void OnSelectedObjectChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyInspector = o as PropertyGrid;
            if (propertyInspector != null)
                propertyInspector.OnSelectedObjectChanged((object)e.OldValue, (object)e.NewValue);
        }

        protected virtual void OnSelectedObjectChanged(object oldValue, object newValue)
        {
            if (newValue == null)
                ResetPropertyGrid();
            else
            {
                SetSelectedObjectNameBinding(newValue);
                SelectedObjectType = newValue.GetType();
                _propertyItemsCache = GetObjectProperties(newValue);
                InitializePropertyGrid(IsCategorized);
            }
        }

        #endregion //SelectedObject

        #region SelectedObjectType

        public static readonly DependencyProperty SelectedObjectTypeProperty = DependencyProperty.Register("SelectedObjectType", typeof(Type), typeof(PropertyGrid), new UIPropertyMetadata(null, OnSelectedObjectTypeChanged));
        public Type SelectedObjectType
        {
            get { return (Type)GetValue(SelectedObjectTypeProperty); }
            private set { SetValue(SelectedObjectTypeProperty, value); }
        }

        private static void OnSelectedObjectTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnSelectedObjectTypeChanged((Type)e.OldValue, (Type)e.NewValue);
        }

        protected virtual void OnSelectedObjectTypeChanged(Type oldValue, Type newValue)
        {
            if (newValue == null)
                SelectedObjectTypeName = string.Empty;
            else
            {
                DisplayNameAttribute displayNameAttribute = newValue.GetCustomAttributes(false).OfType<DisplayNameAttribute>().FirstOrDefault();
                SelectedObjectTypeName = displayNameAttribute == null ? newValue.Name : displayNameAttribute.DisplayName;
            }
        }

        #endregion //SelectedObjectType

        #region SelectedObjectTypeName

        public static readonly DependencyProperty SelectedObjectTypeNameProperty = DependencyProperty.Register("SelectedObjectTypeName", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata(string.Empty));
        public string SelectedObjectTypeName
        {
            get { return (string)GetValue(SelectedObjectTypeNameProperty); }
            private set { SetValue(SelectedObjectTypeNameProperty, value); }
        }

        #endregion //SelectedObjectTypeName

        #region SelectedObjectName

        public static readonly DependencyProperty SelectedObjectNameProperty = DependencyProperty.Register("SelectedObjectName", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata(string.Empty, OnSelectedObjectNameChanged, OnCoerceSelectedObjectName));
        public string SelectedObjectName
        {
            get { return (string)GetValue(SelectedObjectNameProperty); }
            private set { SetValue(SelectedObjectNameProperty, value); }
        }

        private static object OnCoerceSelectedObjectName(DependencyObject o, object baseValue)
        {
            if (String.IsNullOrEmpty((String)baseValue))
                return "<no name>";

            return baseValue;
        }

        private static void OnSelectedObjectNameChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.SelectedObjectNameChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void SelectedObjectNameChanged(string oldValue, string newValue)
        {

        }

        #endregion //SelectedObjectName

        #region SelectedProperty

        public static readonly DependencyProperty SelectedPropertyProperty = DependencyProperty.Register("SelectedProperty", typeof(PropertyItem), typeof(PropertyGrid), new UIPropertyMetadata(null, OnSelectedPropertyChanged));
        public PropertyItem SelectedProperty
        {
            get { return (PropertyItem)GetValue(SelectedPropertyProperty); }
            internal set { SetValue(SelectedPropertyProperty, value); }
        }

        private static void OnSelectedPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnSelectedPropertyChanged((PropertyItem)e.OldValue, (PropertyItem)e.NewValue);
        }

        protected virtual void OnSelectedPropertyChanged(PropertyItem oldValue, PropertyItem newValue)
        {
            if (oldValue != null)
                oldValue.IsSelected = false;

            //if (newValue != null)
            //    newValue.IsSelected = true;

            RaiseEvent(new RoutedEventArgs(PropertyGrid.SelectedPropertyItemChangedEvent, newValue));
        }

        #endregion //SelectedProperty

        #region ShowAdvancedOptions

        public static readonly DependencyProperty ShowAdvancedOptionsProperty = DependencyProperty.Register("ShowAdvancedOptions", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(false));
        public bool ShowAdvancedOptions
        {
            get { return (bool)GetValue(ShowAdvancedOptionsProperty); }
            set { SetValue(ShowAdvancedOptionsProperty, value); }
        }

        #endregion //ShowAdvancedOptions

        #region ShowSearchBox

        public static readonly DependencyProperty ShowSearchBoxProperty = DependencyProperty.Register("ShowSearchBox", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));
        public bool ShowSearchBox
        {
            get { return (bool)GetValue(ShowSearchBoxProperty); }
            set { SetValue(ShowSearchBoxProperty, value); }
        }

        #endregion //ShowSearchBox

        #region ShowSortOptions

        public static readonly DependencyProperty ShowSortOptionsProperty = DependencyProperty.Register("ShowSortOptions", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));
        public bool ShowSortOptions
        {
            get { return (bool)GetValue(ShowSortOptionsProperty); }
            set { SetValue(ShowSortOptionsProperty, value); }
        }

        #endregion //ShowSortOptions

        #endregion //Properties

        #region Constructors

        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
        }

        public PropertyGrid()
        {
            EditorDefinitions = new EditorDefinitionCollection();
            PropertyDefinitions = new PropertyDefinitionCollection();
            CommandBindings.Add(new CommandBinding(PropertyGridCommands.ClearFilter, ClearFilter, CanClearFilter));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_dragThumb != null)
                _dragThumb.DragDelta -= DragThumb_DragDelta;
            _dragThumb = GetTemplateChild("PART_DragThumb") as Thumb;
            if (_dragThumb != null)
                _dragThumb.DragDelta += DragThumb_DragDelta;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            //hitting enter on textbox will update value of underlying source
            if (this.SelectedProperty != null && e.Key == Key.Enter && e.OriginalSource is TextBox)
            {
                if (!(e.OriginalSource as TextBox).AcceptsReturn)
                {
                    BindingExpression be = ((TextBox)e.OriginalSource).GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
            }
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            NameColumnWidth = Math.Max(0, NameColumnWidth + e.HorizontalChange);
        }

        #endregion //Event Handlers

        #region Commands

        private void ClearFilter(object sender, ExecutedRoutedEventArgs e)
        {
            Filter = String.Empty;
        }

        private void CanClearFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !String.IsNullOrEmpty(Filter);
        }

        #endregion //Commands

        #region Methods

        private void InitializePropertyGrid(bool isCategorized)
        {
            LoadProperties(isCategorized);
            SetDragThumbMargin(isCategorized);
        }

        private void LoadProperties(bool isCategorized)
        {
            if (_propertyItemsCache == null)
                return;

            //clear any filters first
            Filter = String.Empty;

            if (isCategorized)
                Properties = PropertyGridUtilities.GetCategorizedProperties(_propertyItemsCache);
            else
                Properties = PropertyGridUtilities.GetAlphabetizedProperties(_propertyItemsCache);
        }

        private List<PropertyItem> GetObjectProperties(object instance)
        {
            var propertyItems = new List<PropertyItem>();
            if (instance == null)
                return propertyItems;

            try
            {
                PropertyDescriptorCollection descriptors = PropertyGridUtilities.GetPropertyDescriptors(instance);

                if (!AutoGenerateProperties)
                {
                    List<PropertyDescriptor> specificProperties = new List<PropertyDescriptor>();
                    foreach (PropertyDefinition pd in PropertyDefinitions)
                    {
                        foreach (PropertyDescriptor descriptor in descriptors)
                        {
                            if (descriptor.Name == pd.Name)
                            {
                                specificProperties.Add(descriptor);
                                break;
                            }
                        }
                    }

                    descriptors = new PropertyDescriptorCollection(specificProperties.ToArray());
                }

                foreach (PropertyDescriptor descriptor in descriptors)
                {
                    if (descriptor.IsBrowsable)
                        propertyItems.Add(PropertyGridUtilities.CreatePropertyItem(descriptor, instance, this, descriptor.Name));
                }
            }
            catch (Exception)
            {
                //TODO: handle this some how
            }

            return propertyItems;
        }

        private void SetSelectedObjectNameBinding(object selectedObject)
        {
            if (selectedObject is FrameworkElement)
            {
                var binding = new Binding("Name");
                binding.Source = selectedObject;
                binding.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(this, PropertyGrid.SelectedObjectNameProperty, binding);
            }
        }

        private void SetDragThumbMargin(bool isCategorized)
        {
            if (_dragThumb == null)
                return;

            if (isCategorized)
                _dragThumb.Margin = new Thickness(6, 0, 0, 0);
            else
                _dragThumb.Margin = new Thickness(-1, 0, 0, 0);
        }

        private void ResetPropertyGrid()
        {
            SelectedObjectName = String.Empty;
            SelectedObjectType = null;
            _propertyItemsCache = null;
            SelectedProperty = null;
            Properties = null;
        }

        /// <summary>
        /// Updates all property values in the PropertyGrid with the data from the SelectedObject
        /// </summary>
        public void Update()
        {
            foreach (var item in Properties)
            {
                BindingOperations.GetBindingExpressionBase(item, PropertyItem.ValueProperty).UpdateTarget();
            }
        }

        #endregion //Methods

        #region Events

        public static readonly RoutedEvent PropertyValueChangedEvent = EventManager.RegisterRoutedEvent("PropertyValueChanged", RoutingStrategy.Bubble, typeof(PropertyValueChangedEventHandler), typeof(PropertyGrid));
        public event PropertyValueChangedEventHandler PropertyValueChanged
        {
            add { AddHandler(PropertyValueChangedEvent, value); }
            remove { RemoveHandler(PropertyValueChangedEvent, value); }
        }
        
        public static readonly RoutedEvent SelectedPropertyItemChangedEvent = EventManager.RegisterRoutedEvent("SelectedPropertyItemChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyGrid));
        public event RoutedEventHandler SelectedPropertyItemChanged
        {
            add { AddHandler(SelectedPropertyItemChangedEvent, value); }
            remove { RemoveHandler(SelectedPropertyItemChangedEvent, value); }
        }

        #endregion //Events
    }

    public delegate void PropertyValueChangedEventHandler(object sender, PropertyValueChangedEventArgs e);
    public class PropertyValueChangedEventArgs : RoutedEventArgs
    {
        public object NewValue { get; set; }
        public object OldValue { get; set; }

        public PropertyValueChangedEventArgs(RoutedEvent routedEvent, object source, object oldValue, object newValue)
            : base(routedEvent, source)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}
