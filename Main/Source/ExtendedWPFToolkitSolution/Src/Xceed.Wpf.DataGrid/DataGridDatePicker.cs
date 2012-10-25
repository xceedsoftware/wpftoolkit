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
          ? new InputValidationErrorEventArgs( _commitException.Message )
          : new InputValidationErrorEventArgs( e.Text );

        InputValidationError( this, args );
      }
    }

    #endregion

    #region Method

    public void CommitInput()
    {
      try
      {
        // Null or empty string is an null date;
        DateTime? dateTime = ( !string.IsNullOrEmpty( Text ) )
          ? DateTime.Parse( Text, DateTimeFormatInfo.GetInstance( CultureInfo.CurrentCulture ) )
          : ( DateTime? )null;

        // This may throw an exception either if the typed date is 
        // part of the blackout dates.
        this.SelectedDate = dateTime;
      }
      catch( Exception e )
      {
        _commitException = e;
        // Return the TextField to the appropritate value for the current SelectedDate.
        // Setting the "Text" property to an invalid content for a date was the
        // observed behavior for the DatePicker. 
        // "Invalid" is expected to be an invalid content.
        // This will raise the "DateValidationError" event from the datepicker
        Text = "Invalid";
        _commitException = null;
      }
    }

    #endregion

    #region Events

    public event InputValidationErrorEventHandler InputValidationError;

    #endregion
  }
}
