/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Themes;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class NavigatorWindow : Window
  {
    private ResourceDictionary currentThemeResourceDictionary; // = null

    static NavigatorWindow()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( NavigatorWindow ), new FrameworkPropertyMetadata( typeof( NavigatorWindow ) ) );
      ShowActivatedProperty.OverrideMetadata( typeof( NavigatorWindow ), new FrameworkPropertyMetadata( false ) );
      ShowInTaskbarProperty.OverrideMetadata( typeof( NavigatorWindow ), new FrameworkPropertyMetadata( false ) );
    }

    DockingManager _manager;
    internal NavigatorWindow( DockingManager manager )
    {
      _manager = manager;

      _internalSetSelectedDocument = true;
      SetAnchorables( _manager.Layout.Descendents().OfType<LayoutAnchorable>().Where( a => a.IsVisible ).Select( d => ( LayoutAnchorableItem )_manager.GetLayoutItemFromModel( d ) ).ToArray() );
      SetDocuments( _manager.Layout.Descendents().OfType<LayoutDocument>().OrderByDescending( d => d.LastActivationTimeStamp.GetValueOrDefault() ).Select( d => ( LayoutDocumentItem )_manager.GetLayoutItemFromModel( d ) ).ToArray() );
      _internalSetSelectedDocument = false;

      if( Documents.Length > 1 )
        InternalSetSelectedDocument( Documents[ 1 ] );

      this.DataContext = this;

      this.Loaded += new RoutedEventHandler( OnLoaded );
      this.Unloaded += new RoutedEventHandler( OnUnloaded );

      UpdateThemeResources();
    }


    internal void UpdateThemeResources( Theme oldTheme = null )
    {
      if( oldTheme != null )
      {
        if( oldTheme is DictionaryTheme )
        {
          if( currentThemeResourceDictionary != null )
          {
            Resources.MergedDictionaries.Remove( currentThemeResourceDictionary );
            currentThemeResourceDictionary = null;
          }
        }
        else
        {
          var resourceDictionaryToRemove =
              Resources.MergedDictionaries.FirstOrDefault( r => r.Source == oldTheme.GetResourceUri() );
          if( resourceDictionaryToRemove != null )
            Resources.MergedDictionaries.Remove(
                resourceDictionaryToRemove );
        }
      }

      if( _manager.Theme != null )
      {
        if( _manager.Theme is DictionaryTheme )
        {
          currentThemeResourceDictionary = ( ( DictionaryTheme )_manager.Theme ).ThemeResourceDictionary;
          Resources.MergedDictionaries.Add( currentThemeResourceDictionary );
        }
        else
        {
          Resources.MergedDictionaries.Add( new ResourceDictionary() { Source = _manager.Theme.GetResourceUri() } );
        }
      }
    }


    void OnLoaded( object sender, RoutedEventArgs e )
    {
      this.Loaded -= new RoutedEventHandler( OnLoaded );

      this.Focus();

      //this.SetParentToMainWindowOf(_manager);
      WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    void OnUnloaded( object sender, RoutedEventArgs e )
    {
      this.Unloaded -= new RoutedEventHandler( OnUnloaded );

      //_hwndSrc.RemoveHook(_hwndSrcHook);
      //_hwndSrc.Dispose();
      //_hwndSrc = null;
    }

    //protected virtual IntPtr FilterMessage(
    //    IntPtr hwnd,
    //    int msg,
    //    IntPtr wParam,
    //    IntPtr lParam,
    //    ref bool handled
    //    )
    //{
    //    handled = false;

    //    switch (msg)
    //    {
    //        case Win32Helper.WM_ACTIVATE:
    //            if (((int)wParam & 0xFFFF) == Win32Helper.WA_INACTIVE)
    //            {
    //                if (lParam == new WindowInteropHelper(this.Owner).Handle)
    //                {
    //                    Win32Helper.SetActiveWindow(_hwndSrc.Handle);
    //                    handled = true;
    //                }

    //            }
    //            break;
    //    }

    //    return IntPtr.Zero;
    //}


    #region Documents

    /// <summary>
    /// Documents Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey DocumentsPropertyKey
        = DependencyProperty.RegisterReadOnly( "Documents", typeof( IEnumerable<LayoutDocumentItem> ), typeof( NavigatorWindow ),
            new FrameworkPropertyMetadata( null ) );

    public static readonly DependencyProperty DocumentsProperty
        = DocumentsPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the Documents property.  This dependency property 
    /// indicates the list of documents.
    /// </summary>
    public LayoutDocumentItem[] Documents
    {
      get
      {
        return ( LayoutDocumentItem[] )GetValue( DocumentsProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the Documents property.  
    /// This dependency property indicates the list of documents.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetDocuments( LayoutDocumentItem[] value )
    {
      SetValue( DocumentsPropertyKey, value );
    }

    #endregion

    #region Anchorables

    /// <summary>
    /// Anchorables Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey AnchorablesPropertyKey
        = DependencyProperty.RegisterReadOnly( "Anchorables", typeof( IEnumerable<LayoutAnchorableItem> ), typeof( NavigatorWindow ),
            new FrameworkPropertyMetadata( ( IEnumerable<LayoutAnchorableItem> )null ) );

    public static readonly DependencyProperty AnchorablesProperty
        = AnchorablesPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the Anchorables property.  This dependency property 
    /// indicates the list of anchorables.
    /// </summary>
    public IEnumerable<LayoutAnchorableItem> Anchorables
    {
      get
      {
        return ( IEnumerable<LayoutAnchorableItem> )GetValue( AnchorablesProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the Anchorables property.  
    /// This dependency property indicates the list of anchorables.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetAnchorables( IEnumerable<LayoutAnchorableItem> value )
    {
      SetValue( AnchorablesPropertyKey, value );
    }

    #endregion

    #region SelectedDocument

    /// <summary>
    /// SelectedDocument Dependency Property
    /// </summary>
    public static readonly DependencyProperty SelectedDocumentProperty =
        DependencyProperty.Register( "SelectedDocument", typeof( LayoutDocumentItem ), typeof( NavigatorWindow ),
            new FrameworkPropertyMetadata( ( LayoutDocumentItem )null,
                new PropertyChangedCallback( OnSelectedDocumentChanged ) ) );

    /// <summary>
    /// Gets or sets the SelectedDocument property.  This dependency property 
    /// indicates the selected document.
    /// </summary>
    public LayoutDocumentItem SelectedDocument
    {
      get
      {
        return ( LayoutDocumentItem )GetValue( SelectedDocumentProperty );
      }
      set
      {
        SetValue( SelectedDocumentProperty, value );
      }
    }

    /// <summary>
    /// Handles changes to the SelectedDocument property.
    /// </summary>
    private static void OnSelectedDocumentChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( NavigatorWindow )d ).OnSelectedDocumentChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the SelectedDocument property.
    /// </summary>
    protected virtual void OnSelectedDocumentChanged( DependencyPropertyChangedEventArgs e )
    {
      if( _internalSetSelectedDocument )
        return;

      if( SelectedDocument != null &&
          SelectedDocument.ActivateCommand.CanExecute( null ) )
      {
        SelectedDocument.ActivateCommand.Execute( null );
        Hide();
      }

    }

    bool _internalSetSelectedDocument = false;
    void InternalSetSelectedDocument( LayoutDocumentItem documentToSelect )
    {
      _internalSetSelectedDocument = true;
      SelectedDocument = documentToSelect;
      _internalSetSelectedDocument = false;
    }

    #endregion

    #region SelectedAnchorable

    /// <summary>
    /// SelectedAnchorable Dependency Property
    /// </summary>
    public static readonly DependencyProperty SelectedAnchorableProperty =
        DependencyProperty.Register( "SelectedAnchorable", typeof( LayoutAnchorableItem ), typeof( NavigatorWindow ),
            new FrameworkPropertyMetadata( ( LayoutAnchorableItem )null,
                new PropertyChangedCallback( OnSelectedAnchorableChanged ) ) );

    /// <summary>
    /// Gets or sets the SelectedAnchorable property.  This dependency property 
    /// indicates the selected anchorable.
    /// </summary>
    public LayoutAnchorableItem SelectedAnchorable
    {
      get
      {
        return ( LayoutAnchorableItem )GetValue( SelectedAnchorableProperty );
      }
      set
      {
        SetValue( SelectedAnchorableProperty, value );
      }
    }

    /// <summary>
    /// Handles changes to the SelectedAnchorable property.
    /// </summary>
    private static void OnSelectedAnchorableChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( NavigatorWindow )d ).OnSelectedAnchorableChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the SelectedAnchorable property.
    /// </summary>
    protected virtual void OnSelectedAnchorableChanged( DependencyPropertyChangedEventArgs e )
    {
      var selectedAnchorable = e.NewValue as LayoutAnchorableItem;
      if( SelectedAnchorable != null &&
          SelectedAnchorable.ActivateCommand.CanExecute( null ) )
      {
        SelectedAnchorable.ActivateCommand.Execute( null );
        Close();
      }
    }

    #endregion


    internal void SelectNextDocument()
    {
      if( SelectedDocument != null )
      {
        int docIndex = Documents.IndexOf<LayoutDocumentItem>( SelectedDocument );
        docIndex++;
        if( docIndex == Documents.Length )
          docIndex = 0;
        InternalSetSelectedDocument( Documents[ docIndex ] );
      }

    }

    protected override void OnKeyDown( System.Windows.Input.KeyEventArgs e )
    {
      base.OnKeyDown( e );
    }

    protected override void OnPreviewKeyDown( System.Windows.Input.KeyEventArgs e )
    {
      if( e.Key == System.Windows.Input.Key.Tab )
      {
        SelectNextDocument();
        e.Handled = true;
      }


      base.OnPreviewKeyDown( e );
    }

    protected override void OnPreviewKeyUp( System.Windows.Input.KeyEventArgs e )
    {
      if( e.Key != System.Windows.Input.Key.Tab )
      {
        if( SelectedAnchorable != null &&
            SelectedAnchorable.ActivateCommand.CanExecute( null ) )
          SelectedAnchorable.ActivateCommand.Execute( null );

        if( SelectedAnchorable == null &&
            SelectedDocument != null &&
            SelectedDocument.ActivateCommand.CanExecute( null ) )
          SelectedDocument.ActivateCommand.Execute( null );
        Close();
        e.Handled = true;
      }


      base.OnPreviewKeyUp( e );
    }


  }
}
