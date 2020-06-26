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

using System;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class ItemsSourceAttributeEditor : TypeEditor<System.Windows.Controls.ComboBox>
  {
    private readonly ItemsSourceAttribute _attribute;

    public ItemsSourceAttributeEditor( ItemsSourceAttribute attribute )
    {
      _attribute = attribute;
    }

    protected override void SetValueDependencyProperty()
    {
      ValueProperty = System.Windows.Controls.ComboBox.SelectedValueProperty;
    }

    protected override System.Windows.Controls.ComboBox CreateEditor()
    {
      return new PropertyGridEditorComboBox();
    }

    protected override void ResolveValueBinding( PropertyItem propertyItem )
    {
      SetItemsSource();
      base.ResolveValueBinding( propertyItem );
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      Editor.DisplayMemberPath = "DisplayName";
      Editor.SelectedValuePath = "Value";
      if( propertyItem != null )
      {
        Editor.IsEnabled = !propertyItem.IsReadOnly;
      }
    }

    private void SetItemsSource()
    {
      Editor.ItemsSource = CreateItemsSource();
    }

    private System.Collections.IEnumerable CreateItemsSource()
    {
      var instance = Activator.CreateInstance( _attribute.Type );
      return ( instance as IItemsSource ).GetValues();
    }
  }
}
