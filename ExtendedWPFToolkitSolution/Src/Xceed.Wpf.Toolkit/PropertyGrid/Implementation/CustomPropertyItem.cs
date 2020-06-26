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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  /// <summary>
  /// Used when properties are provided using a list source of items (eg. Properties or PropertiesSource). 
  /// 
  /// An instance of this class can be used as an item to easily customize the 
  /// display of the property directly by modifying the values of this class 
  /// (e.g., DisplayName, value, Category, etc.).
  /// </summary>
  public class CustomPropertyItem : PropertyItemBase
  {
    #region Constructors

    internal CustomPropertyItem() { }

    internal CustomPropertyItem( bool isPropertyGridCategorized, bool isSortedAlphabetically )
    {
      _isPropertyGridCategorized = isPropertyGridCategorized;
      _isSortedAlphabetically = isSortedAlphabetically;
    }


    #endregion

    #region Properties

    #region Category

    public static readonly DependencyProperty CategoryProperty =
        DependencyProperty.Register( "Category", typeof( string ), typeof( CustomPropertyItem ), new UIPropertyMetadata( null ) );

    public string Category
    {
      get { return ( string )GetValue( CategoryProperty ); }
      set { SetValue( CategoryProperty, value ); }
    }

    #endregion //Category

    #region CategoryOrder

    public int CategoryOrder
    {
      get
      {
        return _categoryOrder;
      }
      set
      {
        if( _categoryOrder != value )
        {
          _categoryOrder = value;
          // Notify the parent helper since this property may affect ordering.
          this.RaisePropertyChanged( () => this.CategoryOrder );
        }
      }
    }

    private int _categoryOrder;

    #endregion //CategoryOrder





    #region PropertyOrder

    public static readonly DependencyProperty PropertyOrderProperty =
        DependencyProperty.Register( "PropertyOrder", typeof( int ), typeof( CustomPropertyItem ), new UIPropertyMetadata( 0 ) );

    public int PropertyOrder
    {
      get
      {
        return ( int )GetValue( PropertyOrderProperty );
      }
      set
      {
        SetValue( PropertyOrderProperty, value );
      }
    }

    #endregion //PropertyOrder

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( object ), typeof( CustomPropertyItem ), new UIPropertyMetadata( null, OnValueChanged, OnCoerceValueChanged ) );
    public object Value
    {
      get
      {
        return ( object )GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private static object OnCoerceValueChanged( DependencyObject o, object baseValue )
    {
      CustomPropertyItem prop = o as CustomPropertyItem;
      if( prop != null )
        return prop.OnCoerceValueChanged( baseValue );

      return baseValue;
    }

    protected virtual object OnCoerceValueChanged( object baseValue )
    {
      return baseValue;
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      CustomPropertyItem propertyItem = o as CustomPropertyItem;
      if( propertyItem != null )
      {
        propertyItem.OnValueChanged( ( object )e.OldValue, ( object )e.NewValue );
      }
    }

    protected virtual void OnValueChanged( object oldValue, object newValue )
    {
      if( IsInitialized )
      {
        RaiseEvent( new PropertyValueChangedEventArgs( PropertyGrid.PropertyValueChangedEvent, this, oldValue, newValue ) );
      }
    }

    #endregion //Value

    #endregion

    #region Overrides

    protected override Type GetPropertyItemType()
    {
      return this.Value.GetType();
    }

    protected override void OnEditorChanged( FrameworkElement oldValue, FrameworkElement newValue )
    {
      if( oldValue != null )
      {
        oldValue.DataContext = null;
      }

      //case 166547 : Do not overwrite a custom Editor's DataContext set by the user.
      if( ( newValue != null ) && ( newValue.DataContext == null ) )
      {
        newValue.DataContext = this;
      }
    }

    #endregion
  }
}
