using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public interface ITypeEditor
    {
        FrameworkElement ResolveEditor(PropertyItem propertyItem);
    }
}
