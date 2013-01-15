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
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridUnboundItemProperty : DataGridItemPropertyBase
  {
    #region CONSTRUTORS

    public DataGridUnboundItemProperty()
    {
    }

    public DataGridUnboundItemProperty( string name, Type dataType )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      this.Initialize( name, null, dataType, null, null, null );
    }

    public DataGridUnboundItemProperty( string name, Type dataType, bool isReadOnly )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      this.Initialize( name, null, dataType, isReadOnly, null, null );
    }

    public DataGridUnboundItemProperty( string name, Type dataType, bool isReadOnly, bool overrideReadOnlyForInsertion )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      this.Initialize( name, null, dataType, isReadOnly, overrideReadOnlyForInsertion, null );
    }

    protected DataGridUnboundItemProperty( DataGridUnboundItemProperty template )
      : base( template )
    {
      this.QueryValue += template.QueryValue;
      this.CommittingValue += template.CommittingValue;
    }

    #endregion CONSTRUTORS

    #region PUBLIC METHODS

    public override object Clone()
    {
      Type type = this.GetType();

      if( type == typeof( DataGridUnboundItemProperty ) )
        return new DataGridUnboundItemProperty( this );

      return base.Clone();
    }

    #endregion PUBLIC METHODS

    #region PUBLIC EVENTS

    public event EventHandler<DataGridItemPropertyQueryValueEventArgs> QueryValue;
    public event EventHandler<DataGridItemPropertyCommittingValueEventArgs> CommittingValue;

    #endregion PUBLIC EVENTS

    #region PROTECTED METHODS

    protected override object GetValueCore( object component )
    {
      if( this.QueryValue != null )
      {
        DataGridItemPropertyQueryValueEventArgs gettingValueEventArgs = new DataGridItemPropertyQueryValueEventArgs( component );
        this.QueryValue( this, gettingValueEventArgs );
        return gettingValueEventArgs.Value;
      }

      return null;
    }

    protected override void SetValueCore( object component, object value )
    {
      if( this.CommittingValue != null )
      {
        this.CommittingValue( this, new DataGridItemPropertyCommittingValueEventArgs( component, value ) );
      }

      base.SetValueCore( component, value );
    }

    #endregion PROTECTED METHODS

    #region INTERNAL METHODS

    internal void Refresh( object component )
    {
      PropertyDescriptorFromItemPropertyBase propertyDescriptor = this.GetPropertyDescriptorForBinding();

      if( propertyDescriptor == null )
        return;

      propertyDescriptor.RaiseValueChanged( component );
    }

    internal override void SetUnspecifiedPropertiesValues( DataGridItemPropertyCollection itemPropertyCollection )
    {
      if( this.DataType == null )
      {
        throw new InvalidOperationException( "An attempt was made to add an item without specifying its data type." );
      }

      if( string.IsNullOrEmpty( this.Title ) )
      {
        this.Title = this.Name;
      }

      if( !this.OverrideReadOnlyForInsertion.HasValue )
      {
        this.OverrideReadOnlyForInsertion = false;
      }
    }

    #endregion INTERNAL METHODS
  }
}
