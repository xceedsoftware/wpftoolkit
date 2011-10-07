using System;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class EditorDefinition : IEditorDefinition
    {
        public DataTemplate EditorTemplate { get; set; }

        private PropertyDefinitionCollection _properties = new PropertyDefinitionCollection();
        public PropertyDefinitionCollection PropertiesDefinitions
        {
            get { return _properties; }
            set { _properties = value; }
        }

        public Type TargetType { get; set; }
    }
}
