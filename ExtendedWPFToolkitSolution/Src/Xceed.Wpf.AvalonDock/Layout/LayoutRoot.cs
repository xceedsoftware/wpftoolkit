﻿/*************************************************************************************

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
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Xml.Serialization;
using Standard;
using System.Xml;
using System.Xml.Schema;

namespace Xceed.Wpf.AvalonDock.Layout
{
    [ContentProperty("RootPanel")]
    [Serializable]
    public class LayoutRoot : LayoutElement, ILayoutContainer, ILayoutRoot, IXmlSerializable
    {
        public LayoutRoot()
        { 
            RightSide = new LayoutAnchorSide();
            LeftSide = new LayoutAnchorSide();
            TopSide = new LayoutAnchorSide();
            BottomSide = new LayoutAnchorSide();
            RootPanel = new LayoutPanel(new LayoutDocumentPane());
        }


        #region RootPanel

        private LayoutPanel _rootPanel;
        public LayoutPanel RootPanel
        {
            get { return _rootPanel; }
            set
            {
                if (_rootPanel != value)
                {
                    RaisePropertyChanging("RootPanel");
                    if (_rootPanel != null &&
                        _rootPanel.Parent == this)
                        _rootPanel.Parent = null;
                    _rootPanel = value;

                    if (_rootPanel == null)
                        _rootPanel = new LayoutPanel(new LayoutDocumentPane());

                    if (_rootPanel != null)
                        _rootPanel.Parent = this;
                    RaisePropertyChanged("RootPanel");
                }
            }
        }

        #endregion

        #region TopSide

        private LayoutAnchorSide _topSide = null;
        public LayoutAnchorSide TopSide
        {
            get { return _topSide; }
            set
            {
                if (_topSide != value)
                {
                    RaisePropertyChanging("TopSide");
                    _topSide = value;
                    if (_topSide != null)
                        _topSide.Parent = this;
                    RaisePropertyChanged("TopSide");
                }
            }
        }

        #endregion

        #region RightSide

        private LayoutAnchorSide _rightSide;
        public LayoutAnchorSide RightSide
        {
            get { return _rightSide; }
            set
            {
                if (_rightSide != value)
                {
                    RaisePropertyChanging("RightSide");
                    _rightSide = value;
                    if (_rightSide != null)
                        _rightSide.Parent = this;
                    RaisePropertyChanged("RightSide");
                }
            }
        }

        #endregion

        #region LeftSide

        private LayoutAnchorSide _leftSide = null;
        public LayoutAnchorSide LeftSide
        {
            get { return _leftSide; }
            set
            {
                if (_leftSide != value)
                {
                    RaisePropertyChanging("LeftSide");
                    _leftSide = value;
                    if (_leftSide != null)
                        _leftSide.Parent = this;
                    RaisePropertyChanged("LeftSide");
                }
            }
        }

        #endregion

        #region BottomSide

        private LayoutAnchorSide _bottomSide = null;
        public LayoutAnchorSide BottomSide
        {
            get { return _bottomSide; }
            set
            {
                if (_bottomSide != value)
                {
                    RaisePropertyChanging("BottomSide");
                    _bottomSide = value;
                    if (_bottomSide != null)
                        _bottomSide.Parent = this;
                    RaisePropertyChanged("BottomSide");
                }
            }
        }

        #endregion

        #region FloatingWindows
        ObservableCollection<LayoutFloatingWindow> _floatingWindows = null;

        public ObservableCollection<LayoutFloatingWindow> FloatingWindows
        {
            get 
            {
                if (_floatingWindows == null)
                {
                    _floatingWindows = new ObservableCollection<LayoutFloatingWindow>();
                    _floatingWindows.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_floatingWindows_CollectionChanged);
                }

                return _floatingWindows; 
            }
        }

        void _floatingWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
            {
                foreach (LayoutFloatingWindow element in e.OldItems)
                {
                    if (element.Parent == this)
                        element.Parent = null;
                }
            } 

            if (e.NewItems != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
            {
                foreach (LayoutFloatingWindow element in e.NewItems)
                    element.Parent = this;
            }
        }
        #endregion

        #region HiddenAnchorables

        ObservableCollection<LayoutAnchorable> _hiddenAnchorables = null;

        public ObservableCollection<LayoutAnchorable> Hidden
        {
            get
            {
                if (_hiddenAnchorables == null)
                {
                    _hiddenAnchorables = new ObservableCollection<LayoutAnchorable>();
                    _hiddenAnchorables.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_hiddenAnchorables_CollectionChanged);
                }

                return _hiddenAnchorables;
            }
        }

        void _hiddenAnchorables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (LayoutAnchorable element in e.OldItems)
                    {
                        if (element.Parent == this)
                            element.Parent = null;
                    }
                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.NewItems != null)
                {
                    foreach (LayoutAnchorable element in e.NewItems)
                    {
                        if (element.Parent != this)
                        {
                            if (element.Parent != null)
                                element.Parent.RemoveChild(element);
                            element.Parent = this;
                        }

                    }
                }
            }



        }

        #endregion

        #region Children
        public IEnumerable<ILayoutElement> Children
        {
            get
            {
                if (RootPanel != null)
                    yield return RootPanel;
                if (_floatingWindows != null)
                {
                    foreach (var floatingWindow in _floatingWindows)
                        yield return floatingWindow;
                }
                if (TopSide != null)
                    yield return TopSide;
                if (RightSide != null)
                    yield return RightSide;
                if (BottomSide != null)
                    yield return BottomSide;
                if (LeftSide != null)
                    yield return LeftSide;
                if (_hiddenAnchorables != null)
                {
                    foreach (var hiddenAnchorable in _hiddenAnchorables)
                        yield return hiddenAnchorable;
                }
            }
        }
        public void RemoveChild(ILayoutElement element)
        {
            if (element == RootPanel)
                RootPanel = null;
            else if (_floatingWindows != null && _floatingWindows.Contains(element))
                _floatingWindows.Remove(element as LayoutFloatingWindow);
            else if (_hiddenAnchorables != null && _hiddenAnchorables.Contains(element))
                _hiddenAnchorables.Remove(element as LayoutAnchorable);
            else if (element == TopSide)
                TopSide = null;
            else if (element == RightSide)
                RightSide = null;
            else if (element == BottomSide)
                BottomSide = null;
            else if (element == LeftSide)
                LeftSide = null;

        }

        public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
        {
            if (oldElement == RootPanel)
                RootPanel = (LayoutPanel)newElement;
            else if (_floatingWindows != null && _floatingWindows.Contains(oldElement))
            {
                int index = _floatingWindows.IndexOf(oldElement as LayoutFloatingWindow);
                _floatingWindows.Remove(oldElement as LayoutFloatingWindow);
                _floatingWindows.Insert(index, newElement as LayoutFloatingWindow);
            }
            else if (_hiddenAnchorables != null && _hiddenAnchorables.Contains(oldElement))
            {
                int index = _hiddenAnchorables.IndexOf(oldElement as LayoutAnchorable);
                _hiddenAnchorables.Remove(oldElement as LayoutAnchorable);
                _hiddenAnchorables.Insert(index, newElement as LayoutAnchorable);
            }
            else if (oldElement == TopSide)
                TopSide = (LayoutAnchorSide)newElement;
            else if (oldElement == RightSide)
                RightSide = (LayoutAnchorSide)newElement;
            else if (oldElement == BottomSide)
                BottomSide = (LayoutAnchorSide)newElement;
            else if (oldElement == LeftSide)
                LeftSide = (LayoutAnchorSide)newElement;
        }

        public int ChildrenCount
        {
            get
            {
                return 5 +
                    (_floatingWindows != null ? _floatingWindows.Count : 0) + 
                    (_hiddenAnchorables != null ? _hiddenAnchorables.Count : 0); 
            }
        }
        #endregion

        #region ActiveContent

        [field:NonSerialized]
        private WeakReference _activeContent = null;
        private bool _activeContentSet = false;

        [XmlIgnore]
        public LayoutContent ActiveContent
        {
            get { return _activeContent.GetValueOrDefault<LayoutContent>(); }
            set
            {
                var currentValue = ActiveContent;
                if (currentValue != value)
                {
                    InternalSetActiveContent(currentValue, value);
                }
            }
        }

        void InternalSetActiveContent(LayoutContent currentValue, LayoutContent newActiveContent)
        {
            RaisePropertyChanging("ActiveContent");
            if (currentValue != null)
                currentValue.IsActive = false;
            _activeContent = new WeakReference(newActiveContent);
            currentValue = ActiveContent;
            if (currentValue != null)
                currentValue.IsActive = true;
            RaisePropertyChanged("ActiveContent");
            _activeContentSet = currentValue != null;
            if (currentValue != null)
            {
                if (currentValue.Parent is LayoutDocumentPane || currentValue is LayoutDocument)
                    LastFocusedDocument = currentValue;
            }
            else
                LastFocusedDocument = null;
        }

        void UpdateActiveContentProperty()
        {
            var activeContent = ActiveContent;
            if (_activeContentSet && (activeContent == null || activeContent.Root != this))
            {
                _activeContentSet = false;
                InternalSetActiveContent(activeContent, null);
            }
        }
        #endregion

        #region LastFocusedDocument

        [field: NonSerialized]
        private WeakReference _lastFocusedDocument = null;
        [field: NonSerialized]
        private bool _lastFocusedDocumentSet = false;

        [XmlIgnore]
        public LayoutContent LastFocusedDocument
        {
            get { return _lastFocusedDocument.GetValueOrDefault<LayoutContent>(); }
            private set
            {
                var currentValue = LastFocusedDocument;
                if (currentValue != value)
                {
                    RaisePropertyChanging("LastFocusedDocument");
                    if (currentValue != null)
                        currentValue.IsLastFocusedDocument = false;
                    _lastFocusedDocument = new WeakReference(value);
                    currentValue = LastFocusedDocument;
                    if (currentValue != null)
                        currentValue.IsLastFocusedDocument = true;
                    _lastFocusedDocumentSet = currentValue != null;
                    RaisePropertyChanged("LastFocusedDocument");
                }
            }
        }

        #endregion

        #region Manager


        [NonSerialized]
        private DockingManager _manager = null;

        [XmlIgnore]
        public DockingManager Manager
        {
            get { return _manager; }
            internal set
            {
                if (_manager != value)
                {
                    RaisePropertyChanging("Manager");
                    _manager = value;
                    RaisePropertyChanged("Manager");
                }
            }
        }

        #endregion

        #region CollectGarbage

        /// <summary>
        /// Removes any empty container not directly referenced by other layout items
        /// </summary>
        public void CollectGarbage()
        {
            bool exitFlag = true;

            #region collect empty panes
            do
            {
                exitFlag = true;

                //for each content that references via PreviousContainer a disconnected Pane set the property to null
                foreach (var content in this.Descendents().OfType<ILayoutPreviousContainer>().Where(c => c.PreviousContainer != null && 
                    (c.PreviousContainer.Parent == null || c.PreviousContainer.Parent.Root != this)))
                {
                    content.PreviousContainer = null;
                }

                //for each pane that is empty
                foreach (var emptyPane in this.Descendents().OfType<ILayoutPane>().Where(p => p.ChildrenCount == 0))
                {
                    //...set null any reference coming from contents not yet hosted in a floating window
                    foreach (var contentReferencingEmptyPane in this.Descendents().OfType<LayoutContent>()
                        .Where(c => ((ILayoutPreviousContainer)c).PreviousContainer == emptyPane && !c.IsFloating))
                    {
                        if (contentReferencingEmptyPane is LayoutAnchorable &&
                            !((LayoutAnchorable)contentReferencingEmptyPane).IsVisible)
                            continue;

                        ((ILayoutPreviousContainer)contentReferencingEmptyPane).PreviousContainer = null;
                        contentReferencingEmptyPane.PreviousContainerIndex = -1;
                    }

                    //...if this pane is the only documentpane present in the layout than skip it
                    if (emptyPane is LayoutDocumentPane &&
                        this.Descendents().OfType<LayoutDocumentPane>().Count(c => c != emptyPane) == 0)
                        continue;

                    //...if this empty panes is not referenced by anyone, than removes it from its parent container
                    if (!this.Descendents().OfType<ILayoutPreviousContainer>().Any(c => c.PreviousContainer == emptyPane))
                    {
                        var parentGroup = emptyPane.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPane);
                        exitFlag = false;
                        break;
                    }
                }

                if (!exitFlag)
                {
                    //removes any empty anchorable pane group
                    foreach (var emptyPaneGroup in this.Descendents().OfType<LayoutAnchorablePaneGroup>().Where(p => p.ChildrenCount == 0))
                    {
                        var parentGroup = emptyPaneGroup.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPaneGroup);
                        exitFlag = false;
                        break;
                    }
                }

                if (!exitFlag)
                {
                    //removes any empty layout panel
                    foreach (var emptyPaneGroup in this.Descendents().OfType<LayoutPanel>().Where(p => p.ChildrenCount == 0))
                    {
                        var parentGroup = emptyPaneGroup.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPaneGroup);
                        exitFlag = false;
                        break;
                    }
                }

                if (!exitFlag)
                {
                    //removes any empty floating window
                    foreach (var emptyPaneGroup in this.Descendents().OfType<LayoutFloatingWindow>().Where(p => p.ChildrenCount == 0))
                    {
                        var parentGroup = emptyPaneGroup.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPaneGroup);
                        exitFlag = false;
                        break;
                    }
                }

                if (!exitFlag)
                {
                    //removes any empty anchor group
                    foreach (var emptyPaneGroup in this.Descendents().OfType<LayoutAnchorGroup>().Where(p => p.ChildrenCount == 0))
                    {
                        var parentGroup = emptyPaneGroup.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPaneGroup);
                        exitFlag = false;
                        break;
                    }
                }

            }
            while (!exitFlag);
            #endregion

            #region collapse single child anchorable pane groups
            do
            {
                exitFlag = true;
                //for each pane that is empty
                foreach (var paneGroupToCollapse in this.Descendents().OfType<LayoutAnchorablePaneGroup>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutAnchorablePaneGroup).ToArray())
                {
                    var singleChild = paneGroupToCollapse.Children[0] as LayoutAnchorablePaneGroup;
                    paneGroupToCollapse.Orientation = singleChild.Orientation;
                    paneGroupToCollapse.RemoveChild(singleChild);
                    while (singleChild.ChildrenCount > 0)
                    {
                        paneGroupToCollapse.InsertChildAt(
                            paneGroupToCollapse.ChildrenCount, singleChild.Children[0]);
                    }

                    exitFlag = false;
                    break;
                }

            }
            while (!exitFlag);



            #endregion

            #region collapse single child document pane groups
            do
            {
                exitFlag = true;
                //for each pane that is empty
                foreach (var paneGroupToCollapse in this.Descendents().OfType<LayoutDocumentPaneGroup>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutDocumentPaneGroup).ToArray())
                {
                    var singleChild = paneGroupToCollapse.Children[0] as LayoutDocumentPaneGroup;
                    paneGroupToCollapse.Orientation = singleChild.Orientation;
                    paneGroupToCollapse.RemoveChild(singleChild);
                    while (singleChild.ChildrenCount > 0)
                    {
                        paneGroupToCollapse.InsertChildAt(
                            paneGroupToCollapse.ChildrenCount, singleChild.Children[0]);
                    }

                    exitFlag = false;
                    break;
                }

            }
            while (!exitFlag);



            #endregion

            //do
            //{
            //  exitFlag = true;
            //  //for each panel that has only one child
            //  foreach( var panelToCollapse in this.Descendents().OfType<LayoutPanel>().Where( p => p.ChildrenCount == 1 && p.Children[ 0 ] is LayoutPanel ).ToArray() )
            //  {
            //    var singleChild = panelToCollapse.Children[ 0 ] as LayoutPanel;
            //    panelToCollapse.Orientation = singleChild.Orientation;
            //    panelToCollapse.RemoveChild( singleChild );
            //    ILayoutPanelElement[] singleChildChildren = new ILayoutPanelElement[ singleChild.ChildrenCount ];
            //    singleChild.Children.CopyTo( singleChildChildren, 0 );
            //    while( singleChild.ChildrenCount > 0 )
            //    {
            //      panelToCollapse.InsertChildAt(
            //          panelToCollapse.ChildrenCount, singleChildChildren[ panelToCollapse.ChildrenCount ] );
            //    }

            //    exitFlag = false;
            //    break;
            //  }

            //}
            //while( !exitFlag );

            #region Update ActiveContent and LastFocusedDocument properties
            UpdateActiveContentProperty();
            #endregion
#if DEBUG
            System.Diagnostics.Debug.Assert(
                !this.Descendents().OfType<LayoutAnchorablePane>().Any(a => a.ChildrenCount == 0 && a.IsVisible));
#if TRACE
            RootPanel.ConsoleDump(4);
#endif
#endif
        }

        #endregion

        public event EventHandler Updated;

        internal void FireLayoutUpdated()
        {
            if (Updated != null)
                Updated(this, EventArgs.Empty);
        }

        #region LayoutElement Added/Removed events

        internal void OnLayoutElementAdded(LayoutElement element)
        {
            if (ElementAdded != null)
                ElementAdded(this, new LayoutElementEventArgs(element));
        }

        public event EventHandler<LayoutElementEventArgs> ElementAdded;

        internal void OnLayoutElementRemoved(LayoutElement element)
        {
            if (element.Descendents().OfType<LayoutContent>().Any(c => c == LastFocusedDocument))
                LastFocusedDocument = null;
            if (element.Descendents().OfType<LayoutContent>().Any(c => c == ActiveContent))
                ActiveContent = null;
            if (ElementRemoved != null)
                ElementRemoved(this, new LayoutElementEventArgs(element));
        }

        public event EventHandler<LayoutElementEventArgs> ElementRemoved;

    #endregion

        public XmlSchema GetSchema()
        {
          return null;
        }

        public void ReadXml( XmlReader reader )
        {
          reader.MoveToContent();
          if( reader.IsEmptyElement )
          {
            reader.Read();
            return;
          }

          RootPanel = ReadRootPanel( reader );

          TopSide = ( LayoutAnchorSide )ReadElement( reader );
          if( TopSide != null )
          {
            foreach(var g in ReadElements<LayoutAnchorGroup>(reader))
              TopSide.Children.Add( g );
            reader.Read();
          } else {
            TopSide = new LayoutAnchorSide();
          }

          RightSide = ( LayoutAnchorSide )ReadElement( reader );
          if( RightSide != null )
          {
            foreach(var g in ReadElements<LayoutAnchorGroup>(reader))
              RightSide.Children.Add( g );
            reader.Read();
          } else {
            RightSide = new LayoutAnchorSide();
          }

          LeftSide = ( LayoutAnchorSide )ReadElement( reader );
          if( LeftSide != null )
          {
            foreach(var g in ReadElements<LayoutAnchorGroup>(reader))
              LeftSide.Children.Add( g );
            reader.Read();
          } else {
            LeftSide = new LayoutAnchorSide();
          }

          BottomSide = ( LayoutAnchorSide )ReadElement( reader );
          if( BottomSide != null )
          {
            foreach(var g in ReadElements<LayoutAnchorGroup>(reader))
              BottomSide.Children.Add( g );
            reader.Read();
          } else {
            BottomSide = new LayoutAnchorSide();
          }

          FloatingWindows.Clear();
          var floatingWindows = ReadElementList<LayoutFloatingWindow>( reader );
          foreach( var floatingWindow in floatingWindows )
          {
            FloatingWindows.Add( floatingWindow );
          }

          Hidden.Clear();
          var hidden = ReadElementList<LayoutAnchorable>( reader );
          foreach( var hiddenObject in hidden )
          {
            Hidden.Add( hiddenObject );
          }
        }

        private LayoutPanel ReadRootPanel( XmlReader reader )
        {
          var startElementName = reader.LocalName;
          reader.Read();
          if( reader.LocalName.Equals(startElementName) && (reader.NodeType == XmlNodeType.EndElement) )
          {
            return null;
          }

          while( reader.NodeType == XmlNodeType.Whitespace )
          {
            reader.Read();
          }

          LayoutPanel rootPanel = null;
          if( reader.LocalName.Equals("RootPanel"))
          {
            rootPanel = ReadElement( reader ) as LayoutPanel;
          } else {
            reader.ReadInnerXml();
          }

          return rootPanel;
        }

        private List<T> ReadElements<T>(XmlReader reader) where T : class 
        {
          var resultList = new List<T>();

          while(true) {
            var result = ReadElement(reader) as T;
            if(result == null) {
              break;
            }

            resultList.Add(result);
          }

          return resultList;
        }

        private List<T> ReadElementList<T>( XmlReader reader ) where T: class
        {
          var resultList = new List<T>();

          if( reader.IsEmptyElement )
          {
            reader.Read();
            return resultList;
          }

          var startElementName = reader.LocalName;
          reader.Read();
          if( reader.LocalName.Equals(startElementName) && (reader.NodeType == XmlNodeType.EndElement) )
          {
            return resultList;
          }

          while( reader.NodeType == XmlNodeType.Whitespace )
          {
            reader.Read();
          }

          resultList = ReadElements<T>(reader);

          reader.ReadEndElement();

          return resultList;
        }

        private object ReadElement( XmlReader reader )
        {
          if( reader.NodeType == XmlNodeType.EndElement )
          {
            return null;
          }

          while( reader.NodeType == XmlNodeType.Whitespace )
          {
            reader.Read();
          }

          XmlSerializer serializer;
          switch( reader.LocalName )
          {
            case "RootPanel":
              serializer = new XmlSerializer( typeof( LayoutPanel ), new XmlRootAttribute("RootPanel") );
              break;
            case "LayoutAnchorablePaneGroup":
              serializer = new XmlSerializer( typeof( LayoutAnchorablePaneGroup ) );
              break;
            case "LayoutAnchorablePane":
              serializer = new XmlSerializer( typeof( LayoutAnchorablePane ) );
              break;
            case "LayoutAnchorable":
              serializer = new XmlSerializer( typeof( LayoutAnchorable ) );
              break;
            case "LayoutDocumentPaneGroup":
              serializer = new XmlSerializer( typeof( LayoutDocumentPaneGroup ) );
              break;
            case "LayoutDocumentPane":
              serializer = new XmlSerializer( typeof( LayoutDocumentPane ) );
              break;
            case "LayoutDocument":
              serializer = new XmlSerializer( typeof( LayoutDocument ) );
              break;
            case "LayoutAnchorGroup":
              serializer = new XmlSerializer( typeof( LayoutAnchorGroup ) );
              break;
            case "LayoutPanel":
              serializer = new XmlSerializer( typeof( LayoutPanel ) );
              break;
            case "LayoutDocumentFloatingWindow":
              serializer = new XmlSerializer( typeof( LayoutDocumentFloatingWindow ) );
              break;
            case "LayoutAnchorableFloatingWindow":
              serializer = new XmlSerializer( typeof( LayoutAnchorableFloatingWindow ) );
              break;
            case "LeftSide":
            case "RightSide":
            case "TopSide":
            case "BottomSide":
              if( reader.IsEmptyElement )
              {
                reader.Read();
                return null;
              }
              reader.Read();
              return new LayoutAnchorSide();
            default:
              var type = LayoutRoot.FindType( reader.LocalName );
              if( type == null )
              {
                throw new ArgumentException( "AvalonDock.LayoutRoot doesn't know how to deserialize " + reader.LocalName );
              }
              serializer = new XmlSerializer( type );
              break;
          }

          return serializer.Deserialize( reader );
        }

        public void WriteXml( XmlWriter writer )
        {
          writer.WriteStartElement( "RootPanel" );
          if( RootPanel != null )
          {
            RootPanel.WriteXml( writer );
          }
          writer.WriteEndElement();

          writer.WriteStartElement( "TopSide" );
          if( TopSide != null )
          {
            TopSide.WriteXml( writer );
          }
          writer.WriteEndElement();

          writer.WriteStartElement( "RightSide" );
          if( RightSide != null )
          {
            RightSide.WriteXml( writer );
          }
          writer.WriteEndElement();

          writer.WriteStartElement( "LeftSide" );
          if( LeftSide != null )
          {
            LeftSide.WriteXml( writer );
          }
          writer.WriteEndElement();

          writer.WriteStartElement( "BottomSide" );
          if( BottomSide != null )
          {
            BottomSide.WriteXml( writer );
          }
          writer.WriteEndElement();

          // Write all floating windows (can be LayoutDocumentFloatingWindow or LayoutAnchorableFloatingWindow).
          // To prevent "can not create instance of abstract type", the type is retrieved with GetType().Name
          writer.WriteStartElement( "FloatingWindows" );
          foreach( var layoutFloatingWindow in FloatingWindows )
          {
            writer.WriteStartElement( layoutFloatingWindow.GetType().Name );
            layoutFloatingWindow.WriteXml( writer );
            writer.WriteEndElement();
          }
          writer.WriteEndElement();

          writer.WriteStartElement( "Hidden" );
          foreach( var layoutAnchorable in Hidden )
          {
            layoutAnchorable.WriteXml( writer );
          }
          writer.WriteEndElement();
        }

        internal static Type FindType( string name )
        {
          foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
          {
            foreach( var type in assembly.GetTypes() )
            {
              if( type.Name.Equals( name ) )
                return type;
            }
          }
          return null;
        }

#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "RootPanel()" );

          RootPanel.ConsoleDump(tab + 1);

          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "FloatingWindows()" );

          foreach (LayoutFloatingWindow fw in FloatingWindows)
              fw.ConsoleDump(tab + 1);

          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "Hidden()" );

          foreach (LayoutAnchorable hidden in Hidden)
              hidden.ConsoleDump(tab + 1);
        }
#endif


    }
}
