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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit
{
  public class RichTextBoxFormatBarManager : DependencyObject
  {
    #region Members

    private global::System.Windows.Controls.RichTextBox _richTextBox;
    private UIElementAdorner<Control> _adorner;
    private IRichTextBoxFormatBar _toolbar;
    private Window _parentWindow;

    private const double _hideAdornerDistance = 150d;

    #endregion //Members

    #region Properties

    #region FormatBar

    public static readonly DependencyProperty FormatBarProperty = DependencyProperty.RegisterAttached( "FormatBar", typeof( IRichTextBoxFormatBar ), typeof( RichTextBox ), new PropertyMetadata( null, OnFormatBarPropertyChanged ) );
    public static void SetFormatBar( UIElement element, IRichTextBoxFormatBar value )
    {
      element.SetValue( FormatBarProperty, value );
    }
    public static IRichTextBoxFormatBar GetFormatBar( UIElement element )
    {
      return ( IRichTextBoxFormatBar )element.GetValue( FormatBarProperty );
    }

    private static void OnFormatBarPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      global::System.Windows.Controls.RichTextBox rtb = d as global::System.Windows.Controls.RichTextBox;
      if( rtb == null )
        throw new Exception( "A FormatBar can only be applied to a RichTextBox." );

      RichTextBoxFormatBarManager manager = new RichTextBoxFormatBarManager();
      manager.AttachFormatBarToRichtextBox( rtb, e.NewValue as IRichTextBoxFormatBar );
    }

    #endregion //FormatBar

    public bool IsAdornerVisible
    {
      get
      {
        return _adorner.Visibility == Visibility.Visible;
      }
    }

    #endregion //Properties

    #region Event Handlers

    void RichTextBox_MouseButtonUp( object sender, MouseButtonEventArgs e )
    {
      if( e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Released )
      {
        if( !_richTextBox.IsReadOnly )
        {
          TextRange selectedText = new TextRange( _richTextBox.Selection.Start, _richTextBox.Selection.End );
#if !VS2008
          if( selectedText.Text.Length > 0 && !String.IsNullOrWhiteSpace( selectedText.Text ) )
            ShowAdorner();
#else
          if( selectedText.Text.Length > 0 && !String.IsNullOrEmpty( selectedText.Text ) )
            ShowAdorner();
#endif
          else
            HideAdorner();

          e.Handled = true;
        }
      }
      else
        HideAdorner();
    }

    private void OnPreviewMouseMoveParentWindow( object sender, MouseEventArgs e )
    {
      Point p = e.GetPosition( _adorner );
      double maxDist = 0d;
      bool preventDisplayFadeOut = ( ( _adorner.Child != null ) && ( _adorner.Child is IRichTextBoxFormatBar ) ) ?
                                  ( ( IRichTextBoxFormatBar )_adorner.Child ).PreventDisplayFadeOut :
                                  false;

      //Mouse is inside FormatBar: Nothing to do.
      if( preventDisplayFadeOut ||
        ( p.X >= 0 ) && ( p.X <= _adorner.ActualWidth ) && ( p.Y >= 0 ) && ( p.Y <= _adorner.ActualHeight ) )
      {
        return;
      }
      //Mouse is too much outside FormatBar: Close it.
      else if( ( p.X < -_hideAdornerDistance ) || ( p.X > _adorner.ActualWidth + _hideAdornerDistance ) || ( p.Y < -_hideAdornerDistance ) || ( p.Y > _adorner.ActualHeight + _hideAdornerDistance ) )
      {
        HideAdorner();
      }
      //Mouse is just outside FormatBar: Vary its opacity.
      else
      {
        if( p.X < 0 )
          maxDist = -p.X;
        else if( p.X > _adorner.ActualWidth )
          maxDist = p.X - _adorner.ActualWidth;

        if( p.Y < 0 )
          maxDist = Math.Max( maxDist, -p.Y );
        else if( p.Y > _adorner.ActualHeight )
          maxDist = Math.Max( maxDist, p.Y - _adorner.ActualHeight );

        _adorner.Opacity = 1d - ( Math.Min( maxDist, 100d ) / 100d );
      }
    }

    void RichTextBox_TextChanged( object sender, TextChangedEventArgs e )
    {
      //This fixes the bug when applying text transformations the text would lose it's highlight. That was because the RichTextBox was losing focus,
      //so we just give it focus again and it seems to do the trick of re-highlighting it.
      if( !_richTextBox.IsFocused && !_richTextBox.Selection.IsEmpty )
        _richTextBox.Focus();
    }

    #endregion //Event Handlers

    #region Methods

    /// <summary>
    /// Attaches a FormatBar to a RichtextBox
    /// </summary>
    /// <param name="richTextBox">The RichtextBox to attach to.</param>
    /// <param name="formatBar">The Formatbar to attach.</param>
    private void AttachFormatBarToRichtextBox( global::System.Windows.Controls.RichTextBox richTextBox, IRichTextBoxFormatBar formatBar )
    {
      _richTextBox = richTextBox;
      //we cannot use the PreviewMouseLeftButtonUp event because of selection bugs.
      //we cannot use the MouseLeftButtonUp event because it is handled by the RichTextBox and does not bubble up to here, so we must
      //add a hander to the MouseUpEvent using the Addhandler syntax, and specify to listen for handled events too.
      _richTextBox.AddHandler( Mouse.MouseUpEvent, new MouseButtonEventHandler( RichTextBox_MouseButtonUp ), true );
      _richTextBox.TextChanged += RichTextBox_TextChanged;

      _adorner = new UIElementAdorner<Control>( _richTextBox );

      formatBar.Target = _richTextBox;
      _toolbar = formatBar;
    }

    /// <summary>
    /// Shows the FormatBar
    /// </summary>
    void ShowAdorner()
    {
      if( _adorner.Visibility == Visibility.Visible )
        return;

      VerifyAdornerLayer();

      Control adorningEditor = _toolbar as Control;

      if( _adorner.Child == null )
        _adorner.Child = adorningEditor;

      adorningEditor.ApplyTemplate();
      _toolbar.Update();

      _adorner.Visibility = Visibility.Visible;

      PositionFormatBar( adorningEditor );

      _parentWindow = TreeHelper.FindParent<Window>( _adorner );
      if( _parentWindow != null )
      {
        Mouse.AddMouseMoveHandler( _parentWindow, OnPreviewMouseMoveParentWindow );
      }
    }

    /// <summary>
    /// Positions the FormatBar so that is does not go outside the bounds of the RichTextBox or covers the selected text
    /// </summary>
    /// <param name="adorningEditor"></param>
    private void PositionFormatBar( Control adorningEditor )
    {
      Point mousePosition = Mouse.GetPosition( _richTextBox );

      var left = mousePosition.X;
      var top = mousePosition.Y;

      // Top boundary
      if( top < 0 )
      {
        top = 5d;
      }

      // Left boundary
      if( left < 0 )
      {
        left = 5d;
      }

      // Right boundary
      if( left + adorningEditor.ActualWidth > _richTextBox.ActualWidth - 10d )
      {
        left = _richTextBox.ActualWidth - adorningEditor.ActualWidth - 10d;
      }

      // Bottom boundary
      if( top + adorningEditor.ActualHeight > _richTextBox.ActualHeight - 10d )
      {
        top = _richTextBox.ActualHeight - adorningEditor.ActualHeight - 10d;
      }

      _adorner.SetOffsets( left, top );
    }

    /// <summary>
    /// Ensures that the IRichTextFormatBar is in the adorner layer.
    /// </summary>
    /// <returns>True if the IRichTextFormatBar is in the adorner layer, else false.</returns>
    bool VerifyAdornerLayer()
    {
      if( _adorner.Parent != null )
        return true;

      AdornerLayer layer = AdornerLayer.GetAdornerLayer( _richTextBox );
      if( layer == null )
        return false;

      layer.Add( _adorner );
      return true;
    }

    /// <summary>
    /// Hides the IRichTextFormatBar that is in the adornor layer.
    /// </summary>
    void HideAdorner()
    {
      if( IsAdornerVisible )
      {
        _adorner.Visibility = Visibility.Collapsed;
        //_adorner.Child = null;
        if( _parentWindow != null )
        {
          Mouse.RemoveMouseMoveHandler( _parentWindow, OnPreviewMouseMoveParentWindow );
        }
      }
    }

    #endregion //Methods
  }
}
