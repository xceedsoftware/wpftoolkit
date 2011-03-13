using System;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class TextBoxEditor : TypeEditor
    {
        protected override void Initialize()
        {
            Editor = new TextBox();
            ValueProperty = TextBox.TextProperty;
        }
    }
}
