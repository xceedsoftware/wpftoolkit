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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Primitives;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_Popup, Type = typeof( Popup ) )]
  public class CheckComboBox : Xceed.Wpf.Toolkit.Primitives.Selector
  {
    private const string PART_Popup = "PART_Popup";

    #region Members

    private ValueChangeHelper _displayMemberPathValuesChangeHelper;
    private bool _ignoreTextValueChanged;
    private Popup _popup;
    private List<object> _initialValue = new List<object>();

    #endregion

    #region Constructors

    static CheckComboBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( CheckComboBox ), new FrameworkPropertyMetadata( typeof( CheckComboBox ) ) );
    }

    public CheckComboBox()
    {
      Keyboard.AddKeyDownHandler( this, OnKeyDown );
      Mouse.AddPreviewMouseDownOutsideCapturedElementHandler( this, OnMouseDownOutsideCapturedElement );
      _displayMemberPathValuesChangeHelper = new ValueChangeHelper( this.OnDisplayMemberPathValuesChanged );
    }

    #endregion //Constructors

    #region Properties

    #region IsEditable

    public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register( "IsEditable", typeof( bool ), typeof( CheckComboBox )
      , new UIPropertyMetadata( false ) );
    public bool IsEditable
    {
      get
      {
        return (bool)GetValue( IsEditableProperty );
      }
      set
      {
        SetValue( IsEditableProperty, value );
      }
    }

    #endregion

    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( CheckComboBox )
      , new UIPropertyMetadata( null, OnTextChanged ) );
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

    private static void OnTextChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var checkComboBox = o as CheckComboBox;
      if( checkComboBox != null )
        checkComboBox.OnTextChanged( (string)e.OldValue, (string)e.NewValue );
    }

    protected virtual void OnTextChanged( string oldValue, string newValue )
    {
      if( !this.IsInitialized || _ignoreTextValueChanged )
        return;

      this.UpdateFromText();
    }

    #endregion

    #region IsDropDownOpen

    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register( "IsDropDownOpen", typeof( bool ), typeof( CheckComboBox ), new UIPropertyMetadata( false, OnIsDropDownOpenChanged ) );
    public bool IsDropDownOpen
    {
      get
      {
        return ( bool )GetValue( IsDropDownOpenProperty );
      }
      set
      {
        SetValue( IsDropDownOpenProperty, value );
      }
    }

    private static void OnIsDropDownOpenChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      CheckComboBox comboBox = o as CheckComboBox;
      if( comboBox != null )
        comboBox.OnIsDropDownOpenChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsDropDownOpenChanged( bool oldValue, bool newValue )
    {
      if( newValue )
      {
        _initialValue.Clear();
        foreach( object o in SelectedItems )
          _initialValue.Add( o );
      }
      else
      {
        _initialValue.Clear();
      }

      // TODO: Add your property changed side-effects. Descendants can override as well.
    }

    #endregion //IsDropDownOpen

    #region MaxDropDownHeight

    public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register( "MaxDropDownHeight", typeof( double ), typeof( CheckComboBox ), new UIPropertyMetadata( SystemParameters.PrimaryScreenHeight / 3.0, OnMaxDropDownHeightChanged ) );
    public double MaxDropDownHeight
    {
      get
      {
        return ( double )GetValue( MaxDropDownHeightProperty );
      }
      set
      {
        SetValue( MaxDropDownHeightProperty, value );
      }
    }

    private static void OnMaxDropDownHeightChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      CheckComboBox comboBox = o as CheckComboBox;
      if( comboBox != null )
        comboBox.OnMaxDropDownHeightChanged( ( double )e.OldValue, ( double )e.NewValue );
    }

    protected virtual void OnMaxDropDownHeightChanged( double oldValue, double newValue )
    {
      // TODO: Add your property changed side-effects. Descendants can override as well.
    }

    #endregion

    #endregion //Properties

    #region Base Class Overrides

    protected override void OnSelectedValueChanged( string oldValue, string newValue )
    {
      base.OnSelectedValueChanged( oldValue, newValue );
      UpdateText();
    }

    protected override void OnDisplayMemberPathChanged( string oldDisplayMemberPath, string newDisplayMemberPath )
    {
      base.OnDisplayMemberPathChanged( oldDisplayMemberPath, newDisplayMemberPath );
      this.UpdateDisplayMemberPathValuesBindings();
    }

    protected override void OnItemsSourceChanged( System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue )
    {
      base.OnItemsSourceChanged( oldValue, newValue );
      this.UpdateDisplayMemberPathValuesBindings();
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _popup != null )
        _popup.Opened -= Popup_Opened;

      _popup = GetTemplateChild( PART_Popup ) as Popup;

      if( _popup != null )
        _popup.Opened += Popup_Opened;
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    private void OnMouseDownOutsideCapturedElement( object sender, MouseButtonEventArgs e )
    {
      CloseDropDown( true );
    }

    private void OnKeyDown( object sender, KeyEventArgs e )
    {
      if( !IsDropDownOpen )
      {
        if( KeyboardUtilities.IsKeyModifyingPopupState( e ) )
        {
          IsDropDownOpen = true;
          // Popup_Opened() will Focus on ComboBoxItem.
          e.Handled = true;
        }
      }
      else
      {
        if( KeyboardUtilities.IsKeyModifyingPopupState( e ) )
        {
          CloseDropDown( true );
          e.Handled = true;
        }
        else if( e.Key == Key.Enter )
        {
          CloseDropDown( true );
          e.Handled = true;
        }
        else if( e.Key == Key.Escape )
        {
          SelectedItems.Clear();
          foreach( object o in _initialValue )
            SelectedItems.Add( o );
          CloseDropDown( true );
          e.Handled = true;
        }
      }
    }

    private void Popup_Opened( object sender, EventArgs e )
    {
      UIElement item = ItemContainerGenerator.ContainerFromItem( SelectedItem ) as UIElement;
      if( (item == null) && (Items.Count > 0) )
        item = ItemContainerGenerator.ContainerFromItem( Items[0] ) as UIElement;
      if( item != null )
        item.Focus();
    }

    #endregion //Event Handlers

    #region Methods

    private void UpdateDisplayMemberPathValuesBindings()
    {
      _displayMemberPathValuesChangeHelper.UpdateValueSource( ItemsCollection, this.DisplayMemberPath );
    }

    private void OnDisplayMemberPathValuesChanged()
    {
      this.UpdateText();
    }

    private void UpdateText()
    {
#if VS2008
      string newValue = String.Join( Delimiter, SelectedItems.Cast<object>().Select( x => GetItemDisplayValue( x ).ToString() ).ToArray() ); 
#else
      string newValue = String.Join( Delimiter, SelectedItems.Cast<object>().Select( x => GetItemDisplayValue( x ) ) );
#endif

      if( String.IsNullOrEmpty( Text ) || !Text.Equals( newValue ) )
      {
        _ignoreTextValueChanged = true;
        Text = newValue;
        _ignoreTextValueChanged = false;
      }
    }

    /// <summary>
    /// Updates the SelectedItems collection based on the content of
    /// the Text property.
    /// </summary>
    private void UpdateFromText()
    {
      List<string> selectedValues = null;
      if( !String.IsNullOrEmpty( this.Text ) )
      {
        selectedValues = this.Text.Replace( " ", string.Empty ).Split( new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries ).ToList();
      }

      this.UpdateFromList( selectedValues, this.GetItemDisplayValue );
    }

    protected object GetItemDisplayValue( object item )
    {
      if( !String.IsNullOrEmpty( DisplayMemberPath ) )
      {
        var property = item.GetType().GetProperty( DisplayMemberPath );
        if( property != null )
          return property.GetValue( item, null );
      }

      return item;
    }

    private void CloseDropDown( bool isFocusOnComboBox )
    {
      if( IsDropDownOpen )
        IsDropDownOpen = false;
      ReleaseMouseCapture();

      if( isFocusOnComboBox )
        Focus();
    }

    #endregion //Methods
  }
}
