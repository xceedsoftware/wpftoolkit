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

using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit
{
  public class TokenItem : ContentControl
  {
    static TokenItem()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( TokenItem ), new FrameworkPropertyMetadata( typeof( TokenItem ) ) );
    }

    public static readonly DependencyProperty TokenKeyProperty = DependencyProperty.Register( "TokenKey", typeof( string ), typeof( TokenItem ), new UIPropertyMetadata( null ) );
    public string TokenKey
    {
      get
      {
        return ( string )GetValue( TokenKeyProperty );
      }
      set
      {
        SetValue( TokenKeyProperty, value );
      }
    }
  }
}
