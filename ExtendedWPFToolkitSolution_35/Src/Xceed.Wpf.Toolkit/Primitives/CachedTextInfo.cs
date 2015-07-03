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
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit.Primitives
{
  internal class CachedTextInfo : ICloneable
  {
    private CachedTextInfo( string text, int caretIndex, int selectionStart, int selectionLength )
    {
      this.Text = text;
      this.CaretIndex = caretIndex;
      this.SelectionStart = selectionStart;
      this.SelectionLength = selectionLength;
    }

    public CachedTextInfo( TextBox textBox )
      : this( textBox.Text, textBox.CaretIndex, textBox.SelectionStart, textBox.SelectionLength )
    {
    }

    public string Text { get; private set; }
    public int CaretIndex { get; private set; }
    public int SelectionStart { get; private set; }
    public int SelectionLength { get; private set; }

    #region ICloneable Members

    public object Clone()
    {
      return new CachedTextInfo( this.Text, this.CaretIndex, this.SelectionStart, this.SelectionLength );
    }

    #endregion
  }
}
