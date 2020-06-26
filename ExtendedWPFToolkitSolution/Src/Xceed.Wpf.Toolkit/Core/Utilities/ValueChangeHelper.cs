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
using System.Collections;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  /// <summary>
  /// This helper class will raise events when a specific
  /// path value on one or many items changes.
  /// </summary>
  internal class ValueChangeHelper : DependencyObject
  {

    #region Value Property
    /// <summary>
    /// This private property serves as the target of a binding that monitors the value of the binding
    /// of each item in the source.
    /// </summary>
    private static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( object ), typeof( ValueChangeHelper ), new UIPropertyMetadata( null, OnValueChanged ) );
    private object Value
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

    private static void OnValueChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      ( ( ValueChangeHelper )sender ).RaiseValueChanged();
    }
    #endregion

    public event EventHandler ValueChanged;

    #region Constructor

    public ValueChangeHelper(Action changeCallback)
    {
      if( changeCallback == null )
        throw new ArgumentNullException( "changeCallback" );

      this.ValueChanged += ( s, args ) => changeCallback();
    }

    #endregion

    #region Methods

    public void UpdateValueSource( object sourceItem, string path )
    {
      BindingBase binding = null;
      if( sourceItem != null && path != null )
      {
        binding = new Binding( path ) { Source = sourceItem };
      }

      this.UpdateBinding( binding );
    }

    public void UpdateValueSource( IEnumerable sourceItems, string path )
    {
      BindingBase binding = null;
      if( sourceItems != null && path != null )
      {
        MultiBinding multiBinding = new MultiBinding();
        multiBinding.Converter = new BlankMultiValueConverter();

        foreach( var item in sourceItems )
        {
          multiBinding.Bindings.Add( new Binding( path ) { Source = item } );
        }

        binding = multiBinding;
      }

      this.UpdateBinding( binding );
    }

    private void UpdateBinding( BindingBase binding )
    {
      if( binding != null )
      {
        BindingOperations.SetBinding( this, ValueChangeHelper.ValueProperty, binding );
      }
      else
      {
        this.ClearBinding();
      }
    }

    private void ClearBinding()
    {
      BindingOperations.ClearBinding( this, ValueChangeHelper.ValueProperty );
    }

    private void RaiseValueChanged()
    {
      if( this.ValueChanged != null )
      {
        this.ValueChanged( this, EventArgs.Empty );
      }
    }

    #endregion

    #region BlankMultiValueConverter private class

    private class BlankMultiValueConverter : IMultiValueConverter
    {
      public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
      {
        // We will not use the result anyway. We just want the change notification to kick in.
        // Return a new object to have a different value.
        return new object();
      }

      public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
      {
        throw new InvalidOperationException();
      }
    }

    #endregion
  }
}
