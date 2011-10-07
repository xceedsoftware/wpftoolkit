using System.Windows;
using System.Windows.Controls;
using Microsoft.Windows.Controls.PropertyGrid.Editors;
using System.Windows.Data;

namespace Samples.Modules.PropertyGrid
{
    /// <summary>
    /// Interaction logic for LastNameUserControlEditor.xaml
    /// </summary>
    public partial class LastNameUserControlEditor : UserControl, ITypeEditor
    {
        public LastNameUserControlEditor()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(LastNameUserControlEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Value = string.Empty;
        }

        public FrameworkElement ResolveEditor(Microsoft.Windows.Controls.PropertyGrid.PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, LastNameUserControlEditor.ValueProperty, binding);
            return this;
        }
    }
}
