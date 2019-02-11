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
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Xceed.Wpf.DataGrid.Views
{
  public abstract class UIViewBase : ViewBase
  {
    #region Static Fields

    // We must add a setter since this value is used as default
    // value of a DependencyObject which will be instanciated
    // before the static constructor is called
    internal static Cursor DefaultGroupDraggedOutsideCursor
    {
      get
      {
        if( UIViewBase.DefaultGroupDraggedOutsiedCursorCache == null )
        {
          try
          {
            var uri = new Uri( _XceedVersionInfo.CurrentAssemblyPackUri + ";component/NoDrop.cur" );
            var info = Application.GetResourceStream( uri );

            if( info != null )
            {
              UIViewBase.DefaultGroupDraggedOutsiedCursorCache = new Cursor( info.Stream );
            }
          }
          catch( SecurityException )
          {
          }
          catch( UriFormatException )
          {
          }
          catch( IOException )
          {
          }
          finally
          {
            if( UIViewBase.DefaultGroupDraggedOutsiedCursorCache == null )
            {
              UIViewBase.DefaultGroupDraggedOutsiedCursorCache = Cursors.No;
            }
          }
        }

        return UIViewBase.DefaultGroupDraggedOutsiedCursorCache;
      }
    }

    private static Cursor DefaultGroupDraggedOutsiedCursorCache; // = null;

    internal static Cursor DefaultColumnResizeWestEastCursor = Cursors.SizeWE;
    internal static Cursor DefaultCannotDropDraggedElementCursor = Cursors.No;

    #endregion

    #region DropMarkPen Attached Property

    public static readonly DependencyProperty DropMarkPenProperty = DependencyProperty.RegisterAttached(
      "DropMarkPen",
      typeof( Pen ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static Pen GetDropMarkPen( DependencyObject obj )
    {
      return ( Pen )obj.GetValue( UIViewBase.DropMarkPenProperty );
    }

    public static void SetDropMarkPen( DependencyObject obj, Pen value )
    {
      obj.SetValue( UIViewBase.DropMarkPenProperty, value );
    }

    #endregion

    #region DropMarkOrientation Attached Property

    public static readonly DependencyProperty DropMarkOrientationProperty = DependencyProperty.RegisterAttached(
      "DropMarkOrientation",
      typeof( DropMarkOrientation ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( DropMarkOrientation.Default, FrameworkPropertyMetadataOptions.Inherits ) );

    public static DropMarkOrientation GetDropMarkOrientation( DependencyObject obj )
    {
      return ( DropMarkOrientation )obj.GetValue( UIViewBase.DropMarkOrientationProperty );
    }

    public static void SetDropMarkOrientation( DependencyObject obj, DropMarkOrientation value )
    {
      obj.SetValue( UIViewBase.DropMarkOrientationProperty, value );
    }

    #endregion

    #region ShowScrollTip Property

    public static readonly DependencyProperty ShowScrollTipProperty = DependencyProperty.Register(
      "ShowScrollTip",
      typeof( bool ),
      typeof( UIViewBase ) );

    public bool ShowScrollTip
    {
      get
      {
        return ( bool )this.GetValue( UIViewBase.ShowScrollTipProperty );
      }
      set
      {
        this.SetValue( UIViewBase.ShowScrollTipProperty, value );
      }
    }

    #endregion

    #region DefaultDropMarkPen Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty DefaultDropMarkPenProperty = DependencyProperty.Register(
      "DefaultDropMarkPen",
      typeof( Pen ),
      typeof( UIViewBase ),
      new UIPropertyMetadata( null ) );

    public Pen DefaultDropMarkPen
    {
      get
      {
        return ( Pen )this.GetValue( UIViewBase.DefaultDropMarkPenProperty );
      }
      set
      {
        this.SetValue( UIViewBase.DefaultDropMarkPenProperty, value );
      }
    }

    #endregion

    #region DefaultDropMarkOrientation Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty DefaultDropMarkOrientationProperty = DependencyProperty.Register(
      "DefaultDropMarkOrientation",
      typeof( DropMarkOrientation ),
      typeof( UIViewBase ),
      new UIPropertyMetadata( DropMarkOrientation.Vertical ) );

    public DropMarkOrientation DefaultDropMarkOrientation
    {
      get
      {
        return ( DropMarkOrientation )this.GetValue( UIViewBase.DefaultDropMarkOrientationProperty );
      }
      set
      {
        this.SetValue( UIViewBase.DefaultDropMarkOrientationProperty, value );
      }
    }

    #endregion

    #region CurrentItemGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty CurrentItemGlyphProperty = DependencyProperty.Register(
      "CurrentItemGlyph",
      typeof( DataTemplate ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate CurrentItemGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( UIViewBase.CurrentItemGlyphProperty );
      }
      set
      {
        this.SetValue( UIViewBase.CurrentItemGlyphProperty, value );
      }
    }

    #endregion

    #region EditingRowGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty EditingRowGlyphProperty = DependencyProperty.Register(
      "EditingRowGlyph",
      typeof( DataTemplate ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate EditingRowGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( UIViewBase.EditingRowGlyphProperty );
      }
      set
      {
        this.SetValue( UIViewBase.EditingRowGlyphProperty, value );
      }
    }

    #endregion

    #region ValidationErrorGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ValidationErrorGlyphProperty = DependencyProperty.Register(
      "ValidationErrorGlyph",
      typeof( DataTemplate ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate ValidationErrorGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( UIViewBase.ValidationErrorGlyphProperty );
      }
      set
      {
        this.SetValue( UIViewBase.ValidationErrorGlyphProperty, value );
      }
    }

    #endregion

    #region ScrollTipContentTemplate Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ScrollTipContentTemplateProperty = DependencyProperty.Register(
      "ScrollTipContentTemplate",
      typeof( DataTemplate ),
      typeof( UIViewBase ) );

    public DataTemplate ScrollTipContentTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( UIViewBase.ScrollTipContentTemplateProperty );
      }
      set
      {
        this.SetValue( UIViewBase.ScrollTipContentTemplateProperty, value );
      }
    }

    #endregion

    #region ScrollTipContentTemplateSelector Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ScrollTipContentTemplateSelectorProperty = DependencyProperty.Register(
      "ScrollTipContentTemplateSelector",
      typeof( DataTemplateSelector ),
      typeof( UIViewBase ) );

    public DataTemplateSelector ScrollTipContentTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( UIViewBase.ScrollTipContentTemplateSelectorProperty );
      }
      set
      {
        this.SetValue( UIViewBase.ScrollTipContentTemplateSelectorProperty, value );
      }
    }

    #endregion

    #region IsConnectionStateGlyphEnabled Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty IsConnectionStateGlyphEnabledProperty = DependencyProperty.Register(
      "IsConnectionStateGlyphEnabled",
      typeof( bool ),
      typeof( UIViewBase ),
      new PropertyMetadata( true ) );

    public bool IsConnectionStateGlyphEnabled
    {
      get
      {
        return ( bool )this.GetValue( UIViewBase.IsConnectionStateGlyphEnabledProperty );
      }
      set
      {
        this.SetValue( UIViewBase.IsConnectionStateGlyphEnabledProperty, value );
      }
    }

    #endregion

    #region ConnectionStateLoadingGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ConnectionStateLoadingGlyphProperty = DependencyProperty.Register(
      "ConnectionStateLoadingGlyph",
      typeof( DataTemplate ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate ConnectionStateLoadingGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( UIViewBase.ConnectionStateLoadingGlyphProperty );
      }
      set
      {
        this.SetValue( UIViewBase.ConnectionStateLoadingGlyphProperty, value );
      }
    }

    #endregion

    #region ConnectionStateCommittingGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ConnectionStateCommittingGlyphProperty = DependencyProperty.Register(
      "ConnectionStateCommittingGlyph",
      typeof( DataTemplate ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate ConnectionStateCommittingGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( UIViewBase.ConnectionStateCommittingGlyphProperty );
      }
      set
      {
        this.SetValue( UIViewBase.ConnectionStateCommittingGlyphProperty, value );
      }
    }

    #endregion

    #region ConnectionStateErrorGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ConnectionStateErrorGlyphProperty = DependencyProperty.Register(
      "ConnectionStateErrorGlyph",
      typeof( DataTemplate ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate ConnectionStateErrorGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( UIViewBase.ConnectionStateErrorGlyphProperty );
      }
      set
      {
        this.SetValue( UIViewBase.ConnectionStateErrorGlyphProperty, value );
      }
    }

    #endregion

    #region BusyCursor Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty BusyCursorProperty = DependencyProperty.Register(
      "BusyCursor",
      typeof( Cursor ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( Cursors.Wait ) );

    public Cursor BusyCursor
    {
      get
      {
        return ( Cursor )this.GetValue( UIViewBase.BusyCursorProperty );
      }
      set
      {
        this.SetValue( UIViewBase.BusyCursorProperty, value );
      }
    }

    #endregion

    #region CannotDropDraggedElementCursor Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty CannotDropDraggedElementCursorProperty = DependencyProperty.Register(
      "CannotDropDraggedElementCursor",
      typeof( Cursor ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( Cursors.No ) );

    public Cursor CannotDropDraggedElementCursor
    {
      get
      {
        return ( Cursor )this.GetValue( UIViewBase.CannotDropDraggedElementCursorProperty );
      }
      set
      {
        this.SetValue( UIViewBase.CannotDropDraggedElementCursorProperty, value );
      }
    }

    #endregion

    #region ColumnResizeWestEastCursor Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ColumnResizeWestEastCursorProperty = DependencyProperty.Register(
      "ColumnResizeWestEastCursor",
      typeof( Cursor ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( UIViewBase.DefaultColumnResizeWestEastCursor ) );

    public Cursor ColumnResizeWestEastCursor
    {
      get
      {
        return ( Cursor )this.GetValue( UIViewBase.ColumnResizeWestEastCursorProperty );
      }
      set
      {
        this.SetValue( UIViewBase.ColumnResizeWestEastCursorProperty, value );
      }
    }

    #endregion

    #region RemovingGroupCursor Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty RemovingGroupCursorProperty = DependencyProperty.Register(
      "RemovingGroupCursor",
      typeof( Cursor ),
      typeof( UIViewBase ),
      new FrameworkPropertyMetadata( UIViewBase.DefaultGroupDraggedOutsideCursor ) );

    public Cursor RemovingGroupCursor
    {
      get
      {
        return ( Cursor )this.GetValue( UIViewBase.RemovingGroupCursorProperty );
      }
      set
      {
        this.SetValue( UIViewBase.RemovingGroupCursorProperty, value );
      }
    }

    #endregion
  }
}
