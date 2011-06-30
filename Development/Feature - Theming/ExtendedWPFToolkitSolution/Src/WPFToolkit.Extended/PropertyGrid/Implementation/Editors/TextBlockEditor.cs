using System;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class TextBlockEditor : TypeEditor<TextBlock>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = TextBlock.TextProperty;
        }

        protected override void SetControlProperties()
        {
            Editor.Margin = new System.Windows.Thickness(5, 0, 0, 0);
        }
    }
}
