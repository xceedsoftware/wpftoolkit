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

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit
{
  public class AutoSelectTextBox : TextBox
  {
    public AutoSelectTextBox()
    {
    }

    #region AutoSelectBehavior PROPERTY

    public AutoSelectBehavior AutoSelectBehavior
    {
      get
      {
        return ( AutoSelectBehavior )GetValue( AutoSelectBehaviorProperty );
      }
      set
      {
        SetValue( AutoSelectBehaviorProperty, value );
      }
    }

    public static readonly DependencyProperty AutoSelectBehaviorProperty =
        DependencyProperty.Register( "AutoSelectBehavior", typeof( AutoSelectBehavior ), typeof( AutoSelectTextBox ),
      new UIPropertyMetadata( AutoSelectBehavior.Never ) );

    #endregion AutoSelectBehavior PROPERTY

    #region AutoMoveFocus PROPERTY

    public bool AutoMoveFocus
    {
      get
      {
        return ( bool )GetValue( AutoMoveFocusProperty );
      }
      set
      {
        SetValue( AutoMoveFocusProperty, value );
      }
    }

    public static readonly DependencyProperty AutoMoveFocusProperty =
        DependencyProperty.Register( "AutoMoveFocus", typeof( bool ), typeof( AutoSelectTextBox ), new UIPropertyMetadata( false ) );

    #endregion AutoMoveFocus PROPERTY

    #region QueryMoveFocus EVENT

    public static readonly RoutedEvent QueryMoveFocusEvent = EventManager.RegisterRoutedEvent( "QueryMoveFocus",
                                                                                                RoutingStrategy.Bubble,
                                                                                                typeof( QueryMoveFocusEventHandler ),
                                                                                                typeof( AutoSelectTextBox ) );
    #endregion QueryMoveFocus EVENT

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      if( !this.AutoMoveFocus )
      {
        base.OnPreviewKeyDown( e );
        return;
      }

      if( ( e.Key == Key.Left )
        && ( ( Keyboard.Modifiers == ModifierKeys.None )
          || ( Keyboard.Modifiers == ModifierKeys.Control ) ) )
      {
        e.Handled = this.MoveFocusLeft();
      }

      if( ( e.Key == Key.Right )
        && ( ( Keyboard.Modifiers == ModifierKeys.None )
          || ( Keyboard.Modifiers == ModifierKeys.Control ) ) )
      {
        e.Handled = this.MoveFocusRight();
      }

      if( ( ( e.Key == Key.Up ) || ( e.Key == Key.PageUp ) )
        && ( ( Keyboard.Modifiers == ModifierKeys.None )
          || ( Keyboard.Modifiers == ModifierKeys.Control ) ) )
      {
        e.Handled = this.MoveFocusUp();
      }

      if( ( ( e.Key == Key.Down ) || ( e.Key == Key.PageDown ) )
       && ( ( Keyboard.Modifiers == ModifierKeys.None )
         || ( Keyboard.Modifiers == ModifierKeys.Control ) ) )
      {
        e.Handled = this.MoveFocusDown();
      }

      base.OnPreviewKeyDown( e );
    }

    protected override void OnPreviewGotKeyboardFocus( KeyboardFocusChangedEventArgs e )
    {
      base.OnPreviewGotKeyboardFocus( e );

      if( this.AutoSelectBehavior == AutoSelectBehavior.OnFocus )
      {
        // If the focus was not in one of our child ( or popup ), we select all the text.
        if( !TreeHelper.IsDescendantOf( e.OldFocus as DependencyObject, this ) )
        {
          this.SelectAll();
        }
      }
    }

    protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      base.OnPreviewMouseLeftButtonDown( e );

      if( this.AutoSelectBehavior == AutoSelectBehavior.Never )
        return;

      if( this.IsKeyboardFocusWithin == false )
      {
        this.Focus();
        e.Handled = true;  //prevent from removing the selection
      }
    }

    protected override void OnTextChanged( TextChangedEventArgs e )
    {
      base.OnTextChanged( e );

      if( !this.AutoMoveFocus )
        return;

      if( ( this.Text.Length != 0 )
          && ( this.Text.Length == this.MaxLength )
          && ( this.CaretIndex == this.MaxLength ) )
      {
        if( this.CanMoveFocus( FocusNavigationDirection.Right, true ) == true )
        {
          FocusNavigationDirection direction = ( this.FlowDirection == FlowDirection.LeftToRight )
            ? FocusNavigationDirection.Right
            : FocusNavigationDirection.Left;

          this.MoveFocus( new TraversalRequest( direction ) );
        }
      }
    }


    private bool CanMoveFocus( FocusNavigationDirection direction, bool reachedMax )
    {
      QueryMoveFocusEventArgs e = new QueryMoveFocusEventArgs( direction, reachedMax );
      this.RaiseEvent( e );
      return e.CanMoveFocus;
    }

    private bool MoveFocusLeft()
    {
      if( this.FlowDirection == FlowDirection.LeftToRight )
      {
        //occurs only if the cursor is at the beginning of the text
        if( ( this.CaretIndex == 0 ) && ( this.SelectionLength == 0 ) )
        {
          if( ComponentCommands.MoveFocusBack.CanExecute( null, this ) )
          {
            ComponentCommands.MoveFocusBack.Execute( null, this );
            return true;
          }
          else if( this.CanMoveFocus( FocusNavigationDirection.Left, false ) )
          {
            this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Left ) );
            return true;
          }
        }
      }
      else
      {
        //occurs only if the cursor is at the end of the text
        if( ( this.CaretIndex == this.Text.Length ) && ( this.SelectionLength == 0 ) )
        {
          if( ComponentCommands.MoveFocusBack.CanExecute( null, this ) )
          {
            ComponentCommands.MoveFocusBack.Execute( null, this );
            return true;
          }
          else if( this.CanMoveFocus( FocusNavigationDirection.Left, false ) )
          {
            this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Left ) );
            return true;
          }
        }
      }

      return false;
    }

    private bool MoveFocusRight()
    {
      if( this.FlowDirection == FlowDirection.LeftToRight )
      {
        //occurs only if the cursor is at the beginning of the text
        if( ( this.CaretIndex == this.Text.Length ) && ( this.SelectionLength == 0 ) )
        {
          if( ComponentCommands.MoveFocusForward.CanExecute( null, this ) )
          {
            ComponentCommands.MoveFocusForward.Execute( null, this );
            return true;
          }
          else if( this.CanMoveFocus( FocusNavigationDirection.Right, false ) )
          {
            this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Right ) );
            return true;
          }
        }
      }
      else
      {
        //occurs only if the cursor is at the end of the text
        if( ( this.CaretIndex == 0 ) && ( this.SelectionLength == 0 ) )
        {
          if( ComponentCommands.MoveFocusForward.CanExecute( null, this ) )
          {
            ComponentCommands.MoveFocusForward.Execute( null, this );
            return true;
          }
          else if( this.CanMoveFocus( FocusNavigationDirection.Right, false ) )
          {
            this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Right ) );
            return true;
          }
        }
      }

      return false;
    }

    private bool MoveFocusUp()
    {
      int lineNumber = this.GetLineIndexFromCharacterIndex( this.SelectionStart );

      //occurs only if the cursor is on the first line
      if( lineNumber == 0 )
      {
        if( ComponentCommands.MoveFocusUp.CanExecute( null, this ) )
        {
          ComponentCommands.MoveFocusUp.Execute( null, this );
          return true;
        }
        else if( this.CanMoveFocus( FocusNavigationDirection.Up, false ) )
        {
          this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Up ) );
          return true;
        }
      }

      return false;
    }

    private bool MoveFocusDown()
    {
      int lineNumber = this.GetLineIndexFromCharacterIndex( this.SelectionStart );

      //occurs only if the cursor is on the first line
      if( lineNumber == ( this.LineCount - 1 ) )
      {
        if( ComponentCommands.MoveFocusDown.CanExecute( null, this ) )
        {
          ComponentCommands.MoveFocusDown.Execute( null, this );
          return true;
        }
        else if( this.CanMoveFocus( FocusNavigationDirection.Down, false ) )
        {
          this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Down ) );
          return true;
        }
      }

      return false;
    }
  }
}

