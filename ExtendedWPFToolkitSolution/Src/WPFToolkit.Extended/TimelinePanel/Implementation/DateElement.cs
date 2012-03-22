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
using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  internal sealed class DateElement : IComparable<DateElement>
  {
    #region Members

    private readonly int _originalIndex;

    #endregion //Members

    #region Fields

    public readonly UIElement Element;
    public readonly DateTime Date;
    public readonly DateTime DateEnd;
    public Rect PlacementRectangle;

    public int Column = 0;
    public int ColumnSpan = 1;

    #endregion //Fields

    #region Constructors

    internal DateElement( UIElement element, DateTime date, DateTime dateEnd )
      : this( element, date, dateEnd, -1 )
    {
    }

    internal DateElement( UIElement element, DateTime date, DateTime dateEnd, int originalIndex )
    {
      Element = element;
      Date = date;
      DateEnd = dateEnd;
      _originalIndex = originalIndex;
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override string ToString()
    {
      var fe = Element as FrameworkElement;
      if( fe == null )
        return base.ToString();

      if( fe.Tag != null )
        return fe.Tag.ToString();

      return fe.Name;
    }

    #endregion //Base Class Overrides

    #region Methods

    public int CompareTo( DateElement d )
    {
      int dateCompare = Date.CompareTo( d.Date );
      if( dateCompare != 0 )
        return dateCompare;

      if( _originalIndex >= 0 )
        return ( _originalIndex < d._originalIndex ) ? -1 : 1;

      return -DateEnd.CompareTo( d.DateEnd );
    }

    #endregion
  }
}
