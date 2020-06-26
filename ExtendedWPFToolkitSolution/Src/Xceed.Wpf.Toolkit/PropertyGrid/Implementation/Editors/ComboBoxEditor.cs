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

using System.Collections.Generic;
using System.Collections;
using System.Windows;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public abstract class ComboBoxEditor : TypeEditor<System.Windows.Controls.ComboBox>
  {
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = System.Windows.Controls.ComboBox.SelectedItemProperty;
    }

    protected override System.Windows.Controls.ComboBox CreateEditor()
    {
      return new PropertyGridEditorComboBox();
    }

    protected override void ResolveValueBinding( PropertyItem propertyItem )
    {
      SetItemsSource( propertyItem );
      base.ResolveValueBinding( propertyItem );
    }

    protected abstract IEnumerable CreateItemsSource( PropertyItem propertyItem );

    private void SetItemsSource( PropertyItem propertyItem )
    {
      Editor.ItemsSource = CreateItemsSource( propertyItem );
    }
  }

  public class PropertyGridEditorComboBox : System.Windows.Controls.ComboBox
  {
    static PropertyGridEditorComboBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorComboBox ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorComboBox ) ) );
    }
  }
}
