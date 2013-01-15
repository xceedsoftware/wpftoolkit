/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Core;
using System.Windows.Media;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Primitives;
using System.Reflection;
using Xceed.Wpf.Toolkit.Core.Input;

namespace Xceed.Wpf.DataGrid
{
  public class ValidateInputWrapper : Border
  {
    #region Base Class Overrides

    protected override void OnVisualChildrenChanged( DependencyObject visualAdded, DependencyObject visualRemoved )
    {
      base.OnVisualChildrenChanged( visualAdded, visualRemoved );

      if( visualAdded != null )
      {
        IValidateInput validateInput = ValidateNewChild( visualAdded );
        RegisterValidationErrorEvent( validateInput, true );
        this.Child.AddHandler( UIElement.KeyDownEvent, new KeyEventHandler( HandleKeyDown ), true ); 
      }
      if( visualRemoved != null )
      {
        RegisterValidationErrorEvent( visualRemoved as IValidateInput, false );
        this.Child.RemoveHandler( UIElement.KeyDownEvent, new KeyEventHandler( HandleKeyDown ) );
      }
    }

    protected override void OnPreviewGotKeyboardFocus( KeyboardFocusChangedEventArgs e )
    {
      base.OnPreviewGotKeyboardFocus( e );

      if( this.Child != null )
      {
        CellEditor.SetHasError( ( DependencyObject )this.Child, false );
      }
    }

    #endregion

    #region Methods

    private IValidateInput ValidateNewChild( DependencyObject newChild )
    {
      IValidateInput validateInput = newChild as IValidateInput;
      if( newChild != null && validateInput == null )
        throw new InvalidOperationException( string.Format( "Child of ValidateInputWrapper should be a {0} instance", typeof( IValidateInput ) ) );

      return validateInput;
    }

    private void OnInputValidationError( object sender, InputValidationErrorEventArgs e )
    {
      if( this.Child != null )
      {
        CellEditor.SetHasError( ( DependencyObject )this.Child, true );
      }
    }

    private void RegisterValidationErrorEvent( IValidateInput validationElement, bool isRegistering )
    {
      if( validationElement != null )
      {
        if( isRegistering )
        {
          validationElement.InputValidationError += new InputValidationErrorEventHandler( this.OnInputValidationError );
        }
        else
        {
          validationElement.InputValidationError -= new InputValidationErrorEventHandler( this.OnInputValidationError );
        }
      }
    }

    private void HandleKeyDown( object sender, KeyEventArgs e )
    {
      if( e.Key == Key.Enter )
      {
        // The DatePicker (and UpDownBase<T> OnValidationError) sets this as handled, which breaks the DataGrid commit.
        e.Handled = false;
      }
      if( e.Key == Key.Tab )
      {
        // We must commit the value before the tab is handled by the 
        // DataGrid, or the ValidationError event won't be triggered.
        IValidateInput validationElement = this.Child as IValidateInput;
        if( validationElement != null )
        {
          validationElement.CommitInput();
        }
      }
    }

    #endregion
  }
}
