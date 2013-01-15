/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace Xceed.Wpf.DataGrid.Automation
{
  internal static class AutomationPeerExtensions
  {
    internal static object GetPropertyValue( this AutomationPeer itemPeer, int id )
    {
      if( AutomationElementIdentifiers.NameProperty.Id == id )
      {
        return itemPeer.GetName();
      }
      else if( AutomationElementIdentifiers.AutomationIdProperty.Id == id )
      {
        return itemPeer.GetAutomationId();
      }
      else if( AutomationElementIdentifiers.ControlTypeProperty.Id == id )
      {
        return AutomationPeerExtensions.GetControlType( itemPeer ).Id;
      }
      else if( SelectionItemPatternIdentifiers.IsSelectedProperty.Id == id )
      {
        ISelectionItemProvider selectionItemProvider = itemPeer.GetPattern( PatternInterface.SelectionItem ) as ISelectionItemProvider;

        if( selectionItemProvider == null )
          return null;

        return selectionItemProvider.IsSelected;
      }

      return null;
    }

    private static ControlType GetControlType( AutomationPeer itemPeer )
    {
      switch( itemPeer.GetAutomationControlType() )
      {
        case AutomationControlType.Button:
          return ControlType.Button;

        case AutomationControlType.Calendar:
          return ControlType.Calendar;

        case AutomationControlType.CheckBox:
          return ControlType.CheckBox;

        case AutomationControlType.ComboBox:
          return ControlType.ComboBox;

        case AutomationControlType.Edit:
          return ControlType.Edit;

        case AutomationControlType.Hyperlink:
          return ControlType.Hyperlink;

        case AutomationControlType.Image:
          return ControlType.Image;

        case AutomationControlType.ListItem:
          return ControlType.ListItem;

        case AutomationControlType.List:
          return ControlType.List;

        case AutomationControlType.Menu:
          return ControlType.Menu;

        case AutomationControlType.MenuBar:
          return ControlType.MenuBar;

        case AutomationControlType.MenuItem:
          return ControlType.MenuItem;

        case AutomationControlType.ProgressBar:
          return ControlType.ProgressBar;

        case AutomationControlType.RadioButton:
          return ControlType.RadioButton;

        case AutomationControlType.ScrollBar:
          return ControlType.ScrollBar;

        case AutomationControlType.Slider:
          return ControlType.Slider;

        case AutomationControlType.Spinner:
          return ControlType.Spinner;

        case AutomationControlType.StatusBar:
          return ControlType.StatusBar;

        case AutomationControlType.Tab:
          return ControlType.Tab;

        case AutomationControlType.TabItem:
          return ControlType.TabItem;

        case AutomationControlType.Text:
          return ControlType.Text;

        case AutomationControlType.ToolBar:
          return ControlType.ToolBar;

        case AutomationControlType.ToolTip:
          return ControlType.ToolTip;

        case AutomationControlType.Tree:
          return ControlType.Tree;

        case AutomationControlType.TreeItem:
          return ControlType.TreeItem;

        case AutomationControlType.Custom:
          return ControlType.Custom;

        case AutomationControlType.Group:
          return ControlType.Group;

        case AutomationControlType.Thumb:
          return ControlType.Thumb;

        case AutomationControlType.DataGrid:
          return ControlType.DataGrid;

        case AutomationControlType.DataItem:
          return ControlType.DataItem;

        case AutomationControlType.Document:
          return ControlType.Document;

        case AutomationControlType.SplitButton:
          return ControlType.SplitButton;

        case AutomationControlType.Window:
          return ControlType.Window;

        case AutomationControlType.Pane:
          return ControlType.Pane;

        case AutomationControlType.Header:
          return ControlType.Header;

        case AutomationControlType.HeaderItem:
          return ControlType.HeaderItem;

        case AutomationControlType.Table:
          return ControlType.Table;

        case AutomationControlType.TitleBar:
          return ControlType.TitleBar;

        case AutomationControlType.Separator:
          return ControlType.Separator;
      }

      return null;
    }
  }
}
