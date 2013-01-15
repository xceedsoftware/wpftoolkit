using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Attributes
{
    public class TypeEditorAttribute : Attribute
    {
        public Type Type { get; set; }

        public TypeEditorAttribute(Type type)
        {
            var valueSourceInterface = type.GetInterface("Microsoft.Windows.Controls.PropertyGrid.Editors.ITypeEditor");
            if (valueSourceInterface == null)
                throw new ArgumentException("Type must implement the ITypeEditor interface.", "type");

            Type = type;
        }
    }
}
