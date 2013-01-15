/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Xceed.Wpf.DataGrid.Views
{
  public class TableViewHeaderFooterPanel : StackPanel
  {
    protected override Size ArrangeOverride( Size arrangeSize )
    {
      Size finalSize = base.ArrangeOverride( arrangeSize );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl dataGridControl = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      RowSelectorPane rowSelectorPane = null;

      // dataGridControl can be null in design-time
      if( dataGridControl != null )
      {
        TableViewScrollViewer scrollViewer = dataGridControl.ScrollViewer as TableViewScrollViewer;
        rowSelectorPane = ( scrollViewer != null ) ? scrollViewer.RowSelectorPane : null;
      }

      if( rowSelectorPane == null )
        return finalSize;

      Visibility rowSelectorPaneVisibility = ( Visibility )rowSelectorPane.GetValue( RowSelectorPane.VisibilityProperty );

      if( rowSelectorPaneVisibility != Visibility.Visible )
        return finalSize;

      foreach( UIElement child in this.InternalChildren )
      {
        Vector offset = VisualTreeHelper.GetOffset( child );
        Size desiredSize = child.DesiredSize;

        rowSelectorPane.SetRowSelectorPosition( child, new Rect( offset.X, offset.Y, desiredSize.Width, desiredSize.Height ), this );
      }

      return finalSize;
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      Size restrictedMeasureSize;
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( this.Orientation == Orientation.Vertical )
      {
        restrictedMeasureSize = new Size( availableSize.Width, double.PositiveInfinity );

        if( availableSize.Width != Double.PositiveInfinity )
          dataGridContext.FixedHeaderFooterViewPortSize = restrictedMeasureSize;
      }
      else
      {
        restrictedMeasureSize = new Size( double.PositiveInfinity, availableSize.Height );

        if( availableSize.Height != Double.PositiveInfinity )
          dataGridContext.FixedHeaderFooterViewPortSize = restrictedMeasureSize;
      }

      foreach( UIElement child in this.Children )
      {
        if( ( child is HeaderFooterItem ) && ( typeof( Row ).IsAssignableFrom( ( ( HeaderFooterItem )child ).VisualRootElementType ) ) )
        {
          dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated = false;

          // Calling Measure with the Viewport's width will have the effect of 
          // distributing the extra space (see FixedCellPanel's MeasureOverride). 
          // Eventually, the FixedCellPanel will receive an adjusted viewport 
          // width (where GroupLevelIndicator's width et al will be substracted).
          child.Measure( restrictedMeasureSize );

          if( dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated )
            break;
        }
      }

      return base.MeasureOverride( availableSize );
    }
  }
}
