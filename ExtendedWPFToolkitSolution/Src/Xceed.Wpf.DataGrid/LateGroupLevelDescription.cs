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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class LateGroupLevelDescription : IGroupLevelDescription, INotifyPropertyChanged, IWeakEventListener
  {
    internal LateGroupLevelDescription( GroupDescription groupDescription, IEnumerable<GroupLevelDescription> groupLevelDescriptions )
    {
      if( groupDescription == null )
        throw new ArgumentNullException( "groupDescription" );

      var finder = new GroupLevelDescriptionFinder( groupDescription, groupLevelDescriptions );

      m_groupDescription = groupDescription;
      m_groupLevelDescription = finder.GroupLevelDescription;

      if( m_groupLevelDescription == null )
      {
        m_finder = finder;
        m_finder.PropertyChanged += new PropertyChangedEventHandler( this.OnGroupLevelDescriptionFound );
      }
      else
      {
        finder.Dispose();

        PropertyChangedEventManager.AddListener( m_groupLevelDescription, this, string.Empty );
      }
    }

    #region GroupDescription Property

    public GroupDescription GroupDescription
    {
      get
      {
        return m_groupDescription;
      }
    }

    private readonly GroupDescription m_groupDescription;

    #endregion

    #region FieldName Property

    public string FieldName
    {
      get
      {
        GroupLevelDescription description;
        if( !this.TryGetGroupLevelDescription( out description ) )
          return null;

        return description.FieldName;
      }
    }

    #endregion

    #region Title Property

    public object Title
    {
      get
      {
        GroupLevelDescription description;
        if( !this.TryGetGroupLevelDescription( out description ) )
          return null;

        return description.Title;
      }
    }

    #endregion

    #region TitleTemplate Property

    public DataTemplate TitleTemplate
    {
      get
      {
        GroupLevelDescription description;
        if( !this.TryGetGroupLevelDescription( out description ) )
          return null;

        return description.TitleTemplate;
      }
    }

    #endregion

    #region TitleTemplateSelector Property

    public DataTemplateSelector TitleTemplateSelector
    {
      get
      {
        GroupLevelDescription description;
        if( !this.TryGetGroupLevelDescription( out description ) )
          return null;

        return description.TitleTemplateSelector;
      }
    }

    #endregion

    #region ValueStringFormat Property

    public string ValueStringFormat
    {
      get
      {
        GroupLevelDescription description;
        if( !this.TryGetGroupLevelDescription( out description ) )
          return null;

        return description.ValueStringFormat;
      }
    }

    #endregion

    #region ValueStringFormatCulture Property

    public CultureInfo ValueStringFormatCulture
    {
      get
      {
        GroupLevelDescription description;
        if( !this.TryGetGroupLevelDescription( out description ) )
          return null;

        return description.ValueStringFormatCulture;
      }
    }

    #endregion

    #region ValueTemplate Property

    public DataTemplate ValueTemplate
    {
      get
      {
        GroupLevelDescription description;
        if( !this.TryGetGroupLevelDescription( out description ) )
          return null;

        return description.ValueTemplate;
      }
    }

    #endregion

    #region ValueTemplateSelector Property

    public DataTemplateSelector ValueTemplateSelector
    {
      get
      {
        GroupLevelDescription description;
        if( !this.TryGetGroupLevelDescription( out description ) )
          return null;

        return description.ValueTemplateSelector;
      }
    }

    #endregion

    internal void Clear()
    {
      this.ClearLateBinding();

      this.PropertyChanged = null;
    }

    private void ClearLateBinding()
    {
      if( m_finder != null )
      {
        m_finder.PropertyChanged -= new PropertyChangedEventHandler( this.OnGroupLevelDescriptionFound );
        m_finder.Dispose();
        m_finder = null;
      }

      if( m_groupLevelDescription != null )
      {
        PropertyChangedEventManager.RemoveListener( m_groupLevelDescription, this, string.Empty );
      }
    }

    private bool TryGetGroupLevelDescription( out GroupLevelDescription result )
    {
      result = m_groupLevelDescription;
      if( result != null )
        return true;

      m_raiseEventOnGroupLevelDescriptionFound = true;

      return false;
    }

    private void OnGroupLevelDescriptionFound( object sender, PropertyChangedEventArgs e )
    {
      var finder = ( GroupLevelDescriptionFinder )sender;
      Debug.Assert( finder == m_finder );

      var groupLevelDescription = finder.GroupLevelDescription;
      Debug.Assert( groupLevelDescription != null );

      this.ClearLateBinding();

      m_groupLevelDescription = groupLevelDescription;
      PropertyChangedEventManager.AddListener( m_groupLevelDescription, this, string.Empty );

      if( m_raiseEventOnGroupLevelDescriptionFound )
      {
        this.RaisePropertyChanged();
      }
    }

    private void OnGroupLevelDescriptionPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      Debug.Assert( sender == m_groupLevelDescription );

      this.RaisePropertyChanged( e );
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged()
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( null ) );
    }

    private void RaisePropertyChanged( PropertyChangedEventArgs e )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( typeof( PropertyChangedEventManager ) == managerType )
      {
        this.OnGroupLevelDescriptionPropertyChanged( sender, ( PropertyChangedEventArgs )e );
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private GroupLevelDescription m_groupLevelDescription;
    private GroupLevelDescriptionFinder m_finder;
    private bool m_raiseEventOnGroupLevelDescriptionFound; //false

    private sealed class GroupLevelDescriptionFinder : INotifyPropertyChanged, IWeakEventListener, IDisposable
    {
      internal GroupLevelDescriptionFinder( GroupDescription groupDescription, IEnumerable<GroupLevelDescription> groupLevelDescriptions )
      {
        if( groupDescription == null )
          throw new ArgumentNullException( "groupDescription" );

        if( groupLevelDescriptions == null )
          throw new ArgumentNullException( "groupLevelDescriptions" );

        if( !GroupLevelDescriptionFinder.TryFind( groupLevelDescriptions, groupDescription, out m_groupLevelDescription ) )
        {
          m_groupDescription = groupDescription;
          m_groupLevelDescriptions = groupLevelDescriptions;

          this.RegisterCollectionChanged( ( INotifyCollectionChanged )groupLevelDescriptions );
        }
      }

      #region GroupLevelDescription Property

      internal GroupLevelDescription GroupLevelDescription
      {
        get
        {
          return m_groupLevelDescription;
        }
      }

      #endregion

      public event PropertyChangedEventHandler PropertyChanged;

      private void RaisePropertyChanged( string propertyName )
      {
        var handler = this.PropertyChanged;
        if( handler == null )
          return;

        handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
      }

      private void RegisterCollectionChanged( INotifyCollectionChanged collection )
      {
        if( collection == null )
          return;

        CollectionChangedEventManager.AddListener( collection, this );
      }

      private void UnregisterCollectionChanged( INotifyCollectionChanged collection )
      {
        if( collection == null )
          return;

        CollectionChangedEventManager.RemoveListener( collection, this );
      }

      private static bool TryFind( IEnumerable<GroupLevelDescription> collection, GroupDescription description, out GroupLevelDescription result )
      {
        result = default( GroupLevelDescription );

        if( ( collection != null ) && ( description != null ) )
        {
          foreach( var item in collection )
          {
            if( ( item == null ) || ( item.GroupDescription != description ) )
              continue;

            result = item;
            break;
          }
        }

        return ( result != null );
      }

      bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
      {
        if( typeof( CollectionChangedEventManager ) == managerType )
        {
          Debug.Assert( sender == m_groupLevelDescriptions );

          switch( ( ( NotifyCollectionChangedEventArgs )e ).Action )
          {
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Remove:
              break;

            default:
              if( GroupLevelDescriptionFinder.TryFind( m_groupLevelDescriptions, m_groupDescription, out m_groupLevelDescription ) )
              {
                this.RaisePropertyChanged( "GroupLevelDescription" );
                this.Dispose();
              }
              break;
          }

          return true;
        }

        return false;
      }

      public void Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        this.PropertyChanged = null;

        this.UnregisterCollectionChanged( ( INotifyCollectionChanged )m_groupLevelDescriptions );

        m_groupLevelDescriptions = null;
        m_groupDescription = null;
      }

      ~GroupLevelDescriptionFinder()
      {
        this.Dispose( false );
      }

      private GroupDescription m_groupDescription; //null
      private GroupLevelDescription m_groupLevelDescription; //null
      private IEnumerable<GroupLevelDescription> m_groupLevelDescriptions; //null
    }
  }
}
