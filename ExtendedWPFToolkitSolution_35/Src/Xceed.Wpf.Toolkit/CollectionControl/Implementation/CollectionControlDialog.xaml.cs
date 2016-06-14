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
using System.Windows;
using System.Windows.Media;

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

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource", typeof( IList ), typeof( CollectionControlDialog ), new UIPropertyMetadata( null ) );
    public IList ItemsSource
    {
      get
      {
        return ( IList )GetValue( ItemsSourceProperty );
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

      //Backup data in case "Cancel" is clicked.
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
      _collectionControl.PersistChanges();
      this.DialogResult = true;
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

    private object Clone( object source )
    {
      var sourceType = source.GetType();
      //Get default constructor
      var result = sourceType.GetConstructor( new Type[] { } ).Invoke( null );
      var properties = sourceType.GetProperties();

      foreach( var propertyInfo in properties )
      {
        var propertyInfoValue = propertyInfo.GetValue( source, null );

        if( propertyInfo.CanWrite )
        {
          //Look for nested object
          if( propertyInfo.PropertyType.IsClass && (propertyInfo.PropertyType != typeof(Transform)) && !propertyInfo.PropertyType.Equals( typeof( string ) ) )
          {
            var nestedObject = this.Clone( propertyInfoValue );
            propertyInfo.SetValue( result, nestedObject, null );
          }
          else
          {
            // copy object
            propertyInfo.SetValue( result, propertyInfoValue, null );
          }
        }
      }  

      return result;
    }

#endregion
  }
}
