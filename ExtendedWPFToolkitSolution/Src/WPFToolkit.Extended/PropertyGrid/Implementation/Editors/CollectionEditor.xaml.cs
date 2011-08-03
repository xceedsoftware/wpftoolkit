using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    /// <summary>
    /// Interaction logic for CollectionEditor.xaml
    /// </summary>
    public partial class CollectionEditor : UserControl, ITypeEditor
    {
        PropertyItem _item;

        public CollectionEditor()
        {
            InitializeComponent();
        }

        public void Attach(PropertyItem propertyItem)
        {
            _item = propertyItem;
        }

        public FrameworkElement ResolveEditor()
        {
            return this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CollectionEditorDialog editor = new CollectionEditorDialog(_item.PropertyType);
            Binding binding = new Binding("Value");
            binding.Source = _item;
            binding.Mode = _item.IsWriteable ? BindingMode.TwoWay : BindingMode.OneWay;
            BindingOperations.SetBinding(editor, CollectionEditorDialog.ItemsSourceProperty, binding);
            editor.ShowDialog();
        }
    }
}
