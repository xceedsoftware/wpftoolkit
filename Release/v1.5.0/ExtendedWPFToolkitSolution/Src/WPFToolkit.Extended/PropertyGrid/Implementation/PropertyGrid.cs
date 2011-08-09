using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Windows.Controls.PropertyGrid.Editors;
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

        #region CustomTypeEditors

        public static readonly DependencyProperty CustomTypeEditorsProperty = DependencyProperty.Register("CustomTypeEditors", typeof(CustomTypeEditorCollection), typeof(PropertyGrid), new UIPropertyMetadata(new CustomTypeEditorCollection()));
        public CustomTypeEditorCollection CustomTypeEditors
        {
            get { return (CustomTypeEditorCollection)GetValue(CustomTypeEditorsProperty); }
            set { SetValue(CustomTypeEditorsProperty, value); }
        }

        #endregion //CustomTypeEditors

        #region DisplaySummary

        public static readonly DependencyProperty DisplaySummaryProperty = DependencyProperty.Register("DisplaySummary", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));
        public bool DisplaySummary
        {
            get { return (bool)GetValue(DisplaySummaryProperty); }
            set { SetValue(DisplaySummaryProperty, value); }
        }

        #endregion //DisplaySummary

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

        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(PropertyCollection), typeof(PropertyGrid), new UIPropertyMetadata(null));
        public PropertyCollection Properties
        {
            get { return (PropertyCollection)GetValue(PropertiesProperty); }
            private set { SetValue(PropertiesProperty, value); }
        }

        #endregion //Properties

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

            if (newValue != null)
                newValue.IsSelected = true;
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

        #endregion //Properties

        #region Constructors

        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
        }

        public PropertyGrid()
        {
            CommandBindings.Add(new CommandBinding(PropertyGridCommands.ClearFilter, ClearFilter, CanClearFilter));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dragThumb = (Thumb)GetTemplateChild("PART_DragThumb");
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
            //clear any filters first
            Filter = String.Empty;

            if (isCategorized)
                Properties = GetCategorizedProperties(_propertyItemsCache);
            else
                Properties = GetAlphabetizedProperties(_propertyItemsCache);
        }

        private List<PropertyItem> GetObjectProperties(object instance)
        {
            var propertyItems = new List<PropertyItem>();
            if (instance == null)
                return propertyItems;

            try
            {
                var descriptors = GetPropertyDescriptors(instance);
                foreach (PropertyDescriptor descriptor in descriptors)
                {
                    if (descriptor.IsBrowsable)
                        propertyItems.Add(CreatePropertyItem(descriptor, instance, this));
                }
            }
            catch (Exception ex)
            {
                //TODO: handle this some how
            }

            return propertyItems;
        }

        private static PropertyDescriptorCollection GetPropertyDescriptors(object instance)
        {
            PropertyDescriptorCollection descriptors;

            TypeConverter tc = TypeDescriptor.GetConverter(instance);
            if (tc == null || !tc.GetPropertiesSupported())
            {

                if (instance is ICustomTypeDescriptor)
                    descriptors = ((ICustomTypeDescriptor)instance).GetProperties();
                else
                    descriptors = TypeDescriptor.GetProperties(instance.GetType());
            }
            else
            {
                descriptors = tc.GetProperties(instance);
            }

            return descriptors;
        }

        private PropertyItem CreatePropertyItem(PropertyDescriptor property, object instance, PropertyGrid grid)
        {
            PropertyItem propertyItem = new PropertyItem(instance, property, grid);

            var binding = new Binding(property.Name)
            {
                Source = instance,
                ValidatesOnExceptions = true,
                ValidatesOnDataErrors = true,
                Mode = propertyItem.IsWriteable ? BindingMode.TwoWay : BindingMode.OneWay
            };
            propertyItem.SetBinding(PropertyItem.ValueProperty, binding);

            ITypeEditor editor = null;

            //check for custom editor
            if (CustomTypeEditors.Count > 0)
            {
                //first check if the custom editor is type based
                ICustomTypeEditor customEditor = CustomTypeEditors[propertyItem.PropertyType];
                if (customEditor == null)
                {
                    //must be property based
                    customEditor = CustomTypeEditors[propertyItem.Name];
                }

                if (customEditor != null)
                    editor = customEditor.Editor;
            }

            try
            {
                //no custom editor found
                if (editor == null)
                {
                    if (propertyItem.IsReadOnly)
                        editor = new TextBlockEditor();
                    else if (propertyItem.PropertyType == typeof(bool) || propertyItem.PropertyType == typeof(bool?))
                        editor = new CheckBoxEditor();
                    else if (propertyItem.PropertyType == typeof(decimal) || propertyItem.PropertyType == typeof(decimal?))
                        editor = new DecimalUpDownEditor();
                    else if (propertyItem.PropertyType == typeof(double) || propertyItem.PropertyType == typeof(double?))
                        editor = new DoubleUpDownEditor();
                    else if (propertyItem.PropertyType == typeof(int) || propertyItem.PropertyType == typeof(int?))
                        editor = new IntegerUpDownEditor();
                    else if (propertyItem.PropertyType == typeof(DateTime) || propertyItem.PropertyType == typeof(DateTime?))
                        editor = new DateTimeUpDownEditor();
                    else if ((propertyItem.PropertyType == typeof(Color)))
                        editor = new ColorEditor();
                    else if (propertyItem.PropertyType.IsEnum)
                        editor = new EnumComboBoxEditor();
                    else if (propertyItem.PropertyType == typeof(FontFamily) || propertyItem.PropertyType == typeof(FontWeight) || propertyItem.PropertyType == typeof(FontStyle) || propertyItem.PropertyType == typeof(FontStretch))
                        editor = new FontComboBoxEditor();
                    else if (propertyItem.PropertyType.IsGenericType)
                    {
                        if (propertyItem.PropertyType.GetInterface("IList") != null)
                        {
                            var t = propertyItem.PropertyType.GetGenericArguments()[0];
                            if (!t.IsPrimitive && !t.Equals(typeof(String)))
                                editor = new Microsoft.Windows.Controls.PropertyGrid.Editors.CollectionEditor();
                            else
                                editor = new Microsoft.Windows.Controls.PropertyGrid.Editors.PrimitiveTypeCollectionEditor();
                        }
                        else
                            editor = new TextBlockEditor();
                    }
                    else
                        editor = new TextBoxEditor();
                }
            }
            catch (Exception ex)
            {
                //TODO: handle this some how
            }

            editor.Attach(propertyItem);
            propertyItem.Editor = editor.ResolveEditor();

            return propertyItem;
        }

        private static PropertyCollection GetCategorizedProperties(List<PropertyItem> propertyItems)
        {
            PropertyCollection propertyCollection = new PropertyCollection(propertyItems);
            propertyCollection.GroupBy("Category");
            propertyCollection.SortBy("Category", ListSortDirection.Ascending);
            propertyCollection.SortBy("Name", ListSortDirection.Ascending);
            return propertyCollection;
        }

        private static PropertyCollection GetAlphabetizedProperties(List<PropertyItem> propertyItems)
        {
            PropertyCollection propertyCollection = new PropertyCollection(propertyItems);
            propertyCollection.SortBy("Name", ListSortDirection.Ascending);
            return propertyCollection;
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
    }
}
