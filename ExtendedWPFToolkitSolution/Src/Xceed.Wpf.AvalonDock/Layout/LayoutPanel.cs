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
using System.Windows.Markup;
using System.Windows.Controls;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [ContentProperty( "Children" )]
  [Serializable]
  public class LayoutPanel : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutPanelElement, ILayoutOrientableGroup, ILayoutElementWithVisibility
  {
    #region Constructors

    public LayoutPanel()
    {
    }

    public LayoutPanel( ILayoutPanelElement firstChild )
    {
      Children.Add( firstChild );
    }

    #endregion

    #region Properties

    #region Orientation

    private Orientation _orientation;
    public Orientation Orientation
    {
      get
      {
        return _orientation;
      }
      set
      {
        if( _orientation != value )
        {
          RaisePropertyChanging( "Orientation" );
          _orientation = value;
          RaisePropertyChanged( "Orientation" );
        }
      }
    }

    #endregion

    #endregion

    #region Overrides

    protected override bool GetVisibility()
    {
      return Children.Any( c => c.IsVisible );
    }

    public override void WriteXml( System.Xml.XmlWriter writer )
    {
      writer.WriteAttributeString( "Orientation", Orientation.ToString() );
      base.WriteXml( writer );
    }

    public override void ReadXml( System.Xml.XmlReader reader )
    {
      if( reader.MoveToAttribute( "Orientation" ) )
        Orientation = ( Orientation )Enum.Parse( typeof( Orientation ), reader.Value, true );
      base.ReadXml( reader );
    }

#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( string.Format( "Panel({0})", Orientation ) );

          foreach (LayoutElement child in Children)
              child.ConsoleDump(tab + 1);
        }
#endif

    #endregion
  }
}
