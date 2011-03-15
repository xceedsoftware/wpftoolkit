using System;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class TextBoxEditor : TypeEditor<TextBox>
    {
        protected override void SetValueDependencyProperty()
        {
            ValueProperty = TextBox.TextProperty;
        }
    }
}
