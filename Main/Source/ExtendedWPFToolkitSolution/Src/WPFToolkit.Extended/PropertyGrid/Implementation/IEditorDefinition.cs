using System;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public interface IEditorDefinition
    {
        DataTemplate EditorTemplate { get; set; }
        PropertyDefinitionCollection PropertiesDefinitions { get; set; }
        Type TargetType { get; set; }
    }
}
