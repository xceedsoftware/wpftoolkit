/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart(Name = PART_FilterBox, Type = typeof(FilterBox))]
  public class CheckListBox : Selector
  {
    private const string PART_FilterBox = "PART_FilterBox";

    #region Members

    private FilterBox _filterBox;
    private bool _allowFilter;

    #endregion

    #region AllowFilter
    public static readonly DependencyProperty AllowFilterProperty = DependencyProperty.Register(
      "AllowFilter",
      typeof(bool),
      typeof(CheckListBox),
      new UIPropertyMetadata(true, AllowFilterChanged));

    private static void AllowFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var selector = d as CheckListBox;
      if (selector != null)
        selector.AllowFilterChanged((bool)e.NewValue);
    }

    protected void AllowFilterChanged(bool value)
    {
      _allowFilter = value;
      if (_filterBox != null)
      {
        SetFilter(value);
      }
    }

    public bool AllowFilter
    {
      get
      {
        return (bool)GetValue(AllowFilterProperty);
      }
      set
      {
        SetValue(AllowFilterProperty, value);
      }
    }

    private void SetFilter(bool value)
    {
      _filterBox.Clear(false);
      _filterBox.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }
    #endregion //AllowFilter

    #region Constructors

    static CheckListBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( CheckListBox ), new FrameworkPropertyMetadata( typeof( CheckListBox ) ) );
    }

    public CheckListBox()
    {

    }

    #endregion //Constructors

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _filterBox = GetTemplateChild(PART_FilterBox) as FilterBox;
      SetFilter(_allowFilter);
    }
  }
}
