/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/
using System.Windows;

namespace Microsoft.Windows.Controls.Core.Utilities
{
  public class ContextMenuUtilities
  {
    public static readonly DependencyProperty OpenOnMouseLeftButtonClickProperty = DependencyProperty.RegisterAttached( "OpenOnMouseLeftButtonClick", typeof( bool ), typeof( ContextMenuUtilities ), new FrameworkPropertyMetadata( false, OpenOnMouseLeftButtonClickChanged ) );
    public static void SetOpenOnMouseLeftButtonClick( FrameworkElement element, bool value )
    {
      element.SetValue( OpenOnMouseLeftButtonClickProperty, value );
    }
    public static bool GetOpenOnMouseLeftButtonClick( FrameworkElement element )
    {
      return ( bool )element.GetValue( OpenOnMouseLeftButtonClickProperty );
    }

    public static void OpenOnMouseLeftButtonClickChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var control = ( FrameworkElement )sender;
      if( ( bool )e.NewValue )
      {
        control.PreviewMouseLeftButtonDown += ( s, args ) =>
        {
          if( control.ContextMenu != null )
          {
            control.ContextMenu.PlacementTarget = control;
            control.ContextMenu.IsOpen = true;
          }
        };
      }
      //TODO: remove handler when set to false
    }
  }
}
