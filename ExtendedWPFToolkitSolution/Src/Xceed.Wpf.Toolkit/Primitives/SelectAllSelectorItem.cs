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


namespace Xceed.Wpf.Toolkit.Primitives
{
  public class SelectAllSelectorItem : SelectorItem
  {
    #region Members

    private bool _ignoreSelectorChanges = false;

    #endregion

    #region Overrides

    // Do not raise an event when this item is Selected/UnSelected.
    protected override void OnIsSelectedChanged( bool? oldValue, bool? newValue )
    {
      if( _ignoreSelectorChanges )
        return;

      var templatedParent = this.TemplatedParent as SelectAllSelector;
      if( templatedParent != null )
      {
        if( newValue.HasValue )
        {
          // Select All
          if( newValue.Value )
          {
            templatedParent.SelectAll();
          }
          // UnSelect All
          else
          {
            templatedParent.UnSelectAll();
          }
        }
      }
    }

    #endregion

    #region Internal Methods

    internal void ModifyCurrentSelection( bool? newSelection )
    {
      _ignoreSelectorChanges = true;
      this.IsSelected = newSelection;
      _ignoreSelectorChanges = false;
    }

    #endregion
  }
}
