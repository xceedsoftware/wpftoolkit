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
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public class GroupLevelIndicator : Control
  {
    static GroupLevelIndicator()
    {
      // This DefaultStyleKey will only be used in design-time.
      DefaultStyleKeyProperty.OverrideMetadata( typeof( GroupLevelIndicator ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( GroupLevelIndicator ) ) ) );

      DataGridControl.DataGridContextPropertyKey.OverrideMetadata( typeof( GroupLevelIndicator ), new FrameworkPropertyMetadata( new PropertyChangedCallback( GroupLevelIndicator.OnDataGridContextChanged ) ) );

      FocusableProperty.OverrideMetadata( typeof( GroupLevelIndicator ), new FrameworkPropertyMetadata( false ) );
    }

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupLevelIndicator ) );
    }

    private static void OnDataGridContextChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridContext dataGridContext = e.NewValue as DataGridContext;

      if( dataGridContext != null )
      {
        ( ( GroupLevelIndicator )sender ).PrepareDefaultStyleKey( dataGridContext.DataGridControl.GetView() );
      }
    }
  }
}
