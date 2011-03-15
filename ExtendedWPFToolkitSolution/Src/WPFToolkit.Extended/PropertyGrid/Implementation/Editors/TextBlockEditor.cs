using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class TextBlockEditor : TypeEditor
    {
        protected override void Initialize()
        {
            Editor = new TextBlock();
            (Editor as TextBlock).Margin = new System.Windows.Thickness(5, 0, 0, 0);
            ValueProperty = TextBlock.TextProperty;
        }
    }
}
