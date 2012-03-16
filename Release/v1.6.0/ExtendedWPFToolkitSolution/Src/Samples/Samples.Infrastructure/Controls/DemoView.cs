/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Samples.Infrastructure.Controls
{
  public class DemoView : ContentControl
  {
    #region Members

    readonly string _applicationPath = String.Empty;
    const string _samplesFolderName = "Samples";

    #endregion //Members

    #region Properties

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

    #endregion //Properties

    #region Constructors

    static DemoView()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( DemoView ), new FrameworkPropertyMetadata( typeof( DemoView ) ) );
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
      ResolveContentCode( ( newContent as FrameworkElement ).Parent );
    }

    #endregion //Base Class Overrides

    #region Methods

    private void ResolveContentCode( object newContent )
    {
      //get the type of the content loaded in the ContentRegion
      var type = newContent.GetType();
      //grab only the name of the content which will correspond to the name of the file to load
      var viewName = type.FullName.Substring( type.FullName.LastIndexOf( "." ) + 1 );
      //get the module name
      var moduleName = type.Module.Name.Replace( ".dll", String.Empty );

      SetText( viewName, moduleName );
    }

    private void SetText( string viewName, string moduleName )
    {
      SetCSharpText( viewName, moduleName );
      SetXamlText( viewName, moduleName );
    }

    private void SetCSharpText( string viewName, string moduleName )
    {
      //now we need to append the file extension
      string fileName = String.Format( "{0}.xaml", viewName );
      string filePath = GetFilePath( moduleName, fileName );
      XamlText = ReadFileText( filePath );
    }

    private void SetXamlText( string viewName, string moduleName )
    {
      //now we need to append the file extension
      string fileName = String.Format( "{0}.xaml.cs", viewName );
      string filePath = GetFilePath( moduleName, fileName );
      CSharpText = ReadFileText( filePath );
    }

    private string GetFilePath( string moduleName, string fileName )
    {
      return Path.Combine( _applicationPath, _samplesFolderName, moduleName, fileName );
    }

    private static string ReadFileText( string filePath )
    {
      string text = String.Empty;
      if( File.Exists( filePath ) )
        text = File.ReadAllText( filePath );
      return text;
    }

    #endregion //Methods
  }
}
