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
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Xceed.Wpf.Toolkit
{
  public class RichTextBox : System.Windows.Controls.RichTextBox
  {
    #region Private Members

    private bool _textSetInternally;
    bool _surpressGetText;

    #endregion //Private Members

    #region Constructors

    public RichTextBox()
    {
      Loaded += RichTextBox_Loaded;
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
      RichTextBox rtb = ( RichTextBox )d;

      // if the text is not being set internally then load the text into the RichTextBox
      if( !rtb._textSetInternally )
      {
        //to help with performance this is placed on the dispatcher for processing. For some reason when this is done the TextChanged event is fired multiple times
        //forcing the UpdateText method to be called multiple times and the setter of the source property to be set multiple times. To fix this, we simply set the _surpressGetText
        //member to true before the operation and set it to false when the operation completes. This will prevent the Text property from being set multiple times.
        rtb._surpressGetText = true;
        DispatcherOperation dop = Dispatcher.CurrentDispatcher.BeginInvoke( new Action( delegate()
            {
              rtb.TextFormatter.SetText( rtb.Document, ( string )e.NewValue );
            } ), DispatcherPriority.Background );
        dop.Completed += ( sender, ea ) =>
            {
              rtb._surpressGetText = false;
            };
      }
    }

    private static object CoerceTextProperty( DependencyObject d, object value )
    {
      return value ?? "";
    }

    #endregion //Text

    #region TextFormatter

    private ITextFormatter _textFormatter;
    /// <summary>
    /// The ITextFormatter the is used to format the text of the RichTextBox.
    /// Deafult formatter is the RtfFormatter
    /// </summary>
    public ITextFormatter TextFormatter
    {
      get
      {
        if( _textFormatter == null )
          _textFormatter = new RtfFormatter(); //default is rtf

        return _textFormatter;
      }
      set
      {
        if( _textFormatter != value )
        {
          _textFormatter = value;
          _textFormatter.SetText( Document, Text );
        }
      }
    }

    #endregion //TextFormatter

    #endregion //Properties

    #region Methods

    private void UpdateText()
    {
      _textSetInternally = true;

      if( !_surpressGetText )
        Text = TextFormatter.GetText( Document );

      _textSetInternally = false;
    }

    /// <summary>
    /// Clears the content of the RichTextBox.
    /// </summary>
    public void Clear()
    {
      Document.Blocks.Clear();
    }

    #endregion //Methods

    #region Event Hanlders

    private void RichTextBox_Loaded( object sender, RoutedEventArgs e )
    {
      Binding binding = BindingOperations.GetBinding( this, TextProperty );

      if( binding != null )
      {
        if( binding.UpdateSourceTrigger == UpdateSourceTrigger.Default || binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus )
        {
          PreviewLostKeyboardFocus += ( o, ea ) => UpdateText();
        }
        else
        {
          TextChanged += ( o, ea ) => UpdateText();
        }
      }
    }

    #endregion //Event Hanlders
  }
}
