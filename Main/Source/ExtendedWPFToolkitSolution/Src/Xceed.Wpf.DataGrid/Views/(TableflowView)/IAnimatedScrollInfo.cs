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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;

namespace Xceed.Wpf.DataGrid.Views
{
  internal interface IAnimatedScrollInfo : IScrollInfo
  {
    #region Horizontal Offset Properties

    ScrollDirection HorizontalScrollingDirection
    {
      get;
      set;
    }

    double OriginalHorizontalOffset
    {
      get;
      set;
    }

    double TargetHorizontalOffset
    {
      get;
      set;
    }

    #endregion Horizontal Offset Properties

    #region Vertical Offset Properties

    ScrollDirection VerticalScrollingDirection
    {
      get;
      set;
    }

    double OriginalVerticalOffset
    {
      get;
      set;
    }
    double TargetVerticalOffset
    {
      get;
      set;
    }

    #endregion Vertical Offset Properties
  }
}
