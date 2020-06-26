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

using System.Windows.Input;

namespace Xceed.Wpf.Toolkit
{
  public static class WizardCommands
  {

    private static RoutedCommand _cancelCommand = new RoutedCommand();
    public static RoutedCommand Cancel
    {
      get
      {
        return _cancelCommand;
      }
    }

    private static RoutedCommand _finishCommand = new RoutedCommand();
    public static RoutedCommand Finish
    {
      get
      {
        return _finishCommand;
      }
    }

    private static RoutedCommand _helpCommand = new RoutedCommand();
    public static RoutedCommand Help
    {
      get
      {
        return _helpCommand;
      }
    }

    private static RoutedCommand _nextPageCommand = new RoutedCommand();
    public static RoutedCommand NextPage
    {
      get
      {
        return _nextPageCommand;
      }
    }

    private static RoutedCommand _previousPageCommand = new RoutedCommand();
    public static RoutedCommand PreviousPage
    {
      get
      {
        return _previousPageCommand;
      }
    }

    private static RoutedCommand _selectPageCommand = new RoutedCommand();
    public static RoutedCommand SelectPage
    {
      get
      {
        return _selectPageCommand;
      }
    }
  }
}
