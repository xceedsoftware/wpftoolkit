/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

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
