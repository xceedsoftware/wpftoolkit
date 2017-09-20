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
using System.ComponentModel;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Automation;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_ChildCheckBox", Type = typeof( System.Windows.Controls.CheckBox ) )]
  public class DataGridCheckBox : ContentControl
  {
    static DataGridCheckBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( DataGridCheckBox ), new FrameworkPropertyMetadata( typeof( DataGridCheckBox ) ) );

      AutomationProperties.AutomationIdProperty.OverrideMetadata( typeof( DataGridCheckBox ), new UIPropertyMetadata( "CheckBox" ) );

      CheckedEvent = System.Windows.Controls.CheckBox.CheckedEvent.AddOwner( typeof( DataGridCheckBox ) );
      UncheckedEvent = System.Windows.Controls.CheckBox.UncheckedEvent.AddOwner( typeof( DataGridCheckBox ) );
      IndeterminateEvent = System.Windows.Controls.CheckBox.IndeterminateEvent.AddOwner( typeof( DataGridCheckBox ) );

      EventManager.RegisterClassHandler( typeof( DataGridCheckBox ), CheckedEvent, new RoutedEventHandler( CheckBoxEventHandler ) );
      EventManager.RegisterClassHandler( typeof( DataGridCheckBox ), UncheckedEvent, new RoutedEventHandler( CheckBoxEventHandler ) );
      EventManager.RegisterClassHandler( typeof( DataGridCheckBox ), IndeterminateEvent, new RoutedEventHandler( CheckBoxEventHandler ) );
    }

    #region IsChecked Property

    public static readonly DependencyProperty IsCheckedProperty =
      System.Windows.Controls.CheckBox.IsCheckedProperty.AddOwner( typeof( DataGridCheckBox ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnIsCheckedChanged ) ) );

    [TypeConverter( typeof( NullableBoolConverter ) )]
    public Nullable<bool> IsChecked
    {
      get
      {
        return ( Nullable<bool> )this.GetValue( DataGridCheckBox.IsCheckedProperty );
      }
      set
      {
        this.SetValue( DataGridCheckBox.IsCheckedProperty, value );
      }
    }

    #endregion IsChecked Property

    #region IsThreeState Property

    public static readonly DependencyProperty IsThreeStateProperty = System.Windows.Controls.CheckBox.IsThreeStateProperty.AddOwner( typeof( DataGridCheckBox ) );

    public bool IsThreeState
    {
      get
      {
        return ( bool )this.GetValue( DataGridCheckBox.IsThreeStateProperty );
      }
      set
      {
        this.SetValue( DataGridCheckBox.IsThreeStateProperty, value );
      }
    }

    #endregion IsThreeState Property

    public static readonly RoutedEvent CheckedEvent;
    public static readonly RoutedEvent UncheckedEvent;
    public static readonly RoutedEvent IndeterminateEvent;

    public event RoutedEventHandler Checked
    {
      add
      {
        base.AddHandler( DataGridCheckBox.CheckedEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridCheckBox.CheckedEvent, value );
      }
    }

    public event RoutedEventHandler Unchecked
    {
      add
      {
        base.AddHandler( DataGridCheckBox.UncheckedEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridCheckBox.UncheckedEvent, value );
      }
    }

    public event RoutedEventHandler Indeterminate
    {
      add
      {
        base.AddHandler( DataGridCheckBox.IndeterminateEvent, value );
      }
      remove
      {
        base.RemoveHandler( DataGridCheckBox.IndeterminateEvent, value );
      }
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      base.OnPreviewKeyDown( e );

      if( e.Handled == false )
      {
        //if this item is the source of the PreviewKeyDown event
        if( e.OriginalSource == this && e.IsRepeat == false )
        {
          e.Handled = this.ProcessKeyEventArgs( e, this.ChildCheckBox );
        }
      }
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      base.OnKeyDown( e );

      if( e.Handled == false )
      {
        if( e.OriginalSource == this && e.IsRepeat == false )
        {
          e.Handled = this.ProcessKeyEventArgs( e, this.ChildCheckBox );
        }
      }
    }

    protected override void OnPreviewKeyUp( KeyEventArgs e )
    {
      base.OnPreviewKeyUp( e );

      if( e.Handled == false )
      {
        if( e.OriginalSource == this && e.IsRepeat == false )
        {
          e.Handled = this.ProcessKeyEventArgs( e, this.ChildCheckBox );
        }
      }
    }

    protected override void OnKeyUp( KeyEventArgs e )
    {
      base.OnKeyUp( e );

      if( e.Handled == false )
      {
        if( e.OriginalSource == this && e.IsRepeat == false )
        {
          e.Handled = this.ProcessKeyEventArgs( e, this.ChildCheckBox );
        }
      }
    }

    protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      base.OnPreviewMouseLeftButtonDown( e );

      if( e.Handled == false )
      {
        this.Focus();

        //do not set the e.Handled to true, we want the mouse down event to go as far as possible (true checkbox)
      }
    }

    private System.Windows.Controls.CheckBox ChildCheckBox
    {
      get
      {
        return this.GetTemplateChild( "PART_ChildCheckBox" ) as System.Windows.Controls.CheckBox;
      }
    }

    private bool ProcessKeyEventArgs( KeyEventArgs e, IInputElement target )
    {
      bool retval = false;

      if( target != null )
      {
        //foward the event to the child check box
        Key realKey;
        if( ( e.Key == Key.None ) && ( e.SystemKey != Key.None ) )
        {
          realKey = e.SystemKey;
        }
        else
        {
          realKey = e.Key;
        }

        //in XBAP the Keyboard.PrimaryDevice.ActiveSource will throw ,therefore, protect against the throw and suppress the exception
        //if its the one we expect.
        try
        {
          KeyEventArgs kea = new KeyEventArgs( Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, realKey );
          kea.RoutedEvent = e.RoutedEvent;

          //send the event
          target.RaiseEvent( kea );

          retval = kea.Handled;
        }
        catch( SecurityException ex )
        {
          //if the exception is for the UIPermission, then we want to suppress it
          if( ( ex.PermissionType.FullName == "System.Security.Permissions.UIPermission" ) == false )
          {
            //not correct type, then rethrow the exception
            throw;
          }
          else //this means that we are in XBAP
          {
            //we want to handle speciallly the case where the space was pressed (so that checkbox works in XBAP)
            //condition taken from the System ChecckBox
            if( ( e.RoutedEvent == Keyboard.KeyDownEvent )
                && ( e.Key == Key.Space )
                && ( ( Keyboard.Modifiers & ( ModifierKeys.Control | ModifierKeys.Alt ) ) != ModifierKeys.Alt )
                && ( this.IsMouseCaptured == false ) )
            {
              if( this.IsChecked.HasValue == true )
              {
                if( this.IsChecked.Value == false )
                {
                  this.IsChecked = true;
                }
                else if( this.IsThreeState == false )
                {
                  this.IsChecked = false;
                }
                else
                {
                  this.IsChecked = null;
                }
              }
              else
              {
                this.IsChecked = false;
              }

              retval = true;
            }
          }
        }
      }

      return retval;
    }

    private static void CheckBoxEventHandler( object sender, RoutedEventArgs e )
    {
      //this condition detects that the event does not originates from himself and suppress the event if it does...
      if( sender != e.OriginalSource )
      {
        e.Handled = true;
      }
    }

    private static void OnIsCheckedChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridCheckBox obj = ( DataGridCheckBox )sender;
      Nullable<bool> newValue = ( Nullable<bool> )e.NewValue;

      if( newValue.HasValue == true )
      {
        if( newValue.GetValueOrDefault() == true )
        {
          obj.RaiseEvent( new RoutedEventArgs( DataGridCheckBox.CheckedEvent ) );
        }
        else
        {
          obj.RaiseEvent( new RoutedEventArgs( DataGridCheckBox.UncheckedEvent ) );
        }
      }
      else
      {
        obj.RaiseEvent( new RoutedEventArgs( DataGridCheckBox.IndeterminateEvent ) );
      }
    }

  }
}
