/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.ComponentModel;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  [Browsable( false )]
  [EditorBrowsable( EditorBrowsableState.Never )]
  public class BindingProxy : Freezable
  {
    #region Value Property

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
      "Value",
      typeof( object ),
      typeof( BindingProxy ),
      new UIPropertyMetadata( null ) );

    public object Value
    {
      get
      {
        return this.GetValue( BindingProxy.ValueProperty );
      }
      set
      {
        this.SetValue( BindingProxy.ValueProperty, value );
      }
    }

    #endregion

    protected override Freezable CreateInstanceCore()
    {
      return new BindingProxy();
    }

    protected sealed override bool FreezeCore( bool isChecking )
    {
      // Only derived from Freezable to have DataContext and ElementName binding.
      // So we don't want to be freezable.
      return false;
    }
  }
}
