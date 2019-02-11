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
using System.Linq;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ValueChangeDataItemIndexerDescriptor : DataItemIndexerDescriptor
  {
    internal ValueChangeDataItemIndexerDescriptor(
      DataItemTypeDescriptor owner,
      IndexerDescriptor parent,
      EventDescriptor eventDescriptor )
      : base( owner, parent )
    {
      if( eventDescriptor == null )
        throw new ArgumentNullException( "eventDescriptor" );

      m_eventDescriptor = eventDescriptor;
      m_eventProxy = new EventProxy( this, eventDescriptor );

      var indexerName = parent.IndexerName;
      var indexerParameters = parent.IndexerParameters;

      m_propertyChangedNames = new string[]
        {
          Binding.IndexerName,
          string.Format( "{0}[]", indexerName ),
          string.Format( "{0}[{1}]", indexerName, indexerParameters )
        };
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
      var descriptor = obj as ValueChangeDataItemIndexerDescriptor;
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

    private void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      var propertyName = e.PropertyName;

      if( string.IsNullOrEmpty( propertyName ) || m_propertyChangedNames.Contains( propertyName ) )
      {
        this.OnValueChanged( sender, e );
      }
    }

    #region Private Fields

    private readonly string[] m_propertyChangedNames;
    private readonly EventProxy m_eventProxy;
    private readonly EventDescriptor m_eventDescriptor;
    private HashSet<object> m_inhibitValueChanged;

    #endregion

    #region EventProxy Private Class

    private sealed class EventProxy
    {
      internal EventProxy( ValueChangeDataItemIndexerDescriptor propertyDescriptor, EventDescriptor eventDescriptor )
      {
        m_propertyDescriptor = new WeakReference( propertyDescriptor );
        m_eventDescriptor = new WeakReference( eventDescriptor );
      }

      private ValueChangeDataItemIndexerDescriptor PropertyDescriptor
      {
        get
        {
          return m_propertyDescriptor.Target as ValueChangeDataItemIndexerDescriptor;
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
      }

      private void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
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

      private readonly WeakReference m_propertyDescriptor;
      private readonly WeakReference m_eventDescriptor;
    }

    #endregion
  }
}
