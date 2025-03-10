/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

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

namespace Xceed.Wpf.Toolkit
{
  public abstract class Spinner : Control
  {
    #region Properties

    public static readonly DependencyProperty ValidSpinDirectionProperty = DependencyProperty.Register( "ValidSpinDirection", typeof( ValidSpinDirections ), typeof( Spinner ), new PropertyMetadata( ValidSpinDirections.Increase | ValidSpinDirections.Decrease, OnValidSpinDirectionPropertyChanged ) );
    public ValidSpinDirections ValidSpinDirection
    {
      get
      {
        return ( ValidSpinDirections )GetValue( ValidSpinDirectionProperty );
      }
      set
      {
        SetValue( ValidSpinDirectionProperty, value );
      }
    }

    private static void OnValidSpinDirectionPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      Spinner source = ( Spinner )d;
      ValidSpinDirections oldvalue = ( ValidSpinDirections )e.OldValue;
      ValidSpinDirections newvalue = ( ValidSpinDirections )e.NewValue;
      source.OnValidSpinDirectionChanged( oldvalue, newvalue );
    }

    #endregion //Properties

    public event EventHandler<SpinEventArgs> Spin;

    #region Events

    public static readonly RoutedEvent SpinnerSpinEvent = EventManager.RegisterRoutedEvent( "SpinnerSpin", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Spinner ) );

    public event RoutedEventHandler SpinnerSpin
    {
      add
      {
        AddHandler( SpinnerSpinEvent, value );
      }
      remove
      {
        RemoveHandler( SpinnerSpinEvent, value );
      }
    }

    #endregion

    protected Spinner()
    {
    }

    protected virtual void OnSpin( SpinEventArgs e )
    {
      ValidSpinDirections valid = e.Direction == SpinDirection.Increase ? ValidSpinDirections.Increase : ValidSpinDirections.Decrease;

      //Only raise the event if spin is allowed.
      if( ( ValidSpinDirection & valid ) == valid )
      {
        EventHandler<SpinEventArgs> handler = Spin;
        if( handler != null )
        {
          handler( this, e );
        }
      }
    }

    protected virtual void OnValidSpinDirectionChanged( ValidSpinDirections oldValue, ValidSpinDirections newValue )
    {
    }
  }
}
