using System;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public class PrimitiveTypeCollectionEditor : TypeEditor<Microsoft.Windows.Controls.PrimitiveTypeCollectionEditor>
    {
        protected override void SetControlProperties()
        {
            Editor.BorderThickness = new System.Windows.Thickness(0);
            Editor.Content = "(Collection)";
        }

        protected override void SetValueDependencyProperty()
        {
            ValueProperty = Microsoft.Windows.Controls.PrimitiveTypeCollectionEditor.ItemsSourceProperty;
        }

        public override void Attach(PropertyItem propertyItem)
        {
            Editor.ItemsSourceType = propertyItem.PropertyType;
            Editor.ItemType = propertyItem.PropertyType.GetGenericArguments()[0];
            base.Attach(propertyItem);
        }
    }
}
