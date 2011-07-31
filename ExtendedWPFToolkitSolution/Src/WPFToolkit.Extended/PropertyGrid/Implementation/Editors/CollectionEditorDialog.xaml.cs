using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ComponentModel;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    /// <summary>
    /// Interaction logic for CollectionEditorDialog.xaml
    /// </summary>
    public partial class CollectionEditorDialog : Window
    {
        #region Properties

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(ObservableCollection<object>), typeof(CollectionEditorDialog), new UIPropertyMetadata(null));
        public ObservableCollection<object> Items
        {
            get { return (ObservableCollection<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CollectionEditorDialog), new UIPropertyMetadata(null, OnItemsSourceChanged));
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionEditorDialog collectionEditor = (CollectionEditorDialog)d;
            if (collectionEditor != null)
                collectionEditor.OnItemSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        public void OnItemSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != null)
            {
                Items = new ObservableCollection<object>();
                foreach (var item in newValue)
                {
                    object clone = Activator.CreateInstance(item.GetType());
                    CopyValues(item, clone);
                    Items.Add(clone);
                }
            }
        }

        public static readonly DependencyProperty NewItemTypesProperty = DependencyProperty.Register("NewItemTypes", typeof(IList), typeof(CollectionEditorDialog), new UIPropertyMetadata(null));
        public IList<Type> NewItemTypes
        {
            get { return (IList<Type>)GetValue(NewItemTypesProperty); }
            set { SetValue(NewItemTypesProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(CollectionEditorDialog), new UIPropertyMetadata(null));
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        #endregion //Properties

        #region Constructors

        private CollectionEditorDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public CollectionEditorDialog(Type type)
            : this()
        {
            NewItemTypes = GetNewItemTypes(type);
        }

        #endregion //Constructors

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            PersistChanges();
            Close();
        }

        private static void CopyValues(object source, object destination)
        {
            FieldInfo[] myObjectFields = source.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fi in myObjectFields)
            {
                fi.SetValue(destination, fi.GetValue(source));
            }
        }

        private static List<Type> GetNewItemTypes(Type type)
        {
            List<Type> types = new List<Type>();
            var newItemTypes = type.GetGenericArguments();
            foreach (var t in newItemTypes)
            {
                types.Add(t);
            }
            return types;
        }

        private void PersistChanges()
        {
            if (ItemsSource is IList)
            {
                IList list = (IList)ItemsSource;
                //Need to copy all changes into ItemsSource
            }
        }

        #region Commands

        private void AddNew(object sender, ExecutedRoutedEventArgs e)
        {
            Type t = (Type)e.Parameter;
            var newItem = Activator.CreateInstance(t);
            Items.Add(newItem);
            SelectedItem = newItem;
        }

        private void CanAddNew(object sender, CanExecuteRoutedEventArgs e)
        {
            Type t = e.Parameter as Type;
            if (t != null && t.IsClass)
                e.CanExecute = true;
        }

        private void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            Items.Remove(e.Parameter);
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null;
        }

        private void MoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedItem = e.Parameter;
            var index = Items.IndexOf(selectedItem);
            Items.RemoveAt(index);
            Items.Insert(++index, selectedItem);
            SelectedItem = selectedItem;
        }

        private void CanMoveDown(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null && Items.IndexOf(e.Parameter) < (Items.Count - 1))
                e.CanExecute = true;
        }

        private void MoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedItem = e.Parameter;
            var index = Items.IndexOf(selectedItem);
            Items.RemoveAt(index);
            Items.Insert(--index, selectedItem);
            SelectedItem = selectedItem;
        }

        private void CanMoveUp(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null && Items.IndexOf(e.Parameter) > 0)
                e.CanExecute = true;
        }

        #endregion //Commands
    }
}
