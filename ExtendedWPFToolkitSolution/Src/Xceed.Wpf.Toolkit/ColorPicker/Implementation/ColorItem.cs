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

using System.Windows.Media;

namespace Xceed.Wpf.Toolkit
{
  public class ColorItem
  {
    public Color? Color
    {
      get;
      set;
    }
    public string Name
    {
      get;
      set;
    }

    public ColorItem( Color? color, string name )
    {
      Color = color;
      Name = name;
    }

    public override bool Equals(object obj)
    {
      var ci = obj as ColorItem;
      if (ci == null)
          return false;
      return ( ci.Color.Equals( Color ) && ci.Name.Equals( Name ) );
    }

    public override int GetHashCode()
    {
      return this.Color.GetHashCode() ^ this.Name.GetHashCode();
    }
  }
}
