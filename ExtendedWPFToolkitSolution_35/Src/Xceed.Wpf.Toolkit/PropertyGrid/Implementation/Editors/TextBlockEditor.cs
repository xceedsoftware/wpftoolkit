/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class TextBlockEditor : TypeEditor<TextBlock>
  {
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = TextBlock.TextProperty;
    }

    protected override void SetControlProperties()
    {
      Editor.Margin = new System.Windows.Thickness( 5, 0, 0, 0 );
    }
  }
}
