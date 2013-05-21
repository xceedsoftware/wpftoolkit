/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

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
