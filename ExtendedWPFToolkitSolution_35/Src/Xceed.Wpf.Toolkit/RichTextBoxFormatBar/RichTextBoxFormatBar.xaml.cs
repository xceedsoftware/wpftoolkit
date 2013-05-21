/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core;
using System.Linq;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit
{
  /// <summary>
  /// Interaction logic for RichTextBoxFormatBar.xaml
  /// </summary>
  public partial class RichTextBoxFormatBar : UserControl, IRichTextBoxFormatBar
  {
    #region Properties

    #region RichTextBox

    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register( "Target", typeof( global::System.Windows.Controls.RichTextBox ), typeof( RichTextBoxFormatBar ), new PropertyMetadata( null, OnRichTextBoxPropertyChanged ) );
    public global::System.Windows.Controls.RichTextBox Target
    {
      get
      {
        return ( global::System.Windows.Controls.RichTextBox )GetValue( TargetProperty );
      }
      set
      {
        SetValue( TargetProperty, value );
      }
    }

    private static void OnRichTextBoxPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      RichTextBoxFormatBar formatBar = d as RichTextBoxFormatBar;
    }

    #endregion //RichTextBox

    public static double[] FontSizes
    {
      get
      {
        return new double[] { 
		            3.0, 4.0, 5.0, 6.0, 6.5, 7.0, 7.5, 8.0, 8.5, 9.0, 9.5, 
		            10.0, 10.5, 11.0, 11.5, 12.0, 12.5, 13.0, 13.5, 14.0, 15.0,
		            16.0, 17.0, 18.0, 19.0, 20.0, 22.0, 24.0, 26.0, 28.0, 30.0,
		            32.0, 34.0, 36.0, 38.0, 40.0, 44.0, 48.0, 52.0, 56.0, 60.0, 64.0, 68.0, 72.0, 76.0,
		            80.0, 88.0, 96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0
		            };
      }
    }

    #endregion

    #region Constructors

    public RichTextBoxFormatBar()
    {
      InitializeComponent();
      Loaded += FormatToolbar_Loaded;

      _cmbFontFamilies.ItemsSource = FontUtilities.Families.OrderBy( fontFamily => fontFamily.Source );
      _cmbFontSizes.ItemsSource = FontSizes;
    }

    #endregion //Constructors

    #region Event Hanlders

    void FormatToolbar_Loaded( object sender, RoutedEventArgs e )
    {
      Update();
    }

    private void FontFamily_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      if( e.AddedItems.Count == 0 )
        return;

      FontFamily editValue = ( FontFamily )e.AddedItems[ 0 ];
      ApplyPropertyValueToSelectedText( TextElement.FontFamilyProperty, editValue );
    }

    private void FontSize_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      if( e.AddedItems.Count == 0 )
        return;

      ApplyPropertyValueToSelectedText( TextElement.FontSizeProperty, e.AddedItems[ 0 ] );
    }

    void FontColor_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color> e )
    {
      Color selectedColor = ( Color )e.NewValue;
      ApplyPropertyValueToSelectedText( TextElement.ForegroundProperty, new SolidColorBrush( selectedColor ) );
    }

    private void FontBackgroundColor_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color> e )
    {
      Color selectedColor = ( Color )e.NewValue;
      ApplyPropertyValueToSelectedText( TextElement.BackgroundProperty, new SolidColorBrush( selectedColor ) );
    }

    private void Bullets_Clicked( object sender, RoutedEventArgs e )
    {
      if( BothSelectionListsAreChecked() )
      {
        _btnNumbers.IsChecked = false;
      }
    }

    private void Numbers_Clicked( object sender, RoutedEventArgs e )
    {
      if( BothSelectionListsAreChecked() )
      {
        _btnBullets.IsChecked = false;
      }
    }

    private void DragWidget_DragDelta( object sender, DragDeltaEventArgs e )
    {
      ProcessMove( e );
    }

    #endregion //Event Hanlders

    #region Methods

    public void Update()
    {
      UpdateToggleButtonState();
      UpdateSelectedFontFamily();
      UpdateSelectedFontSize();
      UpdateFontColor();
      UpdateFontBackgroundColor();
      UpdateSelectionListType();
    }

    private void UpdateToggleButtonState()
    {
      UpdateItemCheckedState( _btnBold, TextElement.FontWeightProperty, FontWeights.Bold );
      UpdateItemCheckedState( _btnItalic, TextElement.FontStyleProperty, FontStyles.Italic );
      UpdateItemCheckedState( _btnUnderline, Inline.TextDecorationsProperty, TextDecorations.Underline );

      UpdateItemCheckedState( _btnAlignLeft, Paragraph.TextAlignmentProperty, TextAlignment.Left );
      UpdateItemCheckedState( _btnAlignCenter, Paragraph.TextAlignmentProperty, TextAlignment.Center );
      UpdateItemCheckedState( _btnAlignRight, Paragraph.TextAlignmentProperty, TextAlignment.Right );
    }

    void UpdateItemCheckedState( ToggleButton button, DependencyProperty formattingProperty, object expectedValue )
    {
      object currentValue = DependencyProperty.UnsetValue;
      if( (Target != null) && (Target.Selection != null) )
      {
        currentValue = Target.Selection.GetPropertyValue( formattingProperty );
      }
      button.IsChecked = ( (currentValue == null) || ( currentValue == DependencyProperty.UnsetValue ) )
                          ? false 
                          : currentValue != null && currentValue.Equals( expectedValue );
    }

    private void UpdateSelectedFontFamily()
    {
      object value = DependencyProperty.UnsetValue;
      if( ( Target != null ) && ( Target.Selection != null ) )
      {
        value = Target.Selection.GetPropertyValue( TextElement.FontFamilyProperty );
      }
      FontFamily currentFontFamily = ( FontFamily )( ( ( value == null) || ( value == DependencyProperty.UnsetValue ) ) ? null : value );
      if( currentFontFamily != null )
      {
        _cmbFontFamilies.SelectedItem = currentFontFamily;
      }
    }

    private void UpdateSelectedFontSize()
    {
      object value = DependencyProperty.UnsetValue;
      if( ( Target != null ) && ( Target.Selection != null ) )
      {
        value = Target.Selection.GetPropertyValue( TextElement.FontSizeProperty );
      }

      _cmbFontSizes.SelectedValue = ( ( value == null ) || ( value == DependencyProperty.UnsetValue ) ) ? null : value;
    }

    private void UpdateFontColor()
    {
      object value = DependencyProperty.UnsetValue;
      if( ( Target != null ) && ( Target.Selection != null ) )
      {
        value = Target.Selection.GetPropertyValue( TextElement.ForegroundProperty );
      }

      Color currentColor = ( Color )( ( (value == null) || ( value == DependencyProperty.UnsetValue ) )
                                    ? Colors.Black 
                                    : ( ( SolidColorBrush )value ).Color );
      _cmbFontColor.SelectedColor = currentColor;
    }

    private void UpdateFontBackgroundColor()
    {
      object value = DependencyProperty.UnsetValue;
      if( ( Target != null ) && ( Target.Selection != null ) )
      {
        value = Target.Selection.GetPropertyValue( TextElement.BackgroundProperty );
      }
      Color currentColor = ( Color )( ( (value == null ) || (value == DependencyProperty.UnsetValue) )
                                      ? Colors.Transparent 
                                      : ( ( SolidColorBrush )value ).Color );
      _cmbFontBackgroundColor.SelectedColor = currentColor;
    }

    /// <summary>
    /// Updates the visual state of the List styles, such as Numbers and Bullets.
    /// </summary>
    private void UpdateSelectionListType()
    {
      //uncheck both
      _btnBullets.IsChecked = false;
      _btnNumbers.IsChecked = false;

      Paragraph startParagraph = ( ( Target != null ) && ( Target.Selection != null ) ) 
                                  ? Target.Selection.Start.Paragraph 
                                  : null;
      Paragraph endParagraph = ( ( Target != null ) && ( Target.Selection != null ) )
                                ? Target.Selection.End.Paragraph
                                : null;
      if( startParagraph != null && endParagraph != null && ( startParagraph.Parent is ListItem ) && ( endParagraph.Parent is ListItem ) && object.ReferenceEquals( ( ( ListItem )startParagraph.Parent ).List, ( ( ListItem )endParagraph.Parent ).List ) )
      {
        TextMarkerStyle markerStyle = ( ( ListItem )startParagraph.Parent ).List.MarkerStyle;
        if( markerStyle == TextMarkerStyle.Disc ) //bullets
        {
          _btnBullets.IsChecked = true;
        }
        else if( markerStyle == TextMarkerStyle.Decimal ) //numbers
        {
          _btnNumbers.IsChecked = true;
        }
      }
    }

    /// <summary>
    /// Checks to see if both selection lists are checked. (Bullets and Numbers)
    /// </summary>
    /// <returns></returns>
    private bool BothSelectionListsAreChecked()
    {
      return _btnBullets.IsChecked == true && _btnNumbers.IsChecked == true;
    }

    void ApplyPropertyValueToSelectedText( DependencyProperty formattingProperty, object value )
    {
      if( ( value == null ) || ( Target == null ) || ( Target.Selection == null ) )
        return;

      Target.Selection.ApplyPropertyValue( formattingProperty, value );
    }

    private void ProcessMove( DragDeltaEventArgs e )
    {
      AdornerLayer layer = AdornerLayer.GetAdornerLayer( Target );
      UIElementAdorner<Control> adorner = layer.GetAdorners( Target )[ 0 ] as UIElementAdorner<Control>;
      adorner.SetOffsets( adorner.OffsetLeft + e.HorizontalChange, adorner.OffsetTop + e.VerticalChange );
    }

    #endregion //Methods
  }
}
