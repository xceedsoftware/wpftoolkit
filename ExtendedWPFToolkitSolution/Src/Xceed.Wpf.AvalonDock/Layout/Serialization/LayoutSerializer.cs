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
using System.Linq;

namespace Xceed.Wpf.AvalonDock.Layout.Serialization
{
  public abstract class LayoutSerializer
  {
    #region Members

    private DockingManager _manager;
    private LayoutAnchorable[] _previousAnchorables = null;
    private LayoutDocument[] _previousDocuments = null;

    #endregion

    #region Constructors

    public LayoutSerializer( DockingManager manager )
    {
      if( manager == null )
        throw new ArgumentNullException( "manager" );

      _manager = manager;
      _previousAnchorables = _manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
      _previousDocuments = _manager.Layout.Descendents().OfType<LayoutDocument>().ToArray();
    }

    #endregion

    #region Properties

    public DockingManager Manager
    {
      get
      {
        return _manager;
      }
    }

    #endregion

    #region Events

    public event EventHandler<LayoutSerializationCallbackEventArgs> LayoutSerializationCallback;

    #endregion

    #region Methods

    protected virtual void FixupLayout( LayoutRoot layout )
    {
      //fix container panes
      foreach( var lcToAttach in layout.Descendents().OfType<ILayoutPreviousContainer>().Where( lc => lc.PreviousContainerId != null ) )
      {
        var paneContainerToAttach = layout.Descendents().OfType<ILayoutPaneSerializable>().FirstOrDefault( lps => lps.Id == lcToAttach.PreviousContainerId );
        if( paneContainerToAttach == null )
          throw new ArgumentException( string.Format( "Unable to find a pane with id ='{0}'", lcToAttach.PreviousContainerId ) );

        lcToAttach.PreviousContainer = paneContainerToAttach as ILayoutContainer;
      }


      //now fix the content of the layoutcontents
      foreach( var lcToFix in layout.Descendents().OfType<LayoutAnchorable>().Where( lc => lc.Content == null ).ToArray() )
      {
        LayoutAnchorable previousAchorable = null;
        if( lcToFix.ContentId != null )
        {
          //try find the content in replaced layout
          previousAchorable = _previousAnchorables.FirstOrDefault( a => a.ContentId == lcToFix.ContentId );
        }

        if( LayoutSerializationCallback != null )
        {
          var args = new LayoutSerializationCallbackEventArgs( lcToFix, previousAchorable != null ? previousAchorable.Content : null );
          LayoutSerializationCallback( this, args );
          if( args.Cancel )
            lcToFix.Close();
          else if( args.Content != null )
            lcToFix.Content = args.Content;
          else if( args.Model.Content != null )
            lcToFix.Hide( false );
        }
        else if( previousAchorable == null )
          lcToFix.Hide( false );
        else
        {
          lcToFix.Content = previousAchorable.Content;
          lcToFix.IconSource = previousAchorable.IconSource;
        }
      }


      foreach( var lcToFix in layout.Descendents().OfType<LayoutDocument>().Where( lc => lc.Content == null ).ToArray() )
      {
        LayoutDocument previousDocument = null;
        if( lcToFix.ContentId != null )
        {
          //try find the content in replaced layout
          previousDocument = _previousDocuments.FirstOrDefault( a => a.ContentId == lcToFix.ContentId );
        }

        if( LayoutSerializationCallback != null )
        {
          var args = new LayoutSerializationCallbackEventArgs( lcToFix, previousDocument != null ? previousDocument.Content : null );
          LayoutSerializationCallback( this, args );

          if( args.Cancel )
            lcToFix.Close();
          else if( args.Content != null )
            lcToFix.Content = args.Content;
          else if( args.Model.Content != null )
            lcToFix.Close();
        }
        else if( previousDocument == null )
          lcToFix.Close();
        else
        {
          lcToFix.Content = previousDocument.Content;
          lcToFix.IconSource = previousDocument.IconSource;
        }
      }

      layout.CollectGarbage();
    }

    protected void StartDeserialization()
    {
      Manager.SuspendDocumentsSourceBinding = true;
      Manager.SuspendAnchorablesSourceBinding = true;
    }

    protected void EndDeserialization()
    {
      Manager.SuspendDocumentsSourceBinding = false;
      Manager.SuspendAnchorablesSourceBinding = false;
    }

    #endregion
  }
}
