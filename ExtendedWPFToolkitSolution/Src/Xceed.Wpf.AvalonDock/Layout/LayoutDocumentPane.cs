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
using System.Windows.Markup;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [ContentProperty( "Children" )]
  [Serializable]
  public class LayoutDocumentPane : LayoutPositionableGroup<LayoutContent>, ILayoutDocumentPane, ILayoutPositionableElement, ILayoutContentSelector, ILayoutPaneSerializable
  {
    #region Constructors

    public LayoutDocumentPane()
    {
    }
    public LayoutDocumentPane( LayoutContent firstChild )
    {
      this.Children.Add( firstChild );
    }

    #endregion

    #region Properties

    #region ShowHeader

    private bool _showHeader = true;
    public bool ShowHeader
    {
      get
      {
        return _showHeader;
      }
      set
      {
        if( value != _showHeader )
        {
          _showHeader = value;
          RaisePropertyChanged( "ShowHeader" );
        }
      }
    }

    #endregion

    #region SelectedContentIndex

    private int _selectedIndex = -1;
    public int SelectedContentIndex
    {
      get
      {
        return _selectedIndex;
      }
      set
      {
        if( value < 0 ||
            value >= Children.Count )
          value = -1;

        if( _selectedIndex != value )
        {
          RaisePropertyChanging( "SelectedContentIndex" );
          RaisePropertyChanging( "SelectedContent" );
          if( _selectedIndex >= 0 &&
              _selectedIndex < Children.Count )
            Children[ _selectedIndex ].IsSelected = false;

          _selectedIndex = value;

          if( _selectedIndex >= 0 &&
              _selectedIndex < Children.Count )
            Children[ _selectedIndex ].IsSelected = true;

          RaisePropertyChanged( "SelectedContentIndex" );
          RaisePropertyChanged( "SelectedContent" );
        }
      }
    }

    #endregion

    #region SelectedContent

    public LayoutContent SelectedContent
    {
      get
      {
        return _selectedIndex == -1 ? null : Children[ _selectedIndex ];
      }
    }

    #endregion

    #region ChildrenSorted

    public IEnumerable<LayoutContent> ChildrenSorted
    {
      get
      {
        var listSorted = this.Children.ToList();
        listSorted.Sort();
        return listSorted;
      }
    }

    #endregion

    #endregion

    #region Overrides

    protected override bool GetVisibility()
    {
      if( this.Parent is LayoutDocumentPaneGroup )
        return ( this.ChildrenCount > 0 ) && this.Children.Any( c => ( c is LayoutDocument && ( ( LayoutDocument )c ).IsVisible ) || ( c is LayoutAnchorable ) );

      return true;
    }

    protected override void ChildMoved( int oldIndex, int newIndex )
    {
      if( _selectedIndex == oldIndex )
      {
        RaisePropertyChanging( "SelectedContentIndex" );
        _selectedIndex = newIndex;
        RaisePropertyChanged( "SelectedContentIndex" );
      }


      base.ChildMoved( oldIndex, newIndex );
    }

    protected override void OnChildrenCollectionChanged()
    {
      if( this.SelectedContentIndex >= this.ChildrenCount )
        this.SelectedContentIndex = this.Children.Count - 1;
      if( this.SelectedContentIndex == -1 )
      {
        if( this.ChildrenCount > 0 )
        {
          if( this.Root == null )
          {
            this.SetNextSelectedIndex();
          }
          else
          {
            var childrenToSelect = this.Children.OrderByDescending( c => c.LastActivationTimeStamp.GetValueOrDefault() ).First();
            this.SelectedContentIndex = this.Children.IndexOf( childrenToSelect );
            childrenToSelect.IsActive = true;
          }
        }
        else
        {
          if( this.Root != null )
          {
            this.Root.ActiveContent = null;
          }
        }
      }

      base.OnChildrenCollectionChanged();

      RaisePropertyChanged( "ChildrenSorted" );
    }

    protected override void OnIsVisibleChanged()
    {
      this.UpdateParentVisibility();
      base.OnIsVisibleChanged();
    }

    public override void WriteXml( System.Xml.XmlWriter writer )
    {
      if( _id != null )
        writer.WriteAttributeString( "Id", _id );
      if( !_showHeader )
        writer.WriteAttributeString( "ShowHeader", _showHeader.ToString() );

      base.WriteXml( writer );
    }

    public override void ReadXml( System.Xml.XmlReader reader )
    {
      if( reader.MoveToAttribute( "Id" ) )
        _id = reader.Value;
      if( reader.MoveToAttribute( "ShowHeader" ) )
        _showHeader = bool.Parse( reader.Value );


      base.ReadXml( reader );
    }


#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "DocumentPane()" );

          foreach (LayoutElement child in Children)
              child.ConsoleDump(tab + 1);
        }
#endif

    #endregion

    #region Public Methods

    public int IndexOf( LayoutContent content )
    {
      return Children.IndexOf( content );
    }

    #endregion

    #region Internal Methods

    internal void SetNextSelectedIndex()
    {
      this.SelectedContentIndex = -1;
      for( int i = 0; i < this.Children.Count; ++i )
      {
        if( this.Children[ i ].IsEnabled )
        {
          this.SelectedContentIndex = i;
          return;
        }
      }
    }

    #endregion

    #region Private Methods

    private void UpdateParentVisibility()
    {
      var parentPane = this.Parent as ILayoutElementWithVisibility;
      if( parentPane != null )
        parentPane.ComputeVisibility();
    }

    #endregion

    #region ILayoutPaneSerializable Interface

    string _id;
    string ILayoutPaneSerializable.Id
    {
      get
      {
        return _id;
      }
      set
      {
        _id = value;
      }
    }

    #endregion
  }
}
