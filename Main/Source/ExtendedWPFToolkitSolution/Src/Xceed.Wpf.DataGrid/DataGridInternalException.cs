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
using System.Runtime.Serialization;
using Xceed.Wpf.DataGrid;

namespace Xceed.Wpf.DataGrid
{
  [Serializable]
  public class DataGridInternalException : DataGridException
  {
    public DataGridInternalException()
      : this( DefaultMessage )
    {
    }

    public DataGridInternalException( string message )
      : base( message )
    {
    }

    public DataGridInternalException( string message, DataGridControl dataGridControl )
      : base( message, dataGridControl )
    {
    }

    public DataGridInternalException( Exception innerException )
      : this( DefaultMessage, innerException )
    {
    }

    public DataGridInternalException( string message, Exception innerException )
      : base( message, innerException )
    {
    }

    public DataGridInternalException( string message, Exception innerException, DataGridControl dataGridControl )
      : base( message, innerException, dataGridControl )
    {
    }

    protected DataGridInternalException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
    }

    private const string DefaultMessage = "An unexpected internal failure occurred in the Xceed WPF DataGrid control.";
  }
}
