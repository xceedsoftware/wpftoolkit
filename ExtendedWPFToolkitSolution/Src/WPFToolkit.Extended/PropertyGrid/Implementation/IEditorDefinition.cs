using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public interface IEditorDefinition
    {
        DataTemplate EditorTemplate { get; set; }
        List<string> Properties { get; set; }
        Type TargetType { get; set; }
    }
}
