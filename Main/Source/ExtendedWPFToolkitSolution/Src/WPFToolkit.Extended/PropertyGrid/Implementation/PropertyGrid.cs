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
            LoadProperties(newValue);
        }

        #endregion //IsCategorized

        #region NameWidth

        public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register("NameColumnWidth", typeof(double), typeof(PropertyGrid), new UIPropertyMetadata(120.0));
        public double NameColumnWidth
        {
            get { return (double)GetValue(NameColumnWidthProperty); }
            set { SetValue(NameColumnWidthProperty, value); }
        }

        #endregion //NameWidth

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
            SetSelectedObjectNameBinding(newValue);

            SelectedObjectType = newValue.GetType();

            _propertyItemsCache = GetObjectProperties(newValue);

            LoadProperties(IsCategorized);
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
            SelectedObjectTypeName = newValue.Name;
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

        public static readonly DependencyProperty SelectedObjectNameProperty = DependencyProperty.Register("SelectedObjectName", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata(string.Empty));
        public string SelectedObjectName
        {
            get { return (string)GetValue(SelectedObjectNameProperty); }
            private set { SetValue(SelectedObjectNameProperty, value); }
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

        #endregion //Properties

        #region Constructors

        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
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
                BindingExpression be = ((TextBox)e.OriginalSource).GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            NameColumnWidth = Math.Max(0, NameColumnWidth + e.HorizontalChange);
        }

        #endregion //Event Handlers

        #region Methods

        private void LoadProperties(bool isCategorized)
        {
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

            var properties = TypeDescriptor.GetProperties(instance.GetType(), new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) });

            // Get all properties of the type
            propertyItems.AddRange(properties.Cast<PropertyDescriptor>().
                Where(p => p.IsBrowsable && p.Name != "GenericParameterAttributes").
                Select(property => CreatePropertyItem(property, instance, this)));

            return propertyItems;
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
                ICustomTypeEditor customEditor = CustomTypeEditors[propertyItem.Name];
                if (customEditor != null)
                {
                    editor = customEditor.Editor;
                }
            }

            //no custom editor found
            if (editor == null)
            {
                if (propertyItem.PropertyType == typeof(bool))
                    editor = new CheckBoxEditor();
                else if (propertyItem.PropertyType.IsEnum)
                    editor = new EnumComboBoxEditor();
                else if (propertyItem.PropertyType == typeof(FontFamily) || propertyItem.PropertyType == typeof(FontWeight) || propertyItem.PropertyType == typeof(FontStyle) || propertyItem.PropertyType == typeof(FontStretch))
                    editor = new FontComboBoxEditor();
                else
                    editor = new TextBoxEditor();
            }

            editor.Attach(propertyItem);
            propertyItem.Editor = editor.ResolveEditor();

            return propertyItem;
        }

        private static PropertyCollection GetCategorizedProperties(List<PropertyItem> propertyItems)
        {
            PropertyCollection propertyCollection = new PropertyCollection();

            CollectionViewSource src = new CollectionViewSource { Source = propertyItems };
            src.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            src.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
            src.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            foreach (CollectionViewGroup item in src.View.Groups)
            {
                PropertyCategoryItem propertyCategoryItem = new PropertyCategoryItem { Category = item.Name.ToString() };
                foreach (var propertyitem in item.Items)
                {
                    propertyCategoryItem.Properties.Add((PropertyItem)propertyitem);
                }
                propertyCollection.Add(propertyCategoryItem);
            }

            return propertyCollection;
        }

        private static PropertyCollection GetAlphabetizedProperties(List<PropertyItem> propertyItems)
        {
            PropertyCollection propertyCollection = new PropertyCollection();

            if (propertyItems == null)
                return propertyCollection;

            CollectionViewSource src = new CollectionViewSource { Source = propertyItems };
            src.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            foreach (var item in ((ListCollectionView)(src.View)))
            {
                propertyCollection.Add((PropertyItem)item);
            }

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

        #endregion //Methods
    }
}
