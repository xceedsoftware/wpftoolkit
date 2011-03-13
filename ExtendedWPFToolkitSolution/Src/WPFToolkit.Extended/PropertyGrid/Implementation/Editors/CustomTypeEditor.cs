using System.Collections.Generic;
using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class CustomTypeEditor : ICustomTypeEditor
    {
        public ITypeEditor Editor { get; set; }

        private IList<string> _properties = new List<string>();
        public IList<string> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        public Type TargetType { get; set; }
    }
}
