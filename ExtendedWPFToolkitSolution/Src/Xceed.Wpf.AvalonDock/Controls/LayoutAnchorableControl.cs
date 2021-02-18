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

using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutAnchorableControl : Control
  {
    #region Constructors

    static LayoutAnchorableControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( LayoutAnchorableControl ), new FrameworkPropertyMetadata( typeof( LayoutAnchorableControl ) ) );
      FocusableProperty.OverrideMetadata( typeof( LayoutAnchorableControl ), new FrameworkPropertyMetadata( false ) );
    }

    public LayoutAnchorableControl()
    {
      //SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
    }

    #endregion

    #region Properties

    #region Model

    /// <summary>
    /// Model Dependency Property
    /// </summary>
    public static readonly DependencyProperty ModelProperty =  DependencyProperty.Register( "Model", typeof( LayoutAnchorable ), typeof( LayoutAnchorableControl ),
            new FrameworkPropertyMetadata( ( LayoutAnchorable )null, new PropertyChangedCallback( OnModelChanged ) ) );

    /// <summary>
    /// Gets or sets the Model property.  This dependency property 
    /// indicates the model attached to this view.
    /// </summary>
    public LayoutAnchorable Model
    {
      get
      {
        return ( LayoutAnchorable )GetValue( ModelProperty );
      }
      set
      {
        SetValue( ModelProperty, value );
      }
    }

    /// <summary>
    /// Handles changes to the Model property.
    /// </summary>
    private static void OnModelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutAnchorableControl )d ).OnModelChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the Model property.
    /// </summary>
    protected virtual void OnModelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
      {
        ( ( LayoutContent )e.OldValue ).PropertyChanged -= this.Model_PropertyChanged;
      }

      if( ( this.Model != null) && ( this.Model.Root != null ) && ( this.Model.Root.Manager != null ) )
      {
        this.Model.PropertyChanged += this.Model_PropertyChanged;
        this.SetLayoutItem( this.Model.Root.Manager.GetLayoutItemFromModel( this.Model ) );
      }
      else
      {
        this.SetLayoutItem( null );
      }
    }

    private void Model_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
    {
      if( e.PropertyName == "IsEnabled" )
      {
        if( Model != null )
        {
          IsEnabled = Model.IsEnabled;
          if( !IsEnabled && Model.IsActive )
          {
            if( ( Model.Parent != null ) && ( Model.Parent is LayoutAnchorablePane ) )
            {
              ( ( LayoutAnchorablePane )Model.Parent ).SetNextSelectedIndex();
            }
          }
        }
      }
    }

    #endregion

    #region LayoutItem

    /// <summary>
    /// LayoutItem Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly( "LayoutItem", typeof( LayoutItem ), typeof( LayoutAnchorableControl ),
            new FrameworkPropertyMetadata( ( LayoutItem )null ) );

    public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the LayoutItem property.  This dependency property 
    /// indicates the LayoutItem attached to this tag item.
    /// </summary>
    public LayoutItem LayoutItem
    {
      get
      {
        return ( LayoutItem )GetValue( LayoutItemProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the LayoutItem property.  
    /// This dependency property indicates the LayoutItem attached to this tag item.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetLayoutItem( LayoutItem value )
    {
      this.SetValue( LayoutItemPropertyKey, value );
    }

    #endregion

    #endregion

    #region Overrides

    protected override void OnGotKeyboardFocus( System.Windows.Input.KeyboardFocusChangedEventArgs e )
    {
      var setIsActive = !( ( e.NewFocus != null ) && ( e.OldFocus != null ) && ( e.OldFocus is LayoutFloatingWindowControl ) );
      if( setIsActive )
      {
        if( Model != null )
          Model.IsActive = true;
      }      

      base.OnGotKeyboardFocus( e );
    }


    #endregion
  }
}
