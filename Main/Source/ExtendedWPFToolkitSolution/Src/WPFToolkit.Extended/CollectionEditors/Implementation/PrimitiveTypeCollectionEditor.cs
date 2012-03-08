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
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls
{
  public class PrimitiveTypeCollectionEditor : ContentControl
  {
    #region Members

    bool _surpressTextChanged;
    bool _conversionFailed;

    #endregion //Members

    #region Properties

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof( bool ), typeof( PrimitiveTypeCollectionEditor ), new UIPropertyMetadata( false, OnIsOpenChanged ) );
    public bool IsOpen
    {
      get
      {
        return ( bool )GetValue( IsOpenProperty );
      }
      set
      {
        SetValue( IsOpenProperty, value );
      }
    }

    private static void OnIsOpenChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PrimitiveTypeCollectionEditor primitiveTypeCollectionEditor = o as PrimitiveTypeCollectionEditor;
      if( primitiveTypeCollectionEditor != null )
        primitiveTypeCollectionEditor.OnIsOpenChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsOpenChanged( bool oldValue, bool newValue )
    {

    }

    #endregion //IsOpen

    #region ItemsSource

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register( "ItemsSource", typeof( IList ), typeof( PrimitiveTypeCollectionEditor ), new UIPropertyMetadata( null, OnItemsSourceChanged ) );
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

    private static void OnItemsSourceChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PrimitiveTypeCollectionEditor primitiveTypeCollectionEditor = o as PrimitiveTypeCollectionEditor;
      if( primitiveTypeCollectionEditor != null )
        primitiveTypeCollectionEditor.OnItemsSourceChanged( ( IList )e.OldValue, ( IList )e.NewValue );
    }

    protected virtual void OnItemsSourceChanged( IList oldValue, IList newValue )
    {
      if( newValue == null )
        return;

      if( ItemsSourceType == null )
        ItemsSourceType = newValue.GetType();

      if( ItemType == null )
        ItemType = newValue.GetType().GetGenericArguments()[ 0 ];

      if( newValue.Count > 0 )
        SetText( newValue );
    }

    #endregion //ItemsSource

    public static readonly DependencyProperty ItemsSourceTypeProperty = DependencyProperty.Register( "ItemsSourceType", typeof( Type ), typeof( PrimitiveTypeCollectionEditor ), new UIPropertyMetadata( null ) );
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

    public static readonly DependencyProperty ItemTypeProperty = DependencyProperty.Register( "ItemType", typeof( Type ), typeof( PrimitiveTypeCollectionEditor ), new UIPropertyMetadata( null ) );
    public Type ItemType
    {
      get
      {
        return ( Type )GetValue( ItemTypeProperty );
      }
      set
      {
        SetValue( ItemTypeProperty, value );
      }
    }

    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( PrimitiveTypeCollectionEditor ), new UIPropertyMetadata( null, OnTextChanged ) );
    public string Text
    {
      get
      {
        return ( string )GetValue( TextProperty );
      }
      set
      {
        SetValue( TextProperty, value );
      }
    }

    private static void OnTextChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      PrimitiveTypeCollectionEditor primitiveTypeCollectionEditor = o as PrimitiveTypeCollectionEditor;
      if( primitiveTypeCollectionEditor != null )
        primitiveTypeCollectionEditor.OnTextChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnTextChanged( string oldValue, string newValue )
    {
      if( !_surpressTextChanged )
        PersistChanges();
    }

    #endregion //Text

    #endregion //Properties

    #region Constructors

    static PrimitiveTypeCollectionEditor()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PrimitiveTypeCollectionEditor ), new FrameworkPropertyMetadata( typeof( PrimitiveTypeCollectionEditor ) ) );
    }

    public PrimitiveTypeCollectionEditor()
    {

    }

    #endregion //Constructors

    #region Methods

    private void PersistChanges()
    {
      IList list = ResolveItemsSource();
      if( list == null )
        return;

      //the easiest way to persist changes to the source is to just clear the source list and then add all items to it.
      list.Clear();

      IList items = ResolveItems();
      foreach( var item in items )
      {
        list.Add( item );
      };

      // if something went wrong during conversion we want to reload the text to show only valid entries
      if( _conversionFailed )
        SetText( list );
    }

    private IList ResolveItems()
    {
      IList items = new List<object>();

      if( ItemType == null )
        return items;

      string[] textArray = Text.Split( '\n' );

      foreach( string s in textArray )
      {
        string valueString = s.TrimEnd( '\r' );
        if( !String.IsNullOrEmpty( valueString ) )
        {
          object value = null;
          try
          {
            value = Convert.ChangeType( valueString, ItemType );
          }
          catch
          {
            //a conversion failed
            _conversionFailed = true;
          }

          if( value != null )
            items.Add( value );
        }
      }

      return items;
    }

    private IList ResolveItemsSource()
    {
      if( ItemsSource == null )
        ItemsSource = CreateItemsSource();

      return ItemsSource;
    }

    private IList CreateItemsSource()
    {
      IList list = null;

      if( ItemsSourceType != null )
      {
        ConstructorInfo constructor = ItemsSourceType.GetConstructor( Type.EmptyTypes );
        list = ( IList )constructor.Invoke( null );
      }

      return list;
    }

    private void SetText( IEnumerable collection )
    {
      _surpressTextChanged = true;
      StringBuilder builder = new StringBuilder();
      foreach( object obj2 in collection )
      {
        builder.Append( obj2.ToString() );
        builder.AppendLine();
      }
      Text = builder.ToString().Trim();
      _surpressTextChanged = false;
    }

    #endregion //Methods
  }
}
