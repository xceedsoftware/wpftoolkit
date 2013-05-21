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
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public class DetailIndicator : GroupLevelIndicator
  {
    static DetailIndicator()
    {
      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( DetailIndicator ), new FrameworkPropertyMetadata( new PropertyChangedCallback( DetailIndicator.OnParentGridControlChanged ) ) );

      FocusableProperty.OverrideMetadata(typeof(DetailIndicator), new FrameworkPropertyMetadata(false));
    }

    protected internal override void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( DetailIndicator ) );
    }

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl grid = e.NewValue as DataGridControl;

      if( grid != null )
        ( ( DetailIndicator )sender ).PrepareDefaultStyleKey( grid.GetView() );
    }
  }
}
