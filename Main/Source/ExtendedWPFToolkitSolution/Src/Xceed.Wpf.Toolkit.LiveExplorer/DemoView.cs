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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.IO;
using System.Windows.Documents;
using System.Diagnostics;

namespace Xceed.Wpf.Toolkit.LiveExplorer
{
  public class DemoView : ContentControl
  {
    #region Members

    readonly string _applicationPath = String.Empty;
    const string _samplesFolderName = "Samples";

    #endregion //Members

    #region Properties

    #region CSharpText

    public static readonly DependencyProperty CSharpTextProperty = DependencyProperty.Register( "CSharpText", typeof( string ), typeof( DemoView ), new UIPropertyMetadata( null ) );
    public string CSharpText
    {
      get
      {
        return ( string )GetValue( CSharpTextProperty );
      }
      set
      {
        SetValue( CSharpTextProperty, value );
      }
    }

    #endregion //CSharpText

    #region Description

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register( "Description", typeof( Paragraph ), typeof( DemoView ), new UIPropertyMetadata( null ) );
    public Paragraph Description
    {
      get
      {
        return ( Paragraph )GetValue( DescriptionProperty );
      }
      set
      {
        SetValue( DescriptionProperty, value );
      }
    }

    #endregion //Description

    #region Title

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register( "Title", typeof( string ), typeof( DemoView ), new UIPropertyMetadata( null ) );
    public string Title
    {
      get
      {
        return ( string )GetValue( TitleProperty );
      }
      set
      {
        SetValue( TitleProperty, value );
      }
    }

    #endregion // Title

    #region VerticalScrollbarVisibility

    public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register( "VerticalScrollBarVisibility", typeof( ScrollBarVisibility ), typeof( DemoView ), new UIPropertyMetadata( ScrollBarVisibility.Auto ) );
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
      get
      {
        return ( ScrollBarVisibility )GetValue( VerticalScrollBarVisibilityProperty );
      }
      set
      {
        SetValue( VerticalScrollBarVisibilityProperty, value );
      }
    }

    #endregion

    #region XamlText

    public static readonly DependencyProperty XamlTextProperty = DependencyProperty.Register( "XamlText", typeof( string ), typeof( DemoView ), new UIPropertyMetadata( null ) );
    public string XamlText
    {
      get
      {
        return ( string )GetValue( XamlTextProperty );
      }
      set
      {
        SetValue( XamlTextProperty, value );
      }
    }

    #endregion // XamlText

    #endregion //Properties

    #region Constructors

    static DemoView()
    {
    }

    public DemoView()
    {
      _applicationPath = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override void OnContentChanged( object oldContent, object newContent )
    {
      base.OnContentChanged( oldContent, newContent );
      //the parent of the content will be the View
      this.UpdateContentCode( ( newContent as FrameworkElement ).Parent );
    }

    #endregion //Base Class Overrides

    #region Methods

    private void UpdateContentCode( object newContent )
    {
      //get the type of the content loaded in the ContentRegion
      var type = newContent.GetType();
      //grab only the name of the content which will correspond to the name of the file to load
      var viewName = type.FullName.Substring( type.FullName.LastIndexOf( "." ) + 1 );
      //get the module name
      var moduleName = type.Module.Name.Replace( ".dll", String.Empty );

      this.SetText( viewName, moduleName );
    }

    private void SetText( string viewName, string moduleName )
    {
      this.SetCSharpText( viewName, moduleName );
      this.SetXamlText( viewName, moduleName );
    }

    private void SetCSharpText( string viewName, string moduleName )
    {
      //now we need to append the file extension
      string fileName = String.Format( "{0}.xaml", viewName );
      string filePath = this.GetFilePath( moduleName, fileName );
      this.XamlText = ReadFileText( filePath );
    }

    private void SetXamlText( string viewName, string moduleName )
    {
      //now we need to append the file extension
      string fileName = String.Format( "{0}.xaml.cs", viewName );
      string filePath = this.GetFilePath( moduleName, fileName );
      this.CSharpText = ReadFileText( filePath );
    }

    private string GetFilePath( string moduleName, string fileName )
    {
      return Path.Combine( _applicationPath, _samplesFolderName, fileName );
    }

    private static string ReadFileText( string filePath )
    {
      string text = String.Empty;
      if( File.Exists( filePath ) )
        text = File.ReadAllText( filePath );
      return text;
    }

    internal void Hyperlink_RequestNavigate( object sender, System.Windows.Navigation.RequestNavigateEventArgs e )
    {
      Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
      e.Handled = true;
    }

    #endregion // Methods

  }
}
