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
using System.Globalization;
using System.Linq;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [Serializable]
  public class LayoutDocument : LayoutContent
  {
    #region Properties

    #region CanMove

    internal bool _canMove = true;
    public bool CanMove
    {
      get
      {
        return _canMove;
      }
      set
      {
        if( _canMove != value )
        {
          _canMove = value;
          RaisePropertyChanged( "CanMove" );
        }
      }
    }

    #endregion

    #region IsVisible

    public bool IsVisible
    {
      get
      {
        return _isVisible;
      }
      internal set
      {
        _isVisible = value;
      }
    }

    private bool _isVisible = true;

    #endregion

    #region Description

    private string _description = null;
    public string Description
    {
      get
      {
        return _description;
      }
      set
      {
        if( _description != value )
        {
          _description = value;
          RaisePropertyChanged( "Description" );
        }
      }
    }

    #endregion

    #endregion

    #region Overrides

    public override void WriteXml( System.Xml.XmlWriter writer )
    {
      base.WriteXml( writer );

      if( !string.IsNullOrWhiteSpace( this.Description ) )
        writer.WriteAttributeString( "Description", this.Description );
      if( !CanMove )
        writer.WriteAttributeString( "CanMove", CanMove.ToString() );
    }

    public override void ReadXml( System.Xml.XmlReader reader )
    {
      if( reader.MoveToAttribute( "Description" ) )
        this.Description = reader.Value;
      if( reader.MoveToAttribute( "CanMove" ) )
        CanMove = bool.Parse( reader.Value );

      base.ReadXml( reader );
    }

    public override void Close()
    {
      if( ( this.Root != null ) && ( this.Root.Manager != null ) )
      {
        var dockingManager = this.Root.Manager;
        dockingManager._ExecuteCloseCommand( this );
      }
      else
      {
        this.CloseDocument();
      }
    }

#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "Document()" );
        }
#endif

    protected override void InternalDock()
    {
      var root = Root as LayoutRoot;
      LayoutDocumentPane documentPane = null;
      if( root.LastFocusedDocument != null &&
          root.LastFocusedDocument != this )
      {
        documentPane = root.LastFocusedDocument.Parent as LayoutDocumentPane;
      }

      if( documentPane == null )
      {
        documentPane = root.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
      }


      bool added = false;
      if( root.Manager.LayoutUpdateStrategy != null )
      {
        added = root.Manager.LayoutUpdateStrategy.BeforeInsertDocument( root, this, documentPane );
      }

      if( !added )
      {
        if( documentPane == null )
          throw new InvalidOperationException( "Layout must contains at least one LayoutDocumentPane in order to host documents" );

        documentPane.Children.Add( this );
        added = true;
      }

      if( root.Manager.LayoutUpdateStrategy != null )
      {
        root.Manager.LayoutUpdateStrategy.AfterInsertDocument( root, this );
      }


      base.InternalDock();
    }

    #endregion

    #region Internal Methods

    internal bool CloseDocument()
    {
      if( this.TestCanClose() )
      {
        this.CloseInternal();
        return true;
      }

      return false;
    }

    #endregion
  }
}
