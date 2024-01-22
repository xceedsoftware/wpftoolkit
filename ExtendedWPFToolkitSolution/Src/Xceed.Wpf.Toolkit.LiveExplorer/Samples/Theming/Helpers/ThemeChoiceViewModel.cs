/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Theming.Helpers
{
  internal class ThemeChoiceViewModel
  {
    private bool m_isSelected = false;
    private Action<ThemeChoiceViewModel> m_actionOnSelect;
    private bool m_isPlus;

    public ThemeChoiceViewModel()
    {
    }

    public string DisplayName { get; set; }

    public string BaseName { get; set; }

    public bool IsDark { get; set; }

    public bool IsPlus
    {
      get
      {
        return m_isPlus;
      }
      set
      {
        m_isPlus = value;
        this.IsPlusVisibility = m_isPlus ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility IsPlusVisibility { get; set; } = Visibility.Collapsed;

    public bool IsSelected
    {
      get
      {
        return m_isSelected;
      }
      set
      {
        var wasSelected = m_isSelected;
        m_isSelected = value;
        if( !wasSelected && m_isSelected )
        {
          this.OnSelected();
        }
      }
    }

    public event Action<ThemeChoiceViewModel> Selected;

    public Action<ThemeChoiceViewModel> ActionOnSelected
    {
      set
      {
        m_actionOnSelect = value;
      }
    }

    protected virtual void OnSelected()
    {
      m_actionOnSelect?.Invoke( this );

      if( this.Selected != null )
      {
        this.Selected( this );
      }
    }
  }

}
