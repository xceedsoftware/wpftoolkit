/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
  public class CheckComboBox : Selector
  {
    #region Constructors

    static CheckComboBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( CheckComboBox ), new FrameworkPropertyMetadata( typeof( CheckComboBox ) ) );
    }

    public CheckComboBox()
    {
      Mouse.AddPreviewMouseDownOutsideCapturedElementHandler( this, OnMouseDownOutsideCapturedElement );
    }

    #endregion //Constructors

    #region Properties

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( CheckComboBox ), new UIPropertyMetadata( null ) );
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
      // TODO: Add your property changed side-effects. Descendants can override as well.
    }

    #endregion //IsDropDownOpen

    #endregion //Properties

    #region Base Class Overrides

    protected override void OnSelectedValueChanged( string oldValue, string newValue )
    {
      base.OnSelectedValueChanged( oldValue, newValue );
      UpdateText();
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    private void OnMouseDownOutsideCapturedElement( object sender, MouseButtonEventArgs e )
    {
      CloseDropDown();
    }

    #endregion //Event Handlers

    #region Methods

    private void UpdateText()
    {
#if VS2008
      string newValue = String.Join( Delimiter, SelectedItems.Cast<object>().Select( x => GetItemDisplayValue( x ).ToString() ).ToArray() ); 
#else
      string newValue = String.Join( Delimiter, SelectedItems.Cast<object>().Select( x => GetItemDisplayValue( x ) ) );
#endif

      if( String.IsNullOrEmpty( Text ) || !Text.Equals( newValue ) )
        Text = newValue;
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

    private void CloseDropDown()
    {
      if( IsDropDownOpen )
        IsDropDownOpen = false;
      ReleaseMouseCapture();
    }

    #endregion //Methods
  }
}
