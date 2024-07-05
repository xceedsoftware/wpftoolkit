/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

/*************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.Toolkit.LiveExplorer.Samples.AvalonDock.Resources;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.AvalonDock.Views
{
  public partial class AvalonDockSourcesView : DemoView
  {
    public AvalonDockSourcesView()
    {
      InitializeComponent();

      this.Documents = new ObservableCollection<DocumentBase>()
      {
        new Resources.Page(){ Title = "Page" },
        new Resources.Document(){ Title = "Document" },
        new Resources.Note(){ Title = "Notes" },
      };

      this.Anchorables = new ObservableCollection<Control>()
      {
        new Label() { Content = "Tools" },
        new Label() { Content = "Settings" }
      };

      this.DataContext = this;
    }

    public ObservableCollection<DocumentBase> Documents
    {
      get; private set;
    }

    public ObservableCollection<Control> Anchorables
    {
      get; private set;
    }
  }

  // Custom LayoutUpdateStrategy class to call when AvalonDock needs to position an anchorable or a document inside an existing layout model.
  public class LayoutManager : ILayoutUpdateStrategy
  {
    // 	Performs actions before an anchorable is to be inserted.
    public bool BeforeInsertAnchorable( LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer )
    {
      return false;
    }

    // 	Performs actions after an anchorable has been inserted.  
    public void AfterInsertAnchorable( LayoutRoot layout, LayoutAnchorable anchorableShown )
    {
    }

    // Performs actions before a document is to be inserted.  
    public bool BeforeInsertDocument( LayoutRoot layout, LayoutDocument documentToShow, ILayoutContainer destinationContainer )
    {
      var documentPanes = layout.Descendents().OfType<LayoutDocumentPane>().ToList();
      if( documentPanes.Count > 0 )
        documentPanes[ 0 ].Children.Add( documentToShow );
      return true;
    }

    // Performs actions after a document has been inserted.  
    public void AfterInsertDocument( LayoutRoot layout, LayoutDocument anchorableShown )
    {
    }
  }

  // StyleSelector Class that allows to apply a Style for a LayoutDocument/LayoutAnchorable.
  public class ContainerStyleSelector : StyleSelector
  {
    public Style LayoutItemStyle
    {
      get; set;
    }

    public Style LayoutAnchorableItemStyle
    {
      get; set;
    }

    public override System.Windows.Style SelectStyle( object item, System.Windows.DependencyObject container )
    {
      if( item is Label )
        return LayoutAnchorableItemStyle;

      if( item is DocumentBase )
        return LayoutItemStyle;

      return base.SelectStyle( item, container );
    }

  }
}
