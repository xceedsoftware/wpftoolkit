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

using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Zoombox
{
  public class ZoomboxCursors
  {
    #region Constructors

    static ZoomboxCursors()
    {
      try
      {
        new EnvironmentPermission( PermissionState.Unrestricted ).Demand();
        _zoom = new Cursor( ResourceHelper.LoadResourceStream( Assembly.GetExecutingAssembly(), "Zoombox/Resources/Zoom.cur" ) );
        _zoomRelative = new Cursor( ResourceHelper.LoadResourceStream( Assembly.GetExecutingAssembly(), "Zoombox/Resources/ZoomRelative.cur" ) );
      }
      catch( SecurityException )
      {
        // partial trust, so just use default cursors
      }
    }

    #endregion

    #region Zoom Static Property

    public static Cursor Zoom
    {
      get
      {
        return _zoom;
      }
    }

    private static readonly Cursor _zoom = Cursors.Arrow;

    #endregion

    #region ZoomRelative Static Property

    public static Cursor ZoomRelative
    {
      get
      {
        return _zoomRelative;
      }
    }

    private static readonly Cursor _zoomRelative = Cursors.Arrow;

    #endregion
  }
}
