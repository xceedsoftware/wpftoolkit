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

namespace Xceed.Wpf.DataGrid.Stats
{
  [Serializable]
  internal class InvalidValueException : DataGridException
  {
    public InvalidValueException()
      : base()
    {
    }

    public InvalidValueException( string message )
      : base( message )
    {
    }

    public InvalidValueException( string message, Exception innerException )
      : base( message, innerException )
    {
    }

    protected InvalidValueException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
    }
  }
}
