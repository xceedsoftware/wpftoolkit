/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Windows;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit
{
  public class CheckListBox : Selector
  {
    #region Constructors

    static CheckListBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( CheckListBox ), new FrameworkPropertyMetadata( typeof( CheckListBox ) ) );
    }

    public CheckListBox()
    {

    }

    #endregion //Constructors
  }
}
