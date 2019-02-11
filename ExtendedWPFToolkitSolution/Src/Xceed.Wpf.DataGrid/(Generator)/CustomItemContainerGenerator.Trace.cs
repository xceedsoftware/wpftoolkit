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
using Xceed.Wpf.DataGrid.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  partial class CustomItemContainerGenerator
  {
    private IDisposable TraceBlock( DataGridTraceEventId eventId )
    {
      return this.TraceBlock( eventId, ( string )null, null );
    }

    private IDisposable TraceBlock( DataGridTraceEventId eventId, params DataGridTraceArg[] args )
    {
      return this.TraceBlock( eventId, null, args );
    }

    private IDisposable TraceBlock( DataGridTraceEventId eventId, string message, params DataGridTraceArg[] args )
    {
      var traceSource = DataGridControlTraceSources.ItemContainerGeneratorSource;
      if( !traceSource.ShouldTraceBlock() )
        return traceSource.NoTrace;

      return traceSource.TraceBlock( eventId, message, this.GetTraceArgs( args ).ToList() );
    }

    [Conditional( "TRACE" )]
    private void TraceEvent( TraceEventType eventType, DataGridTraceEventId eventId )
    {
      this.TraceEvent( eventType, eventId, ( string )null, null );
    }

    [Conditional( "TRACE" )]
    private void TraceEvent( TraceEventType eventType, DataGridTraceEventId eventId, params DataGridTraceArg[] args )
    {
      this.TraceEvent( eventType, eventId, null, args );
    }

    [Conditional( "TRACE" )]
    private void TraceEvent( TraceEventType eventType, DataGridTraceEventId eventId, string message, params DataGridTraceArg[] args )
    {
      var traceSource = DataGridControlTraceSources.ItemContainerGeneratorSource;
      if( !traceSource.ShouldTrace( eventType ) )
        return;

      traceSource.TraceEvent( eventType, eventId, message, this.GetTraceArgs( args ).ToList() );
    }

    private IEnumerable<DataGridTraceArg> GetTraceArgs( DataGridTraceArg[] args )
    {
      if( ( args != null ) && ( args.Length > 0 ) )
      {
        foreach( var arg in args )
        {
          yield return arg;
        }
      }

      yield return DataGridTraceArgs.Generator( this );
      yield return DataGridTraceArgs.DataGridControl( m_dataGridControl );
    }
  }
}
