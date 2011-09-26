using System;
using System.Collections.ObjectModel;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class EditorDefinitionCollection : ObservableCollection<IEditorDefinition>
    {
        public IEditorDefinition this[string propertyName]
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.Properties.Contains(propertyName))
                        return item;
                }

                return null;
            }
        }

        public IEditorDefinition this[Type targetType]
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.TargetType == targetType)
                        return item;
                }

                return null;
            }
        }
    }
}
