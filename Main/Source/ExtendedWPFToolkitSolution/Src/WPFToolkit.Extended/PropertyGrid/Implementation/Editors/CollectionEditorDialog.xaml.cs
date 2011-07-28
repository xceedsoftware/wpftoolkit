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

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    /// <summary>
    /// Interaction logic for CollectionEditorDialog.xaml
    /// </summary>
    public partial class CollectionEditorDialog : Window
    {
        PropertyItem _item;

        public CollectionEditorDialog()
        {
            InitializeComponent();
        }

        public CollectionEditorDialog(PropertyItem item)
            : this()
        {
            _item = item;
            _listBox.ItemsSource = _item.Value as IEnumerable;
        }

        //public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(CollectionEditorDialog), new UIPropertyMetadata(null));
        //public object ItemsSource
        //{
        //    get { return (object)GetValue(ItemsSourceProperty); }
        //    set { SetValue(ItemsSourceProperty, value); }
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }        
    }
}
