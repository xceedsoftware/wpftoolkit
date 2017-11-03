using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit
{
  public delegate void FilterRoutedEventHandler(object sender, FilterEventArgs e);

  [TemplatePart(Name = PART_Header, Type = typeof(TextBlock))]
  [TemplatePart(Name = PART_FilterBox, Type = typeof(TextBox))]
  [TemplatePart(Name = PART_ClearButton, Type = typeof(Button))]
  public sealed class FilterBox : Control
  {
    #region [ Declarations and Fields ]
    private const string PART_Header = "PART_Header";
    private const string PART_FilterBox = "PART_FilterBox";
    private const string PART_ClearButton = "PART_ClearButton";

    public static readonly RoutedEvent FilterEvent;
    public static readonly RoutedEvent ClearFilterEvent;

    private Button _clearButton;
    private TextBox _filterBox;
    private TextBlock _headerText;
    #endregion

    #region [ Constructors ]
    static FilterBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterBox), new FrameworkPropertyMetadata(typeof(FilterBox)));
      FilterEvent = EventManager.RegisterRoutedEvent("Filter", RoutingStrategy.Bubble, typeof(FilterRoutedEventHandler), typeof(FilterBox));
      ClearFilterEvent = EventManager.RegisterRoutedEvent("ClearFilter", RoutingStrategy.Bubble, typeof(RoutedEventArgs), typeof(FilterBox));
    }
    #endregion

    #region [ FilterText property ]
    public static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.Register("FilterText", typeof(string), typeof(FilterBox), new PropertyMetadata(string.Empty));

    public string FilterText
    {
      get => (string)GetValue(FilterTextProperty);
      set => SetValue(FilterTextProperty, value);
    }
    #endregion

    #region Header property
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(FilterBox), new UIPropertyMetadata(string.Empty));

    public string Header
    {
      get => (string)GetValue(HeaderProperty);
      set => SetValue(HeaderProperty, value);
    }
    #endregion

    #region TargetControl property
    public static readonly DependencyProperty TargetControlProperty =
        DependencyProperty.Register("TargetControl", typeof(ItemsControl), typeof(FilterBox), new UIPropertyMetadata(null));

    public ItemsControl TargetControl
    {
      get => (ItemsControl)GetValue(TargetControlProperty);
      set => SetValue(TargetControlProperty, value);
    }
    #endregion

    #region FilterTextBindingPath property
    public static readonly DependencyProperty FilterTextBindingPathProperty =
        DependencyProperty.Register("FilterTextBindingPath", typeof(string), typeof(FilterBox), new PropertyMetadata(string.Empty));

    public string FilterTextBindingPath
    {
      get => (string)GetValue(FilterTextBindingPathProperty);
      set => SetValue(FilterTextBindingPathProperty, value);
    }
    #endregion

    #region FilterOnEnter property
    public static readonly DependencyProperty FilterOnEnterProperty =
        DependencyProperty.Register("FilterOnEnter", typeof(bool), typeof(FilterBox), new UIPropertyMetadata(false));

    public bool FilterOnEnter
    {
      get => (bool)GetValue(FilterOnEnterProperty);
      set => SetValue(FilterOnEnterProperty, value);
    }

    public bool HasFocus => _filterBox.IsFocused;
    #endregion

    #region Filter event
    public event FilterRoutedEventHandler Filter
    {
      add => base.AddHandler(FilterEvent, value);
      remove => base.RemoveHandler(FilterEvent, value);
    }
    #endregion

    #region ClearFilter event
    public event RoutedEventHandler ClearFilter
    {
      add => base.AddHandler(ClearFilterEvent, value);
      remove => base.RemoveHandler(ClearFilterEvent, value);
    }
    #endregion

    #region Overridden Functions/Methods
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      AttachToVisualTree();
    }
    #endregion

    #region [ Private Functions/Methods ]
    private void ApplyFilterOnTarget()
    {
      if (TargetControl?.ItemsSource == null)
        return;

      var collectionView = CollectionViewSource.GetDefaultView(TargetControl.ItemsSource);
      if (collectionView == null)
        throw new InvalidOperationException("The TargetConrol should use ICollectionView as ItemSource.");

      collectionView.Filter =
          m => Regex.IsMatch(
              GetDataValue<string>(m, FilterTextBindingPath),
              Regex.Escape(FilterText),
              RegexOptions.IgnoreCase
          );
    }

    private void ClearFilterOnTarget()
    {
      if (TargetControl?.ItemsSource == null)
        return;

      var collectionView = CollectionViewSource.GetDefaultView(TargetControl.ItemsSource);
      if (collectionView == null)
        throw new InvalidOperationException("The TargetConrol should use ICollectionView as ItemSource.");

      collectionView.Filter = null;
    }

    private void RaiseFilterEvent()
    {
      var args = new FilterEventArgs(FilterEvent, this, this.FilterText);
      RaiseEvent(args);
      if (!args.IsFilterApplied)
        ApplyFilterOnTarget();
    }

    private static T GetDataValue<T>(object data, string propertyName)
    {
      if (!string.IsNullOrEmpty(propertyName))
      {
        var descriptors = TypeDescriptor.GetProperties(data.GetType());
        var descriptor = descriptors[propertyName];
        if (descriptor != null)
        {
          var value = (T)descriptor.GetValue(data);
          return value;
        }
      }
      return (T)Convert.ChangeType(data.ToString(), typeof(T));
    }

    private void AttachToVisualTree()
    {
      _headerText = GetTemplateChild(PART_Header) as TextBlock;

      _filterBox = GetTemplateChild(PART_FilterBox) as TextBox;
      if (_filterBox != null)
      {
        _filterBox.LostKeyboardFocus += OnLostKeyboardFocus;
        _filterBox.GotKeyboardFocus += OnGotKeyboardFocus;
        _filterBox.TextChanged += OnFilterBoxTextChanged;
      }

      _clearButton = GetTemplateChild(PART_ClearButton) as Button;
      if (_clearButton != null)
      {
        _clearButton.Click += OnClearButtonClick;
      }

      GotKeyboardFocus += FilterGotKeyboardFocus;
      KeyDown += OnKeyDown;
    }

    private void FilterGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      Keyboard.Focus(_filterBox);
      _filterBox.CaretBrush = Foreground;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape && !string.IsNullOrEmpty(FilterText))
      {
        Clear();
      }
      else if (e.Key == Key.Enter && FilterOnEnter)
      {
        RaiseFilterEvent();
      }
    }

    private void OnClearButtonClick(object sender, RoutedEventArgs e)
    {
      Clear();
    }

    public void Clear(bool setFocus = true)
    {
      if (_filterBox != null)
      {
        _filterBox.Text = string.Empty;
        if (setFocus)
          Keyboard.Focus(_filterBox);

        var args = new RoutedEventArgs(ClearFilterEvent, this);
        RaiseEvent(args);
        if (!args.Handled)
          ClearFilterOnTarget();
      }
    }

    private void OnFilterBoxTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!FilterOnEnter)
      {
        RaiseFilterEvent();
      }

      if (string.IsNullOrEmpty(_filterBox.Text))
      {
        _clearButton.Visibility = Visibility.Collapsed;
        _headerText.Visibility = !_filterBox.IsFocused ? Visibility.Visible : Visibility.Collapsed;
      }
      else
      {
        _clearButton.Visibility = Visibility.Visible;
        _headerText.Visibility = Visibility.Collapsed;
      }
    }

    private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      _headerText.Visibility = Visibility.Collapsed;
    }

    private void OnLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
      if (string.IsNullOrEmpty(_filterBox.Text))
        _headerText.Visibility = Visibility.Visible;

      _filterBox.CaretBrush = new SolidColorBrush(Colors.Transparent);
    }
    #endregion
  }

  public class FilterEventArgs : RoutedEventArgs
  {
    public string FilterText
    {
      get;
    }

    public bool IsFilterApplied
    {
      get;
      set;
    }

    public FilterEventArgs()
    {
      FilterText = null;
      IsFilterApplied = false;
    }

    public FilterEventArgs(RoutedEvent routedEvent, object source) :
        this(routedEvent, source, string.Empty)
    {
    }

    public FilterEventArgs(RoutedEvent routedEvent, object source, string filterText)
        : base(routedEvent, source)
    {
      FilterText = filterText;
    }
  }
}
