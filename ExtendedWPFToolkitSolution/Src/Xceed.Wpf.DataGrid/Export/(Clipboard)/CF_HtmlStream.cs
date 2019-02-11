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
using System.Globalization;

namespace Xceed.Wpf.DataGrid.Export
{
  // Class used to format data into the CF_HTML Clipbard Format
  // http://msdn.microsoft.com/en-us/library/aa767917(VS.85).aspx
  internal sealed class CF_HtmlStream : Stream
  {
    public CF_HtmlStream( Stream innerStream )
    {
      if( innerStream == null )
        throw new ArgumentNullException( "innerStream" );

      if( innerStream.CanWrite == false )
        throw new InvalidOperationException( "An attempt was made to use a non-writable stream." );

      if( innerStream.CanSeek == false )
        throw new InvalidOperationException( "An attempt was made to use a non-seekable stream." );

      m_innerStream = innerStream;

      StringBuilder headerStringBuilder = new StringBuilder();
      headerStringBuilder.Append( "Version:1.0" );
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "StartHTML:-1" ); // This is optional according to MSDN documentation
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "EndHTML:-1" ); // This is optional according to MSDN documentation
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "StartFragment:0000000109" ); // Always 109 bytes from start of Version to the end of <!--StartFragment--> tag
      headerStringBuilder.Append( Environment.NewLine );

      // Get the offset of the EndFragment: tag to be able to modify the 10 digits
      m_endFragmentOffset = headerStringBuilder.ToString().Length;

      Debug.Assert( m_endFragmentOffset == 65 );

      headerStringBuilder.Append( "EndFragment:0000000000" ); // We write 0000000000 and we will update this field when the Stream is closed
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "<!--StartFragment-->" );

      string headerString = headerStringBuilder.ToString();

      byte[] tempBuffer = Encoding.UTF8.GetBytes( headerString );

      m_headerBytesLength = headerStringBuilder.Length;

      Debug.Assert( m_headerBytesLength == tempBuffer.Length );
      Debug.Assert( tempBuffer.Length == 109 );

      m_innerStream.Write( tempBuffer, 0, m_headerBytesLength );
      m_HtmlContentByteCount = 0;
    }

    public override bool CanRead
    {
      get
      {
        return false;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return false;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return true;
      }
    }

    public override long Length
    {
      get
      {
        return m_headerBytesLength + m_HtmlContentByteCount + s_footerBytes.Length;
      }
    }

    public override long Position
    {
      get
      {
        throw new NotSupportedException();
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    public override void Close()
    {
      // Already closed, nothing to do
      if( m_closed )
        return;

      // Update the value of EndFragment field in the header
      string endFragmentOffset = ( m_HtmlContentByteCount + m_headerBytesLength ).ToString( "0000000000", CultureInfo.InvariantCulture );

      m_innerStream.Seek( m_endFragmentOffset, SeekOrigin.Begin );

      byte[] tempBuffer = Encoding.UTF8.GetBytes( "EndFragment:" + endFragmentOffset );
      Debug.Assert( tempBuffer.Length == 22 );
      m_innerStream.Write( tempBuffer, 0, tempBuffer.Length );

      // Append the final end line and EndFragment tag
      m_innerStream.Seek( 0, SeekOrigin.End );
      m_innerStream.Write( s_footerBytes, 0, s_footerBytes.Length );
      m_innerStream.Flush();

      m_closed = true;
    }

    protected override void Dispose( bool disposing )
    {
      this.Close();
    }

    public override void Flush()
    {
      m_innerStream.Flush();
    }

    public override int Read( byte[] buffer, int offset, int count )
    {
      throw new NotSupportedException();
    }

    public override int ReadByte()
    {
      throw new NotSupportedException();
    }

    public override long Seek( long offset, SeekOrigin origin )
    {
      throw new NotSupportedException();
    }

    public override void SetLength( long value )
    {
      throw new NotSupportedException();
    }

    public override void WriteByte( byte value )
    {
      this.CheckIfClosed();
      m_HtmlContentByteCount++;
      m_innerStream.WriteByte( value );
    }

    public override void Write( byte[] buffer, int offset, int count )
    {
      this.CheckIfClosed();
      m_HtmlContentByteCount += count;
      m_innerStream.Write( buffer, offset, count );
    }

    private void CheckIfClosed()
    {
      if( m_closed )
        throw new InvalidOperationException( "An attempt was made to access a closed stream." );
    }

    private bool m_closed; // = false;
    private int m_endFragmentOffset; // = 0;
    private int m_headerBytesLength; // = 0;
    private long m_HtmlContentByteCount; // = 0;
    private Stream m_innerStream; // = null;

    private static byte[] s_footerBytes = Encoding.UTF8.GetBytes( Environment.NewLine + "<!--EndFragment-->" );
  }
}
