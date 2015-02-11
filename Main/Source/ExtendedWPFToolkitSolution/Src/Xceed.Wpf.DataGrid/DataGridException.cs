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
using System.Text;
using System.Runtime.Serialization;

namespace Xceed.Wpf.DataGrid
{
  [Serializable]
  public class DataGridException : Exception
  {
    public DataGridException()
      : base()
    {
    }

    public DataGridException( string message )
      : base( message )
    {
    }

    public DataGridException( string message, DataGridControl dataGridControl )
      : base( message )
    {
      this.DataGridControl = dataGridControl;
    }

    public DataGridException( string message, Exception innerException )
      : base( message, innerException )
    {
    }

    public DataGridException( string message, Exception innerException, DataGridControl dataGridControl )
      : base( message, innerException )
    {
      this.DataGridControl = dataGridControl;
    }

    protected DataGridException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
    }

    #region DataGridControl Property

    public DataGridControl DataGridControl
    {
      get;
      private set;
    }

    #endregion

    internal static void ThrowSystemException( string message, Type exceptionType, string source, string argument = "" )
    {
      Exception exception;

      if( exceptionType == typeof( ArgumentException ) )
      {
        exception = new ArgumentException( message, argument );
      }
      else if( exceptionType == typeof( ArgumentNullException ) )
      {
        exception = new ArgumentNullException( message );
      }
      else if( exceptionType == typeof( ArgumentOutOfRangeException ) )
      {
        exception = new ArgumentOutOfRangeException( argument, message );
      }
      else if( exceptionType == typeof( IndexOutOfRangeException ) )
      {
        exception = new IndexOutOfRangeException( message );
      }
      else if( exceptionType == typeof( InvalidOperationException ) )
      {
        exception = new InvalidOperationException( message );
      }
      else if( exceptionType == typeof( NotSupportedException ) )
      {
        exception = new NotSupportedException( message );
      }
      else
      {
        exception = new Exception( message );
      }

      exception.Source = source;
      throw exception;
    }
  }
}
