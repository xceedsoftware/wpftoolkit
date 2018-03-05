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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ValueChangeDataItemPropertyDescriptor : DataItemPropertyDescriptor
  {
    internal ValueChangeDataItemPropertyDescriptor(
      DataItemTypeDescriptor owner,
      PropertyDescriptor parent,
      Type componentType,
      Type propertyType,
      Func<object, object> getter,
      Action<object, object> setter,
      Action<object> resetter,
      EventDescriptor eventDescriptor )
      : base( owner, parent, componentType, propertyType, getter, setter, resetter )
    {
      if( eventDescriptor == null )
        throw new ArgumentNullException( "eventDescriptor" );

      m_eventDescriptor = eventDescriptor;
      m_eventProxy = new EventProxy( this, eventDescriptor );
    }

    #region SupportsChangeEvents Property

    public override bool SupportsChangeEvents
    {
      get
      {
        return true;
      }
    }

    #endregion

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      var descriptor = obj as ValueChangeDataItemPropertyDescriptor;
      if( object.ReferenceEquals( descriptor, null ) )
        return false;

      return ( object.Equals( descriptor.m_eventDescriptor, m_eventDescriptor ) )
          && ( base.Equals( descriptor ) );
    }

    protected override void AddValueChangedCore( object component )
    {
      if( component == null )
        return;

      m_eventProxy.AddValueChanged( component );
    }

    protected override void RemoveValueChangedCore( object component )
    {
      if( component == null )
        return;

      m_eventProxy.RemoveValueChanged( component );
    }

    protected override bool MustInhibitValueChanged( object component )
    {
      return ( ( m_inhibitValueChanged == null ) || !m_inhibitValueChanged.Contains( component ) )
          && ( this.GetValueChangedHandler( component ) != null );
    }

    protected override bool IsValueChangedInhibited( object component )
    {
      return ( m_inhibitValueChanged != null )
          && ( m_inhibitValueChanged.Contains( component ) );
    }

    protected override void InhibitValueChanged( object component )
    {
      if( m_inhibitValueChanged == null )
      {
        m_inhibitValueChanged = new HashSet<object>();
      }

      m_inhibitValueChanged.Add( component );
    }

    protected override void ResetInhibitValueChanged( object component )
    {
      Debug.Assert( m_inhibitValueChanged != null );

      m_inhibitValueChanged.Remove( component );

      if( m_inhibitValueChanged.Count == 0 )
      {
        m_inhibitValueChanged = null;
      }
    }

    private void OnPropertyChanged( object sender, EventArgs e )
    {
      var propertyChangedEventArgs = e as PropertyChangedEventArgs;
      if( propertyChangedEventArgs != null )
      {
        var propertyName = propertyChangedEventArgs.PropertyName;

        if( !string.IsNullOrEmpty( propertyName ) && ( string.Compare( propertyName, this.Name, false, CultureInfo.InvariantCulture ) != 0 ) )
          return;
      }

      this.OnValueChanged( sender, e );
    }

    #region Private Fields

    private readonly EventProxy m_eventProxy;
    private readonly EventDescriptor m_eventDescriptor;
    private HashSet<object> m_inhibitValueChanged;

    #endregion

    #region EventProxy Private Class

    private sealed class EventProxy
    {
      internal EventProxy( ValueChangeDataItemPropertyDescriptor propertyDescriptor, EventDescriptor eventDescriptor )
      {
        m_propertyDescriptor = new WeakReference( propertyDescriptor );
        m_eventDescriptor = new WeakReference( eventDescriptor );
      }

      private ValueChangeDataItemPropertyDescriptor PropertyDescriptor
      {
        get
        {
          return m_propertyDescriptor.Target as ValueChangeDataItemPropertyDescriptor;
        }
      }

      private EventDescriptor EventDescriptor
      {
        get
        {
          return m_eventDescriptor.Target as EventDescriptor;
        }
      }

      internal void AddValueChanged( object component )
      {
        var descriptor = this.EventDescriptor;
        if( descriptor == null )
          return;

        var handlerType = descriptor.EventType;

        if( typeof( PropertyChangedEventHandler ).IsAssignableFrom( handlerType ) )
        {
          descriptor.AddEventHandler( component, new PropertyChangedEventHandler( this.OnPropertyChanged ) );
        }
        else if( typeof( EventHandler ).IsAssignableFrom( handlerType ) )
        {
          descriptor.AddEventHandler( component, new EventHandler( this.OnPropertyChanged ) );
        }
      }

      internal void RemoveValueChanged( object component )
      {
        var descriptor = this.EventDescriptor;
        if( descriptor == null )
          return;

        var handlerType = descriptor.EventType;

        if( typeof( PropertyChangedEventHandler ).IsAssignableFrom( handlerType ) )
        {
          descriptor.RemoveEventHandler( component, new PropertyChangedEventHandler( this.OnPropertyChanged ) );
        }
        else if( typeof( EventHandler ).IsAssignableFrom( handlerType ) )
        {
          descriptor.RemoveEventHandler( component, new EventHandler( this.OnPropertyChanged ) );
        }
      }

      private void HandlePropertyChanged( object sender, EventArgs e )
      {
        var propertyDescriptor = this.PropertyDescriptor;

        if( propertyDescriptor != null )
        {
          propertyDescriptor.OnPropertyChanged( sender, e );
        }
        else
        {
          // Since the real listener is no longer available, try to unregister the handler.
          this.RemoveValueChanged( sender );
        }
      }

      private void OnPropertyChanged( object sender, EventArgs e )
      {
        this.HandlePropertyChanged( sender, e );
      }

      private void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
      {
        this.HandlePropertyChanged( sender, e );
      }

      private readonly WeakReference m_propertyDescriptor;
      private readonly WeakReference m_eventDescriptor;
    }

    #endregion
  }
}
