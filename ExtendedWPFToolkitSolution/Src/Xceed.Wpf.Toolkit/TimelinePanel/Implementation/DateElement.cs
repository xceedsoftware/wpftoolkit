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

using System;
using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  public sealed class DateElement : IComparable<DateElement>
  {
    #region Members

    internal Rect PlacementRectangle;

    private readonly int _originalIndex;

    #endregion //Members

    #region Properties

    #region Date

    public DateTime Date
    {
      get
      {
        return _date;
      }
    }

    private readonly DateTime _date;

    #endregion

    #region DateEnd

    public DateTime DateEnd
    {
      get
      {
        return _dateEnd;
      }
    }

    private readonly DateTime _dateEnd;

    #endregion

    #region Element

    public UIElement Element
    {
      get
      {
        return _element;
      }
    }

    private readonly UIElement _element;

    #endregion

    #endregion

    #region Constructors

    internal DateElement( UIElement element, DateTime date, DateTime dateEnd )
      : this( element, date, dateEnd, -1 )
    {
    }

    internal DateElement( UIElement element, DateTime date, DateTime dateEnd, int originalIndex )
    {
      _element = element;
      _date = date;
      _dateEnd = dateEnd;
      _originalIndex = originalIndex;
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override string ToString()
    {
      var fe = this.Element as FrameworkElement;
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
      int dateCompare = this.Date.CompareTo( d.Date );
      if( dateCompare != 0 )
        return dateCompare;

      if( _originalIndex >= 0 )
        return ( _originalIndex < d._originalIndex ) ? -1 : 1;

      return -this.DateEnd.CompareTo( d.DateEnd );
    }

    #endregion
  }
}
