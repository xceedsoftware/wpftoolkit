/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xceed.Wpf.Toolkit.PropertyGrid;
using System.Windows.Controls;
using System.Reflection;

namespace Xceed.Wpf.Toolkit
{
  public partial class CollectionControlDialogBase :
    Window
  {
  }

  /// <summary>
  /// Interaction logic for CollectionControlDialog.xaml
  /// </summary>
  public partial class CollectionControlDialog : CollectionControlDialogBase
  {
    #region Private Members

    private IList originalData = new List<object>();

    #endregion

    #region Properties

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource", typeof( IEnumerable ), typeof( CollectionControlDialog ), new UIPropertyMetadata( null ) );
    public IEnumerable ItemsSource
    {
      get
      {
        return ( IEnumerable )GetValue( ItemsSourceProperty );
      }
      set
      {
        SetValue( ItemsSourceProperty, value );
      }
    }

    public static readonly DependencyProperty ItemsSourceTypeProperty = DependencyProperty.Register( "ItemsSourceType", typeof( Type ), typeof( CollectionControlDialog ), new UIPropertyMetadata( null ) );
    public Type ItemsSourceType
    {
      get
      {
        return ( Type )GetValue( ItemsSourceTypeProperty );
      }
      set
      {
        SetValue( ItemsSourceTypeProperty, value );
      }
    }

    public static readonly DependencyProperty NewItemTypesProperty = DependencyProperty.Register( "NewItemTypes", typeof( IList ), typeof( CollectionControlDialog ), new UIPropertyMetadata( null ) );
    public IList<Type> NewItemTypes
    {
      get
      {
        return ( IList<Type> )GetValue( NewItemTypesProperty );
      }
      set
      {
        SetValue( NewItemTypesProperty, value );
      }
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register( "IsReadOnly", typeof( bool ), typeof( CollectionControlDialog ), new UIPropertyMetadata( false ) );
    public bool IsReadOnly
    {
      get
      {
        return ( bool )GetValue( IsReadOnlyProperty );
      }
      set
      {
        SetValue( IsReadOnlyProperty, value );
      }
    }

    public static readonly DependencyProperty EditorDefinitionsProperty = DependencyProperty.Register( "EditorDefinitions", typeof( EditorDefinitionCollection ), typeof( CollectionControlDialog ), new UIPropertyMetadata( null ) );
    public EditorDefinitionCollection EditorDefinitions
    {
      get
      {
        return ( EditorDefinitionCollection )GetValue( EditorDefinitionsProperty );
      }
      set
      {
        SetValue( EditorDefinitionsProperty, value );
      }
    }

    public CollectionControl CollectionControl
    {
      get
      {
        return _collectionControl;
      }
    }

    #endregion //Properties

    #region Constructors

    public CollectionControlDialog()
    {
      InitializeComponent();
    }

    public CollectionControlDialog( Type itemsourceType )
      : this()
    {
      ItemsSourceType = itemsourceType;
    }

    public CollectionControlDialog( Type itemsourceType, IList<Type> newItemTypes )
      : this( itemsourceType )
    {
      NewItemTypes = newItemTypes;
    }

    #endregion //Constructors

    #region Overrides

    protected override void OnSourceInitialized( EventArgs e )
    {
      base.OnSourceInitialized( e );

      //Backup data if case "Cancel" is clicked.
      if( this.ItemsSource != null )
      {
        foreach( var item in this.ItemsSource )
        {
          originalData.Add( this.Clone( item ) );
        }
      }
    }

    #endregion

    #region Event Handlers

    private void OkButton_Click( object sender, RoutedEventArgs e )
    {
      if( this.ItemsSource is IDictionary )
      {
        if( !this.AreDictionaryKeysValid() )
        {
          MessageBox.Show( "All dictionary items should have distinct non-null Key values.", "Warning" );
          return;
        }
      }

      this.DialogResult = _collectionControl.PersistChanges();
      this.Close();
    }

    private void CancelButton_Click( object sender, RoutedEventArgs e )
    {
      _collectionControl.PersistChanges( originalData );
      this.DialogResult = false;
      this.Close();
    }

    #endregion //Event Hanlders

    #region Private Methods

    [SecuritySafeCritical]
    private object Clone( object source )
    {
      if( source == null )
        return null;

      object result = null;
      var sourceType = source.GetType();

      if( source is Array )
      {
        using( var stream = new MemoryStream() )
        {
          var formatter = new BinaryFormatter();
          formatter.Serialize( stream, source );
          stream.Seek( 0, SeekOrigin.Begin );
          result = ( Array )formatter.Deserialize( stream );
        }
      }
      // For IDictionary, we need to create EditableKeyValuePair to edit the Key-Value.
      else if( ( this.ItemsSource is IDictionary )
        && sourceType.IsGenericType
        && typeof( KeyValuePair<,> ).IsAssignableFrom( sourceType.GetGenericTypeDefinition() ) )
      {
        result = this.GenerateEditableKeyValuePair( source );
      }
      else
      {
        // Initialized a new object with default values
        try
        {
          result = FormatterServices.GetUninitializedObject( sourceType );
        }
        catch( Exception )
        {
        }

        var constructor = sourceType.GetConstructor( Type.EmptyTypes );
        if( constructor != null )
        {
          constructor.Invoke( result, null );
        }
        else
        {
          result = source;
        }
      }
      Debug.Assert( result != null );
      if( result != null )
      {
        var properties = sourceType.GetProperties();

        foreach( var propertyInfo in properties )
        {
          try
          {
            if( propertyInfo.CanWrite )
            {
              var parameters = propertyInfo.GetIndexParameters();
              var isIndexed = ( parameters.GetLength( 0 ) != 0 );
              if( !isIndexed )
              {
                var propertyInfoValue = propertyInfo.GetValue( source, null );
                this.GenerateValue( propertyInfo, propertyInfoValue, result );
              }
              else 
              {
                var countPropertyInfo = sourceType.GetProperty( "Count" );
                if( countPropertyInfo != null )
                {
                  var count = countPropertyInfo.GetValue( source, null ) as int?;
                  if( ( count != null) && count.HasValue )
                  {
                    for( int i = 0; i < count.Value; ++i )
                    {
                      var propertyInfoValue = propertyInfo.GetValue( source, new object[] { i } );
                      this.GenerateValue( propertyInfo, propertyInfoValue, result, true );
                    }
                  }
                }
              }
            }
          }
          catch( Exception )
          {
          }
        }
      }

      return result;
    }

    private void GenerateValue( PropertyInfo propertyInfo, object propertyInfoValue, object result, bool isIndexed = false )
    {
      // Look for nested object
      if( propertyInfo.PropertyType.IsClass
        && ( propertyInfo.PropertyType != typeof( Transform ) )
        && ( propertyInfo.PropertyType != typeof( ControlTemplate ) )
        && !propertyInfo.PropertyType.Equals( typeof( string ) ) )
      {
        // We have a Collection/List of T.
        if( propertyInfo.PropertyType.IsGenericType )
        {
          // Clone sub-objects if the T are non-primitive types objects. 
          var arg = propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
          if( ( arg != null ) && !arg.IsPrimitive && !arg.Equals( typeof( String ) ) && !arg.IsEnum )
          {
            var nestedObject = this.Clone( propertyInfoValue );
            propertyInfo.SetValue( result, nestedObject, null );
          }
          else
          {
            // copy object if the T are primitive types objects.
            propertyInfo.SetValue( result, propertyInfoValue, null );
          }
        }
        else
        {
          var nestedObject = this.Clone( propertyInfoValue );
          if( nestedObject != null )
          {
            // For T object included in List/Collections, Add it to the List/Collection of T.
            if( isIndexed )
            {
              result.GetType().GetMethod( "Add" ).Invoke( result, new[] { nestedObject } );
            }
            else
            {
              propertyInfo.SetValue( result, nestedObject, null );
            }
          }
        }
      }
      else
      {
        // For T object included in List/Collections, Add it to the List/Collection of T.
        if( isIndexed )
        {
          result.GetType().GetMethod( "Add" ).Invoke( result, new[] { propertyInfoValue } );
        }
        else
        {
          // copy regular object
          propertyInfo.SetValue( result, propertyInfoValue, null );
        }
      }
    }

    private object GenerateEditableKeyValuePair( object source )
    {
      var sourceType = source.GetType();
      if( ( sourceType.GetGenericArguments() == null ) || ( sourceType.GetGenericArguments().GetLength( 0 ) != 2 ) )
        return null;

      var propInfoKey = sourceType.GetProperty( "Key" );
      var propInfoValue = sourceType.GetProperty( "Value" );
      if( ( propInfoKey != null ) && ( propInfoValue != null ) )
      {
        return ListUtilities.CreateEditableKeyValuePair( propInfoKey.GetValue( source, null )
                                                          , sourceType.GetGenericArguments()[ 0 ]
                                                          , propInfoValue.GetValue( source, null )
                                                          , sourceType.GetGenericArguments()[ 1 ] );
      }
      return null;
    }

    private bool AreDictionaryKeysValid()
    {
      var keys = _collectionControl.Items.Select( x =>
      {
        var keyType = x.GetType().GetProperty( "Key" );
        if( keyType != null )
        {
          return keyType.GetValue( x, null );
        }
        return null;
      } );

      return ( keys.Distinct().Count() == _collectionControl.Items.Count )
             && keys.All( x => x != null );
    }

    #endregion
  }
}
