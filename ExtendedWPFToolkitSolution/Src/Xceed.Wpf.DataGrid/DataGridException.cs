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
using System.Diagnostics;
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

    internal static Exception Create<T>( string message, DataGridControl dataGridControl, string argument = "" )
    {
      Exception exception;

      var exceptionType = typeof( T );

      if( typeof( ArgumentException ) == exceptionType )
      {
        exception = new ArgumentException( message, argument );
      }
      else if( typeof( ArgumentNullException ) == exceptionType )
      {
        exception = new ArgumentNullException( message );
      }
      else if( typeof( ArgumentOutOfRangeException ) == exceptionType )
      {
        exception = new ArgumentOutOfRangeException( argument, message );
      }
      else if( typeof( IndexOutOfRangeException ) == exceptionType )
      {
        exception = new IndexOutOfRangeException( message );
      }
      else if( typeof( InvalidOperationException ) == exceptionType )
      {
        exception = new InvalidOperationException( message );
      }
      else if( typeof( NotSupportedException ) == exceptionType )
      {
        exception = new NotSupportedException( message );
      }
      else if( typeof( DataGridException ) == exceptionType )
      {
        return new DataGridException( message, dataGridControl );
      }
      else if( typeof( DataGridInternalException ) == exceptionType )
      {
        return new DataGridInternalException( message, dataGridControl );
      }
      else
      {
        exception = new Exception( message );
      }

      if( dataGridControl != null )
      {
        var name = dataGridControl.GridUniqueName;
        if( string.IsNullOrEmpty( name ) )
        {
          name = dataGridControl.Name;
        }

        if( !string.IsNullOrEmpty( name ) )
        {
          exception.Source = name;
        }
      }

      Debug.Assert( exception != null );

      return exception;
    }
  }
}
