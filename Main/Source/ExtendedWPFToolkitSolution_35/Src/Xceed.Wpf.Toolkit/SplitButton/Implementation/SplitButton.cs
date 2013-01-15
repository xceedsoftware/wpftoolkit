/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

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

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      Button = GetTemplateChild( PART_ActionButton ) as Button;
    }

    #endregion //Base Class Overrides
  }
}
