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
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  /// <summary>
  /// Interaction logic for CollectionControlDialog.xaml
  /// </summary>
  public partial class CollectionControlDialog : Window
  {
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

    #endregion //Properties

    #region Constructors

    public CollectionControlDialog()
    {
      InitializeComponent();
    }

    public CollectionControlDialog( Type type )
      : this()
    {
      ItemsSourceType = type;
    }

    #endregion //Constructors

    #region Event Handlers

    private void OkButton_Click( object sender, RoutedEventArgs e )
    {
      _propertyGrid.PersistChanges();
      Close();
    }

    private void CancelButton_Click( object sender, RoutedEventArgs e )
    {
      Close();
    }

    #endregion //Event Hanlders
  }
}
