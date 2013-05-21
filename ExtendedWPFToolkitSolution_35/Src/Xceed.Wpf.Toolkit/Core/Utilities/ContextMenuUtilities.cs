/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.Toolkit.Core.Utilities
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
