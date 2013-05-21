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
using System.ComponentModel;
using System.Data;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class UnboundDataRowPropertyDescriptor : PropertyDescriptor
  {
    public UnboundDataRowPropertyDescriptor( string fieldName, Type dataType )
      : base( fieldName, null )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        throw new ArgumentException( "fieldName must not be null or empty.", "fieldName" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      m_fieldName = fieldName;
      m_dataType = dataType;
    }

    public override string DisplayName
    {
      get
      {
        return m_fieldName;
      }
    }

    public override AttributeCollection Attributes
    {
      get
      {
        if( typeof( IList ).IsAssignableFrom( this.PropertyType ) )
        {
          Attribute[] array = new Attribute[ base.Attributes.Count + 1 ];
          base.Attributes.CopyTo( array, 0 );
          array[ array.Length - 1 ] = new ListBindableAttribute( false );
          return new AttributeCollection( array );
        }

        return base.Attributes;
      }
    }

    public override Type ComponentType
    {
      get
      {
        return typeof( DataRow );
      }
    }

    public override bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public override Type PropertyType
    {
      get
      {
        return m_dataType;
      }
    }

    public override bool CanResetValue( object component )
    {
      object value = this.GetValue( component );
      return ( value != null ) && ( value != DBNull.Value );
    }

    public override object GetValue( object component )
    {
      DataRow dataRow = component as DataRow;

      if( dataRow == null )
        return null;

      Cell cell = dataRow.Cells[ m_fieldName ];

      if( cell == null )
        return null;

      return cell.Content;
    }

    public override void ResetValue( object component )
    {
      this.SetValue( component, null );
    }

    public override void SetValue( object component, object value )
    {
      if( this.IsReadOnly )
        throw new InvalidOperationException( "An attempt was made to set a value on a read-only field." );

      DataRow dataRow = component as DataRow;

      if( dataRow == null )
        throw new InvalidOperationException( "An attempt was made to set a value on a DataRow that does not exist." );

      Cell cell = dataRow.Cells[ m_fieldName ];

      if( cell == null )
        throw new InvalidOperationException( "An attempt was made to set a value on a Cell that does not exist." );

      cell.Content = value;
    }

    public override bool ShouldSerializeValue( object component )
    {
      return false;
    }

    private string m_fieldName;
    private Type m_dataType;
  }
}
