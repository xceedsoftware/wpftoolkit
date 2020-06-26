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

using System.Linq;
using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit.Core;

namespace Xceed.Wpf.Toolkit
{
  public class RichTextBoxFormatBar : Control, IRichTextBoxFormatBar
  {
    #region Members
    private ComboBox _cmbFontFamilies;
    private ComboBox _cmbFontSizes;
    private ColorPicker _cmbFontBackgroundColor;
    private ColorPicker _cmbFontColor;

    private ToggleButton _btnNumbers;
    private ToggleButton _btnBullets;
    private ToggleButton _btnBold;
    private ToggleButton _btnItalic;
    private ToggleButton _btnUnderline;
    private ToggleButton _btnAlignLeft;
    private ToggleButton _btnAlignCenter;
    private ToggleButton _btnAlignRight;

    private Thumb _dragWidget;
    private bool _waitingForMouseOver;
    #endregion

    #region Properties

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

    static RichTextBoxFormatBar()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( RichTextBoxFormatBar ), new FrameworkPropertyMetadata( typeof( RichTextBoxFormatBar ) ) );
    }

    public RichTextBoxFormatBar()
    {
    }

    #endregion //Constructors

    #region Base Class Overrides




    #endregion

    #region Event Hanlders

    private void FontFamily_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      if( e.AddedItems.Count == 0 )
        return;

      FontFamily editValue = ( FontFamily )e.AddedItems[ 0 ];
      ApplyPropertyValueToSelectedText( TextElement.FontFamilyProperty, editValue );
      _waitingForMouseOver = true;
    }

    private void FontSize_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      if( e.AddedItems.Count == 0 )
        return;

      ApplyPropertyValueToSelectedText( TextElement.FontSizeProperty, e.AddedItems[ 0 ] );
      _waitingForMouseOver = true;
    }

    void FontColor_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
    {
      Color? selectedColor = ( Color? )e.NewValue;
      ApplyPropertyValueToSelectedText( TextElement.ForegroundProperty, selectedColor.HasValue ? new SolidColorBrush( selectedColor.Value ) : null );
      _waitingForMouseOver = true;
    }

    private void FontBackgroundColor_SelectedColorChanged( object sender, RoutedPropertyChangedEventArgs<Color?> e )
    {
      Color? selectedColor = ( Color? )e.NewValue;
      ApplyPropertyValueToSelectedText( TextElement.BackgroundProperty, selectedColor.HasValue ? new SolidColorBrush( selectedColor.Value ) : null );
      _waitingForMouseOver = true;
    }

    private void Bullets_Clicked( object sender, RoutedEventArgs e )
    {
      if( BothSelectionListsAreChecked() && ( _btnNumbers != null) )
      {
        _btnNumbers.IsChecked = false;
      }
    }

    private void Numbers_Clicked( object sender, RoutedEventArgs e )
    {
      if( BothSelectionListsAreChecked() && ( _btnBullets != null) )
      {
        _btnBullets.IsChecked = false;
      }
    }

    private void DragWidget_DragDelta( object sender, DragDeltaEventArgs e )
    {
      ProcessMove( e );
    }

    protected override void OnMouseEnter( System.Windows.Input.MouseEventArgs e )
    {
      base.OnMouseEnter( e );
      _waitingForMouseOver = false;
    }

    #endregion //Event Hanlders

    #region Methods

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _dragWidget != null )
      {
        _dragWidget.DragDelta -= new DragDeltaEventHandler( DragWidget_DragDelta );
      }

      if( _cmbFontFamilies != null )
      {
        _cmbFontFamilies.SelectionChanged -= new SelectionChangedEventHandler( FontFamily_SelectionChanged );
      }

      if( _cmbFontSizes != null )
      {
        _cmbFontSizes.SelectionChanged -= new SelectionChangedEventHandler( FontSize_SelectionChanged );
      }

      if( _btnBullets != null )
      {
        _btnBullets.Click -= new RoutedEventHandler( Bullets_Clicked );
      }

      if( _btnNumbers != null )
      {
        _btnNumbers.Click -= new RoutedEventHandler( Numbers_Clicked );
      }

      if( _cmbFontBackgroundColor != null )
      {
        _cmbFontBackgroundColor.SelectedColorChanged -= new RoutedPropertyChangedEventHandler<Color?>( FontBackgroundColor_SelectedColorChanged );
      }

      if( _cmbFontColor != null )
      {
        _cmbFontColor.SelectedColorChanged -= new RoutedPropertyChangedEventHandler<Color?>( FontColor_SelectedColorChanged );
      }

      this.GetTemplateComponent( ref _cmbFontFamilies, "_cmbFontFamilies" );
      this.GetTemplateComponent( ref _cmbFontSizes, "_cmbFontSizes" );
      this.GetTemplateComponent( ref _cmbFontBackgroundColor, "_cmbFontBackgroundColor" );
      this.GetTemplateComponent( ref _cmbFontColor, "_cmbFontColor" );
      this.GetTemplateComponent( ref _btnNumbers, "_btnNumbers" );
      this.GetTemplateComponent( ref _btnBullets, "_btnBullets" );
      this.GetTemplateComponent( ref _btnBold, "_btnBold" );
      this.GetTemplateComponent( ref _btnItalic, "_btnItalic" );
      this.GetTemplateComponent( ref _btnUnderline, "_btnUnderline" );
      this.GetTemplateComponent( ref _btnAlignLeft, "_btnAlignLeft" );
      this.GetTemplateComponent( ref _btnAlignCenter, "_btnAlignCenter" );
      this.GetTemplateComponent( ref _btnAlignRight, "_btnAlignRight" );
      this.GetTemplateComponent( ref _dragWidget, "_dragWidget" );

      if( _dragWidget != null )
      {
        _dragWidget.DragDelta += new DragDeltaEventHandler( DragWidget_DragDelta );
      }

      if( _cmbFontFamilies != null )
      {
        _cmbFontFamilies.ItemsSource = FontUtilities.Families.OrderBy( fontFamily => fontFamily.Source );
        _cmbFontFamilies.SelectionChanged += new SelectionChangedEventHandler( FontFamily_SelectionChanged );
      }

      if( _cmbFontSizes != null )
      {
        _cmbFontSizes.ItemsSource = FontSizes;
        _cmbFontSizes.SelectionChanged += new SelectionChangedEventHandler( FontSize_SelectionChanged );
      }

      if( _btnBullets != null )
      {
        _btnBullets.Click += new RoutedEventHandler( Bullets_Clicked );
      }

      if( _btnNumbers != null )
      {
        _btnNumbers.Click += new RoutedEventHandler( Numbers_Clicked );
      }

      if( _cmbFontBackgroundColor != null )
      {
        _cmbFontBackgroundColor.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color?>( FontBackgroundColor_SelectedColorChanged );
      }

      if( _cmbFontColor != null )
      {
        _cmbFontColor.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color?>( FontColor_SelectedColorChanged );
      }

      // Update the ComboBoxes when changing themes.
      this.Update();
    }

    private void GetTemplateComponent<T>( ref T partMember, string partName ) where T : class
    {
      partMember = ( this.Template != null )
        ? this.Template.FindName( partName, this ) as T
        : null;
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
      if( ( Target != null ) && ( Target.Selection != null ) )
      {
        currentValue = Target.Selection.GetPropertyValue( formattingProperty );
      }

      if( currentValue == DependencyProperty.UnsetValue )
        return;

      if( button != null )
      {
        button.IsChecked = ( currentValue == null )
                            ? false
                            : currentValue != null && currentValue.Equals( expectedValue );
      }
    }

    private void UpdateSelectedFontFamily()
    {
      object value = DependencyProperty.UnsetValue;
      if( ( Target != null ) && ( Target.Selection != null ) )
      {
        value = Target.Selection.GetPropertyValue( TextElement.FontFamilyProperty );
      }

      if( value == DependencyProperty.UnsetValue )
        return;

      FontFamily currentFontFamily = ( FontFamily )value;
      if( (currentFontFamily != null) && ( _cmbFontFamilies != null) )
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

      if( value == DependencyProperty.UnsetValue )
        return;

      if( _cmbFontSizes != null )
      {
        _cmbFontSizes.SelectedValue = value;
      }
    }

    private void UpdateFontColor()
    {
      object value = DependencyProperty.UnsetValue;
      if( ( Target != null ) && ( Target.Selection != null ) )
      {
        value = Target.Selection.GetPropertyValue( TextElement.ForegroundProperty );
      }

      if( value == DependencyProperty.UnsetValue )
        return;

      Color? currentColor =  ( ( value == null )
                              ? null
                              : ( Color? )( ( SolidColorBrush )value ).Color );
      if( _cmbFontColor != null )
      {
        _cmbFontColor.SelectedColor = currentColor;
      }
    }

    private void UpdateFontBackgroundColor()
    {
      object value = DependencyProperty.UnsetValue;
      if( ( Target != null ) && ( Target.Selection != null ) )
      {
        value = Target.Selection.GetPropertyValue( TextElement.BackgroundProperty );
      }

      if( value == DependencyProperty.UnsetValue )
        return;

      Color? currentColor = ( ( value == null )
                              ? null
                              : ( Color? )( ( SolidColorBrush )value ).Color );
      if( _cmbFontBackgroundColor != null )
      {
        _cmbFontBackgroundColor.SelectedColor = currentColor;
      }
    }

    /// <summary>
    /// Updates the visual state of the List styles, such as Numbers and Bullets.
    /// </summary>
    private void UpdateSelectionListType()
    {
      if( (_btnNumbers == null) || ( _btnBullets == null) )
        return;

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
      return (( _btnBullets != null) && (_btnBullets.IsChecked == true) )
          && (( _btnNumbers != null) && (_btnNumbers.IsChecked == true));
    }

    void ApplyPropertyValueToSelectedText( DependencyProperty formattingProperty, object value )
    {
      if( ( Target == null ) || ( Target.Selection == null ) )
        return;

      SolidColorBrush solidColorBrush = value as SolidColorBrush;
      if( ( solidColorBrush != null ) && solidColorBrush.Color.Equals( Colors.Transparent ) )
      {
        Target.Selection.ApplyPropertyValue( formattingProperty, null );
      }
      else
      {
        Target.Selection.ApplyPropertyValue( formattingProperty, value );
      }
    }

    private void ProcessMove( DragDeltaEventArgs e )
    {
      AdornerLayer layer = AdornerLayer.GetAdornerLayer( Target );
      UIElementAdorner<Control> adorner = layer.GetAdorners( Target ).OfType<UIElementAdorner<Control>>().First();
      adorner.SetOffsets( adorner.OffsetLeft + e.HorizontalChange, adorner.OffsetTop + e.VerticalChange );
    }

    #endregion //Methods

    #region IRichTextBoxFormatBar Interface

    #region Target

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

    #endregion //Target

    public bool PreventDisplayFadeOut
    {
      get
      {
        return ( (( _cmbFontFamilies != null) && _cmbFontFamilies.IsDropDownOpen)
              || (( _cmbFontSizes != null) && _cmbFontSizes.IsDropDownOpen)
              ||  (( _cmbFontBackgroundColor != null) && _cmbFontBackgroundColor.IsOpen)
              || (( _cmbFontColor != null) && _cmbFontColor.IsOpen)
              || _waitingForMouseOver );
      }
    }

    public void Update()
    {
      UpdateToggleButtonState();
      UpdateSelectedFontFamily();
      UpdateSelectedFontSize();
      UpdateFontColor();
      UpdateFontBackgroundColor();
      UpdateSelectionListType();
    }

    #endregion
  }
}
