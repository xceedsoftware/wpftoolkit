using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Microsoft.Windows.Controls.PropertyGrid.Implementation.EditorProviders;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyGrid : Control
    {
        #region Members

        private Thumb _dragThumb;
        private List<PropertyItem> _propertyItemsCache;

        #endregion //Members

        #region Properties

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

        public static readonly DependencyProperty NameWidthProperty = DependencyProperty.Register("NameWidth", typeof(double), typeof(PropertyGrid), new UIPropertyMetadata(120.0, OnNameWidthChanged));
        public double NameWidth
        {
            get { return (double)GetValue(NameWidthProperty); }
            set { SetValue(NameWidthProperty, value); }
        }

        private static void OnNameWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnNameWidthChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnNameWidthChanged(double oldValue, double newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //NameWidth

        #region Properties

        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(PropertyCollection), typeof(PropertyGrid), new UIPropertyMetadata(null, OnPropertiesChanged));
        public PropertyCollection Properties
        {
            get { return (PropertyCollection)GetValue(PropertiesProperty); }
            private set { SetValue(PropertiesProperty, value); }
        }

        private static void OnPropertiesChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnPropertiesChanged((PropertyCollection)e.OldValue, (PropertyCollection)e.NewValue);
        }

        protected virtual void OnPropertiesChanged(PropertyCollection oldValue, PropertyCollection newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
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
            SelectedObjectType = newValue.GetType();

            if (newValue is FrameworkElement)
                SelectedObjectName = (newValue as FrameworkElement).Name;

            _propertyItemsCache = GetObjectProperties(newValue);

            LoadProperties(IsCategorized);
        }

        #endregion //SelectedObject

        #region SelectedObjectType

        public static readonly DependencyProperty SelectedObjectTypeProperty = DependencyProperty.Register("SelectedObjectType", typeof(Type), typeof(PropertyGrid), new UIPropertyMetadata(null, OnSelectedObjectTypeChanged));
        public Type SelectedObjectType
        {
            get { return (Type)GetValue(SelectedObjectTypeProperty); }
            set { SetValue(SelectedObjectTypeProperty, value); }
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

        public static readonly DependencyProperty SelectedObjectTypeNameProperty = DependencyProperty.Register("SelectedObjectTypeName", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata(string.Empty, OnSelectedObjectTypeNameChanged));
        public string SelectedObjectTypeName
        {
            get { return (string)GetValue(SelectedObjectTypeNameProperty); }
            set { SetValue(SelectedObjectTypeNameProperty, value); }
        }

        private static void OnSelectedObjectTypeNameChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnSelectedObjectTypeNameChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnSelectedObjectTypeNameChanged(string oldValue, string newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //SelectedObjectTypeName

        #region SelectedObjectName

        public static readonly DependencyProperty SelectedObjectNameProperty = DependencyProperty.Register("SelectedObjectName", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata(string.Empty, OnSelectedObjectNameChanged));
        public string SelectedObjectName
        {
            get { return (string)GetValue(SelectedObjectNameProperty); }
            set { SetValue(SelectedObjectNameProperty, value); }
        }

        private static void OnSelectedObjectNameChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnSelectedObjectNameChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnSelectedObjectNameChanged(string oldValue, string newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //SelectedObjectName

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

        #endregion //Base Class Overrides

        #region Event Handlers

        void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            NameWidth = Math.Max(0, NameWidth + e.HorizontalChange);
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

        private static List<PropertyItem> GetObjectProperties(object instance)
        {
            var propertyItems = new List<PropertyItem>();
            if (instance == null)
                return propertyItems;

            var properties = TypeDescriptor.GetProperties(instance.GetType(), new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) });

            // Get all properties of the type
            propertyItems.AddRange(properties.Cast<PropertyDescriptor>().
                Where(p => p.IsBrowsable && p.Name != "GenericParameterAttributes").
                Select(property => CreatePropertyItem(property, instance)));

            return propertyItems;
        }

        private static PropertyItem CreatePropertyItem(PropertyDescriptor property, object instance)
        {
            PropertyItem propertyItem = new PropertyItem(instance, property);
            ITypeEditorProvider editorProvider = null;

            if (propertyItem.PropertyType == typeof(string))
                editorProvider = new TextBoxEditorProvider();
            else if (propertyItem.PropertyType == typeof(bool))
                editorProvider = new CheckBoxEditorProvider();
            else if (propertyItem.PropertyType.IsEnum)
                editorProvider = new EnumComboBoxEditorProvider();
            else if (propertyItem.PropertyType == typeof(FontFamily) || propertyItem.PropertyType == typeof(FontWeight) || propertyItem.PropertyType == typeof(FontStyle) || propertyItem.PropertyType == typeof(FontStretch))
                editorProvider = new FontComboBoxEditorProvider();
            else if (propertyItem.PropertyType == typeof(double))
                editorProvider = new TextBoxEditorProvider();
            else if (propertyItem.PropertyType == typeof(object) || propertyItem.PropertyType == typeof(Thickness))
                editorProvider = new TextBoxEditorProvider();

            if (editorProvider != null)
            {
                editorProvider.Initialize(propertyItem);
                propertyItem.Editor = editorProvider.ResolveEditor();
            }

            return propertyItem;
        }

        private PropertyCollection GetCategorizedProperties(List<PropertyItem> propertyItems)
        {
            PropertyCollection propertyCollection = new PropertyCollection();

            CollectionViewSource src = new CollectionViewSource();
            src.Source = propertyItems;
            src.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            src.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
            src.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            foreach (CollectionViewGroup item in src.View.Groups)
            {
                PropertyCategoryItem propertyCategoryItem = new PropertyCategoryItem();
                propertyCategoryItem.Category = item.Name.ToString();
                foreach (var propertyitem in item.Items)
                {
                    propertyCategoryItem.Properties.Add((PropertyItem)propertyitem);
                }
                propertyCollection.Add(propertyCategoryItem);
            }

            return propertyCollection;
        }

        private PropertyCollection GetAlphabetizedProperties(List<PropertyItem> propertyItems)
        {
            PropertyCollection propertyCollection = new PropertyCollection();

            if (propertyItems == null)
                return propertyCollection;

            CollectionViewSource src = new CollectionViewSource();
            src.Source = propertyItems;
            src.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            foreach (var item in ((ListCollectionView)(src.View)))
            {
                propertyCollection.Add((PropertyItem)item);
            }

            return propertyCollection;
        }

        #endregion //Methods
    }
}
