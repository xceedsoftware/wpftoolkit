/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

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