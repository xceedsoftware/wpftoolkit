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
using System.Windows;
using System.Windows.Threading;

namespace Xceed.Utils.Wpf
{
  internal static class DispatcherHelper
  {
    private static object ExitFrame( Object state )
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
