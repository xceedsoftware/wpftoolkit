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
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit
{
  public class CollectionControlButton : Button
  {
    #region Constructors

    static CollectionControlButton()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( CollectionControlButton ), new FrameworkPropertyMetadata( typeof( CollectionControlButton ) ) );
    }

    public CollectionControlButton()
    {
      this.Click += this.CollectionControlButton_Click;
    }

    #endregion //Constructors

    #region Properties

    #region IsReadOnly Property

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register( "IsReadOnly", typeof( bool ), typeof( CollectionControlButton ), new UIPropertyMetadata( false ) );
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

    #endregion  //IsReadOnly

    #region ItemsSource Property

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource", typeof( IEnumerable ), typeof( CollectionControlButton ), new UIPropertyMetadata( null ) );
    public IEnumerable ItemsSource
    {
      get
      {
        return (IEnumerable)GetValue( ItemsSourceProperty );
      }
      set
      {
        SetValue( ItemsSourceProperty, value );
      }
    }

    #endregion  //ItemsSource

    #region ItemsSourceType Property

    public static readonly DependencyProperty ItemsSourceTypeProperty = DependencyProperty.Register( "ItemsSourceType", typeof( Type ), typeof( CollectionControlButton ), new UIPropertyMetadata( null ) );
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

    #endregion //ItemsSourceType

    #region NewItemTypes Property

    public static readonly DependencyProperty NewItemTypesProperty = DependencyProperty.Register( "NewItemTypes", typeof( IList ), typeof( CollectionControlButton ), new UIPropertyMetadata( null ) );
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

    #endregion  //NewItemTypes

    #endregion

    #region Methods

    private void CollectionControlButton_Click( object sender, RoutedEventArgs e )
    {
      var collectionControlDialog = new CollectionControlDialog();
      collectionControlDialog.ItemsSource = this.ItemsSource;
      collectionControlDialog.NewItemTypes = this.NewItemTypes;
      collectionControlDialog.ItemsSourceType = this.ItemsSourceType;
      collectionControlDialog.IsReadOnly = this.IsReadOnly;
      collectionControlDialog.ShowDialog();
    }

    #endregion
  }
}
