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
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [Serializable]
  public abstract class LayoutGroup<T> : LayoutGroupBase, ILayoutContainer, ILayoutGroup, IXmlSerializable where T : class, ILayoutElement
  {
    #region Members

    ObservableCollection<T> _children = new ObservableCollection<T>();

    #endregion

    #region Constructors

    internal LayoutGroup()
    {
      _children.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( _children_CollectionChanged );
    }

    #endregion

    #region Properties

    #region Children

    public ObservableCollection<T> Children
    {
      get
      {
        return _children;
      }
    }

    #endregion

    #region IsVisible

    private bool _isVisible = true;
    public bool IsVisible
    {
      get
      {
        return _isVisible;
      }
      protected set
      {
        if( _isVisible != value )
        {
          RaisePropertyChanging( "IsVisible" );
          _isVisible = value;
          OnIsVisibleChanged();
          RaisePropertyChanged( "IsVisible" );
        }
      }
    }

    #endregion

    #region ChildrenCount

    public int ChildrenCount
    {
      get
      {
        return _children.Count;
      }
    }

    #endregion

    #endregion

    #region Overrides

    protected override void OnParentChanged( ILayoutContainer oldValue, ILayoutContainer newValue )
    {
      base.OnParentChanged( oldValue, newValue );

      ComputeVisibility();
    }

    #endregion

    #region Public Methods

    public void ComputeVisibility()
    {
      IsVisible = GetVisibility();
    }

    public void MoveChild( int oldIndex, int newIndex )
    {
      if( oldIndex == newIndex )
        return;
      _children.Move( oldIndex, newIndex );
      ChildMoved( oldIndex, newIndex );
    }

    public void RemoveChildAt( int childIndex )
    {
      _children.RemoveAt( childIndex );
    }

    public int IndexOfChild( ILayoutElement element )
    {
      return _children.Cast<ILayoutElement>().ToList().IndexOf( element );
    }

    public void InsertChildAt( int index, ILayoutElement element )
    {
      _children.Insert( index, ( T )element );
    }

    public void RemoveChild( ILayoutElement element )
    {
      _children.Remove( ( T )element );
    }

    public void ReplaceChild( ILayoutElement oldElement, ILayoutElement newElement )
    {
      int index = _children.IndexOf( ( T )oldElement );
      _children.Insert( index, ( T )newElement );
      _children.RemoveAt( index + 1 );
    }

    public void ReplaceChildAt( int index, ILayoutElement element )
    {
      _children[ index ] = ( T )element;
    }


    public System.Xml.Schema.XmlSchema GetSchema()
    {
      return null;
    }

    public virtual void ReadXml( System.Xml.XmlReader reader )
    {
      reader.MoveToContent();
      if( reader.IsEmptyElement )
      {
        reader.Read();
        ComputeVisibility();
        return;
      }
      string localName = reader.LocalName;
      reader.Read();
      while( true )
      {
        if( ( reader.LocalName == localName ) &&
            ( reader.NodeType == System.Xml.XmlNodeType.EndElement ) )
        {
          break;
        }
        if( reader.NodeType == System.Xml.XmlNodeType.Whitespace )
        {
          reader.Read();
          continue;
        }

        XmlSerializer serializer = null;
        if( reader.LocalName == "LayoutAnchorablePaneGroup" )
          serializer = new XmlSerializer( typeof( LayoutAnchorablePaneGroup ) );
        else if( reader.LocalName == "LayoutAnchorablePane" )
          serializer = new XmlSerializer( typeof( LayoutAnchorablePane ) );
        else if( reader.LocalName == "LayoutAnchorable" )
          serializer = new XmlSerializer( typeof( LayoutAnchorable ) );
        else if( reader.LocalName == "LayoutDocumentPaneGroup" )
          serializer = new XmlSerializer( typeof( LayoutDocumentPaneGroup ) );
        else if( reader.LocalName == "LayoutDocumentPane" )
          serializer = new XmlSerializer( typeof( LayoutDocumentPane ) );
        else if( reader.LocalName == "LayoutDocument" )
          serializer = new XmlSerializer( typeof( LayoutDocument ) );
        else if( reader.LocalName == "LayoutAnchorGroup" )
          serializer = new XmlSerializer( typeof( LayoutAnchorGroup ) );
        else if( reader.LocalName == "LayoutPanel" )
          serializer = new XmlSerializer( typeof( LayoutPanel ) );
        else
        {
          Type type = this.FindType( reader.LocalName );
          if( type == null )
            throw new ArgumentException( "AvalonDock.LayoutGroup doesn't know how to deserialize " + reader.LocalName );
          serializer = new XmlSerializer( type );
        }

        Children.Add( ( T )serializer.Deserialize( reader ) );
      }

      reader.ReadEndElement();
    }

    public virtual void WriteXml( System.Xml.XmlWriter writer )
    {
      foreach( var child in Children )
      {
        var type = child.GetType();
        XmlSerializer serializer = new XmlSerializer( type );
        serializer.Serialize( writer, child );
      }

    }

    #endregion

    #region Internal Methods

    protected virtual void OnIsVisibleChanged()
    {
      UpdateParentVisibility();
    }

    protected abstract bool GetVisibility();

    protected virtual void ChildMoved( int oldIndex, int newIndex )
    {
    }

    #endregion

    #region Private Methods

    private void _children_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
    {
      if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace )
      {
        if( e.OldItems != null )
        {
          foreach( LayoutElement element in e.OldItems )
          {
            if( element.Parent == this )
              element.Parent = null;
          }
        }
      }

      if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace )
      {
        if( e.NewItems != null )
        {
          foreach( LayoutElement element in e.NewItems )
          {
            if( element.Parent != this )
            {
              if( element.Parent != null )
                element.Parent.RemoveChild( element );
              element.Parent = this;
            }

          }
        }
      }

      ComputeVisibility();
      OnChildrenCollectionChanged();
      NotifyChildrenTreeChanged( ChildrenTreeChange.DirectChildrenChanged );
      RaisePropertyChanged( "ChildrenCount" );
    }

    private void UpdateParentVisibility()
    {
      var parentPane = Parent as ILayoutElementWithVisibility;
      if( parentPane != null )
        parentPane.ComputeVisibility();
    }

    private Type FindType( string name )
    {
      foreach( var a in AppDomain.CurrentDomain.GetAssemblies() )
      {
        foreach( var t in a.GetTypes() )
        {
          if( t.Name.Equals( name ) )
            return t;
        }
      }
      return null;
    }

    #endregion

    #region ILayoutContainer Interface

    IEnumerable<ILayoutElement> ILayoutContainer.Children
    {
      get
      {
        return _children.Cast<ILayoutElement>();
      }
    }

    #endregion

  }
}
