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
  internal class DataGridValidationException : DataGridException
  {
    public DataGridValidationException()
      : base()
    {
    }

    public DataGridValidationException( string message )
      : base( message )
    {
    }

    public DataGridValidationException( string message, Exception innerException )
      : base( message, innerException )
    {
    }

    protected DataGridValidationException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
    }

  }
}
