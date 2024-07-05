/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.IO;
  using System.Runtime.InteropServices;
  using System.Runtime.InteropServices.ComTypes;

  // disambiguate with System.Runtime.InteropServices.STATSTG
  using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

  // All these methods return void.  Does the standard marshaller convert them to HRESULTs?
  internal sealed class ManagedIStream : IStream, IDisposable
  {
    private const int STGTY_STREAM = 2;
    private const int STGM_READWRITE = 2;
    private const int LOCK_EXCLUSIVE = 2;

    private Stream _source;

    public ManagedIStream( Stream source )
    {
      Verify.IsNotNull( source, "source" );
      _source = source;
    }

    private void _Validate()
    {
      if( null == _source )
      {
        throw new ObjectDisposedException( "this" );
      }
    }

    // Comments are taken from MSDN IStream documentation.
    #region IStream Members

    [SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)" )]
    [Obsolete( "The method is not implemented", true )]
    public void Clone( out IStream ppstm )
    {
      ppstm = null;
      HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed( "The method is not implemented." );
    }

    public void Commit( int grfCommitFlags )
    {
      _Validate();
      _source.Flush();
    }

    [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0" )]
    [SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands" )]
    public void CopyTo( IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten )
    {
      Verify.IsNotNull( pstm, "pstm" );

      _Validate();

      // Reasonbly sized buffer, don't try to copy large streams in bulk.
      var buffer = new byte[ 4096 ];
      long cbWritten = 0;

      while( cbWritten < cb )
      {
        int cbRead = _source.Read( buffer, 0, buffer.Length );
        if( 0 == cbRead )
        {
          break;
        }

        // COM documentation is a bit vague here whether NULL is valid for the third parameter.
        // Going to assume it is, as most implementations I've seen treat it as optional.
        // It's possible this will break on some IStream implementations.
        pstm.Write( buffer, cbRead, IntPtr.Zero );
        cbWritten += cbRead;
      }

      if( IntPtr.Zero != pcbRead )
      {
        Marshal.WriteInt64( pcbRead, cbWritten );
      }

      if( IntPtr.Zero != pcbWritten )
      {
        Marshal.WriteInt64( pcbWritten, cbWritten );
      }
    }

    [SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)" ), Obsolete( "The method is not implemented", true )]
    public void LockRegion( long libOffset, long cb, int dwLockType )
    {
      HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed( "The method is not implemented." );
    }

    [SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands" )]
    public void Read( byte[] pv, int cb, IntPtr pcbRead )
    {
      _Validate();

      int cbRead = _source.Read( pv, 0, cb );

      if( IntPtr.Zero != pcbRead )
      {
        Marshal.WriteInt32( pcbRead, cbRead );
      }
    }


    [SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)" ), Obsolete( "The method is not implemented", true )]
    public void Revert()
    {
      HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed( "The method is not implemented." );
    }

    [SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands" )]
    public void Seek( long dlibMove, int dwOrigin, IntPtr plibNewPosition )
    {
      _Validate();

      long position = _source.Seek( dlibMove, ( SeekOrigin )dwOrigin );

      if( IntPtr.Zero != plibNewPosition )
      {
        Marshal.WriteInt64( plibNewPosition, position );
      }
    }

    public void SetSize( long libNewSize )
    {
      _Validate();
      _source.SetLength( libNewSize );
    }

    public void Stat( out STATSTG pstatstg, int grfStatFlag )
    {
      pstatstg = default( STATSTG );
      _Validate();

      pstatstg.type = STGTY_STREAM;
      pstatstg.cbSize = _source.Length;
      pstatstg.grfMode = STGM_READWRITE;
      pstatstg.grfLocksSupported = LOCK_EXCLUSIVE;
    }

    [SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)" )]
    [Obsolete( "The method is not implemented", true )]
    public void UnlockRegion( long libOffset, long cb, int dwLockType )
    {
      HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed( "The method is not implemented." );
    }

    [SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands" )]
    public void Write( byte[] pv, int cb, IntPtr pcbWritten )
    {
      _Validate();

      _source.Write( pv, 0, cb );

      if( IntPtr.Zero != pcbWritten )
      {
        Marshal.WriteInt32( pcbWritten, cb );
      }
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      _source = null;
    }

    #endregion
  }
}
