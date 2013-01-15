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
  internal class InvalidSourcePropertyNameException : DataGridException
  {
    public InvalidSourcePropertyNameException()
      : base()
    {
    }

    public InvalidSourcePropertyNameException( string message )
      : base( message )
    {
    }

    public InvalidSourcePropertyNameException( string message, string sourcePropertyName )
      : base( message )
    {
      m_sourcePropertyName = sourcePropertyName;
    }

    public InvalidSourcePropertyNameException( string message, Exception innerException )
      : base( message, innerException )
    {
    }

    protected InvalidSourcePropertyNameException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
      m_sourcePropertyName = info.GetString( "SourcePropertyName" );
    }

    [System.Security.Permissions.SecurityPermissionAttribute( System.Security.Permissions.SecurityAction.Demand, SerializationFormatter = true )]
    public override void GetObjectData( SerializationInfo info, StreamingContext context )
    {
      base.GetObjectData( info, context );
      info.AddValue( "SourcePropertyName", m_sourcePropertyName, typeof( string ) );
    }

    private string m_sourcePropertyName;

    public string SourcePropertyName
    {
      get
      {
        return m_sourcePropertyName;
      }
      set
      {
        m_sourcePropertyName = value;
      }
    }
  }
}
