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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Xceed.Wpf.Toolkit
{
  public class AutoCompletingMaskEventArgs : CancelEventArgs
  {
    public AutoCompletingMaskEventArgs( MaskedTextProvider maskedTextProvider, int startPosition, int selectionLength, string input )
    {
      m_autoCompleteStartPosition = -1;

      m_maskedTextProvider = maskedTextProvider;
      m_startPosition = startPosition;
      m_selectionLength = selectionLength;
      m_input = input;
    }

    #region MaskedTextProvider PROPERTY

    private MaskedTextProvider m_maskedTextProvider;  

    public MaskedTextProvider MaskedTextProvider
    {
      get { return m_maskedTextProvider; }
    }

    #endregion MaskedTextProvider PROPERTY

    #region StartPosition PROPERTY

    private int m_startPosition;

    public int StartPosition
    {
      get { return m_startPosition; }
    }

    #endregion StartPosition PROPERTY

    #region SelectionLength PROPERTY

    private int m_selectionLength;

    public int SelectionLength
    {
      get { return m_selectionLength; }
    }

    #endregion SelectionLength PROPERTY

    #region Input PROPERTY

    private string m_input;

    public string Input
    {
      get { return m_input; }
    }

    #endregion Input PROPERTY


    #region AutoCompleteStartPosition PROPERTY

    private int m_autoCompleteStartPosition;

    public int AutoCompleteStartPosition
    {
      get { return m_autoCompleteStartPosition; }
      set { m_autoCompleteStartPosition = value; }
    }

    #endregion AutoCompleteStartPosition PROPERTY

    #region AutoCompleteText PROPERTY

    private string m_autoCompleteText;

    public string AutoCompleteText
    {
      get { return m_autoCompleteText; }
      set { m_autoCompleteText = value; }
    }

    #endregion AutoCompleteText PROPERTY
  }
}
