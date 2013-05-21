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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Globalization;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core;
using Xceed.Wpf.Toolkit.Core.Input;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = PART_TextBox, Type = typeof( DatePickerTextBox ) )]
  public class DataGridDatePicker : DatePicker, IValidateInput
  {
    private const string PART_TextBox = "PART_TextBox";
    private Exception _commitException;

    #region Method Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      DatePickerTextBox textBox = GetTemplateChild( PART_TextBox ) as DatePickerTextBox;
      if( textBox != null )
      {
        textBox.Background = new SolidColorBrush( Colors.Transparent );
      }
    }

    protected override void OnDateValidationError( DatePickerDateValidationErrorEventArgs e )
    {
      base.OnDateValidationError( e );

      // This validation error may have been raised by the "CommitInput()" call.
      // If this is the case, use the _commitException member.
      if( InputValidationError != null )
      {
        InputValidationErrorEventArgs args = ( _commitException != null )
          ? new InputValidationErrorEventArgs( _commitException )
          : new InputValidationErrorEventArgs( e.Exception );

        InputValidationError( this, args );
        if( args.ThrowException )
        {
          throw args.Exception;
        }
      }
    }

    #endregion

    #region Method

    public bool CommitInput()
    {
      bool returnValue = true;
      try
      {
        // Null or empty string is a null date;
        DateTime? dateTime = ( !string.IsNullOrEmpty( Text ) )
          ? DateTime.Parse( Text, DateTimeFormatInfo.GetInstance( CultureInfo.CurrentCulture ) )
          : ( DateTime? )null;

        // This may throw an exception if the typed date is 
        // part of the blackout dates.
        this.SelectedDate = dateTime;
      }
      catch( Exception e )
      {
        _commitException = e;
        // Return the TextField to the appropritate value for the current SelectedDate.
        // Setting the "Text" property to invalid content for a date was the
        // observed behavior for the DatePicker. 
        // "Invalid" is expected to be an invalid content.
        // This will raise the "DateValidationError" event from the datepicker
        Text = "Invalid";
        _commitException = null;
        returnValue = false;
      }

      return returnValue;
    }

    #endregion

    #region Events

    public event InputValidationErrorEventHandler InputValidationError;

    #endregion
  }
}
