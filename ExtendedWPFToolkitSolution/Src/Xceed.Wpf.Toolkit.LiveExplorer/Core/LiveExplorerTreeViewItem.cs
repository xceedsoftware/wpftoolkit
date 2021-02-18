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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Core
{
  public class LiveExplorerTreeViewItem : TreeViewItem
  {
    #region Properties

    #region IsNewFeature

    public static readonly DependencyProperty IsNewFeatureProperty = DependencyProperty.Register( "IsNewFeature", typeof( bool ), typeof( LiveExplorerTreeViewItem ), new UIPropertyMetadata( false ) );
    public bool IsNewFeature
    {
      get
      {
        return ( bool )GetValue( IsNewFeatureProperty );
      }
      set
      {
        SetValue( IsNewFeatureProperty, value );
      }
    }

    #endregion //IsNewFeature

    #region IsPlusOnlyFeature

    public static readonly DependencyProperty IsPlusOnlyFeatureProperty = DependencyProperty.Register( "IsPlusOnlyFeature", typeof( bool ), typeof( LiveExplorerTreeViewItem ), new UIPropertyMetadata( false ) );
    public bool IsPlusOnlyFeature
    {
      get
      {
        return ( bool )GetValue( IsPlusOnlyFeatureProperty );
      }
      set
      {
        SetValue( IsPlusOnlyFeatureProperty, value );
      }
    }

    #endregion //IsPlusOnlyFeature

    #region SampleType

    public static readonly DependencyProperty SampleTypeProperty = DependencyProperty.Register( "SampleType", typeof( Type ), typeof( LiveExplorerTreeViewItem ), new UIPropertyMetadata( null ) );
    public Type SampleType
    {
      get
      {
        return ( Type )GetValue( SampleTypeProperty );
      }
      set
      {
        SetValue( SampleTypeProperty, value );
      }
    }

    #endregion //Sample

    #endregion //Properties

    #region Methods

    protected override void OnMouseLeftButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      if( this.SampleType == null )
      {
        this.IsExpanded = !this.IsExpanded;
      }
      else
      {
        this.IsSelected = true;
      }
      e.Handled = true;
    }

    #endregion 

  }
}
