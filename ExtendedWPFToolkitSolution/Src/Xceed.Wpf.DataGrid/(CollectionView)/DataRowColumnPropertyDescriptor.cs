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
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataRowColumnPropertyDescriptor : PropertyDescriptor
  {
    internal DataRowColumnPropertyDescriptor( DataColumn column )
      : base( column.ColumnName, null )
    {
      if( column == null )
        throw new ArgumentNullException( "column" );

      m_column = column;
    }

    #region DisplayName Property

    public override string DisplayName
    {
      get
      {
        return m_column.Caption;
      }
    }

    #endregion

    #region Attributes Property

    public override AttributeCollection Attributes
    {
      get
      {
        if( typeof( IList ).IsAssignableFrom( this.PropertyType ) )
        {
          var array = new Attribute[ base.Attributes.Count + 1 ];
          base.Attributes.CopyTo( array, 0 );
          array[ array.Length - 1 ] = new ListBindableAttribute( false );
          return new AttributeCollection( array );
        }

        return base.Attributes;
      }
    }

    #endregion

    #region ComponentType Property

    public override Type ComponentType
    {
      get
      {
        return typeof( System.Data.DataRow );
      }
    }

    #endregion

    #region IsBrowsable Property

    public override bool IsBrowsable
    {
      get
      {
        return ( m_column.ColumnMapping != MappingType.Hidden )
            && ( base.IsBrowsable );
      }
    }

    #endregion

    #region IsReadOnly Property

    public override bool IsReadOnly
    {
      get
      {
        return m_column.ReadOnly;
      }
    }

    #endregion

    #region PropertyType Property

    public override Type PropertyType
    {
      get
      {
        return ItemsSourceHelper.GetColumnDataType( m_column );
      }
    }

    #endregion

    public override bool CanResetValue( object component )
    {
      var value = this.GetValue( component );

      return ( value != null )
          && ( value != DBNull.Value );
    }

    public override object GetValue( object component )
    {
      var dataRow = component as System.Data.DataRow;
      if( dataRow == null )
        return null;

      var value = default( object );

      try
      {
        value = dataRow[ m_column ];
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
      var dataRow = component as System.Data.DataRow;
      if( dataRow == null )
        throw new InvalidOperationException( "An attempt was made to set a value on a DataRow that does not exist." );

      if( this.IsReadOnly )
        throw new InvalidOperationException( "An attempt was made to set a value on a read-only field." );

      var oldValue = dataRow[ m_column ];

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
        dataRow[ m_column ] = DBNull.Value;
      }
      else
      {
        dataRow[ m_column ] = value;
      }

      this.OnValueChanged( component, EventArgs.Empty );
    }

    public override bool ShouldSerializeValue( object component )
    {
      return false;
    }

    private readonly DataColumn m_column;
  }
}
