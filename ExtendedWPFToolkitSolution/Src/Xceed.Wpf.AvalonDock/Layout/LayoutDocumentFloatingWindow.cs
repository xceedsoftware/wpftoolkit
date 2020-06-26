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
using System.Windows.Markup;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [ContentProperty( "RootDocument" )]
  [Serializable]
  public class LayoutDocumentFloatingWindow : LayoutFloatingWindow
  {
    #region Constructors

    public LayoutDocumentFloatingWindow()
    {
    }

    #endregion

    #region Properties

    #region RootDocument

    private LayoutDocument _rootDocument = null;
    public LayoutDocument RootDocument
    {
      get
      {
        return _rootDocument;
      }
      set
      {
        if( _rootDocument != value )
        {
          RaisePropertyChanging( "RootDocument" );
          _rootDocument = value;
          if( _rootDocument != null )
            _rootDocument.Parent = this;
          RaisePropertyChanged( "RootDocument" );

          if( RootDocumentChanged != null )
            RootDocumentChanged( this, EventArgs.Empty );
        }
      }
    }

    #endregion

    #endregion

    #region Overrides

    public override IEnumerable<ILayoutElement> Children
    {
      get
      {
        if( RootDocument == null )
          yield break;

        yield return RootDocument;
      }
    }

    public override void RemoveChild( ILayoutElement element )
    {
      Debug.Assert( element == RootDocument && element != null );
      RootDocument = null;
    }

    public override void ReplaceChild( ILayoutElement oldElement, ILayoutElement newElement )
    {
      Debug.Assert( oldElement == RootDocument && oldElement != null );
      RootDocument = newElement as LayoutDocument;
    }

    public override int ChildrenCount
    {
      get
      {
        return RootDocument != null ? 1 : 0;
      }
    }

    public override bool IsValid
    {
      get
      {
        return RootDocument != null;
      }
    }

    public override void ReadXml( XmlReader reader )
    {
      reader.MoveToContent();
      if( reader.IsEmptyElement )
      {
        reader.Read();
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
        if( reader.LocalName.Equals( "LayoutDocument" ) )
        {
          serializer = new XmlSerializer( typeof( LayoutDocument ) );
        }
        else
        {
          var type = LayoutRoot.FindType( reader.LocalName );
          if( type == null )
          {
            throw new ArgumentException( "AvalonDock.LayoutDocumentFloatingWindow doesn't know how to deserialize " + reader.LocalName );
          }
          serializer = new XmlSerializer( type );
        }

        RootDocument = ( LayoutDocument )serializer.Deserialize( reader );
      }

      reader.ReadEndElement();
    }

#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "FloatingDocumentWindow()" );

          RootDocument.ConsoleDump(tab + 1);
        }
#endif

    #endregion

    #region Events

    public event EventHandler RootDocumentChanged;

    #endregion
  }
}
