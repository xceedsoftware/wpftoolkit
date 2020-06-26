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

namespace Xceed.Wpf.AvalonDock.Controls
{
  internal class ReentrantFlag
  {
    #region Members

    private bool _flag = false;

    #endregion

    #region Properties

    public bool CanEnter
    {
      get
      {
        return !_flag;
      }
    }

    #endregion

    #region Public Methods

    public _ReentrantFlagHandler Enter()
    {
      if( _flag )
        throw new InvalidOperationException();
      return new _ReentrantFlagHandler( this );
    }

    #endregion

    #region Internal Classes

    public class _ReentrantFlagHandler : IDisposable
    {
      private ReentrantFlag _owner;
      public _ReentrantFlagHandler( ReentrantFlag owner )
      {
        _owner = owner;
        _owner._flag = true;
      }

      public void Dispose()
      {
        _owner._flag = false;
      }
    }

    #endregion
  }
}
