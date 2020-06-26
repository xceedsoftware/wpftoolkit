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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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

    #region HighlightedBrush

    public static readonly DependencyProperty HighlightedBrushProperty = DependencyProperty.Register( "HighlightedBrush", typeof( Brush ), typeof( TrimmedTextBlock ), new FrameworkPropertyMetadata( Brushes.Yellow ) );

    public Brush HighlightedBrush
    {
      get
      {
        return ( Brush )GetValue( HighlightedBrushProperty );
      }
      set
      {
        SetValue( HighlightedBrushProperty, value );
      }
    }

    #endregion

    #region HighlightedText

    public static readonly DependencyProperty HighlightedTextProperty = DependencyProperty.Register( "HighlightedText", typeof( string ), typeof( TrimmedTextBlock ), new FrameworkPropertyMetadata( null, HighlightedTextChanged ) );

    public string HighlightedText
    {
      get
      {
        return ( string )GetValue( HighlightedTextProperty );
      }
      set
      {
        SetValue( HighlightedTextProperty, value );
      }
    }

    private static void HighlightedTextChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var trimmedTextBlock = sender as TrimmedTextBlock;
      if( trimmedTextBlock != null )
      {
        trimmedTextBlock.HighlightedTextChanged( ( string )e.OldValue, ( string )e.NewValue );
      }
    }

    protected virtual void HighlightedTextChanged( string oldValue, string newValue )
    {
      if( this.Text.Length == 0 )
        return;

      // Set original text without highlight.
      if( newValue == null )
      {
        var newrRun = new Run( this.Text );
        this.Inlines.Clear();        
        this.Inlines.Add( newrRun );

        return;
      }

      var startHighlightedIndex = this.Text.IndexOf( newValue, StringComparison.InvariantCultureIgnoreCase );
      var endHighlightedIndex = startHighlightedIndex + newValue.Length;

      var startUnHighlightedText = this.Text.Substring( 0, startHighlightedIndex );
      var highlightedText = this.Text.Substring( startHighlightedIndex, newValue.Length );
      var endUnHighlightedText = this.Text.Substring( endHighlightedIndex, this.Text.Length - endHighlightedIndex );

      this.Inlines.Clear();

      // Start Un-Highlighted text
      var run = new Run( startUnHighlightedText );
      this.Inlines.Add( run );

      // Highlighted text
      run = new Run( highlightedText );
      run.Background = this.HighlightedBrush;
      this.Inlines.Add( run );

      // End Un-Highlighted text
      run = new Run( endUnHighlightedText );
      this.Inlines.Add( run );
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
