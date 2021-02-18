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

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_ActionButton, Type = typeof( Button ) )]
  public class SplitButton : DropDownButton
  {
    private const string PART_ActionButton = "PART_ActionButton";

    #region Constructors

    static SplitButton()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( SplitButton ), new FrameworkPropertyMetadata( typeof( SplitButton ) ) );
    }

    #endregion //Constructors

    #region Properties

    #region DropDownContent

    public static readonly DependencyProperty DropDownTooltipProperty = DependencyProperty.Register( "DropDownTooltip", typeof( object ), typeof( SplitButton ), new UIPropertyMetadata( null, OnDropDownTooltipChanged ) );
    public object DropDownTooltip
    {
      get
      {
        return ( object )GetValue( DropDownTooltipProperty );
      }
      set
      {
        SetValue( DropDownTooltipProperty, value );
      }
    }

    private static void OnDropDownTooltipChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var splitButton = o as SplitButton;
      if( splitButton != null )
        splitButton.OnDropDownTooltipChanged( ( object )e.OldValue, ( object )e.NewValue );
    }

    protected virtual void OnDropDownTooltipChanged( object oldValue, object newValue )
    {
      // TODO: Add your property changed side-effects. Descendants can override as well.
    }

    #endregion //DropDownTooltip

    #endregion

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      Button = GetTemplateChild( PART_ActionButton ) as Button;
    }


  #endregion //Base Class Overrides
  }
}
