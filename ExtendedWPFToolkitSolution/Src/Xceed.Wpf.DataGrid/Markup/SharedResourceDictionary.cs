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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Xceed.Wpf.DataGrid.Markup
{
  [Browsable( false )]
  [EditorBrowsable( EditorBrowsableState.Never )]
  public abstract class SharedResourceDictionary : ResourceDictionary
  {
    #region SharedDictionaries Private Static Property

    private static List<KeyValuePair<Uri, WeakResourceDictionary>> SharedDictionaries
    {
      get
      {
        if( s_sharedDictionaries == null )
        {
          Interlocked.CompareExchange<List<KeyValuePair<Uri, WeakResourceDictionary>>>( ref s_sharedDictionaries, new List<KeyValuePair<Uri, WeakResourceDictionary>>(), null );
        }

        return s_sharedDictionaries;
      }
    }

    private static List<KeyValuePair<Uri, WeakResourceDictionary>> s_sharedDictionaries; //null

    #endregion

    #region Source Property

    public new Uri Source
    {
      get
      {
        if( m_sourceUri != null )
          return m_sourceUri;

        return base.Source;
      }
      set
      {
        Uri sourceUri;
        if( !this.TryCreateAbsoluteUri( ( ( IUriContext )this ).BaseUri, value, out sourceUri )
          || ( sourceUri == null )
          || ( !sourceUri.IsAbsoluteUri ) )
        {
          base.Source = value;
        }
        else
        {
          var dictionaries = SharedResourceDictionary.SharedDictionaries;
          lock( ( ( ICollection )dictionaries ).SyncRoot )
          {
            bool cleanUp;
            ResourceDictionary dictionary;

            var index = SharedResourceDictionary.FindIndex( dictionaries, sourceUri, out cleanUp );
            if( index >= 0 )
            {
              var container = dictionaries[ index ].Value;
              if( container.Status == ResourceDictionaryStatus.NotSet )
                throw new InvalidOperationException( "A circular reference has been detected in a ResourceDictionary." );

              dictionary = container.Target;
            }
            else
            {
              dictionary = null;
            }

            m_sourceUri = value;

            if( dictionary != null )
            {
              var mergedDictionaries = this.MergedDictionaries;
              lock( ( ( ICollection )this ).SyncRoot )
              {
                lock( ( ( ICollection )mergedDictionaries ).SyncRoot )
                {
                  mergedDictionaries.Add( dictionary );
                }
              }
            }
            else
            {
              // We must add the entry in the collection before we actually load the ResourceDictionary because
              // the ResourceDictionary may load other ResourceDictionary as well.  Our insertion index may no longer
              // be valid after the ResourceDictionary is loaded.
              var container = new WeakResourceDictionary();
              var newEntry = new KeyValuePair<Uri, WeakResourceDictionary>( sourceUri, container );
              if( index >= 0 )
              {
                dictionaries[ index ] = newEntry;
              }
              else
              {
                dictionaries.Insert( ~index, newEntry );
              }

              bool exceptionOccured = true;

              try
              {
                base.Source = sourceUri;

                exceptionOccured = false;
              }
              finally
              {
                if( exceptionOccured )
                {
                  dictionaries.Remove( newEntry );
                }
                else
                {
                  container.Target = this;
                }
              }
            }

            if( cleanUp )
            {
              for( int i = dictionaries.Count - 1; i >= 0; i-- )
              {
                if( dictionaries[ i ].Value.Status != ResourceDictionaryStatus.GarbageCollected )
                  continue;

                dictionaries.RemoveAt( i );
              }
            }
          }
        }
      }
    }

    private Uri m_sourceUri; //null

    #endregion

    protected virtual bool TryCreateAbsoluteUri( Uri baseUri, Uri sourceUri, out Uri result )
    {
      result = null;

      if( ( sourceUri == null ) || !sourceUri.IsAbsoluteUri )
        return false;

      if( string.Compare( sourceUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase ) != 0 )
      {
        result = sourceUri;
      }
      else if( string.Compare( sourceUri.OriginalString, 0, Uri.UriSchemeFile, 0, Uri.UriSchemeFile.Length, StringComparison.OrdinalIgnoreCase ) == 0 )
      {
        result = sourceUri;
      }
      else
      {
        result = new Uri( sourceUri.AbsoluteUri );
      }

      return true;
    }

    private static int FindIndex( IList<KeyValuePair<Uri, WeakResourceDictionary>> collection, Uri sourceUri, out bool cleanUp )
    {
      cleanUp = false;

      Debug.Assert( collection != null );
      Debug.Assert( sourceUri != null );

      var lowerBound = 0;
      var upperBound = collection.Count - 1;

      while( lowerBound <= upperBound )
      {
        var middle = lowerBound + ( upperBound - lowerBound ) / 2;
        var entry = collection[ middle ];
        var compare = Uri.Compare( entry.Key, sourceUri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase );

        cleanUp = cleanUp || ( entry.Value.Status == ResourceDictionaryStatus.GarbageCollected );

        if( compare < 0 )
        {
          if( middle == lowerBound )
            return ~middle;

          upperBound = middle - 1;
        }
        else if( compare > 0 )
        {
          if( middle == upperBound )
            return ~( middle + 1 );

          lowerBound = middle + 1;
        }
        else
        {
          return middle;
        }
      }

      return ~0;
    }

    #region WeakResourceDictionary Private Class

    private sealed class WeakResourceDictionary
    {
      internal ResourceDictionary Target
      {
        get
        {
          var resource = m_resource;
          if( resource == null )
            return null;

          return ( ResourceDictionary )resource.Target;
        }
        set
        {
          if( value == null )
            throw new ArgumentNullException( "value" );

          if( m_resource != null )
            throw new InvalidOperationException( "The property can only be set once." );

          m_resource = new WeakReference( value );
        }
      }

      internal ResourceDictionaryStatus Status
      {
        get
        {
          var resource = m_resource;
          if( resource == null )
            return ResourceDictionaryStatus.NotSet;

          if( resource.IsAlive )
            return ResourceDictionaryStatus.Alive;

          return ResourceDictionaryStatus.GarbageCollected;
        }
      }

      private WeakReference m_resource; //null
    }

    #endregion

    #region ResourceDictionaryStatus Private Enum

    private enum ResourceDictionaryStatus
    {
      NotSet = 0,
      Alive,
      GarbageCollected
    }

    #endregion
  }
}
