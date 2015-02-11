/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class TableViewColumnVirtualizationManager : TableViewColumnVirtualizationManagerBase
  {
    public TableViewColumnVirtualizationManager( DataGridContext dataGridContext )
      : base( dataGridContext )
    {
    }

    #region FixedColumnsWidth Internal Property

    internal override double FixedColumnsWidth
    {
      get
      {
        return m_fixedColumnsWidth;
      }
    }

    protected override void SetFixedColumnsWidth( double value )
    {
      m_fixedColumnsWidth = value;
    }

    private double m_fixedColumnsWidth;

    #endregion

    #region ScrollingColumnsWidth Internal Property

    internal override double ScrollingColumnsWidth
    {
      get
      {
        return m_scrollingColumnsWidth;
      }
    }

    protected override void SetScrollingColumnsWidth( double value )
    {
      m_scrollingColumnsWidth = value;
    }

    private double m_scrollingColumnsWidth;

    #endregion

    #region VisibleColumnsWidth Internal Property

    internal override double VisibleColumnsWidth
    {
      get
      {
        return m_visibleColumnsWidth;
      }
    }

    protected override void SetVisibleColumnsWidth( double value )
    {
      m_visibleColumnsWidth = value;
    }

    private double m_visibleColumnsWidth;

    #endregion

    #region FieldNameToOffset Internal Property

    internal override IColumnInfoCollection<double> FieldNameToOffset
    {
      get
      {
        return m_fieldNameToOffset;
      }
    }

    private readonly IColumnInfoCollection<double> m_fieldNameToOffset = new TableViewColumnVirtualizationManagerBase.ColumnInfoCollection<double>();

    #endregion

    #region FieldNameToWidth Internal Property

    internal override IColumnInfoCollection<double> FieldNameToWidth
    {
      get
      {
        return m_fieldNameToWidth;
      }
    }

    private readonly IColumnInfoCollection<double> m_fieldNameToWidth = new TableViewColumnVirtualizationManagerBase.ColumnInfoCollection<double>();

    #endregion

    #region FieldNameToPosition Internal Property

    internal override IColumnInfoCollection<int> FieldNameToPosition
    {
      get
      {
        return m_fieldNameToPosition;
      }
    }

    private readonly IColumnInfoCollection<int> m_fieldNameToPosition = new TableViewColumnVirtualizationManagerBase.ColumnInfoCollection<int>();

    #endregion

    #region FixedFieldNames Internal Property

    internal override IColumnNameCollection FixedFieldNames
    {
      get
      {
        return m_fixedFieldNames;
      }
    }

    private readonly IColumnNameCollection m_fixedFieldNames = new TableViewColumnVirtualizationManagerBase.ColumnNameCollection();

    #endregion

    #region ScrollingFieldNames Internal Property

    internal override IColumnNameCollection ScrollingFieldNames
    {
      get
      {
        return m_scrollingFieldNames;
      }
    }

    private readonly IColumnNameCollection m_scrollingFieldNames = new TableViewColumnVirtualizationManagerBase.ColumnNameCollection();

    #endregion

    #region FirstViewportColumnFieldNameIndex Internal Property

    internal override int FirstViewportColumnFieldNameIndex
    {
      get
      {
        return m_firstViewportColumnFieldNameIndex;
      }
    }

    protected override void SetFirstViewportColumnFieldNameIndex( int value )
    {
      m_firstViewportColumnFieldNameIndex = value;
    }

    private int m_firstViewportColumnFieldNameIndex;

    #endregion
  }
}

