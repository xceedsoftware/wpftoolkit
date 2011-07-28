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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            CollectionEditorDialog editor = new CollectionEditorDialog(_item);
            editor.ShowDialog();
        }
    }
}
