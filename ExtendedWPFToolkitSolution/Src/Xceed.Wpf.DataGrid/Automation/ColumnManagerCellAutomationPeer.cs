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

namespace Xceed.Wpf.DataGrid.Automation
{
  public class ColumnManagerCellAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider, ITransformProvider
  {
    public ColumnManagerCellAutomationPeer( ColumnManagerCell owner )
      : base( owner )
    {
    }

    public new ColumnManagerCell Owner
    {
      get
      {
        return base.Owner as ColumnManagerCell;
      }
    }

    public override object GetPattern( PatternInterface patternInterface )
    {
      switch( patternInterface )
      {
        case PatternInterface.Invoke:
        case PatternInterface.Transform:
          return this;
      }

      return base.GetPattern( patternInterface );
    }

    protected override string GetClassNameCore()
    {
      return "ColumnManagerCell";
    }

    protected override bool IsContentElementCore()
    {
      return false;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
      return AutomationControlType.HeaderItem;
    }

    protected override string GetNameCore()
    {
      ColumnBase parentColumn = this.Owner.ParentColumn;

      if( parentColumn == null )
        return string.Empty;

      object title = this.Owner.ParentColumn.Title;

      if( title == null )
        return string.Empty;

      return string.Format( CultureInfo.CurrentCulture, "{0}", title );
    }

    protected override string GetAutomationIdCore()
    {
      string automationId = null;
      automationId = base.GetAutomationIdCore();

      if( string.IsNullOrEmpty( automationId ) )
      {
        ColumnBase parentColumn = this.Owner.ParentColumn;

        if( parentColumn == null )
          return string.Empty;

        return "Cell_" + parentColumn.FieldName;
      }

      return string.Empty;
    }

    #region IInvokeProvider Members

    void IInvokeProvider.Invoke()
    {
      this.Owner.DoSort( true );
    }

    #endregion IInvokeProvider Members

    #region ITransformProvider Members

    bool ITransformProvider.CanMove
    {
      get
      {
        return false;
      }
    }

    bool ITransformProvider.CanResize
    {
      get
      {
        return true;
      }
    }

    bool ITransformProvider.CanRotate
    {
      get
      {
        return false;
      }
    }

    void ITransformProvider.Move( double x, double y )
    {
      return;
    }

    void ITransformProvider.Resize( double width, double height )
    {
      this.Owner.DoResize( width );
    }

    void ITransformProvider.Rotate( double degrees )
    {
      return;
    }

    #endregion ITransformProvider
  }
}
