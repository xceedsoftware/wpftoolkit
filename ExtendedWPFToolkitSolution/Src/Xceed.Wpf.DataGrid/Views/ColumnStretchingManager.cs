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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class ColumnStretchingManager
  {
    public ColumnStretchingManager( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      m_dataGridContext = dataGridContext;
    }

    #region ColumnStretchingCalculated Property

    private bool m_columnStretchingCalculated = false;

    /// <summary>
    /// This property is useful in the case where the first row(s) are not measured
    /// (collapsed, for instance). The code that calls the Measure with a restricted
    /// size should set this property to false and check if it is true after having
    /// called Measure on the Row.
    /// </summary>
    public bool ColumnStretchingCalculated
    {
      get
      {
        return m_columnStretchingCalculated;
      }

      set
      {
        m_columnStretchingCalculated = value;
      }
    }

    #endregion ColumnStretchingCalculated Property

    public void DisableColumnStretching()
    {
      m_columnStretchingDisabled = true;

      foreach( ColumnBase column in m_dataGridContext.Columns )
      {
        if( column.ReadLocalValue( ColumnBase.DesiredWidthProperty ) != DependencyProperty.UnsetValue )
        {
          column.Width = new ColumnWidth( column.DesiredWidth );
          // Necessary to enable column sizing for Stretch First/Last/All.
          column.ClearValue( ColumnBase.DesiredWidthProperty );
        }
      }
    }

    public void CalculateColumnStretchWidths( double widthToDistribute, ColumnStretchMode columnStretchMode, double columnStretchMinWidth, bool canIncreaseColumnWidth )
    {
      if( m_columnStretchingDisabled )
        return;

      int currentDataGridScrollViewerMeasureVersion = 0;
      DataGridControl dataGridControl = m_dataGridContext.DataGridControl;

      if( dataGridControl != null )
      {
        DataGridScrollViewer scrollViewer = dataGridControl.ScrollViewer as DataGridScrollViewer;

        if( scrollViewer != null )
        {
          currentDataGridScrollViewerMeasureVersion = scrollViewer.MeasureVersion;
        }
      }

      this.ColumnStretchingCalculated = true;

      if( m_dataGridScrollViewerMeasureVersion != currentDataGridScrollViewerMeasureVersion )
      {
        // Reset the widthToDistribute since we are in a new pass of measure in the DataGridScrollViewer
        m_widthToDistribute = widthToDistribute;
        m_dataGridScrollViewerMeasureVersion = currentDataGridScrollViewerMeasureVersion;
      }
      else
      {
        if( widthToDistribute >= m_widthToDistribute )
        {
          widthToDistribute = m_widthToDistribute;
        }
        else
        {
          m_widthToDistribute = widthToDistribute;
        }
      }

      List<WorkingColumnWidth> excludedColumns = null;
      ColumnBase stretchedColumn = null;
      ReadOnlyObservableCollection<ColumnBase> visibleColumns = m_dataGridContext.VisibleColumns;

      if( visibleColumns.Count == 0 )
        return;

      switch( columnStretchMode )
      {
        case ColumnStretchMode.First:
          stretchedColumn = visibleColumns[ 0 ];
          excludedColumns = new List<WorkingColumnWidth>( 1 );
          break;

        case ColumnStretchMode.Last:
          stretchedColumn = visibleColumns[ visibleColumns.Count - 1 ];
          excludedColumns = new List<WorkingColumnWidth>( 1 );
          break;

        case ColumnStretchMode.All:
          excludedColumns = new List<WorkingColumnWidth>( visibleColumns.Count );
          foreach( ColumnBase column in visibleColumns )
          {
            excludedColumns.Add( new WorkingColumnWidth( column, 1d, columnStretchMinWidth ) );
          }
          break;

        case ColumnStretchMode.None:
          foreach( ColumnBase column in visibleColumns )
          {
            if( column.Width.UnitType == ColumnWidthUnitType.Star )
            {
              if( excludedColumns == null )
                excludedColumns = new List<WorkingColumnWidth>();

              excludedColumns.Add( new WorkingColumnWidth( column, column.Width.Value, columnStretchMinWidth ) );
            }
            else
            {
              column.ClearValue( Column.DesiredWidthProperty );
              widthToDistribute -= column.ActualWidth;
            }
          }
          break;
      }

      if( excludedColumns != null )
      {
        if( stretchedColumn != null )
        {
          foreach( ColumnBase column in visibleColumns )
          {
            if( column == stretchedColumn )
            {
              excludedColumns.Add( new WorkingColumnWidth( column, 1, columnStretchMinWidth ) );
            }
            else
            {
              column.ClearValue( Column.DesiredWidthProperty );
              widthToDistribute -= column.ActualWidth;
            }
          }
        }

        this.CalculateColumnDesiredWidth( widthToDistribute, excludedColumns, canIncreaseColumnWidth );
      }
    }

    private void CalculateColumnDesiredWidth( double widthToDistribute, List<WorkingColumnWidth> excludedColumns, bool canIncreaseColumnWidth )
    {
      double starToDistribute = 0d;

      Debug.Assert( excludedColumns.Count > 0, "Should have at least one excluded Column." );

      foreach( WorkingColumnWidth columnWidth in excludedColumns )
      {
        starToDistribute += columnWidth.StarValue;
      }

      double unitWidth = ColumnStretchingManager.CalculateUnitWidth( widthToDistribute, starToDistribute );
      WorkingColumnWidth primaryColumnWidth;
      WorkingColumnWidth previousColumnWidth;

      // This width distribution algorithm will perform at most (n² + n)/2 iterations; 
      // where n is the number of columns involved in the distribution.
      // This worst-case scenario will happen if each column has an active MinWidth 
      // or MaxWidth.
      // But it's better than the Framework's grid algorithm which breaks down when star
      // distribution is used with Min/MaxWidth.
      for( int i = 0; i < excludedColumns.Count; i++ )
      {
        primaryColumnWidth = excludedColumns[ i ];
        primaryColumnWidth.CalculateWidths( unitWidth );

        if( primaryColumnWidth.OverriddenWidth >= 0d )
        {
          widthToDistribute -= primaryColumnWidth.OverriddenWidth;
          starToDistribute -= primaryColumnWidth.StarValue;
          unitWidth = ColumnStretchingManager.CalculateUnitWidth( widthToDistribute, starToDistribute );

          for( int j = 0; j < i; j++ )
          {
            previousColumnWidth = excludedColumns[ j ];

            if( previousColumnWidth.OverriddenWidth >= 0d )
            {
              widthToDistribute += previousColumnWidth.OverriddenWidth;
              starToDistribute += previousColumnWidth.StarValue;
              unitWidth = ColumnStretchingManager.CalculateUnitWidth( widthToDistribute, starToDistribute );
            }

            previousColumnWidth.CalculateWidths( unitWidth );

            if( previousColumnWidth.OverriddenWidth >= 0d )
            {
              widthToDistribute -= previousColumnWidth.OverriddenWidth;
              starToDistribute -= previousColumnWidth.StarValue;
              unitWidth = ColumnStretchingManager.CalculateUnitWidth( widthToDistribute, starToDistribute );
            }
          }
        }
      }

      foreach( WorkingColumnWidth columnWidth in excludedColumns )
      {
        if( columnWidth.OverriddenWidth >= 0d )
        {
          if( !canIncreaseColumnWidth && ( columnWidth.Column.DesiredWidth < columnWidth.DesiredWidth ) )
            continue;

          // Assign the desired calculated Width. Column.ActualWidth will be in charge of
          // applying the MinWidth and MaxWidth. This ultimate responsability has to be 
          // kept in Column to make MinWidth and MaxWidth modifications having an impact.
          // For instance, an inactive MinWidth of 200 would become active if 
          // DesiredWidth was set to a temporary MaxWidth of 200 (resetting MaxWidth
          // would not trigger an immediate recalculation).
          columnWidth.Column.DesiredWidth = columnWidth.DesiredWidth;
        }
        else
        {
          var desiredWidth = unitWidth * columnWidth.StarValue;

          if( !canIncreaseColumnWidth && ( columnWidth.Column.DesiredWidth < desiredWidth ) )
            continue;

          columnWidth.Column.DesiredWidth = desiredWidth;
        }
      }
    }

    private static double CalculateUnitWidth( double widthToDistribute, double starToDistribute )
    {
      if( ( widthToDistribute > 0d ) && ( starToDistribute > 0d ) )
        return widthToDistribute / starToDistribute;

      return 0d;
    }

    #region Private WorkingColumnWidth Class

    private class WorkingColumnWidth
    {
      public WorkingColumnWidth( ColumnBase column, double starValue, double columnStretchMinWidth )
      {
        bool unsetMinValue = ( column.ReadLocalValue( ColumnBase.MinWidthProperty ) == DependencyProperty.UnsetValue );

        this.Column = column;
        this.StarValue = starValue;
        m_maxWidth = column.MaxWidth;
        m_minWidth = column.MinWidth;
        m_columnStretchMinWidth = ( ( unsetMinValue ) && ( columnStretchMinWidth < m_maxWidth ) ) ? columnStretchMinWidth : -1d;
      }

      public void CalculateWidths( double unitWidth )
      {
        double desiredWidth = unitWidth * this.StarValue;

        this.OverriddenWidth = -1d;

        if( desiredWidth < m_columnStretchMinWidth )
        {
          desiredWidth = m_columnStretchMinWidth;
          this.OverriddenWidth = desiredWidth;
        }

        // No matter what the Column's MinWidth and MaxWidth are, this is the real
        // desired width.
        this.DesiredWidth = desiredWidth;

        if( desiredWidth > m_maxWidth )
        {
          desiredWidth = m_maxWidth;
          this.OverriddenWidth = desiredWidth;
        }

        if( desiredWidth < m_minWidth )
        {
          this.OverriddenWidth = m_minWidth;
        }
      }

      public double StarValue;
      public ColumnBase Column;
      public double DesiredWidth;
      public double OverriddenWidth;

      private double m_columnStretchMinWidth;
      private double m_minWidth;
      private double m_maxWidth;
    }

    #endregion Private WorkingColumnWidth Class

    private DataGridContext m_dataGridContext;
    private bool m_columnStretchingDisabled;
    private int m_dataGridScrollViewerMeasureVersion;
    private double m_widthToDistribute = double.MaxValue;
  }
}
