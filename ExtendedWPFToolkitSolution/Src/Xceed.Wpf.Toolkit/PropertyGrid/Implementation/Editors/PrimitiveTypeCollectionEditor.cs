/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class PrimitiveTypeCollectionEditor : TypeEditor<PrimitiveTypeCollectionControl>
  {
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = PrimitiveTypeCollectionControl.ItemsSourceProperty;
    }

    protected override PrimitiveTypeCollectionControl CreateEditor()
    {
      return new PropertyGridEditorPrimitiveTypeCollectionControl();
    }

    protected override void ResolveValueBinding( PropertyItem propertyItem )
    {
      var type = propertyItem.PropertyType;
      Editor.ItemsSourceType = type;

      if( type.BaseType == typeof( System.Array ) )
      {
        Editor.ItemType = type.GetElementType();
      }
      else
      {
        var typeArguments = type.GetGenericArguments();
        if( typeArguments.Length > 0 )
        {
          Editor.ItemType = typeArguments[ 0 ];
        }
      }

      base.ResolveValueBinding( propertyItem );
    }
  }

  public class PropertyGridEditorPrimitiveTypeCollectionControl : PrimitiveTypeCollectionControl
  {
    static PropertyGridEditorPrimitiveTypeCollectionControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorPrimitiveTypeCollectionControl ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorPrimitiveTypeCollectionControl ) ) );
    }
  }
}
