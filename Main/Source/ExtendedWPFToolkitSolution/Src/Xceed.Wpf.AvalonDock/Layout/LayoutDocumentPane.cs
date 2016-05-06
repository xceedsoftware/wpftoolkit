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
using System.Collections.ObjectModel;
using System.Windows.Markup;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [ContentProperty( "Children" )]
  [Serializable]
  public class LayoutDocumentPane : LayoutPositionableGroup<LayoutContent>, ILayoutDocumentPane, ILayoutPositionableElement, ILayoutContentSelector, ILayoutPaneSerializable
  {
    public LayoutDocumentPane()
    {
    }
    public LayoutDocumentPane( LayoutContent firstChild )
    {
      Children.Add( firstChild );
    }

    protected override bool GetVisibility()
    {
      if( Parent is LayoutDocumentPaneGroup )
        return ChildrenCount > 0;

      return true;
    }

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

    public LayoutContent SelectedContent
    {
      get
      {
        return _selectedIndex == -1 ? null : Children[ _selectedIndex ];
      }
    }
    #endregion

    protected override void OnChildrenCollectionChanged()
    {
      if( SelectedContentIndex >= ChildrenCount )
        SelectedContentIndex = Children.Count - 1;
      if( SelectedContentIndex == -1 && ChildrenCount > 0 )
      {
        if( Root == null )
        {
          SetNextSelectedIndex();
        }
        else
        {
          var childrenToSelect = Children.OrderByDescending( c => c.LastActivationTimeStamp.GetValueOrDefault() ).First();
          SelectedContentIndex = Children.IndexOf( childrenToSelect );
          childrenToSelect.IsActive = true;
        }
      }

      base.OnChildrenCollectionChanged();

      RaisePropertyChanged( "ChildrenSorted" );
    }

    public int IndexOf( LayoutContent content )
    {
      return Children.IndexOf( content );
    }

    protected override void OnIsVisibleChanged()
    {
      UpdateParentVisibility();
      base.OnIsVisibleChanged();
    }

    internal void SetNextSelectedIndex()
    {
      SelectedContentIndex = -1;
      for( int i = 0; i < this.Children.Count; ++i )
      {
        if( Children[ i ].IsEnabled )
        {
          SelectedContentIndex = i;
          return;
        }
      }
    }

    void UpdateParentVisibility()
    {
      var parentPane = Parent as ILayoutElementWithVisibility;
      if( parentPane != null )
        parentPane.ComputeVisibility();
    }

    public IEnumerable<LayoutContent> ChildrenSorted
    {
      get
      {
        var listSorted = Children.ToList();
        listSorted.Sort();
        return listSorted;
      }
    }

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

  }
}
