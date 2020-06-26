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

namespace Xceed.Wpf.Toolkit
{
  public interface IRichTextBoxFormatBar
  {
    /// <summary>
    /// Represents the RichTextBox that will be the target for all text manipulations in the format bar.
    /// </summary>
    System.Windows.Controls.RichTextBox Target
    {
      get;
      set;
    }

    /// <summary>
    /// Represents the property that will be used to know if the formatBar should fade when mouse goes away.
    /// </summary>
    bool PreventDisplayFadeOut
    {
      get;
    }

    /// <summary>
    /// Represents the Method that will be used to update the format bar values based on the Selection.
    /// </summary>
    void Update();
  }
}
