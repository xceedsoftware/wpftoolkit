/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

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
