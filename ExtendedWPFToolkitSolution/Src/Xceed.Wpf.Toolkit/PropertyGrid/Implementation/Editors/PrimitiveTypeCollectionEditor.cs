/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class PrimitiveTypeCollectionEditor : TypeEditor<PrimitiveTypeCollectionControl>
  {
    protected override void SetControlProperties()
    {
      Editor.BorderThickness = new System.Windows.Thickness( 0 );
      Editor.Content = "(Collection)";
    }

    protected override void SetValueDependencyProperty()
    {
      ValueProperty = PrimitiveTypeCollectionControl.ItemsSourceProperty;
    }

    protected override void ResolveValueBinding( PropertyItem propertyItem )
    {
      Editor.ItemsSourceType = propertyItem.PropertyType;
      Editor.ItemType = propertyItem.PropertyType.GetGenericArguments()[ 0 ];
      base.ResolveValueBinding( propertyItem );
    }
  }
}
