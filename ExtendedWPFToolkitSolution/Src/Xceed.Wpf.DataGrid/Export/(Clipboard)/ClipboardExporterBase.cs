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

namespace Xceed.Wpf.DataGrid.Export
{
  public abstract class ClipboardExporterBase
  {
    protected ClipboardExporterBase()
    {
      this.UseFieldNamesInHeader = false;
    }

    #region IncludeColumnHeaders Property

    public bool IncludeColumnHeaders
    {
      get;
      set;
    }

    #endregion

    #region UseFieldNamesInHeader Property

    public bool UseFieldNamesInHeader
    {
      get;
      set;
    }

    #endregion

    #region ClipboardData Property

    protected abstract object ClipboardData
    {
      get;
    }

    #endregion

    protected virtual void Indent()
    {
    }

    protected virtual void Unindent()
    {
    }

    protected virtual void StartExporter( string dataFormat )
    {
    }

    protected virtual void EndExporter( string dataFormat )
    {
    }

    protected virtual void ResetExporter()
    {
    }

    protected virtual void StartHeader( DataGridContext dataGridContext )
    {
    }

    protected virtual void StartHeaderField( DataGridContext dataGridContext, Column column )
    {
    }

    protected virtual void EndHeaderField( DataGridContext dataGridContext, Column column )
    {
    }

    protected virtual void EndHeader( DataGridContext dataGridContext )
    {
    }

    protected virtual void StartDataItem( DataGridContext dataGridContext, object dataItem )
    {
    }

    protected virtual void StartDataItemField( DataGridContext dataGridContext, Column column, object fieldValue )
    {
    }

    protected virtual void EndDataItemField( DataGridContext dataGridContext, Column column, object fieldValue )
    {
    }

    protected virtual void EndDataItem( DataGridContext dataGridContext, object dataItem )
    {
    }

    internal static IDataObject CreateDataObject( DataGridControl dataGridControl )
    {
      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( dataGridControl );

      if( dataGridContext == null )
        return null;

      XceedDataObject dataObject = null;
      bool containsData = false;

      try
      {
        dataGridControl.ShowWaitCursor();

        Dictionary<string, ClipboardExporterBase> exporters = dataGridControl.ClipboardExporters;

        foreach( KeyValuePair<string, ClipboardExporterBase> keyPair in exporters )
        {
          if( keyPair.Value == null )
            throw new DataGridException( "ClipboardExporterBase cannot be null." );

          keyPair.Value.StartExporter( keyPair.Key );
        }

        using( ManualExporter exporter = new ManualExporter( exporters.Values as IEnumerable<ClipboardExporterBase> ) )
        {
          exporter.Export( dataGridContext );
        }

        foreach( KeyValuePair<string, ClipboardExporterBase> keyPair in exporters )
        {
          keyPair.Value.EndExporter( keyPair.Key );

          if( dataObject == null )
          {
            dataObject = new XceedDataObject();
          }

          object clipboardExporterValue = keyPair.Value.ClipboardData;

          // For other formats, we directly copy the content to the IDataObject
          if( clipboardExporterValue != null )
          {
            ( ( IDataObject )dataObject ).SetData( keyPair.Key, clipboardExporterValue );
            containsData = true;
          }

          keyPair.Value.ResetExporter();
        }
      }
      finally
      {
        dataGridControl.HideWaitCursor();
      }

      // Only return dataObject some data was copied
      if( containsData )
        return dataObject as IDataObject;

      return null;
    }

    private class ManualExporter : IDisposable
    {
      public ManualExporter( IEnumerable<ClipboardExporterBase> clipboardExporters )
      {
        m_columnToBindingPathExtractor = new Dictionary<Column, BindingPathValueExtractor>();
        m_clipboardExporters = new List<ClipboardExporterBase>( clipboardExporters );
        m_visitedDetailExportedVisiblePositions = new Dictionary<int, int[]>();
        m_visitedDetailVisibleColumnsCache = new Dictionary<int, ColumnBase[]>();
      }

      #region HasItems Property

      public bool HasItems
      {
        get;
        set;
      }

      #endregion

      internal void Export( DataGridContext dataGridContext )
      {
        if( dataGridContext == null )
          return;

        // Get the detail level for the current DataGridContext
        int detailLevel = dataGridContext.DetailLevel;

        // Update indentation of ClipboardExporters for this detail level
        this.UpdateExporterIndentation( detailLevel );

        // Get master indexes that have mapped expanded details
        List<int> indexesToMasterItemList = dataGridContext.CustomItemContainerGenerator.GetMasterIndexesWithExpandedDetails();

        // Get the index of the next detail to export in case
        // it is expanded before the first selected index of the
        // current dataGridContext
        int nextDetailIndex = ( indexesToMasterItemList.Count > 0 ) ? indexesToMasterItemList[ 0 ] : -1;

        // Get informations on columns, visible positions and selected item indexes
        // for this dataGridContext
        int[] exportedVisiblePositions = this.GetVisiblePositionsForContext( dataGridContext );
        ColumnBase[] columnsByVisiblePosition = this.GetVisibleColumnsArrayForContext( dataGridContext );
        SelectionRange[] selectedItemsRanges = this.GetSelectedItemsStoreForDataGridContext( dataGridContext );

        Debug.Assert( exportedVisiblePositions != null );
        Debug.Assert( columnsByVisiblePosition != null );
        Debug.Assert( selectedItemsRanges != null );

        int selectedItemsRangesCount = selectedItemsRanges.Length;

        // Ensure to flag the exporter has items
        this.HasItems = this.HasItems || ( selectedItemsRangesCount > 0 );

        for( int i = 0; i < selectedItemsRangesCount; i++ )
        {
          // For the first level, ensure to export the headers
          // before anything else
          if( detailLevel == 0 )
          {
            this.ExportHeaders( dataGridContext, detailLevel, exportedVisiblePositions, columnsByVisiblePosition );
          }

          SelectionRange range = selectedItemsRanges[ i ];

          int startIndex = range.StartIndex;
          int endIndex = range.EndIndex;

          // If range is inverted
          if( startIndex > endIndex )
          {
            startIndex = range.EndIndex;
            endIndex = range.StartIndex;
          }

          // For each index in the range
          for( int itemIndex = startIndex; itemIndex <= endIndex; itemIndex++ )
          {
            // Export details that are before the itemIndex
            while( ( nextDetailIndex != -1 ) && ( itemIndex > nextDetailIndex ) )
            {
              this.ExportDetailForMasterIndex( dataGridContext, detailLevel, nextDetailIndex );

              // Remove it since the detail is processed
              indexesToMasterItemList.Remove( nextDetailIndex );

              nextDetailIndex = ( indexesToMasterItemList.Count > 0 ) ? indexesToMasterItemList[ 0 ] : -1;
            }

            // Ensure to re-export the headers if a detail was previously exported
            this.ExportHeaders( dataGridContext, detailLevel, exportedVisiblePositions, columnsByVisiblePosition );

            object exportedItem = dataGridContext.Items.GetItemAt( itemIndex );

            this.ExportDataItem( dataGridContext, itemIndex, exportedItem, detailLevel, exportedVisiblePositions, columnsByVisiblePosition );
          }
        }

        nextDetailIndex = ( indexesToMasterItemList.Count > 0 ) ? indexesToMasterItemList[ 0 ] : -1;

        // Export all remaining details since none are before the context's
        // selected index ranges
        while( nextDetailIndex != -1 )
        {
          this.ExportDetailForMasterIndex( dataGridContext, detailLevel, nextDetailIndex );

          // Remove it since the detail is processed
          indexesToMasterItemList.Remove( nextDetailIndex );

          nextDetailIndex = ( indexesToMasterItemList.Count > 0 ) ? indexesToMasterItemList[ 0 ] : -1;
        }
      }

      private void ExportDataItemCore( DataGridContext dataGridContext, ClipboardExporterBase clipboardExporter, int itemIndex, object item,
                                       int[] exportedVisiblePositions, ColumnBase[] columnsByVisiblePosition )
      {
        var dataGridCollectionViewBase = dataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;

        clipboardExporter.StartDataItem( dataGridContext, item );

        // Ensure the count does not exceeds the columns count
        var exportedVisiblePositionsCount = exportedVisiblePositions.Length;
        exportedVisiblePositionsCount = Math.Min( exportedVisiblePositionsCount, columnsByVisiblePosition.Length );

        var intersectedIndexes = this.GetIntersectedRangesForIndex( dataGridContext, itemIndex, exportedVisiblePositions, exportedVisiblePositionsCount );

        for( int i = 0; i < exportedVisiblePositionsCount; i++ )
        {
          var fieldValue = default( object );
          var column = default( Column );
          var visiblePosition = exportedVisiblePositions[ i ];

          // Export null if not intersected by a SelectionRange
          if( intersectedIndexes.Contains( visiblePosition ) )
          {
            // Only export visible data column
            column = columnsByVisiblePosition[ visiblePosition ] as Column;

            if( column == null )
              continue;

            // Use DataGridCollectionView directly since the DataGridItemProperty uses PropertyDescriptor which increase the read of the field value
            var dataGridItemProperty = default( DataGridItemPropertyBase );

            // Try to get a DataGridItemProperty matching the column FieldName and get the value from it
            if( dataGridCollectionViewBase != null )
            {
              dataGridItemProperty = ItemsSourceHelper.GetItemPropertyFromProperty( dataGridCollectionViewBase.ItemProperties, column.FieldName );

              if( dataGridItemProperty != null )
              {
                fieldValue = ItemsSourceHelper.GetValueFromItemProperty( dataGridItemProperty, item );
              }
            }

            // If none was found, create a BindingPathValueExtractor from this column
            if( ( dataGridCollectionViewBase == null ) || ( dataGridItemProperty == null ) )
            {
              // We don't have a DataGridCollectionView, use a BindingPathValueExtractor to create a binding to help us get the value for the Column in the data item
              var extractorForRead = default( BindingPathValueExtractor );

              if( m_columnToBindingPathExtractor.TryGetValue( column, out extractorForRead ) == false )
              {
                extractorForRead = dataGridContext.GetBindingPathExtractorForColumn( column, item );
                m_columnToBindingPathExtractor.Add( column, extractorForRead );
              }

              fieldValue = extractorForRead.GetValueFromItem( item );
            }
          }

          if( fieldValue != null )
          {
            //Verify if the value should be converted to the displayed value for exporting.
            var foreignKeyConfiguration = column.ForeignKeyConfiguration;
            if( foreignKeyConfiguration != null && foreignKeyConfiguration.UseDisplayedValueWhenExporting )
            {
              fieldValue = foreignKeyConfiguration.GetDisplayMemberValue( fieldValue );
            }
            else
            {
              var valueConverter = column.DisplayedValueConverter;
              if( valueConverter != null )
              {
                fieldValue = valueConverter.Convert( fieldValue, typeof( object ), column.DisplayedValueConverterParameter,
                                                     column.GetCulture( column.DisplayedValueConverterCulture ) );
              }
              else
              {
                var valueFormat = column.CellContentStringFormat;
                if( !string.IsNullOrEmpty( valueFormat ) )
                {
                  fieldValue = string.Format( column.GetCulture(), valueFormat, fieldValue );
                }
              }
            }
          }

          clipboardExporter.StartDataItemField( dataGridContext, column, fieldValue );
          clipboardExporter.EndDataItemField( dataGridContext, column, fieldValue );
        }

        clipboardExporter.EndDataItem( dataGridContext, item );
      }

      private void ExportHeadersCore( DataGridContext dataGridContext, ClipboardExporterBase clipboardExporter, int[] exportedVisiblePositions,
                                      ColumnBase[] columnsByVisiblePosition )
      {
        clipboardExporter.StartHeader( dataGridContext );

        // Ensure the count does not exceeds the columns count
        int exportedVisiblePositionsCount = exportedVisiblePositions.Length;
        exportedVisiblePositionsCount = Math.Min( exportedVisiblePositionsCount, columnsByVisiblePosition.Length );

        for( int i = 0; i < exportedVisiblePositionsCount; i++ )
        {
          int visiblePosition = exportedVisiblePositions[ i ];

          // Only export visible data column
          Column column = columnsByVisiblePosition[ visiblePosition ] as Column;

          if( column == null )
            continue;

          clipboardExporter.StartHeaderField( dataGridContext, column );
          clipboardExporter.EndHeaderField( dataGridContext, column );
        }

        clipboardExporter.EndHeader( dataGridContext );
      }

      private HashSet<int> GetIntersectedRangesForIndex( DataGridContext dataGridContext, int itemIndex, int[] exportedVisiblePositions, int correctedExportedVisiblePositionsCount )
      {
        HashSet<int> intersectedIndexes = null;

        if( correctedExportedVisiblePositionsCount == 0 )
          return intersectedIndexes;

        SelectionItemRangeCollection selectedRanges = dataGridContext.SelectedItemRanges as SelectionItemRangeCollection;

        if( selectedRanges.Contains( itemIndex ) )
        {
          intersectedIndexes = new HashSet<int>( exportedVisiblePositions );
        }
        else
        {
          intersectedIndexes = new HashSet<int>();

          SelectionCellRange columnsRange = new SelectionCellRange( itemIndex, exportedVisiblePositions[ 0 ], itemIndex,
                                                                    exportedVisiblePositions[ correctedExportedVisiblePositionsCount - 1 ] );

          IEnumerable<SelectionRange> intersectedRanges = dataGridContext.SelectedCellsStore.GetIntersectedColumnRanges( columnsRange );

          foreach( SelectionRange range in intersectedRanges )
          {
            int startIndex = range.StartIndex;
            int endIndex = range.EndIndex;

            if( startIndex > endIndex )
            {
              startIndex = range.EndIndex;
              endIndex = range.StartIndex;
            }

            for( int i = startIndex; i <= endIndex; i++ )
            {
              if( !intersectedIndexes.Contains( i ) )
              {
                intersectedIndexes.Add( i );
              }
            }
          }
        }

        return intersectedIndexes;
      }

      private void GetAllVisibleColumnsVisiblePosition( DataGridContext dataGridContext, ref HashSet<int> exportedColumnPositions )
      {
        if( exportedColumnPositions == null )
        {
          exportedColumnPositions = new HashSet<int>();
        }

        if( dataGridContext == null )
          return;

        ReadOnlyObservableCollection<ColumnBase> visibleColumns = dataGridContext.VisibleColumns;

        int visibleColumnsCount = visibleColumns.Count;

        for( int i = 0; i < visibleColumnsCount; i++ )
        {
          ColumnBase column = dataGridContext.VisibleColumns[ i ];

          if( column == null )
            continue;

          exportedColumnPositions.Add( column.VisiblePosition );
        }
      }

      private void ExportDataItem( DataGridContext dataGridContext, int itemIndex, object item, int detailLevel, int[] exportedVisiblePositions,
                                   ColumnBase[] columnsByVisiblePosition )
      {

        foreach( ClipboardExporterBase clipboardExporter in m_clipboardExporters )
        {
          this.ExportDataItemCore( dataGridContext, clipboardExporter, itemIndex, item, exportedVisiblePositions, columnsByVisiblePosition );
        }
      }

      private void ExportHeaders( DataGridContext dataGridContext, int detailLevel, int[] exportedVisiblePositions, ColumnBase[] columnsByVisiblePosition )
      {
        // Master level was already exported, only update the lastExportedHeaderDetailLevel
        if( ( m_lastExportedHeaderDetailLevel != -1 ) && ( detailLevel == 0 ) )
        {
          m_lastExportedHeaderDetailLevel = 0;
          return;
        }

        // Headers are already exported for this detail level
        if( m_lastExportedHeaderDetailLevel == detailLevel )
          return;


        foreach( ClipboardExporterBase clipboardExporter in m_clipboardExporters )
        {
          // We always add the headers for detail levels every time
          if( clipboardExporter.IncludeColumnHeaders )
          {
            this.ExportHeadersCore( dataGridContext, clipboardExporter, exportedVisiblePositions, columnsByVisiblePosition );
          }
        }

        m_lastExportedHeaderDetailLevel = detailLevel;
      }

      private void ExportDetailForMasterIndex( DataGridContext dataGridContext, int detailLevel, int masterIndexForDetail )
      {
        object item = dataGridContext.Items.GetItemAt( masterIndexForDetail );

        IEnumerable<DataGridContext> dataGridContexts = dataGridContext.CustomItemContainerGenerator.GetChildContextsForMasterItem( item );

        foreach( DataGridContext childContext in dataGridContexts )
        {
          this.Export( childContext );
        }

        this.UpdateExporterIndentation( detailLevel );
      }

      private ColumnBase[] GetVisibleColumnsArrayForContext( DataGridContext dataGridContext )
      {
        if( dataGridContext == null )
          throw new ArgumentNullException( "sourceContext" );

        int detailLevel = dataGridContext.DetailLevel;

        if( m_visitedDetailVisibleColumnsCache.ContainsKey( detailLevel ) )
          return m_visitedDetailVisibleColumnsCache[ detailLevel ];

        int columnsByVisiblePositionCount = dataGridContext.ColumnsByVisiblePosition.Count;
        ColumnBase[] columnsByVisiblePosition = new ColumnBase[ columnsByVisiblePositionCount ];
        dataGridContext.ColumnsByVisiblePosition.CopyTo( columnsByVisiblePosition, 0 );

        m_visitedDetailVisibleColumnsCache.Add( dataGridContext.DetailLevel, columnsByVisiblePosition );

        return columnsByVisiblePosition;
      }

      private int[] GetVisiblePositionsForContext( DataGridContext dataGridContext )
      {
        if( dataGridContext == null )
          throw new ArgumentNullException( "dataGridContext" );

        int detailLevel = dataGridContext.DetailLevel;

        if( m_visitedDetailExportedVisiblePositions.ContainsKey( detailLevel ) )
          return m_visitedDetailExportedVisiblePositions[ detailLevel ];

        // We must keep a list of VisiblePositions to export
        HashSet<int> exportedColumnPositions = new HashSet<int>();

        DataGridContext rootDataGridContext = dataGridContext.DataGridControl.DataGridContext;

        this.GetVisiblePositionsForContextDetailLevel( rootDataGridContext, detailLevel, exportedColumnPositions );

        int exportedColumnPositionsCount = exportedColumnPositions.Count;
        int[] exportedVisiblePositionsArray = null;

        exportedVisiblePositionsArray = new int[ exportedColumnPositionsCount ];
        exportedColumnPositions.CopyTo( exportedVisiblePositionsArray );
        Array.Sort( exportedVisiblePositionsArray );

        m_visitedDetailExportedVisiblePositions.Add( detailLevel, exportedVisiblePositionsArray );

        return exportedVisiblePositionsArray;
      }

      // Parse all the expanded details recursively
      private void GetVisiblePositionsForContextDetailLevel( DataGridContext dataGridContext, int desiredDetailLevel, HashSet<int> exportedColumnPositions )
      {
        int dataGridContextDetailLevel = dataGridContext.DetailLevel;

        // The detail level is too deep, return immediately
        if( dataGridContextDetailLevel > desiredDetailLevel )
          return;

        // The desired detail level is reached get the exportedColumnPositions
        if( dataGridContextDetailLevel == desiredDetailLevel )
        {
          DataGridContext parentDataGridContext = dataGridContext.ParentDataGridContext;

          if( parentDataGridContext == null )
          {
            this.GetVisibleColumnsVisiblePositionForDataGridContext( dataGridContext, exportedColumnPositions );
          }
          else
          {
            foreach( DataGridContext childContext in parentDataGridContext.GetChildContexts() )
            {
              if( this.GetVisibleColumnsVisiblePositionForDataGridContext( childContext, exportedColumnPositions ) )
              {
                // All columns need to be exported, stop parsing child DataGridContexts
                break;
              }
            }
          }
        }
        else
        {
          // The detail level differs, parse the child contexts recursively
          foreach( DataGridContext childContext in dataGridContext.GetChildContexts() )
          {
            this.GetVisiblePositionsForContextDetailLevel( childContext, desiredDetailLevel, exportedColumnPositions );
          }
        }
      }

      private SelectionRange[] GetSelectedItemsStoreForDataGridContext( DataGridContext dataGridContext )
      {
        if( dataGridContext == null )
          return null;

        SelectedItemsStorage itemStorage = new SelectedItemsStorage( null );

        foreach( SelectionRange range in dataGridContext.SelectedItemRanges )
        {
          itemStorage.Add( new SelectionRangeWithItems( range, null ) );
        }

        foreach( SelectionCellRange range in dataGridContext.SelectedCellRanges )
        {
          SelectionRangeWithItems itemRange = new SelectionRangeWithItems( range.ItemRange, null );

          if( !itemStorage.Contains( itemRange ) )
          {
            itemStorage.Add( itemRange );
          }
        }

        SelectionRange[] itemStorageArray = itemStorage.ToSelectionRangeArray();
        Array.Sort( itemStorageArray );

        return itemStorageArray;
      }

      private bool GetVisibleColumnsVisiblePositionForDataGridContext( DataGridContext dataGridContext, HashSet<int> exportedColumnPositions )
      {
        if( dataGridContext == null )
          return false;

        if( exportedColumnPositions == null )
        {
          exportedColumnPositions = new HashSet<int>();
        }

        // At least 1 row was completely selected, add
        // all VisibleColumns' VisiblePosition 
        if( dataGridContext.SelectedItemRanges.Count > 0 )
        {
          this.GetAllVisibleColumnsVisiblePosition( dataGridContext, ref exportedColumnPositions );

          // Ensure to set the allColumnExported
          return true;
        }

        foreach( SelectionCellRange range in dataGridContext.SelectedCellRanges )
        {
          // If all columns were already exported, no need to 
          // ensure visible positions in the exportedColumnPositions
          SelectionRange columnRange = range.ColumnRange;

          int startIndex = columnRange.StartIndex;
          int endIndex = columnRange.EndIndex;

          if( startIndex > endIndex )
          {
            startIndex = columnRange.EndIndex;
            endIndex = columnRange.StartIndex;
          }

          for( int i = startIndex; i <= endIndex; i++ )
          {
            if( !exportedColumnPositions.Contains( i ) )
            {
              exportedColumnPositions.Add( i );
            }
          }
        }

        return false;
      }

      // Returns if the indentation changed
      private bool UpdateExporterIndentation( int detailLevel )
      {
        // Indent / Unindent up to the desired detail level
        if( m_currentIndentationLevel < detailLevel )
        {
          while( m_currentIndentationLevel < detailLevel )
          {
            foreach( ClipboardExporterBase clipboardExporter in m_clipboardExporters )
            {
              clipboardExporter.Indent();
            }

            Debug.Indent();
            m_currentIndentationLevel++;
          }

          return true;
        }
        else if( m_currentIndentationLevel > detailLevel )
        {
          while( m_currentIndentationLevel > detailLevel )
          {
            foreach( ClipboardExporterBase clipboardExporter in m_clipboardExporters )
            {
              clipboardExporter.Unindent();
            }

            Debug.Unindent();
            m_currentIndentationLevel--;
          }

          return true;
        }

        return false;
      }

      #region IDisposable Members

      public void Dispose()
      {
        m_clipboardExporters.Clear();
        m_columnToBindingPathExtractor.Clear();
        m_visitedDetailExportedVisiblePositions.Clear();
        m_visitedDetailVisibleColumnsCache.Clear();
      }

      #endregion IDisposable Members

      private Dictionary<Column, BindingPathValueExtractor> m_columnToBindingPathExtractor; // = null;
      private Dictionary<int, int[]> m_visitedDetailExportedVisiblePositions; // = null; 
      private Dictionary<int, ColumnBase[]> m_visitedDetailVisibleColumnsCache; // = null; 
      private List<ClipboardExporterBase> m_clipboardExporters; // = null;
      private int m_currentIndentationLevel; // = 0;
      private int m_lastExportedHeaderDetailLevel = -1;
    }
  }
}
