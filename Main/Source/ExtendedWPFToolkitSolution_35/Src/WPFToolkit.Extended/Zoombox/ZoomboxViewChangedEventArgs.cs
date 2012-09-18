/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

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
