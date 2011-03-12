using System;
using System.Windows;

namespace Microsoft.Windows.Controls.PropertyGrid.Implementation.EditorProviders
{
    interface ITypeEditorProvider
    {
        void Initialize(PropertyItem propertyItem);
        FrameworkElement ResolveEditor();
    }
}
