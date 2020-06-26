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

namespace Xceed.Wpf.Toolkit
{
  internal class DateTimeInfo
  {
    public string Content
    {
      get;
      set;
    }
    public string Format
    {
      get;
      set;
    }
    public bool IsReadOnly
    {
      get;
      set;
    }
    public int Length
    {
      get;
      set;
    }
    public int StartPosition
    {
      get;
      set;
    }
    public DateTimePart Type
    {
      get;
      set;
    }
  }
}
