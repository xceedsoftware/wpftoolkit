/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.AvalonDock.Controls
{
  abstract class DropTargetBase : DependencyObject
  {
    #region Properties

    #region IsDraggingOver

    public static readonly DependencyProperty IsDraggingOverProperty = DependencyProperty.RegisterAttached( "IsDraggingOver", typeof( bool ), typeof( DropTargetBase ),
            new FrameworkPropertyMetadata( ( bool )false ) );

    public static bool GetIsDraggingOver( DependencyObject d )
    {
      return ( bool )d.GetValue( IsDraggingOverProperty );
    }

    public static void SetIsDraggingOver( DependencyObject d, bool value )
    {
      d.SetValue( IsDraggingOverProperty, value );
    }

    #endregion

    #endregion
  }
}
