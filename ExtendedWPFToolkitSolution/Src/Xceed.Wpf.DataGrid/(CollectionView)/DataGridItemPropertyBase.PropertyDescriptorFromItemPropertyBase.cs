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

namespace Xceed.Wpf.DataGrid
{
  public abstract partial class DataGridItemPropertyBase
  {
    internal class PropertyDescriptorFromItemPropertyBase : EmptyDataItemSafePropertyDescriptor
    {
      public PropertyDescriptorFromItemPropertyBase( DataGridItemPropertyBase dataGridItemProperty )
        : base( dataGridItemProperty.FieldName )
      {
        m_dataGridItemProperty = new WeakReference( dataGridItemProperty );
        m_propertyType = dataGridItemProperty.DataType;
      }

      public DataGridItemPropertyBase DataGridItemProperty
      {
        get
        {
          return m_dataGridItemProperty.Target as DataGridItemPropertyBase;
        }
      }

      public override string DisplayName
      {
        get
        {
          DataGridItemPropertyBase dataGridItemProperty = this.DataGridItemProperty;

          if( dataGridItemProperty != null )
            return dataGridItemProperty.Title;

          return string.Empty;
        }
      }

      public override bool IsReadOnly
      {
        get
        {
          DataGridItemPropertyBase dataGridItemProperty = this.DataGridItemProperty;

          if( dataGridItemProperty == null )
            return true;

          return ( dataGridItemProperty.OverrideReadOnlyForInsertion ?? false )
            ? false : dataGridItemProperty.IsReadOnly;
        }
      }

      public override Type PropertyType
      {
        get
        {
          // This value need to not be changed during the life time of the PropertyDescriptor
          return m_propertyType;
        }
      }

      public override object GetValue( object component )
      {
        DataGridItemPropertyBase dataGridItemProperty = this.DataGridItemProperty;

        if( dataGridItemProperty == null )
          return null;

        return dataGridItemProperty.GetValue( base.GetValue( component ) );
      }

      public override void SetValue( object component, object value )
      {
        if( component is EmptyDataItem )
          throw new InvalidOperationException( "An attempt was made to set a value on an empty data item." );

        DataGridItemPropertyBase dataGridItemProperty = this.DataGridItemProperty;

        if( dataGridItemProperty == null )
          return;

        dataGridItemProperty.SetValue( component, value );
      }

      public void RaiseValueChanged( object component )
      {
        EventHandler valueChangedHandler = this.GetValueChangedHandler( component );

        if( valueChangedHandler != null )
        {
          valueChangedHandler.Invoke( component, EventArgs.Empty );
        }
      }

      private WeakReference m_dataGridItemProperty;
      private Type m_propertyType;
    }
  }
}
