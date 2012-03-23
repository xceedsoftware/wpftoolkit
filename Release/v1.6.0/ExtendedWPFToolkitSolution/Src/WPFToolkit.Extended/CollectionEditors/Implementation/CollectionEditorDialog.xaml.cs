/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  /// <summary>
  /// Interaction logic for CollectionEditorDialog.xaml
  /// </summary>
  public partial class CollectionEditorDialog : Window
  {
    #region Properties

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource", typeof( IList ), typeof( CollectionEditorDialog ), new UIPropertyMetadata( null ) );
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

    public static readonly DependencyProperty ItemsSourceTypeProperty = DependencyProperty.Register( "ItemsSourceType", typeof( Type ), typeof( CollectionEditorDialog ), new UIPropertyMetadata( null ) );
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

    public static readonly DependencyProperty NewItemTypesProperty = DependencyProperty.Register( "NewItemTypes", typeof( IList ), typeof( CollectionEditorDialog ), new UIPropertyMetadata( null ) );
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

    public CollectionEditorDialog()
    {
      InitializeComponent();
    }

    public CollectionEditorDialog( Type type )
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

    #endregion //Event Hanlders
  }
}
