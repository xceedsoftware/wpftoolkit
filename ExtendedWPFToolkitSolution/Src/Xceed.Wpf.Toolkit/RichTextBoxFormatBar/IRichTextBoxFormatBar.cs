/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

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
    /// Represents the Method that will be used to update the format bar values based on the Selection.
    /// </summary>
    void Update();
  }
}
