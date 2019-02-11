/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Diagnostics;
using System.Threading;

namespace Xceed.Wpf.DataGrid.Diagnostics
{
  internal static class DataGridControlTraceSources
  {
    #region CollectionViewSource Property

    internal static DataGridTraceSource CollectionViewSource
    {
      get
      {
        return DataGridControlTraceSources.DataGridTraceSource;
      }
    }

    #endregion

    #region ItemContainerGeneratorSource Property

    internal static DataGridTraceSource ItemContainerGeneratorSource
    {
      get
      {
        return DataGridControlTraceSources.DataGridTraceSource;
      }
    }

    #endregion

    #region DataSource Private Property

    private static DataGridTraceSource DataGridTraceSource
    {
      get
      {
        if( s_traceSource == null )
        {
          var source = new DataGridTraceSource( new TraceSource( "Xceed.Wpf.DataGrid" ) );
          Interlocked.CompareExchange<DataGridTraceSource>( ref s_traceSource, source, null );
        }

        return s_traceSource;
      }
    }

    private static DataGridTraceSource s_traceSource; //null

    #endregion
  }
}
