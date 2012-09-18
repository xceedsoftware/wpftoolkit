/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.Core;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  public class VisibilityToBoolConverter : IValueConverter
  {
    #region Inverted Property

    public bool Inverted
    {
      get
      {
        return _inverted;
      }
      set
      {
        _inverted = value;
      }
    }

    private bool _inverted; //false

    #endregion

    #region Not Property

    public bool Not
    {
      get
      {
        return _not;
      }
      set
      {
        _not = value;
      }
    }

    private bool _not; //false

    #endregion

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return this.Inverted ? this.BoolToVisibility( value ) : this.VisibilityToBool( value );
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return this.Inverted ? this.VisibilityToBool( value ) : this.BoolToVisibility( value );
    }

    private object VisibilityToBool( object value )
    {
      if( !( value is Visibility ) )
        throw new InvalidOperationException( ErrorMessages.GetMessage( "SuppliedValueWasNotVisibility" ) );

      return ( ( ( Visibility )value ) == Visibility.Visible ) ^ Not;
    }

    private object BoolToVisibility( object value )
    {
      if( !( value is bool ) )
        throw new InvalidOperationException( ErrorMessages.GetMessage( "SuppliedValueWasNotBool" ) );

      return ( ( bool )value ^ Not ) ? Visibility.Visible : Visibility.Collapsed;
    }
  }
}
