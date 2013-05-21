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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xceed.Wpf.DataGrid
{
  [Serializable]
  public class DataGridFocusException : DataGridException
  {
    public DataGridFocusException()
      : base()
    {
    }

    public DataGridFocusException( string message )
      : base( message )
    {
    }

    public DataGridFocusException( string message, Exception innerException )
      : base( message, innerException )
    {
    }

    protected DataGridFocusException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
    }
  }
}
