using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public interface ITypeEditor
    {
        void Attach(PropertyItem propertyItem);
        FrameworkElement ResolveEditor();
    }
}
