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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Themes;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System;

namespace Xceed.Wpf.AvalonDock.Controls
{
  [TemplatePart( Name = PART_AnchorableListBox, Type = typeof( ListBox ) )]
  [TemplatePart( Name = PART_DocumentListBox, Type = typeof( ListBox ) )]
  public class NavigatorWindow : Window
  {
    #region Members

    private const string PART_AnchorableListBox = "PART_AnchorableListBox";
    private const string PART_DocumentListBox = "PART_DocumentListBox";

    private ResourceDictionary currentThemeResourceDictionary; // = null
    private DockingManager _manager;
    private bool _isSelectingDocument;
    private ListBox _anchorableListBox;
    private ListBox _documentListBox;
    private bool _internalSetSelectedDocument = false;
    private bool _internalSetSelectedAnchorable = false;

    #endregion

    #region Constructors

    static NavigatorWindow()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( NavigatorWindow ), new FrameworkPropertyMetadata( typeof( NavigatorWindow ) ) );
      ShowActivatedProperty.OverrideMetadata( typeof( NavigatorWindow ), new FrameworkPropertyMetadata( false ) );
      ShowInTaskbarProperty.OverrideMetadata( typeof( NavigatorWindow ), new FrameworkPropertyMetadata( false ) );
    }

    internal NavigatorWindow( DockingManager manager )
    {
      _manager = manager;

      _internalSetSelectedDocument = true;
      this.SetAnchorables( _manager.Layout.Descendents().OfType<LayoutAnchorable>().Where( a => a.IsVisible ).Select( d => (LayoutAnchorableItem)_manager.GetLayoutItemFromModel( d ) ).ToArray() );
      this.SetDocuments( _manager.Layout.Descendents().OfType<LayoutDocument>().OrderByDescending( d => d.LastActivationTimeStamp.GetValueOrDefault() ).Select( d => (LayoutDocumentItem)_manager.GetLayoutItemFromModel( d ) ).ToArray() );
      _internalSetSelectedDocument = false;

      if( this.Documents.Length > 1 )
      {
        this.InternalSetSelectedDocument( this.Documents[ 1 ] );
        _isSelectingDocument = true;
      }
      else if( this.Anchorables.Count() > 1 )
      {
        this.InternalSetSelectedAnchorable( this.Anchorables.ToArray()[ 1 ] );
        _isSelectingDocument = false;
      }

      this.DataContext = this;

      this.Loaded += new RoutedEventHandler( OnLoaded );
      this.Unloaded += new RoutedEventHandler( OnUnloaded );

      this.UpdateThemeResources();
    }

    #endregion

    #region Properties

    #region Documents

    /// <summary>
    /// Documents Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey DocumentsPropertyKey = DependencyProperty.RegisterReadOnly( "Documents", typeof( IEnumerable<LayoutDocumentItem> ), typeof( NavigatorWindow ),
            new FrameworkPropertyMetadata( null ) );

    public static readonly DependencyProperty DocumentsProperty = DocumentsPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the Documents property.  This dependency property 
    /// indicates the list of documents.
    /// </summary>
    public LayoutDocumentItem[] Documents
    {
      get
      {
        return (LayoutDocumentItem[])GetValue( DocumentsProperty );
      }
    }

    #endregion

    #region Anchorables

    /// <summary>
    /// Anchorables Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey AnchorablesPropertyKey = DependencyProperty.RegisterReadOnly( "Anchorables", typeof( IEnumerable<LayoutAnchorableItem> ), typeof( NavigatorWindow ),
            new FrameworkPropertyMetadata( (IEnumerable<LayoutAnchorableItem>)null ) );

    public static readonly DependencyProperty AnchorablesProperty = AnchorablesPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the Anchorables property.  This dependency property 
    /// indicates the list of anchorables.
    /// </summary>
    public IEnumerable<LayoutAnchorableItem> Anchorables
    {
      get
      {
        return (IEnumerable<LayoutAnchorableItem>)GetValue( AnchorablesProperty );
      }
    }

    #endregion

    #region SelectedDocument

    /// <summary>
    /// SelectedDocument Dependency Property
    /// </summary>
    public static readonly DependencyProperty SelectedDocumentProperty = DependencyProperty.Register( "SelectedDocument", typeof( LayoutDocumentItem ), typeof( NavigatorWindow ),
            new FrameworkPropertyMetadata( (LayoutDocumentItem)null, new PropertyChangedCallback( OnSelectedDocumentChanged ) ) );

    /// <summary>
    /// Gets or sets the SelectedDocument property.  This dependency property 
    /// indicates the selected document.
    /// </summary>
    public LayoutDocumentItem SelectedDocument
    {
      get
      {
        return (LayoutDocumentItem)GetValue( SelectedDocumentProperty );
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
      ( (NavigatorWindow)d ).OnSelectedDocumentChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the SelectedDocument property.
    /// </summary>
    protected virtual void OnSelectedDocumentChanged( DependencyPropertyChangedEventArgs e )
    {
      if( _internalSetSelectedDocument )
        return;

      if( this.SelectedDocument != null &&
          this.SelectedDocument.ActivateCommand.CanExecute( null ) )
      {
        this.Hide();
        this.SelectedDocument.ActivateCommand.Execute( null );
      }
    }

    #endregion

    #region SelectedAnchorable

    /// <summary>
    /// SelectedAnchorable Dependency Property
    /// </summary>
    public static readonly DependencyProperty SelectedAnchorableProperty = DependencyProperty.Register( "SelectedAnchorable", typeof( LayoutAnchorableItem ), typeof( NavigatorWindow ),
            new FrameworkPropertyMetadata( (LayoutAnchorableItem)null, new PropertyChangedCallback( OnSelectedAnchorableChanged ) ) );

    /// <summary>
    /// Gets or sets the SelectedAnchorable property.  This dependency property 
    /// indicates the selected anchorable.
    /// </summary>
    public LayoutAnchorableItem SelectedAnchorable
    {
      get
      {
        return (LayoutAnchorableItem)GetValue( SelectedAnchorableProperty );
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
      ( (NavigatorWindow)d ).OnSelectedAnchorableChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the SelectedAnchorable property.
    /// </summary>
    protected virtual void OnSelectedAnchorableChanged( DependencyPropertyChangedEventArgs e )
    {
      if( _internalSetSelectedAnchorable )
        return;

      var selectedAnchorable = e.NewValue as LayoutAnchorableItem;
      if( this.SelectedAnchorable != null &&
          this.SelectedAnchorable.ActivateCommand.CanExecute( null ) )
      {
        this.Close();
        this.SelectedAnchorable.ActivateCommand.Execute( null );
      }
    }

    #endregion

    #endregion

    #region Overrides   

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _anchorableListBox = this.GetTemplateChild( PART_AnchorableListBox ) as ListBox;
      _documentListBox = this.GetTemplateChild( PART_DocumentListBox ) as ListBox;
    }

    protected override void OnPreviewKeyDown( System.Windows.Input.KeyEventArgs e )
    {
      bool shouldHandle = false;

      // Press Tab to switch Selected LayoutContent.
      if( ( e.Key == System.Windows.Input.Key.Tab )
        || ( e.Key == System.Windows.Input.Key.Left )
        || ( e.Key == System.Windows.Input.Key.Right )
        || ( e.Key == System.Windows.Input.Key.Up )
        || ( e.Key == System.Windows.Input.Key.Down ) )
      {
        // Selecting LayoutDocuments
        if( _isSelectingDocument )
        {
          if( this.SelectedDocument != null )
          {
            var docIndex = this.Documents.IndexOf<LayoutDocumentItem>( this.SelectedDocument );

            if( e.Key == System.Windows.Input.Key.Tab )
            {
              // Jump to next LayoutDocument
              if( ( docIndex < ( this.Documents.Length - 1 ) )
                || ( this.Anchorables.Count() == 0 ) )
              {
                this.SelectNextDocument();
                shouldHandle = true;
              }
              // Jump to first LayoutAnchorable
              else if( this.Anchorables.Count() > 0 )
              {
                _isSelectingDocument = false;
                this.InternalSetSelectedDocument( null );
                this.InternalSetSelectedAnchorable( this.Anchorables.First() );
                shouldHandle = true;
              }
            }
            else if( e.Key == System.Windows.Input.Key.Down )
            {
              // Jump to next LayoutDocument
              this.SelectNextDocument();
              shouldHandle = true;
            }
            else if( e.Key == System.Windows.Input.Key.Up )
            {
              // Jump to previous LayoutDocument
              this.SelectPreviousDocument();
              shouldHandle = true;
            }
            else if( ( e.Key == System.Windows.Input.Key.Left ) || ( e.Key == System.Windows.Input.Key.Right ) )
            {
              if( this.Anchorables.Count() > 0 )
              {
                _isSelectingDocument = false;
                this.InternalSetSelectedDocument( null );
                if( docIndex < this.Anchorables.Count() )
                {
                  var anchorablesArray = this.Anchorables.ToArray();
                  this.InternalSetSelectedAnchorable( anchorablesArray[ docIndex ] );
                }
                else
                {
                  this.InternalSetSelectedAnchorable( this.Anchorables.Last() );
                }
              }
              shouldHandle = true;
            }
          }
          // There is no SelectedDocument, select the first one.
          else
          {
            if( this.Documents.Length > 0 )
            {
              this.InternalSetSelectedDocument( this.Documents[ 0 ] );
              shouldHandle = true;
            }
          }
        }
        // Selecting LayoutAnchorables
        else
        {
          if( this.SelectedAnchorable != null )
          {
            var anchorableIndex = this.Anchorables.ToArray().IndexOf<LayoutAnchorableItem>( this.SelectedAnchorable );

            if( e.Key == System.Windows.Input.Key.Tab )
            {
              // Jump to next LayoutAnchorable
              if( ( anchorableIndex < ( this.Anchorables.Count() - 1 ) )
                || ( this.Documents.Length == 0 ) )
              {
                this.SelectNextAnchorable();
                shouldHandle = true;
              }
              // Jump to first LayoutDocument
              else if( this.Documents.Length > 0 )
              {
                _isSelectingDocument = true;
                this.InternalSetSelectedAnchorable( null );
                this.InternalSetSelectedDocument( this.Documents[ 0 ] );
                shouldHandle = true;
              }
            }
            else if( e.Key == System.Windows.Input.Key.Down )
            {
              // Jump to next LayoutAnchorable
              this.SelectNextAnchorable();
              shouldHandle = true;
            }
            else if( e.Key == System.Windows.Input.Key.Up )
            {
              // Jump to previous LayoutDocument
              this.SelectPreviousAnchorable();
              shouldHandle = true;
            }
            else if( ( e.Key == System.Windows.Input.Key.Left ) || ( e.Key == System.Windows.Input.Key.Right ) )
            {
              if( this.Documents.Count() > 0 )
              {
                _isSelectingDocument = true;
                this.InternalSetSelectedAnchorable( null );
                if( anchorableIndex < this.Documents.Count() )
                {
                  this.InternalSetSelectedDocument( this.Documents[ anchorableIndex ] );
                }
                else
                {
                  this.InternalSetSelectedDocument( this.Documents.Last() );
                }
              }
              shouldHandle = true;
            }
          }
          // There is no SelectedAnchorable, select the first one.
          else
          {
            if( this.Anchorables.Count() > 0 )
            {
              this.InternalSetSelectedAnchorable( this.Anchorables.ToArray()[ 0 ] );
              shouldHandle = true;
            }
          }
        }
      }

      if( shouldHandle )
      {
        e.Handled = true;
      }
      base.OnPreviewKeyDown( e );
    }

    protected override void OnPreviewKeyUp( System.Windows.Input.KeyEventArgs e )
    {
      if( ( e.Key != System.Windows.Input.Key.Tab )
        && ( e.Key != System.Windows.Input.Key.Left )
        && ( e.Key != System.Windows.Input.Key.Right )
        && ( e.Key != System.Windows.Input.Key.Up )
        && ( e.Key != System.Windows.Input.Key.Down ) )
      {
        this.Close();

        if( this.SelectedDocument != null &&
           this.SelectedDocument.ActivateCommand.CanExecute( null ) )
        {
          this.SelectedDocument.ActivateCommand.Execute( null );
          this.FocusContent( this.SelectedDocument );
        }

        if( this.SelectedDocument == null &&
            this.SelectedAnchorable != null &&
            this.SelectedAnchorable.ActivateCommand.CanExecute( null ) )
        {
          this.SelectedAnchorable.ActivateCommand.Execute( null );
          this.FocusContent( this.SelectedAnchorable );
        }

        e.Handled = true;
      }

      base.OnPreviewKeyUp( e );
    }


    #endregion

    #region Internal Methods

    /// <summary>
    /// Provides a secure method for setting the Anchorables property.  
    /// This dependency property indicates the list of anchorables.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetAnchorables( IEnumerable<LayoutAnchorableItem> value )
    {
      this.SetValue( AnchorablesPropertyKey, value );
    }

    /// <summary>
    /// Provides a secure method for setting the Documents property.  
    /// This dependency property indicates the list of documents.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetDocuments( LayoutDocumentItem[] value )
    {
      this.SetValue( DocumentsPropertyKey, value );
    }

    internal void UpdateThemeResources( Theme oldTheme = null )
    {
      if( oldTheme != null )
      {
        if( oldTheme is DictionaryTheme )
        {
          if( currentThemeResourceDictionary != null )
          {
            this.Resources.MergedDictionaries.Remove( currentThemeResourceDictionary );
            currentThemeResourceDictionary = null;
          }
        }
        else
        {
          var resourceDictionaryToRemove = this.Resources.MergedDictionaries.FirstOrDefault( r => r.Source == oldTheme.GetResourceUri() );
          if( resourceDictionaryToRemove != null )
          {
            this.Resources.MergedDictionaries.Remove( resourceDictionaryToRemove );
          }
        }
      }

      if( _manager.Theme != null )
      {
        if( _manager.Theme is DictionaryTheme )
        {
          currentThemeResourceDictionary = ( (DictionaryTheme)_manager.Theme ).ThemeResourceDictionary;
          this.Resources.MergedDictionaries.Add( currentThemeResourceDictionary );
        }
        else
        {
          this.Resources.MergedDictionaries.Add( new ResourceDictionary() { Source = _manager.Theme.GetResourceUri() } );
        }
      }
    }

    internal void SelectNextDocument()
    {
      if( this.SelectedDocument != null )
      {
        int docIndex = this.Documents.IndexOf<LayoutDocumentItem>( this.SelectedDocument );
        docIndex++;
        if( docIndex == this.Documents.Length )
        {
          docIndex = 0;
        }
        this.InternalSetSelectedDocument( this.Documents[ docIndex ] );
      }
    }

    internal void SelectPreviousDocument()
    {
      if( this.SelectedDocument != null )
      {
        int docIndex = this.Documents.IndexOf<LayoutDocumentItem>( this.SelectedDocument );
        docIndex--;
        if( docIndex == -1 )
        {
          docIndex = this.Documents.Length - 1;
        }
        this.InternalSetSelectedDocument( this.Documents[ docIndex ] );
      }
    }

    internal void SelectNextAnchorable()
    {
      if( this.SelectedAnchorable != null )
      {
        var anchorablesArray = this.Anchorables.ToArray();
        int anchorableIndex = anchorablesArray.IndexOf<LayoutAnchorableItem>( this.SelectedAnchorable );
        anchorableIndex++;
        if( anchorableIndex == this.Anchorables.Count() )
        {
          anchorableIndex = 0;
        }
        this.InternalSetSelectedAnchorable( anchorablesArray[ anchorableIndex ] );
      }
    }

    internal void SelectPreviousAnchorable()
    {
      if( this.SelectedAnchorable != null )
      {
        var anchorablesArray = this.Anchorables.ToArray();
        int anchorableIndex = anchorablesArray.IndexOf<LayoutAnchorableItem>( this.SelectedAnchorable );
        anchorableIndex--;
        if( anchorableIndex == -1 )
        {
          anchorableIndex = this.Anchorables.Count() - 1;
        }
        this.InternalSetSelectedAnchorable( anchorablesArray[ anchorableIndex ] );
      }
    }

    #endregion

    #region Private Methods

    private void InternalSetSelectedAnchorable( LayoutAnchorableItem anchorableToSelect )
    {
      _internalSetSelectedAnchorable = true;
      this.SelectedAnchorable = anchorableToSelect;
      _internalSetSelectedAnchorable = false;

      if( _anchorableListBox != null )
      {
        _anchorableListBox.Focus();
      }
    }

    private void InternalSetSelectedDocument( LayoutDocumentItem documentToSelect )
    {
      _internalSetSelectedDocument = true;
      this.SelectedDocument = documentToSelect;
      _internalSetSelectedDocument = false;

      if( ( _documentListBox != null ) && ( documentToSelect != null ) )
      {
        _documentListBox.Focus();
      }
    }

    private void FocusContent( LayoutItem layoutItem )
    {
      if( ( layoutItem == null ) || ( layoutItem.LayoutElement == null ) )
        return;

      // Set focus inside selected LayoutItem.
      var content = layoutItem.LayoutElement.Content as UIElement;
      if( content != null )
      {
        this.Dispatcher.BeginInvoke( DispatcherPriority.Input, new Action( () =>
        {
          if( content.Focusable )
          {
            content.Focus();
          }
          else
          {
            content.MoveFocus( new TraversalRequest( FocusNavigationDirection.Next ) );
          }
        }
         ) );

      }
    }

    private void OnLoaded( object sender, RoutedEventArgs e )
    {
      this.Loaded -= new RoutedEventHandler( OnLoaded );

      if( ( _documentListBox != null ) && ( this.SelectedDocument != null ) )
      {
        _documentListBox.Focus();
      }
      else if( ( _anchorableListBox != null ) && ( this.SelectedAnchorable != null ) )
      {
        _anchorableListBox.Focus();
      }

      WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    private void OnUnloaded( object sender, RoutedEventArgs e )
    {
      this.Unloaded -= new RoutedEventHandler( OnUnloaded );
    }

    #endregion
  }
}
