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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class DataItemPropertyDescriptorBase : PropertyDescriptor
  {
    #region Static Fields

    private static readonly object NotInitialized = new object();
    private static readonly object NotSet = new object();

    #endregion

    protected DataItemPropertyDescriptorBase(
      DataItemTypeDescriptor owner,
      PropertyDescriptor parent,
      Func<object, object> getter,
      Action<object, object> setter,
      Action<object> resetter )
      : base( parent )
    {
      if( owner == null )
        throw new ArgumentNullException( "owner" );

      if( getter == null )
        throw new ArgumentNullException( "getter" );

      m_owner = owner;
      m_getter = getter;
      m_setter = setter;
      m_resetter = resetter;
    }

    #region IsReadOnly Property

    public sealed override bool IsReadOnly
    {
      get
      {
        if( m_setter == null )
          return true;

        var attribute = ( ReadOnlyAttribute )this.Attributes[ typeof( ReadOnlyAttribute ) ];

        return ( attribute != null )
            && ( attribute.IsReadOnly );
      }
    }

    #endregion

    #region Owner Protected Property

    protected DataItemTypeDescriptor Owner
    {
      get
      {
        return m_owner;
      }
    }

    private readonly DataItemTypeDescriptor m_owner;

    #endregion

    #region IsValueChangeEventEnabled Private Property

    private bool IsValueChangeEventEnabled
    {
      get
      {
        return ( this.SupportsChangeEvents )
            || ( !this.IsReadOnly );
      }
    }

    #endregion

    #region DefaultValue Private Property

    private object DefaultValue
    {
      get
      {
        if( m_defaultValue == DataItemPropertyDescriptorBase.NotInitialized )
        {
          var attribute = ( DefaultValueAttribute )this.Attributes[ typeof( DefaultValueAttribute ) ];
          if( attribute != null )
          {
            var propertyType = this.PropertyType;
            var value = attribute.Value;

            if( ( value != null ) && ( propertyType.IsEnum ) && ( Enum.GetUnderlyingType( propertyType ) == value.GetType() ) )
            {
              value = Enum.ToObject( propertyType, value );
            }

            m_defaultValue = value;
          }
          else
          {
            m_defaultValue = DataItemPropertyDescriptorBase.NotSet;
          }
        }

        return m_defaultValue;
      }
    }

    private object m_defaultValue = DataItemPropertyDescriptorBase.NotInitialized;

    #endregion

    #region AmbientValue Private Property

    private object AmbientValue
    {
      get
      {
        if( m_ambientValue == DataItemPropertyDescriptorBase.NotInitialized )
        {
          var attribute = ( AmbientValueAttribute )this.Attributes[ typeof( AmbientValueAttribute ) ];
          if( attribute != null )
          {
            m_ambientValue = attribute.Value;
          }
          else
          {
            m_ambientValue = DataItemPropertyDescriptorBase.NotSet;
          }
        }

        return m_ambientValue;
      }
    }

    private object m_ambientValue = DataItemPropertyDescriptorBase.NotInitialized;

    #endregion

    public override bool ShouldSerializeValue( object component )
    {
      if( this.IsReadOnly )
        return this.Attributes.Contains( DesignerSerializationVisibilityAttribute.Content );

      if( this.DefaultValue != DataItemPropertyDescriptorBase.NotSet )
        return !object.Equals( this.GetValue( component ), this.DefaultValue );

      return true;
    }

    public sealed override object GetValue( object component )
    {
      if( component == null )
        return null;

      return m_getter.Invoke( component );
    }

    public sealed override void SetValue( object component, object value )
    {
      if( ( component == null ) || this.IsReadOnly )
        return;

      Debug.Assert( m_setter != null );

      bool inhibit;

      lock( m_syncRoot )
      {
        inhibit = this.MustInhibitValueChanged( component );

        if( inhibit )
        {
          this.InhibitValueChanged( component );
        }
      }

      try
      {
        m_setter.Invoke( component, value );
      }
      finally
      {
        if( inhibit )
        {
          lock( m_syncRoot )
          {
            this.ResetInhibitValueChanged( component );
          }
        }
      }

      this.OnValueChanged( component, EventArgs.Empty );
    }

    public sealed override bool CanResetValue( object component )
    {
      if( this.IsReadOnly )
        return false;

      if( this.DefaultValue != DataItemPropertyDescriptorBase.NotSet )
        return !object.Equals( this.GetValue( component ), this.DefaultValue );

      if( m_resetter != null )
        return true;

      return ( this.AmbientValue != DataItemPropertyDescriptorBase.NotSet )
          && ( this.ShouldSerializeValue( component ) );
    }

    public sealed override void ResetValue( object component )
    {
      if( ( component == null ) || this.IsReadOnly )
        return;

      if( this.DefaultValue != DataItemPropertyDescriptorBase.NotSet )
      {
        this.SetValue( component, this.DefaultValue );
      }
      else if( this.AmbientValue != DataItemPropertyDescriptorBase.NotSet )
      {
        this.SetValue( component, this.AmbientValue );
      }
      else if( m_resetter != null )
      {
        Debug.Assert( this.CanResetValue( component ) );
        m_resetter.Invoke( component );
      }
    }

    public sealed override void AddValueChanged( object component, EventHandler handler )
    {
      // There is no need to set the handler if the ValueChanged event will never be triggered.
      if( !this.IsValueChangeEventEnabled )
        return;

      lock( m_syncRoot )
      {
        // We register only once on the target component.
        if( this.SupportsChangeEvents && ( this.GetValueChangedHandler( component ) == null ) )
        {
          this.AddValueChangedCore( component );
        }

        base.AddValueChanged( component, handler );
      }
    }

    public sealed override void RemoveValueChanged( object component, EventHandler handler )
    {
      // There is no need to remove the handler if it was never set in the first place.
      if( !this.IsValueChangeEventEnabled )
        return;

      lock( m_syncRoot )
      {
        base.RemoveValueChanged( component, handler );

        // We unregister only when there is no handler left for the target component.
        if( this.SupportsChangeEvents && ( this.GetValueChangedHandler( component ) == null ) )
        {
          this.RemoveValueChangedCore( component );
        }
      }
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      var descriptor = obj as DataItemPropertyDescriptorBase;
      if( object.ReferenceEquals( descriptor, null ) )
        return false;

      return ( base.Equals( descriptor ) )
          && ( descriptor.ComponentType == this.ComponentType )
          && ( descriptor.IsReadOnly == this.IsReadOnly )
          && ( descriptor.SupportsChangeEvents == this.SupportsChangeEvents );
    }

    protected sealed override void OnValueChanged( object component, EventArgs e )
    {
      if( component == null )
        return;

      lock( m_syncRoot )
      {
        if( this.IsValueChangedInhibited( component ) )
          return;
      }

      base.OnValueChanged( component, e );
    }

    protected abstract bool MustInhibitValueChanged( object component );
    protected abstract bool IsValueChangedInhibited( object component );
    protected abstract void InhibitValueChanged( object component );
    protected abstract void ResetInhibitValueChanged( object component );

    protected virtual void AddValueChangedCore( object component )
    {
    }

    protected virtual void RemoveValueChangedCore( object component )
    {
    }

    #region Private Fields

    private readonly object m_syncRoot = new object();
    private readonly Func<object, object> m_getter;
    private readonly Action<object, object> m_setter;
    private readonly Action<object> m_resetter;

    #endregion
  }
}
