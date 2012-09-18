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
