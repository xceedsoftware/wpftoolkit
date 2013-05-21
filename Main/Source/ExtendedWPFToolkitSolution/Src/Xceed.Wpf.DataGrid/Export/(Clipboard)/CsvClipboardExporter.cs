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
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Export
{
  public class CsvClipboardExporter : ClipboardExporterBase
  {
    #region PUBLIC CONSTRUCTORS

    public CsvClipboardExporter()
      : base()
    {
      this.IncludeColumnHeaders = true;
      this.FormatSettings = new CsvFormatSettings();
      m_indentationString = string.Empty;
      m_baseStream = new ToStringMemoryStream();
    }

    #endregion

    #region PUBLIC PROPERTIES

    public CsvFormatSettings FormatSettings
    {
      get;
      private set;
    }

    #endregion

    #region PROTECTED OVERRIDES

    protected override object ClipboardData
    {
      get
      {
        return m_baseStream;
      }
    }

    protected override void Indent()
    {
      char separator = this.FormatSettings.Separator;
      m_indentationString += separator;
    }

    protected override void Unindent()
    {
      char separator = this.FormatSettings.Separator;

      if( m_indentationString == null )
      {
        Debug.Fail( "Indentation must at least be string.empty when unindenting." );
        m_indentationString = string.Empty;
      }
      else
      {
        int separatorLength = 1;
        int indentationLength = m_indentationString.Length;

        // If there are less characters in indentationString than in the empty field, just set indentation
        // as string.empty
        if( indentationLength < separatorLength )
        {
          m_indentationString = string.Empty;
        }
        else
        {
          m_indentationString = m_indentationString.Substring( 0, indentationLength - separatorLength );
        }
      }
    }

    protected override void ResetExporter()
    {
      m_baseStream = new ToStringMemoryStream();
      m_indentationString = string.Empty;
    }

    protected override void StartHeader( DataGridContext dataGridContext )
    {
      if( string.IsNullOrEmpty( m_indentationString ) == false )
        this.WriteToBaseStream( m_indentationString );

      // The next StartDataItemField will be considered as first column
      m_isFirstColumn = true;
    }

    protected override void StartHeaderField( DataGridContext dataGridContext, Column column )
    {
      // We always insert the separator before the value except for the first item
      if( !m_isFirstColumn )
      {
        this.WriteToBaseStream( this.FormatSettings.Separator );
      }
      else
      {
        m_isFirstColumn = false;
      }

      object columnHeader = ( ( this.UseFieldNamesInHeader ) || ( column.Title == null ) ) ? column.FieldName : column.Title;

      string fieldValueString = FormatHelper.FormatCsvData( null, columnHeader, this.FormatSettings );

      this.WriteToBaseStream( fieldValueString );
    }

    protected override void EndHeader( DataGridContext dataGridContext )
    {
      this.WriteToBaseStream( this.FormatSettings.NewLine );
    }

    protected override void StartDataItem( DataGridContext dataGridContext, object dataItem )
    {
      if( string.IsNullOrEmpty( m_indentationString ) == false )
        this.WriteToBaseStream( m_indentationString );

      // The next StartDataItemField will be considered as first column
      m_isFirstColumn = true;
    }

    protected override void StartDataItemField( DataGridContext dataGridContext, Column column, object fieldValue )
    {
      // We always insert the separator before the value except for the first item
      if( !m_isFirstColumn )
      {
        this.WriteToBaseStream( this.FormatSettings.Separator );
      }
      else
      {
        m_isFirstColumn = false;
      }

      string fieldValueString = FormatHelper.FormatCsvData( null, fieldValue, this.FormatSettings );

      this.WriteToBaseStream( fieldValueString );
    }

    protected override void EndDataItem( DataGridContext dataGridContext, object dataItem )
    {
      this.WriteToBaseStream( this.FormatSettings.NewLine );
    }

    #endregion

    #region PRIVATE METHODS

    private void WriteToBaseStream( char value )
    {
      byte[] tempBuffer = Encoding.Default.GetBytes( new char[] { value } );
      m_baseStream.Write( tempBuffer, 0, tempBuffer.Length );
    }

    private void WriteToBaseStream( string value )
    {
      if( string.IsNullOrEmpty( value ) )
        return;

      byte[] tempBuffer = Encoding.Default.GetBytes( value );
      m_baseStream.Write( tempBuffer, 0, tempBuffer.Length );
    }

    #endregion

    #region PRIVATE FIELDS

    private string m_indentationString; // = null;
    private MemoryStream m_baseStream; // = null;
    private bool m_isFirstColumn; // = false;

    #endregion

    #region ToStringMemoryStream Private Class

    // This class is used to force the ToString of the
    // MemoryStream to return the content of the Stream
    // instead of the name of the Type. 
    private class ToStringMemoryStream : MemoryStream
    {
      public override string ToString()
      {
        if( this.Length == 0 )
        {
          return string.Empty;
        }
        else
        {
          try
          {
            return Encoding.Default.GetString( this.ToArray() );
          }
          catch( Exception )
          {
            return base.ToString();
          }
        }
      }
    }

    #endregion
  }
}
