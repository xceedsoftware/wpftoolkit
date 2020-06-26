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

using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Panels
{
  public static class SwitchTemplate
  {
    #region ID Attached Property

    public static readonly DependencyProperty IDProperty =
      DependencyProperty.RegisterAttached( "ID", typeof( string ), typeof( SwitchTemplate ),
        new FrameworkPropertyMetadata( null, 
          new PropertyChangedCallback( SwitchTemplate.OnIDChanged ) ) );

    public static string GetID( DependencyObject d )
    {
      return ( string )d.GetValue( SwitchTemplate.IDProperty );
    }

    public static void SetID( DependencyObject d, string value )
    {
      d.SetValue( SwitchTemplate.IDProperty, value );
    }

    private static void OnIDChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      if( ( e.NewValue == null ) || !( d is UIElement ) )
        return;

      SwitchPresenter parentPresenter = VisualTreeHelperEx.FindAncestorByType<SwitchPresenter>( d );
      if( parentPresenter != null )
      {
        parentPresenter.RegisterID( e.NewValue as string, d as FrameworkElement );
      }
      else
      {
        d.Dispatcher.BeginInvoke( DispatcherPriority.Loaded,
            ( ThreadStart )delegate()
        {
          parentPresenter = VisualTreeHelperEx.FindAncestorByType<SwitchPresenter>( d );
          if( parentPresenter != null )
          {
            parentPresenter.RegisterID( e.NewValue as string, d as FrameworkElement );
          }
        } );
      }
    }

    #endregion
  }
}
