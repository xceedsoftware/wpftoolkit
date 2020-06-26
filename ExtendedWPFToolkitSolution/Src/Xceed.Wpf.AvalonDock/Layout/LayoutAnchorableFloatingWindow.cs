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
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [Serializable]
  [ContentProperty( "RootPanel" )]
  public class LayoutAnchorableFloatingWindow : LayoutFloatingWindow, ILayoutElementWithVisibility
  {
    #region Members

    private LayoutAnchorablePaneGroup _rootPanel = null;
    [NonSerialized]
    private bool _isVisible = true;

    #endregion

    #region Constructors

    public LayoutAnchorableFloatingWindow()
    {
    }

    #endregion

    #region Properties

    #region IsSinglePane

    public bool IsSinglePane
    {
      get
      {
        return RootPanel != null && RootPanel.Descendents().OfType<ILayoutAnchorablePane>().Where( p => p.IsVisible ).Count() == 1;
      }
    }

    #endregion

    #region IsVisible

    [XmlIgnore]
    public bool IsVisible
    {
      get
      {
        return _isVisible;
      }
      private set
      {
        if( _isVisible != value )
        {
          RaisePropertyChanging( "IsVisible" );
          _isVisible = value;
          RaisePropertyChanged( "IsVisible" );
          if( IsVisibleChanged != null )
            IsVisibleChanged( this, EventArgs.Empty );
        }
      }
    }

    #endregion

    #region RootPanel

    public LayoutAnchorablePaneGroup RootPanel
    {
      get
      {
        return _rootPanel;
      }
      set
      {
        if( _rootPanel != value )
        {
          RaisePropertyChanging( "RootPanel" );

          if( _rootPanel != null )
            _rootPanel.ChildrenTreeChanged -= new EventHandler<ChildrenTreeChangedEventArgs>( _rootPanel_ChildrenTreeChanged );

          _rootPanel = value;
          if( _rootPanel != null )
            _rootPanel.Parent = this;

          if( _rootPanel != null )
            _rootPanel.ChildrenTreeChanged += new EventHandler<ChildrenTreeChangedEventArgs>( _rootPanel_ChildrenTreeChanged );

          RaisePropertyChanged( "RootPanel" );
          RaisePropertyChanged( "IsSinglePane" );
          RaisePropertyChanged( "SinglePane" );
          RaisePropertyChanged( "Children" );
          RaisePropertyChanged( "ChildrenCount" );
          ( ( ILayoutElementWithVisibility )this ).ComputeVisibility();
        }
      }
    }

    #endregion

    #region SinglePane

    public ILayoutAnchorablePane SinglePane
    {
      get
      {
        if( !IsSinglePane )
          return null;

        var singlePane = RootPanel.Descendents().OfType<LayoutAnchorablePane>().Single( p => p.IsVisible );
        singlePane.UpdateIsDirectlyHostedInFloatingWindow();
        return singlePane;
      }
    }

    #endregion

    #endregion

    #region Overrides

    public override IEnumerable<ILayoutElement> Children
    {
      get
      {
        if( ChildrenCount == 1 )
          yield return RootPanel;

        yield break;
      }
    }

    public override void RemoveChild( ILayoutElement element )
    {
      Debug.Assert( element == RootPanel && element != null );
      RootPanel = null;
    }

    public override void ReplaceChild( ILayoutElement oldElement, ILayoutElement newElement )
    {
      Debug.Assert( oldElement == RootPanel && oldElement != null );
      RootPanel = newElement as LayoutAnchorablePaneGroup;
    }

    public override int ChildrenCount
    {
      get
      {
        if( RootPanel == null )
          return 0;
        return 1;
      }
    }

    public override bool IsValid
    {
      get
      {
        return RootPanel != null;
      }
    }

    public override void ReadXml( XmlReader reader )
    {
      reader.MoveToContent();
      if( reader.IsEmptyElement )
      {
        reader.Read();
        ComputeVisibility();
        return;
      }

      var localName = reader.LocalName;
      reader.Read();

      while( true )
      {
        if( reader.LocalName.Equals( localName ) && ( reader.NodeType == XmlNodeType.EndElement ) )
        {
          break;
        }

        if( reader.NodeType == XmlNodeType.Whitespace )
        {
          reader.Read();
          continue;
        }

        XmlSerializer serializer;
        if( reader.LocalName.Equals( "LayoutAnchorablePaneGroup" ) )
        {
          serializer = new XmlSerializer( typeof( LayoutAnchorablePaneGroup ) );
        }
        else
        {
          var type = LayoutRoot.FindType( reader.LocalName );
          if( type == null )
          {
            throw new ArgumentException( "AvalonDock.LayoutAnchorableFloatingWindow doesn't know how to deserialize " + reader.LocalName );
          }
          serializer = new XmlSerializer( type );
        }

        RootPanel = ( LayoutAnchorablePaneGroup )serializer.Deserialize( reader );
      }

      reader.ReadEndElement();
    }

#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "FloatingAnchorableWindow()" );

          RootPanel.ConsoleDump(tab + 1);
        }
#endif

    #endregion

    #region Private Methods

    private void _rootPanel_ChildrenTreeChanged( object sender, ChildrenTreeChangedEventArgs e )
    {
      RaisePropertyChanged( "IsSinglePane" );
      RaisePropertyChanged( "SinglePane" );
    }

    private void ComputeVisibility()
    {
      if( RootPanel != null )
        IsVisible = RootPanel.IsVisible;
      else
        IsVisible = false;
    }

    #endregion

    #region Events

    public event EventHandler IsVisibleChanged;

    #endregion

    #region ILayoutElementWithVisibility Interface

    void ILayoutElementWithVisibility.ComputeVisibility()
    {
      ComputeVisibility();
    }

    #endregion
  }
}
