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
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class TrimmedTextBlock : TextBlock
  {
    #region Constructor

    public TrimmedTextBlock()
    {
      this.SizeChanged += this.TrimmedTextBlock_SizeChanged;
    }

    #endregion

    #region IsTextTrimmed Property

    public static readonly DependencyProperty IsTextTrimmedProperty = DependencyProperty.Register( "IsTextTrimmed", typeof( bool ), typeof( TrimmedTextBlock ), new PropertyMetadata( false, OnIsTextTrimmedChanged ) );
    public bool IsTextTrimmed
    {
      get
      {
        return ( bool )GetValue( IsTextTrimmedProperty );
      }
      private set
      {
        SetValue( IsTextTrimmedProperty, value );
      }
    }

    private static void OnIsTextTrimmedChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      var textBlock = d as TrimmedTextBlock;
      if( textBlock != null )
      {
        textBlock.OnIsTextTrimmedChanged( ( bool )e.OldValue, ( bool )e.NewValue );
      }
    }

    private void OnIsTextTrimmedChanged( bool oldValue, bool newValue )
    {
        this.ToolTip = ( newValue ) ? this.Text : null;
    }

    #endregion

    #region Event Handler

    private void TrimmedTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      var textBlock = sender as TextBlock;
      if( textBlock != null )
      {
        this.IsTextTrimmed = this.GetIsTextTrimmed( textBlock );
      }
    }

    #endregion

    #region Private Methods

    private bool GetIsTextTrimmed( TextBlock textBlock )
    {
      if( textBlock == null )
        return false;
      if( textBlock.TextTrimming == TextTrimming.None )
        return false;
      if( textBlock.TextWrapping != TextWrapping.NoWrap )
        return false;

      var textBlockActualWidth = textBlock.ActualWidth;
      textBlock.Measure( new Size( double.MaxValue, double.MaxValue ) );
      var textBlockDesiredWidth = textBlock.DesiredSize.Width;

      return ( textBlockActualWidth < textBlockDesiredWidth );
    }

    #endregion
  }
}
