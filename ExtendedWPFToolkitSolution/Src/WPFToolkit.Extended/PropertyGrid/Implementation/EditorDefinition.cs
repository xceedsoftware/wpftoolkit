using System.Collections.Generic;
using System;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class EditorDefinition : IEditorDefinition
    {
        public DataTemplate EditorTemplate { get; set; }

        private IList<string> _properties = new List<string>();
        public IList<string> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        public Type TargetType { get; set; }
    }
}
