using System;
using System.Collections.Generic;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public interface ICustomTypeEditor
    {
        ITypeEditor Editor { get; set; }
        IList<string> Properties { get; set; }
        Type TargetType { get; set; }
    }
}
