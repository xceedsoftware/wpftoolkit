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
using System.Windows.Automation.Provider;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;

namespace Xceed.Wpf.DataGrid.Automation
{
  public class CellAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
  {
    public CellAutomationPeer( Cell owner )
      : base( owner )
    {
    }

    public new Cell Owner
    {
      get
      {
        return base.Owner as Cell;
      }
    }

    public override object GetPattern( PatternInterface patternInterface )
    {
      switch( patternInterface )
      {
        case PatternInterface.Value:
          return this;
      }

      return base.GetPattern( patternInterface );
    }

    protected override string GetClassNameCore()
    {
      return "HeaderFooterCell";
    }

    protected override string GetNameCore()
    {
      string name = AutomationProperties.GetName(this.Owner);

      if( !string.IsNullOrEmpty( name ) )
        return name;

      object title = null;
      ColumnBase column = this.Owner.ParentColumn;

      if( column != null )
        title = column.Title;

      if( title == null )
        return string.Empty;

      return string.Format( CultureInfo.CurrentCulture, "{0}", title );
    }

    private void GetTextFromVisualChildren( DependencyObject parentObject, StringBuilder text )
    {
      int count = VisualTreeHelper.GetChildrenCount( parentObject );

      for (int i = 0; i < count; i++)
			{
        DependencyObject child = VisualTreeHelper.GetChild( parentObject, i );

        TextBlock childText = child as TextBlock;

        if( childText != null && childText.IsVisible )
        {
          if( text.Length > 0 )
            text.Append( ' ' );

          text.Append( childText.Text );
        }
        else
        {
          TextBox childTextBox = child as TextBox;

          if( childTextBox != null && childTextBox.IsVisible )
          {
            if( text.Length > 0 )
              text.Append( ' ' );

            text.Append( childTextBox.Text );
          }
          else
          {
            this.GetTextFromVisualChildren( child, text );
          }
        }
			}
    }

    protected override string GetAutomationIdCore()
    {
      string automationId = null;
      automationId = base.GetAutomationIdCore();

      if( string.IsNullOrEmpty( automationId ) )
      {
        ColumnBase parentColumn = this.Owner.ParentColumn;

        if( parentColumn != null )
        {
          automationId = "Cell_" + this.Owner.ParentColumn.FieldName;
        }
        else
        {
          automationId = string.Empty;
        }
      }

      return automationId;
    }

    #region IValueProvider Members

    public bool IsReadOnly
    {
      get { return true; }
    }

    public void SetValue( string value )
    {
      throw new NotSupportedException( "Value is read only." );
    }

    public string Value
    {
      get 
      {
        // Get all the TextBlocks in our children and concatenate them.
        StringBuilder text = new StringBuilder( 256 );
        this.GetTextFromVisualChildren( this.Owner, text );
        return text.ToString();
      }
    }

    #endregion
  }
}
