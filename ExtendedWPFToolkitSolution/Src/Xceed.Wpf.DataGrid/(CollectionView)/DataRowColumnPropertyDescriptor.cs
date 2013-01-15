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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class DataRowColumnPropertyDescriptor : PropertyDescriptor
  {
    public DataRowColumnPropertyDescriptor( System.Data.DataColumn column )
      : base( column.ColumnName, null )
    {
      if( column == null )
        throw new ArgumentNullException( "column" );

      m_columnName = column.ColumnName;
      m_columnDataType = ItemsSourceHelper.GetColumnDataType( column );
      m_columnCaption = column.Caption;
      m_columnReadOnly = column.ReadOnly;
      m_columnHidden = ( column.ColumnMapping == MappingType.Hidden );
    }

    public override string DisplayName
    {
      get
      {
        return m_columnCaption;
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
        return typeof( System.Data.DataRow );
      }
    }

    public override bool IsBrowsable
    {
      get
      {
        if( m_columnHidden )
          return false;

        return base.IsBrowsable;
      }
    }

    public override bool IsReadOnly
    {
      get
      {
        return m_columnReadOnly;
      }
    }

    public override Type PropertyType
    {
      get
      {
        return m_columnDataType;
      }
    }

    public override bool CanResetValue( object component )
    {
      object value = this.GetValue( component );
      return ( value != null ) && ( value != DBNull.Value );
    }

    public override object GetValue( object component )
    {
      System.Data.DataRow dataRow = component as System.Data.DataRow;

      if( dataRow == null )
        return null;

      object value = null;

      try
      {
        value = dataRow[ m_columnName ];
      }
      catch
      {
      }

      if( value == DBNull.Value )
        return null;

      return value;
    }

    public override void ResetValue( object component )
    {
      this.SetValue( component, null );
    }

    public override void SetValue( object component, object value )
    {
      System.Data.DataRow dataRow = component as System.Data.DataRow;

      if( dataRow == null )
        throw new InvalidOperationException( "An attempt was made to set a value on a DataRow that does not exist." );

      if( this.IsReadOnly )
        throw new InvalidOperationException( "An attempt was made to set a value on a read-only field." );

      object oldValue = dataRow[ m_columnName ];

      if( ( oldValue == null ) || ( oldValue == DBNull.Value ) )
      {
        if( ( ( value == null ) || ( value == DBNull.Value ) ) )
          return;
      }
      else
      {
        if( oldValue.Equals( value ) )
          return;
      }

      if( value == null )
      {
        dataRow[ m_columnName ] = DBNull.Value;
      }
      else
      {
        dataRow[ m_columnName ] = value;
      }

      this.OnValueChanged( component, EventArgs.Empty );
    }

    public override bool ShouldSerializeValue( object component )
    {
      return false;
    }

    private string m_columnName;
    private Type m_columnDataType;
    private string m_columnCaption;
    private bool m_columnReadOnly;
    private bool m_columnHidden;
  }
}
