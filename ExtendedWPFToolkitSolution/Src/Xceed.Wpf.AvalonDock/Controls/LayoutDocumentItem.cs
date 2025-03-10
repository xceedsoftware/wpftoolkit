/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Commands;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutDocumentItem : LayoutItem
  {
    #region Members

    private LayoutDocument _document;

    #endregion

    #region Constructors

    internal LayoutDocumentItem()
    {
    }

    #endregion

    #region Properties

    #region Description

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register( "Description", typeof( string ), typeof( LayoutDocumentItem ),
                new FrameworkPropertyMetadata( ( string )null, new PropertyChangedCallback( OnDescriptionChanged ) ) );

    public string Description
    {
      get
      {
        return ( string )GetValue( DescriptionProperty );
      }
      set
      {
        SetValue( DescriptionProperty, value );
      }
    }

    private static void OnDescriptionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutDocumentItem )d ).OnDescriptionChanged( e );
    }

    protected virtual void OnDescriptionChanged( DependencyPropertyChangedEventArgs e )
    {
      _document.Description = ( string )e.NewValue;
    }

    #endregion

    #endregion

    #region Overrides

    protected override void Close()
    {
      if( ( _document.Root != null ) && ( _document.Root.Manager != null ) )
      {
        var dockingManager = _document.Root.Manager;
        dockingManager._ExecuteCloseCommand( _document );
      }
    }

    protected override void OnVisibilityChanged()
    {
      if( ( _document != null ) && ( _document.Root != null ) )
      {
        _document.IsVisible = ( this.Visibility == Visibility.Visible );

        if( _document.Parent is LayoutDocumentPane )
        {
          ( ( LayoutDocumentPane )_document.Parent ).ComputeVisibility();
        }
      }

      base.OnVisibilityChanged();
    }









    internal override void Attach( LayoutContent model )
    {
      _document = model as LayoutDocument;
      base.Attach( model );
    }

    internal override void Detach()
    {
      _document = null;
      base.Detach();
    }

    #endregion

    #region Private Methods







    #endregion
  }
}
