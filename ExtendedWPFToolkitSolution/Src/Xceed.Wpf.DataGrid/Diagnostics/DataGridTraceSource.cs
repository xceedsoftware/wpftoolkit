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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Xceed.Wpf.DataGrid.Diagnostics
{
  internal class DataGridTraceSource
  {
    #region Static Fields

    internal readonly IDisposable NoTrace = new EmptyDisposable();

    #endregion

    internal DataGridTraceSource( TraceSource traceSource )
    {
      if( traceSource == null )
        throw new ArgumentNullException( "traceSource" );

      m_traceSource = traceSource;
    }

    internal bool ShouldTrace( TraceEventType eventType )
    {
#if TRACE
      var listeners = m_traceSource.Listeners;
      if( ( listeners == null ) || ( listeners.Count <= 0 ) )
        return false;

      var sourceSwitch = m_traceSource.Switch;

      return ( sourceSwitch != null )
          && ( sourceSwitch.ShouldTrace( eventType ) );
#else
      return false;
#endif
    }

    internal bool ShouldTraceBlock()
    {
      return ( this.ShouldTrace( TraceEventType.Start ) )
          || ( this.ShouldTrace( TraceEventType.Stop ) );
    }

    internal IDisposable TraceBlock( DataGridTraceEventId eventId, string message, IEnumerable<DataGridTraceArg> args )
    {
      if( this.ShouldTrace( TraceEventType.Start ) )
      {
        this.TraceEventCore( TraceEventType.Start, eventId, message, args );
      }

      if( this.ShouldTrace( TraceEventType.Stop ) )
        return new TraceBlockEndDisposable( this, eventId, message, args );

      return this.NoTrace;
    }

    [Conditional( "TRACE" )]
    internal void TraceEvent( TraceEventType eventType, DataGridTraceEventId eventId, string message, IEnumerable<DataGridTraceArg> args )
    {
      if( !this.ShouldTrace( eventType ) )
        return;

      this.TraceEventCore( eventType, eventId, message, args );
    }

    private void TraceEventCore( TraceEventType eventType, DataGridTraceEventId eventId, string message, IEnumerable<DataGridTraceArg> args )
    {
      var format = DataGridTraceSource.Format( args );

      if( string.IsNullOrEmpty( message ) )
      {
        if( string.IsNullOrEmpty( format ) )
        {
          DataGridTraceSource.TraceEvent( m_traceSource, eventType, ( int )eventId, null );
        }
        else
        {
          DataGridTraceSource.TraceEvent( m_traceSource, eventType, ( int )eventId, format );
        }
      }
      else
      {
        if( string.IsNullOrEmpty( format ) )
        {
          DataGridTraceSource.TraceEvent( m_traceSource, eventType, ( int )eventId, message );
        }
        else
        {
          DataGridTraceSource.TraceEvent( m_traceSource, eventType, ( int )eventId, message + " | " + format );
        }
      }
    }

    private static void TraceEvent( TraceSource source, TraceEventType eventType, int eventId, string message )
    {
      if( string.IsNullOrEmpty( message ) )
      {
        source.TraceEvent( eventType, eventId );
      }
      else
      {
        source.TraceEvent( eventType, eventId, message );
      }

      if( Debugger.IsAttached )
      {
        source.Flush();
      }
    }

    private static string Format( IEnumerable<DataGridTraceArg> args )
    {
      if( ( args == null ) || !args.Any() )
        return null;

      var sb = new StringBuilder( 128 );

      foreach( var arg in args )
      {
        var value = arg.ToString();

        if( string.IsNullOrEmpty( value ) )
          continue;

        if( sb.Length > 0 )
        {
          sb.Append( "; " );
        }

        sb.Append( value );
      }

      return sb.ToString();
    }

    private readonly TraceSource m_traceSource;

    #region TraceBlockEndDisposable Private Class

    private sealed class TraceBlockEndDisposable : IDisposable
    {
      internal TraceBlockEndDisposable( DataGridTraceSource owner, DataGridTraceEventId eventId, string message, IEnumerable<DataGridTraceArg> args )
      {
        if( owner == null )
          throw new ArgumentNullException( "owner" );

        m_owner = owner;
        m_eventId = eventId;
        m_message = message;
        m_args = args;
      }

      void IDisposable.Dispose()
      {
        var owner = Interlocked.Exchange( ref m_owner, null );
        if( owner == null )
          return;

        owner.TraceEventCore( TraceEventType.Stop, m_eventId, m_message, m_args );
      }

      private DataGridTraceSource m_owner;
      private readonly DataGridTraceEventId m_eventId;
      private readonly string m_message;
      private readonly IEnumerable<DataGridTraceArg> m_args;
    }

    #endregion

    #region EmptyDisposable Private Class

    private sealed class EmptyDisposable : IDisposable
    {
      void IDisposable.Dispose()
      {
      }
    }

    #endregion
  }
}
