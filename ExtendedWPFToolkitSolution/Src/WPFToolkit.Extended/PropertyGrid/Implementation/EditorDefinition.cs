using System.Collections.Generic;
using System;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class EditorDefinition : IEditorDefinition
    {
        public DataTemplate EditorTemplate { get; set; }

        private List<string> _properties = new List<string>();
        public List<string> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        public Type TargetType { get; set; }
    }
}
