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
using System.Windows;
using System.Windows.Threading;

namespace Xceed.Utils.Wpf
{
  internal static class DispatcherHelper
  {
    private static object ExitFrame( object state )
    {
      DispatcherFrame frame = state as DispatcherFrame;
      frame.Continue = false;
      return null;
    }

    private static DispatcherOperationCallback ExitFrameCallback = new
             DispatcherOperationCallback( DispatcherHelper.ExitFrame );

    public static void DoEvents( Dispatcher dispatcher )
    {
      if( dispatcher == null )
        throw new ArgumentNullException( "dispatcher" );

      DispatcherFrame nestedFrame = new DispatcherFrame();

      DispatcherOperation exitOperation = dispatcher.BeginInvoke( 
        DispatcherPriority.Background,
        DispatcherHelper.ExitFrameCallback,
        nestedFrame );

      Dispatcher.PushFrame( nestedFrame );

      if( exitOperation.Status != DispatcherOperationStatus.Completed )
        exitOperation.Abort();
    }
  }
}
