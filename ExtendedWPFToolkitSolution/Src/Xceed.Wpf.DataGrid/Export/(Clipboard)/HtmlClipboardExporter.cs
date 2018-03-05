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
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Export
{
  public class HtmlClipboardExporter : ClipboardExporterBase
  {
    #region PUBLIC CONSTRUCTORS

    public HtmlClipboardExporter()
      : base()
    {
      this.IncludeColumnHeaders = true;
      this.FormatSettings = new HtmlFormatSettings();
      m_indentationString = string.Empty;

      // We keep a reference to the innerStream to return it when clipboard export is finished
      m_memoryStream = new MemoryStream();
      m_baseStream = new CF_HtmlStream( m_memoryStream );
    }

    #endregion

    #region PUBLIC PROPERTIES

    public HtmlFormatSettings FormatSettings
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
        // Return the innerStream of the CF_HtmlStream which contains the CF_HTML formatted data
        return m_memoryStream;
      }
    }

    protected override void Indent()
    {
      string startDelimiter = this.FormatSettings.FieldStartDelimiter;
      string endDelimiter = this.FormatSettings.FieldEndDelimiter;

      if( startDelimiter == null )
        startDelimiter = string.Empty;

      if( endDelimiter == null )
        endDelimiter = string.Empty;

      // By default, we suppose indentation as an empty field
      m_indentationString += startDelimiter + endDelimiter;
    }

    protected override void Unindent()
    {
      string startDelimiter = this.FormatSettings.FieldStartDelimiter;
      string endDelimiter = this.FormatSettings.FieldEndDelimiter;

      if( startDelimiter == null )
        startDelimiter = string.Empty;

      if( endDelimiter == null )
        endDelimiter = string.Empty;

      if( m_indentationString == null )
      {
        Debug.Fail( "Indentation must at least be string.empty when unindenting." );

        // We initalize the indentation string and return
        m_indentationString = string.Empty;
      }
      else
      {
        int emptyFieldLength = startDelimiter.Length + endDelimiter.Length;
        int indentationLength = m_indentationString.Length;

        // If there are less characters in indentationString than in the empty field, just set indentation
        // as string.empty
        if( indentationLength < emptyFieldLength )
        {
          m_indentationString = string.Empty;
        }
        else
        {
          m_indentationString = m_indentationString.Substring( 0, indentationLength - emptyFieldLength );
        }
      }
    }

    protected override void ResetExporter()
    {
      m_tempBuffer = null;
      m_indentationString = string.Empty;

      // We must NOT close or dispose the previous MemoryStream since we pass this
      // instance to the Clipboard directly and it becomes responsible of 
      // closing/disposing it
      m_memoryStream = new MemoryStream();
      m_baseStream = new CF_HtmlStream( m_memoryStream );
    }

    protected override void StartExporter( string dataFormat )
    {
      this.WriteToBaseStream( this.FormatSettings.ExporterStartDelimiter );
    }

    protected override void EndExporter( string dataFormat )
    {
      this.WriteToBaseStream( this.FormatSettings.ExporterEndDelimiter );

      // Force the header to be updated with length of HTML data and add the footer
      m_baseStream.Close();
    }

    protected override void StartHeader( DataGridContext dataGridContext )
    {
      this.WriteToBaseStream( this.FormatSettings.HeaderDataStartDelimiter );

      if( string.IsNullOrEmpty( m_indentationString ) == false )
      {
        this.WriteToBaseStream( m_indentationString );
      }
    }

    protected override void StartHeaderField( DataGridContext dataGridContext, Column column )
    {
      this.WriteToBaseStream( this.FormatSettings.HeaderFieldStartDelimiter );

      object columnHeader = ( ( this.UseFieldNamesInHeader ) || ( column.Title == null ) ) ? column.FieldName : column.Title;

      string fieldValueString = FormatHelper.FormatHtmlFieldData( null, columnHeader, this.FormatSettings );

      this.WriteToBaseStream( fieldValueString );
      this.WriteToBaseStream( this.FormatSettings.HeaderFieldEndDelimiter );
    }

    protected override void EndHeader( DataGridContext dataGridContext )
    {
      this.WriteToBaseStream( this.FormatSettings.HeaderDataEndDelimiter );
    }

    protected override void StartDataItem( DataGridContext dataGridContext, object dataItem )
    {
      this.WriteToBaseStream( this.FormatSettings.DataStartDelimiter );

      if( string.IsNullOrEmpty( m_indentationString ) == false )
      {
        this.WriteToBaseStream( m_indentationString );
      }
    }

    protected override void StartDataItemField( DataGridContext dataGridContext, Column column, object fieldValue )
    {
      this.WriteToBaseStream( this.FormatSettings.FieldStartDelimiter );
      string fieldValueString = FormatHelper.FormatHtmlFieldData( null, fieldValue, this.FormatSettings );
      this.WriteToBaseStream( fieldValueString );
      this.WriteToBaseStream( this.FormatSettings.FieldEndDelimiter );
    }

    protected override void EndDataItem( DataGridContext dataGridContext, object dataItem )
    {
      this.WriteToBaseStream( this.FormatSettings.DataEndDelimiter );
    }

    #endregion

    #region PRIVATE METHODS

    private void WriteToBaseStream( string value )
    {
      if( string.IsNullOrEmpty( value ) )
        return;

      m_tempBuffer = Encoding.UTF8.GetBytes( value );

      m_baseStream.Write( m_tempBuffer, 0, m_tempBuffer.Length );
    }

    #endregion

    #region PRIVATE FIELDS

    private string m_indentationString; // = null;
    private MemoryStream m_memoryStream; // = null;
    private CF_HtmlStream m_baseStream; // = null;
    private byte[] m_tempBuffer; // = null;

    #endregion
  }
}
