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
using System.ComponentModel;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  [DebuggerDisplay( "Name = {Name}" )]
  public class DataGridUnboundItemProperty : DataGridItemPropertyBase
  {
    public DataGridUnboundItemProperty()
    {
    }

    public DataGridUnboundItemProperty( string name, Type dataType )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      this.Initialize( name, null, dataType, null, null, null, null );
    }

    public DataGridUnboundItemProperty( string name, Type dataType, bool isReadOnly )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      this.Initialize( name, null, dataType, isReadOnly, null, null, null );
    }

    public DataGridUnboundItemProperty( string name, Type dataType, bool isReadOnly, bool overrideReadOnlyForInsertion )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      this.Initialize( name, null, dataType, isReadOnly, overrideReadOnlyForInsertion, null, null );
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "This constructor is obsolete and should no longer be used.", true )]
    protected DataGridUnboundItemProperty( DataGridUnboundItemProperty template )
    {
      throw new NotSupportedException();
    }

    public event EventHandler<DataGridItemPropertyQueryValueEventArgs> QueryValue;
    public event EventHandler<DataGridItemPropertyCommittingValueEventArgs> CommittingValue;

    protected override object GetValueCore( object component )
    {
      var handler = this.QueryValue;
      if( handler == null )
        return null;

      var e = new DataGridItemPropertyQueryValueEventArgs( component );

      handler.Invoke( this, e );

      return e.Value;
    }

    protected override void SetValueCore( object component, object value )
    {
      var handler = this.CommittingValue;
      if( handler != null )
      {
        handler.Invoke( this, new DataGridItemPropertyCommittingValueEventArgs( component, value ) );
      }

      base.SetValueCore( component, value );
    }

    internal void Refresh( object component )
    {
      var propertyDescriptor = this.GetPropertyDescriptorForBinding();
      if( propertyDescriptor == null )
        return;

      propertyDescriptor.RaiseValueChanged( component );
    }

    internal override void SetUnspecifiedPropertiesValues(
      PropertyDescription description,
      Type itemType,
      bool defaultItemPropertiesCreated )
    {
      if( this.DataType == null )
        throw new InvalidOperationException( "An attempt was made to add an item without specifying its data type." );

      if( string.IsNullOrEmpty( this.Title ) )
      {
        this.Title = this.Name;
      }

      if( !this.OverrideReadOnlyForInsertion.HasValue )
      {
        this.OverrideReadOnlyForInsertion = false;
      }
    }
  }
}
