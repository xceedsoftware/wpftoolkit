/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Collections.Generic;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public abstract class ComboBoxEditor : TypeEditor<System.Windows.Controls.ComboBox>
  {
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = System.Windows.Controls.ComboBox.SelectedItemProperty;
    }

    protected override void ResolveValueBinding( PropertyItem propertyItem )
    {
      SetItemsSource( propertyItem );
      base.ResolveValueBinding( propertyItem );
    }

    protected abstract IList<object> CreateItemsSource( PropertyItem propertyItem );

    private void SetItemsSource( PropertyItem propertyItem )
    {
      Editor.ItemsSource = CreateItemsSource( propertyItem );
    }
  }
}
