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
using System.Windows;
using System.Globalization;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [Serializable]
  public abstract class LayoutPositionableGroup<T> : LayoutGroup<T>, ILayoutPositionableElement, ILayoutPositionableElementWithActualSize where T : class, ILayoutElement
  {
    #region Members

    private static GridLengthConverter _gridLengthConverter = new GridLengthConverter();

    #endregion

    #region Constructors

    public LayoutPositionableGroup()
    {
    }

    #endregion

    #region Properties

    #region DockWidth

    GridLength _dockWidth = new GridLength( 1.0, GridUnitType.Star );
    public GridLength DockWidth
    {
      get
      {
        return _dockWidth;
      }
      set
      {
        if( DockWidth != value )
        {
          RaisePropertyChanging( "DockWidth" );
          _dockWidth = value;
          RaisePropertyChanged( "DockWidth" );

          OnDockWidthChanged();
        }
      }
    }

    #endregion

    #region DockHeight

    GridLength _dockHeight = new GridLength( 1.0, GridUnitType.Star );
    public GridLength DockHeight
    {
      get
      {
        return _dockHeight;
      }
      set
      {
        if( DockHeight != value )
        {
          RaisePropertyChanging( "DockHeight" );
          _dockHeight = value;
          RaisePropertyChanged( "DockHeight" );

          OnDockHeightChanged();
        }
      }
    }

    #endregion

    #region AllowDuplicateContent

    private bool _allowDuplicateContent = true;
    /// <summary>
    /// Gets or sets the AllowDuplicateContent property.
    /// When this property is true, then the LayoutDocumentPane or LayoutAnchorablePane allows dropping
    /// duplicate content (according to its Title and ContentId). When this dependency property is false,
    /// then the LayoutDocumentPane or LayoutAnchorablePane hides the OverlayWindow.DropInto button to prevent dropping of duplicate content.
    /// </summary>
    public bool AllowDuplicateContent
    {
      get
      {
        return _allowDuplicateContent;
      }
      set
      {
        if( _allowDuplicateContent != value )
        {
          RaisePropertyChanging( "AllowDuplicateContent" );
          _allowDuplicateContent = value;
          RaisePropertyChanged( "AllowDuplicateContent" );
        }
      }
    }

    #endregion

    #region CanRepositionItems

    private bool _canRepositionItems = true;
    public bool CanRepositionItems
    {
      get
      {
        return _canRepositionItems;
      }
      set
      {
        if( _canRepositionItems != value )
        {
          RaisePropertyChanging( "CanRepositionItems" );
          _canRepositionItems = value;
          RaisePropertyChanged( "CanRepositionItems" );
        }
      }
    }

    #endregion

    #region DockMinWidth

    private double _dockMinWidth = 25.0;
    public double DockMinWidth
    {
      get
      {
        return _dockMinWidth;
      }
      set
      {
        if( _dockMinWidth != value )
        {
          MathHelper.AssertIsPositiveOrZero( value );
          RaisePropertyChanging( "DockMinWidth" );
          _dockMinWidth = value;
          RaisePropertyChanged( "DockMinWidth" );
        }
      }
    }

    #endregion

    #region DockMinHeight

    private double _dockMinHeight = 25.0;
    public double DockMinHeight
    {
      get
      {
        return _dockMinHeight;
      }
      set
      {
        if( _dockMinHeight != value )
        {
          MathHelper.AssertIsPositiveOrZero( value );
          RaisePropertyChanging( "DockMinHeight" );
          _dockMinHeight = value;
          RaisePropertyChanged( "DockMinHeight" );
        }
      }
    }

    #endregion

    #region FloatingWidth

    private double _floatingWidth = 0.0;
    public double FloatingWidth
    {
      get
      {
        return _floatingWidth;
      }
      set
      {
        if( _floatingWidth != value )
        {
          RaisePropertyChanging( "FloatingWidth" );
          _floatingWidth = value;
          RaisePropertyChanged( "FloatingWidth" );
        }
      }
    }

    #endregion

    #region FloatingHeight

    private double _floatingHeight = 0.0;
    public double FloatingHeight
    {
      get
      {
        return _floatingHeight;
      }
      set
      {
        if( _floatingHeight != value )
        {
          RaisePropertyChanging( "FloatingHeight" );
          _floatingHeight = value;
          RaisePropertyChanged( "FloatingHeight" );
        }
      }
    }

    #endregion

    #region FloatingLeft

    private double _floatingLeft = 0.0;
    public double FloatingLeft
    {
      get
      {
        return _floatingLeft;
      }
      set
      {
        if( _floatingLeft != value )
        {
          RaisePropertyChanging( "FloatingLeft" );
          _floatingLeft = value;
          RaisePropertyChanged( "FloatingLeft" );
        }
      }
    }

    #endregion

    #region FloatingTop

    private double _floatingTop = 0.0;
    public double FloatingTop
    {
      get
      {
        return _floatingTop;
      }
      set
      {
        if( _floatingTop != value )
        {
          RaisePropertyChanging( "FloatingTop" );
          _floatingTop = value;
          RaisePropertyChanged( "FloatingTop" );
        }
      }
    }

    #endregion






    #region IsMaximized

    private bool _isMaximized = false;
    public bool IsMaximized
    {
      get
      {
        return _isMaximized;
      }
      set
      {
        if( _isMaximized != value )
        {
          _isMaximized = value;
          RaisePropertyChanged( "IsMaximized" );
        }
      }
    }

    #endregion

    #region ActualWidth

    [NonSerialized]
    double _actualWidth;
    double ILayoutPositionableElementWithActualSize.ActualWidth
    {
      get
      {
        return _actualWidth;
      }
      set
      {
        _actualWidth = value;
      }
    }

    #endregion

    #region ActualHeight

    [NonSerialized]
    double _actualHeight;
    double ILayoutPositionableElementWithActualSize.ActualHeight
    {
      get
      {
        return _actualHeight;
      }
      set
      {
        _actualHeight = value;
      }
    }

    #endregion

    #endregion

    #region Overrides

    public override void WriteXml( System.Xml.XmlWriter writer )
    {
      if( DockWidth.Value != 1.0 || !DockWidth.IsStar )
        writer.WriteAttributeString( "DockWidth", _gridLengthConverter.ConvertToInvariantString( DockWidth ) );
      if( DockHeight.Value != 1.0 || !DockHeight.IsStar )
        writer.WriteAttributeString( "DockHeight", _gridLengthConverter.ConvertToInvariantString( DockHeight ) );

      if( DockMinWidth != 25.0 )
        writer.WriteAttributeString( "DockMinWidth", DockMinWidth.ToString( CultureInfo.InvariantCulture ) );
      if( DockMinHeight != 25.0 )
        writer.WriteAttributeString( "DockMinHeight", DockMinHeight.ToString( CultureInfo.InvariantCulture ) );

      if( FloatingWidth != 0.0 )
        writer.WriteAttributeString( "FloatingWidth", FloatingWidth.ToString( CultureInfo.InvariantCulture ) );
      if( FloatingHeight != 0.0 )
        writer.WriteAttributeString( "FloatingHeight", FloatingHeight.ToString( CultureInfo.InvariantCulture ) );
      if( FloatingLeft != 0.0 )
        writer.WriteAttributeString( "FloatingLeft", FloatingLeft.ToString( CultureInfo.InvariantCulture ) );
      if( FloatingTop != 0.0 )
        writer.WriteAttributeString( "FloatingTop", FloatingTop.ToString( CultureInfo.InvariantCulture ) );
      if( IsMaximized )
        writer.WriteAttributeString( "IsMaximized", IsMaximized.ToString() );

      base.WriteXml( writer );
    }


    public override void ReadXml( System.Xml.XmlReader reader )
    {
      if( reader.MoveToAttribute( "DockWidth" ) )
        _dockWidth = ( GridLength )_gridLengthConverter.ConvertFromInvariantString( reader.Value );
      if( reader.MoveToAttribute( "DockHeight" ) )
        _dockHeight = ( GridLength )_gridLengthConverter.ConvertFromInvariantString( reader.Value );

      if( reader.MoveToAttribute( "DockMinWidth" ) )
        _dockMinWidth = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "DockMinHeight" ) )
        _dockMinHeight = double.Parse( reader.Value, CultureInfo.InvariantCulture );

      if( reader.MoveToAttribute( "FloatingWidth" ) )
        _floatingWidth = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "FloatingHeight" ) )
        _floatingHeight = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "FloatingLeft" ) )
        _floatingLeft = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "FloatingTop" ) )
        _floatingTop = double.Parse( reader.Value, CultureInfo.InvariantCulture );
      if( reader.MoveToAttribute( "IsMaximized" ) )
        _isMaximized = bool.Parse( reader.Value );

      base.ReadXml( reader );
    }

    #endregion

    #region Internal Methods

    protected virtual void OnDockWidthChanged()
    {
    }

    protected virtual void OnDockHeightChanged()
    {
    }

    #endregion  
  }
}
