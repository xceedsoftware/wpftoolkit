using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
    public class ReadOnlyTextBoxEditor : TypeEditor<TextBox>
    {
        protected override TextBox CreateEditor()
        {
            return new PropertyGridReadOnlyTextBoxEditor();
        }

        protected override void SetValueDependencyProperty()
        {
            ValueProperty = TextBox.TextProperty;
        }

    }

    public class PropertyGridReadOnlyTextBoxEditor : TextBox
    {
        static PropertyGridReadOnlyTextBoxEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridReadOnlyTextBoxEditor), new FrameworkPropertyMetadata(typeof(PropertyGridReadOnlyTextBoxEditor)));
        }
    }
}
