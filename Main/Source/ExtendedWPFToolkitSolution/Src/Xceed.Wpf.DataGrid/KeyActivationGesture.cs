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
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public sealed class KeyActivationGesture : ActivationGesture
  {
    #region Key Property

    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
      "Key",
      typeof( Key ),
      typeof( KeyActivationGesture ),
      new FrameworkPropertyMetadata( Key.None ) );

    public Key Key
    {
      get
      {
        return ( Key )this.GetValue( KeyActivationGesture.KeyProperty );
      }
      set
      {
        this.SetValue( KeyActivationGesture.KeyProperty, value );
      }
    }

    #endregion

    #region SystemKey Property

    public static readonly DependencyProperty SystemKeyProperty = DependencyProperty.Register(
      "SystemKey",
      typeof( Key ),
      typeof( KeyActivationGesture ),
      new FrameworkPropertyMetadata( Key.None ) );

    public Key SystemKey
    {
      get
      {
        return ( Key )this.GetValue( KeyActivationGesture.SystemKeyProperty );
      }
      set
      {
        this.SetValue( KeyActivationGesture.SystemKeyProperty, value );
      }
    }

    #endregion

    #region Modifiers Property

    public static readonly DependencyProperty ModifiersProperty = DependencyProperty.Register(
      "Modifiers",
      typeof( ModifierKeys ),
      typeof( KeyActivationGesture ),
      new FrameworkPropertyMetadata( ( ModifierKeys )ModifierKeys.None ) );

    public ModifierKeys Modifiers
    {
      get
      {
        return ( ModifierKeys )this.GetValue( KeyActivationGesture.ModifiersProperty );
      }
      set
      {
        this.SetValue( KeyActivationGesture.ModifiersProperty, value );
      }
    }

    #endregion

    public bool IsActivationKey( Key key, Key systemKey, ModifierKeys modifiers)
    {
      if( key == Key.System )
      {
        return
          ( ( systemKey == this.SystemKey ) &&
            ( modifiers == this.Modifiers ) );
      }
      else
      {
        return
          ( ( key == this.Key ) &&
            ( modifiers == this.Modifiers ) );
      }
    }

    protected override Freezable CreateInstanceCore()
    {
      return new KeyActivationGesture();
    }
  }
}
