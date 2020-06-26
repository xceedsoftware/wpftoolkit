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
using System.Windows.Markup;
using System.Xml.Serialization;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [ContentProperty( "Children" )]
  [Serializable]
  public class LayoutAnchorGroup : LayoutGroup<LayoutAnchorable>, ILayoutPreviousContainer, ILayoutPaneSerializable
  {
    #region Constructors

    public LayoutAnchorGroup()
    {
    }

    #endregion

    #region Overrides

    protected override bool GetVisibility()
    {
      return Children.Count > 0;
    }

    public override void WriteXml( System.Xml.XmlWriter writer )
    {
      if( _id != null )
        writer.WriteAttributeString( "Id", _id );
      if( _previousContainer != null )
      {
        var paneSerializable = _previousContainer as ILayoutPaneSerializable;
        if( paneSerializable != null )
        {
          writer.WriteAttributeString( "PreviousContainerId", paneSerializable.Id );
        }
      }

      base.WriteXml( writer );
    }

    public override void ReadXml( System.Xml.XmlReader reader )
    {
      if( reader.MoveToAttribute( "Id" ) )
        _id = reader.Value;
      if( reader.MoveToAttribute( "PreviousContainerId" ) )
        ( ( ILayoutPreviousContainer )this ).PreviousContainerId = reader.Value;


      base.ReadXml( reader );
    }

    #endregion

    #region ILayoutPreviousContainer Interface

    #region PreviousContainer

    [field: NonSerialized]
    private ILayoutContainer _previousContainer = null;
    [XmlIgnore]
    ILayoutContainer ILayoutPreviousContainer.PreviousContainer
    {
      get
      {
        return _previousContainer;
      }
      set
      {
        if( _previousContainer != value )
        {
          _previousContainer = value;
          RaisePropertyChanged( "PreviousContainer" );
          var paneSerializable = _previousContainer as ILayoutPaneSerializable;
          if( paneSerializable != null &&
              paneSerializable.Id == null )
            paneSerializable.Id = Guid.NewGuid().ToString();
        }
      }
    }

    #endregion

    string ILayoutPreviousContainer.PreviousContainerId
    {
      get;
      set;
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
