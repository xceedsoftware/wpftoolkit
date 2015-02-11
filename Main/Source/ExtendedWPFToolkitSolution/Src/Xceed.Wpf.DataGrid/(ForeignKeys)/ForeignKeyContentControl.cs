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
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal class ForeignKeyContentControl : ContentControl
  {
    private static readonly Binding BindingToValue;

    static ForeignKeyContentControl()
    {
      ForeignKeyContentControl.BindingToValue = new Binding();
      ForeignKeyContentControl.BindingToValue.Mode = BindingMode.OneWay;
      ForeignKeyContentControl.BindingToValue.Path = new PropertyPath( ForeignKeyContentControl.ValueProperty );
      ForeignKeyContentControl.BindingToValue.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      ForeignKeyContentControl.FocusableProperty.OverrideMetadata( typeof( ForeignKeyContentControl ), new FrameworkPropertyMetadata( false ) );
    }

    public ForeignKeyContentControl()
    {
      // Bind ContentProperty to the Value property.
      this.SetBinding( ForeignKeyContentControl.ContentProperty, ForeignKeyContentControl.BindingToValue );

      // Ensure this control is not Focusable, it only displays converted value between
      // ID and ForeignKey
      this.Focusable = false;
    }

    #region ForeignKeyConfiguration Property

    public static readonly DependencyProperty ForeignKeyConfigurationProperty = DependencyProperty.Register(
      "ForeignKeyConfiguration",
      typeof( ForeignKeyConfiguration ),
      typeof( ForeignKeyContentControl ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( ForeignKeyContentControl.OnForeignKeyConfigurationChanged ) ) );

    public ForeignKeyConfiguration ForeignKeyConfiguration
    {
      get
      {
        return ( ForeignKeyConfiguration )this.GetValue( ForeignKeyContentControl.ForeignKeyConfigurationProperty );
      }
      set
      {
        this.SetValue( ForeignKeyContentControl.ForeignKeyConfigurationProperty, value );
      }
    }

    private static void OnForeignKeyConfigurationChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyContentControl contentControl = sender as ForeignKeyContentControl;

      if( contentControl != null )
      {
        contentControl.NotifyKeyChanged();
      }
    }

    #endregion

    #region Key Property

    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
      "Key",
      typeof( object ),
      typeof( ForeignKeyContentControl ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( ForeignKeyContentControl.OnKeyChanged ) ) );

    public object Key
    {
      get
      {
        return ( object )this.GetValue( ForeignKeyContentControl.KeyProperty );
      }
      set
      {
        this.SetValue( ForeignKeyContentControl.KeyProperty, value );
      }
    }

    private static void OnKeyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyContentControl contentControl = o as ForeignKeyContentControl;

      if( contentControl != null )
      {
        contentControl.NotifyKeyChanged();
      }
    }

    #endregion

    #region Value Property

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
      "Value",
      typeof( object ),
      typeof( ForeignKeyContentControl ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( OnValueChanged ) ) );

    public object Value
    {
      get
      {
        return ( object )this.GetValue( ForeignKeyContentControl.ValueProperty );
      }
      set
      {
        this.SetValue( ForeignKeyContentControl.ValueProperty, value );
      }
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyContentControl contentControl = o as ForeignKeyContentControl;

      if( contentControl != null )
      {
        contentControl.NotifyValueChanged();
      }
    }

    #endregion

    protected void NotifyKeyChanged()
    {
      if( m_isBeingModified )
        return;

      try
      {
        m_isBeingModified = true;

        this.OnKeyChanged();
      }
      finally
      {
        m_isBeingModified = false;
      }
    }

    protected void NotifyValueChanged()
    {
      if( m_isBeingModified )
        return;

      try
      {
        m_isBeingModified = true;

        this.OnValueChanged();
      }
      finally
      {
        m_isBeingModified = false;
      }
    }

    private void OnKeyChanged()
    {
      ForeignKeyConfiguration configuration = this.ForeignKeyConfiguration;

      if( ( configuration != null ) && ( configuration.ForeignKeyConverter != null ) )
      {
        this.Value = configuration.ForeignKeyConverter.GetValueFromKey( this.Key, configuration );
      }
      else
      {
        this.Value = this.Key;
      }
    }

    private void OnValueChanged()
    {
      ForeignKeyConfiguration configuration = this.ForeignKeyConfiguration;

      if( ( configuration != null ) && ( configuration.ForeignKeyConverter != null ) )
      {
        this.Key = configuration.ForeignKeyConverter.GetKeyFromValue( this.Value, configuration );
      }
      else
      {
        this.Key = this.Value;
      }
    }

    private bool m_isBeingModified; // = false;
  }
}
