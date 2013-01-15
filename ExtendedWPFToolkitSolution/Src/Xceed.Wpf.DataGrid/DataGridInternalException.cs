/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

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

    public DataGridInternalException( Exception innerException )
      : this( DefaultMessage, innerException )
    {
    }

    public DataGridInternalException( string message, Exception innerException )
      : base( message, innerException )
    {
    }

    protected DataGridInternalException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
    }

    private const string DefaultMessage = "An unexpected internal failure occurred in the Xceed WPF DataGrid control.";
  }
}
