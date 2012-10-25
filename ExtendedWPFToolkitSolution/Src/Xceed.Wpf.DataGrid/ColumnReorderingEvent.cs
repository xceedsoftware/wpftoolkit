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

using System;
using System.Windows;

// TODO (case 117282) : Renommer ce fichier.
namespace Xceed.Wpf.DataGrid
{
  internal delegate void ColumnReorderingEventHandler( object sender, ColumnReorderingEventArgs e );

  internal class ColumnReorderingEventArgs : RoutedEventArgs
  {
    #region Constructors

    public ColumnReorderingEventArgs(RoutedEvent routedEvent, int oldVisiblePosition, int newVisiblePosition )
      : base( routedEvent )
    {
      m_oldVisiblePosition = oldVisiblePosition;
      m_newVisiblePosition = newVisiblePosition;
    } 

    #endregion

    #region OldVisiblePosition

    public int OldVisiblePosition
    {
      get
      {
        return m_oldVisiblePosition;
      }
      set
      {
        m_oldVisiblePosition = value;
      }
    }

    private int m_oldVisiblePosition;

    #endregion

    #region NewVisiblePosition

    public int NewVisiblePosition
    {
      get
      {
        return m_newVisiblePosition;
      }
      set
      {
        m_newVisiblePosition = value;
      }
    }

    private int m_newVisiblePosition;

    #endregion
  }
}
