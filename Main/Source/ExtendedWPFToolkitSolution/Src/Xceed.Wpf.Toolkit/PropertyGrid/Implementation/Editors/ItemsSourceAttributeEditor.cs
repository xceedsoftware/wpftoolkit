/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

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

    protected override void ResolveValueBinding( PropertyItem propertyItem )
    {
      SetItemsSource();
      base.ResolveValueBinding( propertyItem );
    }

    protected override void SetControlProperties()
    {
      Editor.DisplayMemberPath = "DisplayName";
      Editor.SelectedValuePath = "Value";
      Editor.Style = PropertyGridUtilities.ComboBoxStyle;
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
