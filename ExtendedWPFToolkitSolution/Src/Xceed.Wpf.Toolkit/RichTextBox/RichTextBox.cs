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
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Xceed.Wpf.Toolkit
{
  public class RichTextBox : System.Windows.Controls.RichTextBox
  {
    #region Private Members

    private bool _preventDocumentUpdate;
    private bool _preventTextUpdate;

    #endregion //Private Members

    #region Constructors

    public RichTextBox()
    {
    }

    public RichTextBox( System.Windows.Documents.FlowDocument document )
      : base( document )
    {
    }

    #endregion //Constructors

    #region Properties

    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( RichTextBox ), new FrameworkPropertyMetadata( String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged, CoerceTextProperty, true, UpdateSourceTrigger.LostFocus ) );
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

    private static void OnTextPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( RichTextBox )d ).UpdateDocumentFromText();
    }

    private static object CoerceTextProperty( DependencyObject d, object value )
    {
      return value ?? "";
    }

    #endregion //Text

    #region TextFormatter

    public static readonly DependencyProperty TextFormatterProperty = DependencyProperty.Register( "TextFormatter", typeof( ITextFormatter ), typeof( RichTextBox ), new FrameworkPropertyMetadata( new RtfFormatter(), OnTextFormatterPropertyChanged ) );
    public ITextFormatter TextFormatter
    {
      get
      {
        return ( ITextFormatter )GetValue( TextFormatterProperty );
      }
      set
      {
        SetValue( TextFormatterProperty, value );
      }
    }

    private static void OnTextFormatterPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      RichTextBox richTextBox = d as RichTextBox;
      if( richTextBox != null )
        richTextBox.OnTextFormatterPropertyChanged( ( ITextFormatter )e.OldValue, ( ITextFormatter )e.NewValue );
    }

    protected virtual void OnTextFormatterPropertyChanged( ITextFormatter oldValue, ITextFormatter newValue )
    {
      this.UpdateTextFromDocument();
    }

    #endregion //TextFormatter

    #endregion //Properties

    #region Methods

    protected override void OnTextChanged( System.Windows.Controls.TextChangedEventArgs e )
    {
      this.UpdateTextFromDocument();
      base.OnTextChanged( e );     
    }

    private void UpdateTextFromDocument()
    {
      if( _preventTextUpdate )
        return;

      _preventDocumentUpdate = true;
#if VS2008
      Text = this.TextFormatter.GetText( this.Document );
#else
      this.SetCurrentValue( RichTextBox.TextProperty, this.TextFormatter.GetText( this.Document ) );
#endif
      _preventDocumentUpdate = false;
    }

    private void UpdateDocumentFromText()
    {
      if( _preventDocumentUpdate )
        return;

      _preventTextUpdate = true;
      this.TextFormatter.SetText( this.Document, Text );
      _preventTextUpdate = false;
    }

    /// <summary>
    /// Clears the content of the RichTextBox.
    /// </summary>
    public void Clear()
    {
      this.Document.Blocks.Clear();
    }

    public override void BeginInit()
    {
      base.BeginInit();
      // Do not update anything while initializing. See EndInit
      _preventTextUpdate = true;
      _preventDocumentUpdate = true;
    }

    public override void EndInit()
    {
      base.EndInit();
      _preventTextUpdate = false;
      _preventDocumentUpdate = false;
      // Possible conflict here if the user specifies 
      // the document AND the text at the same time 
      // in XAML. Text has priority.
      if( !string.IsNullOrEmpty( Text ) )
      {
        this.UpdateDocumentFromText();
      }
      else
      {
        this.UpdateTextFromDocument();
      }
    }

    #endregion //Methods
  }
}
