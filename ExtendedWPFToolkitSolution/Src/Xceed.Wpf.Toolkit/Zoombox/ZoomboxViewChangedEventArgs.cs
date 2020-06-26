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

using System;
using Xceed.Wpf.Toolkit.Core;

namespace Xceed.Wpf.Toolkit.Zoombox
{
  public class ZoomboxViewChangedEventArgs : PropertyChangedEventArgs<ZoomboxView>
  {
    #region Constructors

    public ZoomboxViewChangedEventArgs(
      ZoomboxView oldView,
      ZoomboxView newView,
      int oldViewStackIndex,
      int newViewStackIndex )
      : base( Zoombox.CurrentViewChangedEvent, oldView, newView )
    {
      _newViewStackIndex = newViewStackIndex;
      _oldViewStackIndex = oldViewStackIndex;
    }

    #endregion

    #region NewViewStackIndex Property

    public int NewViewStackIndex
    {
      get
      {
        return _newViewStackIndex;
      }
    }

    private readonly int _newViewStackIndex = -1;

    #endregion

    #region NewViewStackIndex Property

    public int OldViewStackIndex
    {
      get
      {
        return _oldViewStackIndex;
      }
    }

    private readonly int _oldViewStackIndex = -1;

    #endregion

    #region NewViewStackIndex Property

    public bool IsNewViewFromStack
    {
      get
      {
        return _newViewStackIndex >= 0;
      }
    }

    #endregion

    #region NewViewStackIndex Property

    public bool IsOldViewFromStack
    {
      get
      {
        return _oldViewStackIndex >= 0;
      }
    }

    #endregion

    protected override void InvokeEventHandler( Delegate genericHandler, object genericTarget )
    {
      ( ( ZoomboxViewChangedEventHandler )genericHandler )( genericTarget, this );
    }
  }
}
