/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

#if DEBUG

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Xml;

using Xceed.Wpf.DataGrid.FilterCriteria;

namespace Xceed.Wpf.DataGrid
{
  public class FilterParserTestWindow : Window
  {
    public FilterParserTestWindow()
    {
      string mainGridXaml =
@"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        xmlns:s=""clr-namespace:System;assembly=mscorlib""
        Margin=""5"">
    <Grid.Resources>
      <Style TargetType=""Button"">
        <Setter Property=""MinWidth"" Value=""100"" />
      </Style>
    </Grid.Resources>

    <Grid.RowDefinitions>
      <RowDefinition Height=""Auto"" />
      <RowDefinition Height=""Auto"" />
      <RowDefinition Height=""*"" />
      <RowDefinition Height=""3*"" />
      <RowDefinition Height=""Auto"" />
    </Grid.RowDefinitions>

    <TextBlock MaxWidth=""400""
               HorizontalAlignment=""Left""
               TextWrapping=""Wrap""
               Margin=""0,0,0,5""
               Grid.Row=""0"">
Here, you can test various expression that will be parsed. 
The result of the parsing will be shown in the second TextBox.
Don't forget to check the Output Window for errors that could have occured during the
automated tests. Untested expressions by the automated tests should be added when found.
    </TextBlock>

    <StackPanel Margin=""0,5,0,0""
                Orientation=""Horizontal""
                Grid.Row=""1"">
      <TextBlock Text=""Data type: ""
                 VerticalAlignment=""Center"" />

      <ComboBox x:Name=""dataTypeComboBox""
                Width=""150""
                SelectedValuePath=""Content""
                SelectedIndex=""0"">
        <ListBoxItem Content=""{x:Type s:String}""/>
        <ListBoxItem Content=""{x:Type s:Char}""/>
        <ListBoxItem Content=""{x:Type s:DateTime}""/>
        <ListBoxItem Content=""{x:Type s:Byte}""/>
        <ListBoxItem Content=""{x:Type s:Boolean}""/>
        <ListBoxItem Content=""{x:Type s:Int16}""/>
        <ListBoxItem Content=""{x:Type s:Int32}""/>
        <ListBoxItem Content=""{x:Type s:Int64}""/>
        <ListBoxItem Content=""{x:Type s:Single}""/>
        <ListBoxItem Content=""{x:Type s:Double}""/>
        <ListBoxItem Content=""{x:Type s:Decimal}""/>
        <ListBoxItem Content=""{x:Type s:SByte}""/>
        <ListBoxItem Content=""{x:Type s:UInt16}""/>
        <ListBoxItem Content=""{x:Type s:UInt32}""/>
        <ListBoxItem Content=""{x:Type s:UInt64}""/>
      </ComboBox>

      <TextBlock Text=""Culture (blank for current): ""
                 Margin=""10,0,0,0""
                 VerticalAlignment=""Center"" />

      <ComboBox x:Name=""cultureComboBox""
                IsEditable=""True""
                Width=""100"">
        <s:String>Invariant</s:String>
        <s:String>fr-FR</s:String>
        <s:String>en-US</s:String>
      </ComboBox>
    </StackPanel>

    <TextBox x:Name=""expressionTextBox""
             TextWrapping=""Wrap""
             Grid.Row=""2"" />

    <TextBox x:Name=""resultTextBox""
             TextWrapping=""Wrap""
             IsReadOnly=""True""
             Foreground=""Gray""
             VerticalScrollBarVisibility=""Visible""
             Grid.Row=""3"" />

    <Grid Margin=""0,5,0,0""
          Grid.Row=""4"">
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""Auto"" />
        <ColumnDefinition Width=""Auto"" />
        <ColumnDefinition />
        <ColumnDefinition Width=""Auto"" />
      </Grid.ColumnDefinitions>

      <Button x:Name=""parseButton""
              Content=""Parse""
              IsDefault=""True""
              Grid.Column=""0"" />

      <Button x:Name=""runUnitTestsButton""
              Content=""Run Unit Tests""
              Margin=""5,0,0,0""
              Grid.Column=""1"" />

      <Button x:Name=""closeButton""
              Content=""Close""
              IsCancel=""True""
              Margin=""0,0,10,0""
              Grid.Column=""3"" />
    </Grid>
  </Grid>";

      this.Title = "Expression Parser Test Window";
      this.Width = 600;
      this.Height = 400;
      this.MinHeight = 224;
      this.MinWidth = 467;
      this.ResizeMode = ResizeMode.CanResizeWithGrip;

      StringReader strReader = new StringReader( mainGridXaml );
      XmlTextReader xmlReader = new XmlTextReader( strReader );
      this.Content = XamlReader.Load( xmlReader ) as UIElement;

      Button button = ( ( Grid )this.Content ).FindName( "closeButton" ) as Button;

      if( button != null )
        button.Click += new RoutedEventHandler( this.closeButton_Click );

      m_parseButton = ( ( Grid )this.Content ).FindName( "parseButton" ) as Button;
      m_parseButton.Click += new RoutedEventHandler( this.parseButton_Click );

      m_runUnitTestsButton = ( ( Grid )this.Content ).FindName( "runUnitTestsButton" ) as Button;
      m_runUnitTestsButton.Click += new RoutedEventHandler( this.runUnitTestsButton_Click );

      m_dataTypeComboBox = ( ( Grid )this.Content ).FindName( "dataTypeComboBox" ) as ComboBox;
      m_cultureComboBox = ( ( Grid )this.Content ).FindName( "cultureComboBox" ) as ComboBox;
      m_expressionTextBox = ( ( Grid )this.Content ).FindName( "expressionTextBox" ) as TextBox;
      m_resultTextBox = ( ( Grid )this.Content ).FindName( "resultTextBox" ) as TextBox;
    }

    private void parseButton_Click( object sender, RoutedEventArgs e )
    {
      CultureInfo culture = null;
      string invalidCultureErrorText = null;
      string cultureName = m_cultureComboBox.Text;

      if( !string.IsNullOrEmpty( cultureName ) )
      {
        try
        {
          if( cultureName.ToLower() == "invariant" )
          {
            culture = CultureInfo.InvariantCulture;
          }
          else
          {
            culture = CultureInfo.GetCultureInfo( cultureName );
          }
        }
        catch( Exception ex )
        {
          invalidCultureErrorText = cultureName + " is an invalid culture : " + ex.Message;
        }
      }

      FilterCriterion criterion = FilterParser.TryParse( m_expressionTextBox.Text, m_dataTypeComboBox.SelectedValue as Type, culture );

      if( criterion == null )
      {
        m_resultTextBox.Text = "Error while parsing" + Environment.NewLine + 
          FilterParser.LastError;
      }
      else
      {
        m_resultTextBox.Text = criterion.ToString() + Environment.NewLine + 
          Environment.NewLine + 
          "Normalized expression:" + Environment.NewLine +
          criterion.ToExpression( culture );
      }

      if( !string.IsNullOrEmpty( invalidCultureErrorText ) )
      {
        m_resultTextBox.Text += Environment.NewLine + Environment.NewLine + invalidCultureErrorText;
      }
    }

    private void runUnitTestsButton_Click( object sender, RoutedEventArgs e )
    {
      m_runUnitTestsButton.IsEnabled = false;
      m_parseButton.IsEnabled = false;
      m_resultTextBox.Text = "";

      ThreadedUnitTester unitTester = new ThreadedUnitTester( new Action( this.UnitTestsCompletedCallback ), new Action<string>( this.LogMessageCallback ) );
      Thread unitTestThread = new Thread( new ThreadStart( unitTester.RunTests ) );

      unitTestThread.Start();
    }

    private void UnitTestsCompletedCallback()
    {
      this.LogMessageCallback( "Unit tests completed." );

      this.Dispatcher.BeginInvoke( new Action( delegate
      {
        m_runUnitTestsButton.IsEnabled = true;
        m_parseButton.IsEnabled = true;
      } ) );
    }

    private void LogMessageCallback( string message )
    {
      this.Dispatcher.BeginInvoke( new Action( delegate
      {
        m_resultTextBox.Text += message + Environment.NewLine;
        m_resultTextBox.ScrollToEnd();
      } ) );
    }

    private void closeButton_Click( object sender, RoutedEventArgs e )
    {
      this.Close();
    }

    private TextBox m_expressionTextBox;
    private TextBox m_resultTextBox;
    private ComboBox m_dataTypeComboBox;
    private ComboBox m_cultureComboBox;
    private Button m_parseButton;
    private Button m_runUnitTestsButton;

    #region ThreadedUnitTester Nested Type

    private class ThreadedUnitTester
    {
      public ThreadedUnitTester( Action testCompletedCallback, Action<string> logMessageCallback )
      {
        if( testCompletedCallback == null )
          throw new ArgumentNullException( "testCompletedCallback" );

        if( logMessageCallback == null )
          throw new ArgumentNullException( "logMessageCallback" );

        m_testCompletedCallback = testCompletedCallback;
        m_logMessageCallback = logMessageCallback;
      }

      public void RunTests()
      {
        if( FilterParser.LogMessageCallback != null )
          throw new InvalidOperationException( "Unit tests already running." );

        try
        {
          FilterParser.LogMessageCallback = m_logMessageCallback;
          FilterParser.TestQuoteParser();
          FilterParser.TestCriterionBuilder();

          if( m_testCompletedCallback != null )
            m_testCompletedCallback();
        }
        finally
        {
          FilterParser.LogMessageCallback = null;
        }
      }

      private Action m_testCompletedCallback = null;
      private Action<string> m_logMessageCallback = null;
    }

    #endregion ThreadedUnitTester Nested Type
  }
}

#endif
