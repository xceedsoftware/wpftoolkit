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
using System.Xml.Serialization;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [Serializable]
  public abstract class LayoutGroupBase : LayoutElement
  {
    #region Internal Methods

    protected virtual void OnChildrenCollectionChanged()
    {
      if( ChildrenCollectionChanged != null )
        ChildrenCollectionChanged( this, EventArgs.Empty );
    }

    protected void NotifyChildrenTreeChanged( ChildrenTreeChange change )
    {
      OnChildrenTreeChanged( change );
      var parentGroup = Parent as LayoutGroupBase;
      if( parentGroup != null )
        parentGroup.NotifyChildrenTreeChanged( ChildrenTreeChange.TreeChanged );
    }

    protected virtual void OnChildrenTreeChanged( ChildrenTreeChange change )
    {
      if( ChildrenTreeChanged != null )
        ChildrenTreeChanged( this, new ChildrenTreeChangedEventArgs( change ) );
    }

    #endregion

    #region Events

    [field: NonSerialized]
    [field: XmlIgnore]
    public event EventHandler ChildrenCollectionChanged;

    [field: NonSerialized]
    [field: XmlIgnore]
    public event EventHandler<ChildrenTreeChangedEventArgs> ChildrenTreeChanged;

    #endregion
  }
}
