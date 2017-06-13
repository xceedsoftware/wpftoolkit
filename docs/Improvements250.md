{anchor:Community250}
## v2.5 Community Edition (July 3, 2015)

**1 new control**

* WatermarkComboBox. A ComboBox that lets you display text, graphics or other content while nothing has yet been selected.

**45 bug fixes and improvements**

* In PropertyGrid, List<enum> are now handled with a DropDown list, like primitive types.
* In AvalonDock, the DockingManager will no longer be empty when used in a DataTemplate.
* In AvalonDock, Calling Layout.CollectGarbage() will no longer destroy panels.
* In CollectionControl, 2 new events will be raised: ItemMovedDown and ItemMovedUp when an item is moved up or down in the ListBox of the CollectionControl.
* In DateTimePicker, TimePicker, DateTimeUpDown, TimeSPanUpDown, NumericUpDown, setting Minimum/Maximum to null will now allow spinning.
* In Zoombox, The new property zoomBox.IsUsingScrollbars can now be set to true to display scrollbars that will let user scroll a zoomed content.
* In PrimitiveTypeCollectionControl, the width of the popup is now the same as the width of the control.
* In DateTimePicker, it is no longer possible to input an out-of-bounds date (Maximum, Minimum properties) using the calendar dropdown.
* In CalculatorUpDown, DateTimeUpDown, TimeSpanUpDown, TimePicker and DateTimePicker, the ButtonSpinnerLocation property now works propertly.
* In NumericUpDown, if the MouseWheelActiveTrigger is set to Focused, double-clicking on the control will still keep the mouse wheel active for incrementing/decrementing when the mouse is away from the control.
* In NumericUpDown, DateTimeUpDown, TimeSpanUpDown, DateTimePicker and TimePicker, there is a new Property InputBase.IsUndoEnabled that is set to true by default. When false, Ctrl-Z won't work to undo what is typed.
* In TimePicker, clicking between dropdown items has been fixed.
* In TimePicker, DropDown width now follows the width of the control itself.
* In TimePicker, MaxDropDownHeight will now work properly when items require less height than MaxDropDownHeight.
* Xceed.Wpf.Toolkit.Core.Primitives.DateTimeUpDownBase type has changed its namespace to Xceed.Wpf.Toolkit.Primitives.DateTimeUpDownBase
* In DateTimePicker, DateTimeUpDown, TimePicker, MouseWheelActiveTrigger will now work in those controls when set to Focused.
* In PropertyGrid, the error "Cannont modify XXX once the definition has been added to the collection" will no longer be thrown when setting any property of PropertyDefinition.
* In TimeSpanUpDown, using 2 digits for days will have an accurate Text selection. Also, incrementing fractions of seconds now increments milliseconds, not seconds.
* In all controls, when IsEnabled is set to False in themes Windows7, Windows8, Metro and Office2007, the controls will now be displayed properly.
* In DateTimeUpDown, TimePicker, DateTimePicker, TimeSpanUpDown, when Format is set to Custom, if a dot, comma, colon, semi-colon, or slash is typed, and possibly other characters, no exception will be thrown.
* In ButtonSpinner, doing a mouse wheel on controls with a ButtonSpinner will no longer "eat" the mouseWheel event, unless the control has focus.
* In PropertyGrid, the PropertyName, SelectedObjectTypeName and SelectedObjectName now supports text trimming. When the mouse is over those trimmed TextBlocks, a tooltip will show the complete Text.
* In PropertyGrid, a new property PropertyGrid.IsMiscCategoryLabelHidden is now available. When set to True and using PropertyGrid.IsCategorized = True, The PropertyItems in the "Misc" category will no longer have the "Misc" expander.
* In NumericUpDown, using a "P" in FormatString, if the "P" is between "'", it will now be considered as a string, it will not be used to convert as percentage.
* In PropertyGrid, under Windows7 theme, the PropertyGrid's ShowPreview will now work properly.
* In ColorPicker, 2 new Properties have been added to edit the text of the buttons : AdvancedButtonHeader and StandardButtonHeader.
* In ColorCanvas, The ColorSpectrum in ColorCanvas will have a range from 0 to 360, not 1 to 360. This caused the blue channel to have the wrong initial value.
* In AvalonDock, The "CloseAll" option will be available to complete the "Close" and "CloseAllButThis" options.
* In PropertyGrid, when using EditorAttribute or ItemsSourceAttribute, if the property is set to ReadOnly, the editor will not be set to ReadOnly unless editor is set to IsEanabled=False in ITypeEditor.ResolveEditor()
* In NumericUpDown, DateTimeUpDown, DateTimePicker, TimePicker, TimeSpanUpDown, a new boolean property was added: UpdateValueOnEnterKey. When set to True and the user is typing text, the synchronization between "Value" and "Text" will only be done on a 'enter' key press, or a LostFocus.
* In TimeSpanUpDown, a new property has been added: TimeSpanUpDown.FractionalSecondsDigitsCount. It lets the user choose from 0 to 3 decimals digits that represent fractions of seconds.
* In TimeSpanUpDown, The DefaultValue is now 0:00:00 instead of null.
* In MaskedTextBox, setting the MaskedTextBox.Mask from string1 to string.Empty to string2 will no longer crash.
* In CheckComboBox, TimePicker, DateTimePicker, CalculatorUpDown, ColorPicker, MultiLineTextEditor, DropDownButton, SplitButton, clicking on the ToggleButton to close the popup will now keep the focus on the control.
* In PropertyGrid and PrimitiveTypeCollectionEditor, using a List<T> in PropertyGrid will show the PrimitiveTypeCollectionEditor and editing it will no longer result in a loss of data.
* In AvalonDock, Using a floatingWindow to move a LayoutAnchorable or LayoutDocument around will no longer be empty.
* In PropertyGrid, Properties of type 2 dimensions array will no longer throw an exception in the PropertyGrid.
* In BusyIndicator, the property FocusAferBusy has been renamed FocusAfterBusy.
* In PropertyGrid, BorderBrush and BorderThickness are now templatedBinded for PropertyItem's Part_Name and Part_Editor.
* In PropertyGrid, Instead of expanding/collapsing categories only by clicking the Arrow icon, users will now be able to expand/collapse by clicking anywhere on the category header.
* In ColorPicker, ColorCanvas, the SelectedColor type has been changed from "Color" to "Nullable<Color>". ColorPicker and ColorCanvas now supports Nullable<Color>.
* In PropertyGrid, when a ToolTip is added to a CustomPropertyItem, it will be shown only over the PropertyItem Name, not on its Editor.
* In TimePicker, the control now shares the same base class as DateTimePicker, which adds 10 new properties to the TimePicker.
* In TimePicker, Xceed.Wpf.Toolkit.TimeFormat has been removed. Xceed.Wpf.Toolkit.DateTimeFormat should now be used instead.
* In all controls, the windows7, Windows8, Metro or Office2007 themes have been revised to make sure the following properties are binded when applicables: Background, BorderBrush, BorderThickness, Padding, HorizontalContentAlignment, VerticalContentAlignment, Content, and ContentTemplate.
{anchor:Plus250}
## v2.5.0 Plus Edition additional improvements

_Shipped to registered users of the Plus Edition on Nov. 24, 2014._

**2 new controls**

* FilePicker. Full featured file picker textbox that lets end users select one or more files and see selected files in textbox format.
* SlideShow. Full featured slideshow control, with animated transitions and a variety of navigation options. Can display live WPF content.

**4 additional bug fixes and improvements**

* In Chart, the Tick labels will be displayed with any small values. There is no more rounding to 2 decimals.
* In PropertyGrid, CategoryDefinition has a new Property : IsBrowsable. This property will control wether you wish to display the entire category with all its properties.
* In TokenizedTextBox, the position of the popup in X will be at the caret X position. The margin of the TokenizedTextBox will no longer be added.
* In PropertyGrid, using PropertyGrid.PropertiesSource along with CategoryOrderAttribute and PropertyOrderAttribute will now sort Categories and propertyItems properly.
* The 45 bug fixes listed above for [Community Edition v2.5.0](#Community250).

{anchor:Plus260}
## v2.6.0 Plus Edition

_Shipped to registered users of the Plus Edition on Feb. 9, 2015._

**17 bug fixes and improvements**

* In TokenizedTextBox, the Foreground and caret colors can now be set. (Plus Only)
* In TokenizedTextBox, alias values are now supported for tokens. Theses values will not appear in the suggested list. (Plus Only)
* In DateTimePicker, DateTimeUpDown and TimePicker, when TimeFormat has a foreign culture, it will no longer cause a crash.
* In PropertyGrid, PropertyItem now has a new read-only property: PropertyName. It returns the name of the property referenced by the PropertyItem.
* In WatermarkComboBox, the BorderBrush can now be set when IsEditable is true.
* In ColorPicker, when ShowRecentColors is True, selecting a color from the popup will select the proper color.
* In ColorPicker, the RecentColors property can now be bound.
* In MessageBox, when a user is not using Windows 8, changing the FontSize won't affect the Caption anymore, it will only affect the content.
* In WatermarkTextBox, when a user is not in Windows 8 and IsEnabled is set to false, the Background and BorderBrush will no longer be gray.
* In the BusyIndicator, width is now based on its content.
* In DataGrid, groups should now be properly displayed when grouping is activated for data sources other than a DataGridCollectionView.
* In AvalonDock, AutoHideWindow will now be disposed when DockingManager is unloaded to prevent memory leaks.
* In RangeSlider, the RangeSlider now has a new property : IsDeferredUpdateValues. When set to True, the update of the LowerValue/HigherValue will be done on the Thumbs DragCompleted events, not on the DragDelta events. Default value is False. 
* In DateTimePicker/TimePicker, a new property ShowDropDownButton property as been added. When set to false, the dropDownButton of the DateTimePicker/TimePicker will be removed. Default value is true.
* In PropertyGrid, when using EditorTemplateDefinition TargetProperties with something else than a type or propertyName (like an interface), the EditorTemplateDefinition will now be applied. PropertyDefinitionBaseCollection<T> now has a protected constructor and a virtual indexer.
* In TimePicker, when setting the Format to Custom and setting a FormatString, pressing the ButtonSpinner will no longer cause a crash.
* In TimeSpanUpDown, negative values are now supported.

{anchor:Plus270}
## v2.7.0 Plus Edition

_Shipped to registered users of the Plus Edition on June 8, 2015._

**New controls**

* A set of 15 new controls for building apps with the Material Design look, to give your WPF apps a modern look and feel that blends in with the latest Web applications.

**41 bug fixes and improvements**

* In PropertyGrid, using the DependsOn attribute on properties will now update the validation on the depending properties.
* In ListBox, the SearchTextBox's Background, BorderThickness and BorderBrush can now be set when styling the ListBox's SearchTextBox.
* In ToggleSwitch, the ToggleSwitch can now stretch in all the available space.
* In TokenizedTextBox, the event ItemSelectionChanged is now fired when TokenizedTextBox.SelectedItems adds or removes items in code-behind.
* In PropertyGrid, EditorNumericUpDownDefinition now supports the property UpdateValueOnEnterKey.
* In PropertyGrid, using CustomPropertyItems with customEditors will now have the correct DataContext.
* In ListBox, the Drag and Drop will now be available when using a ListBox inside a WinForm's ElementHost.
* In ListBox, binding on ListBox.Items.Count now works when ItemsSource is used.
* In PropertyGrid, the categories can now be collapsed/expanded in code-behind with 4 new methods :CollapseAllCategories(), ExpandAllCategories(), CollapseCategory(string) and ExpandCategory(string)
* In ToggleSwitch, the properties ThumbLeftContent and ThumbRightContent are now of type "object" instead of "string". So any content can be put in the ToggleSwitch. The property ThumbStyle has been added to redefine the Style of the Thumb. Please note that using ThumbStyle will disable the following properties : ThumbLeftContent, ThumbRightContent, ThumbForeground, ThumbBackground, ThumbBorderBrush, ThumbBorderThickness, ThumbHoverBackground, ThumbHoverBorderBrush, ThumbPressedBackground, ThumbPressedBorderBrush
* The ZoomBox will no longer throw an exception at startup when its Content is null.
* In WatermarkTextBox, the new property KeepWatermarkOnGotFocus makes it possible to keep or remove the watermark on GotFocus.
* In PropertyGrid, you can now only display the SelectedObject properties, hidding the inherited ones.
* In PropertyGrid, the RefreshPropertiesAttribute is now supported.
* In PropertyGrid, Removing the Misc category label is now possible in any language.
* In DropDownButton, if KeyboardFocusWithin becomes false, the DropDownButton will now close.
* In PropertyGrid, expandable PropertyItems are now updated when their Sub-PropertyItems are modified.
* In PropertyGrid, Nullable are now supported.
* In DropDownButton, SplitButtons : Under Windows7, Button's Style will now follow Windows 7 and have their bottom half background painted in gray.
* In ColorCanvas, the ColorCanvas's SpectrumSlider value will no longer be updated while selecting a color in the ShadingColor rectangle.
* In ColorPicker, when the property DisplayColorAndName is False, the Tooltip displaying the Color name will no longer appear in the ColorPicker popup.
* In Calculator, the Calculator buttons content won't be overriden with default values anymore. Also, The new property CalculatorButtonPanelTemplate can now be set to define and position the calculator buttons.
* In AvalonDock , The BorderBrush, BorderThickness and Background can now be modified when styling a LayoutAnchorableFloatingWindowControl or LayoutDocumentFloatingWindowControl.
* In PropertyGrid, Properties Array type, with ExpandableObject attribute, are now displayed in a numerical order instead of alphabetical order to have the indexes displayed correctly.
* In AvalonDock, The LayoutDocument and LayoutAnchorable can now be Enabled/Disabled with the new property LayoutContent.IsEnabled.
* In MessageBox, the MessageBox owner is now assigned correctly, whether the MessageBox is created normally or from another thread.
* In AvalonDock, with a LayoutFloatingWindow active, switching Windows Theme won't cause an invalid Window Handle Exception anymore.
* In AvalonDock, the LayoutDocuments repositionning in the LayoutDocumentPane (or the LayoutAnchorable repositionning in the LayoutAnchorablePane) can now be blocked with the new property LayoutPositionableGroup.CanRepositionItems.
* In CheckComboBox, CheckComboBox now has the IsEditable property. When True, the TextBox of the CheckComboBox can be edited to specify the SelectedItems.
* In CheckComboBox, when using Windows Classic Theme, the CheckMark will no longer disappear when Checked and MouseOver are true.
* In DropDownButton and SplitButton, a new property MaxDropDownContent has been created, allowing a maximum height for the popup in the control.
* In AvalonDock, using a dark Background Brush for the DockingManager in the System Theme won't hide the icons "DropDownMenu", "Pin" and "Close" anymore.
* In NavigationWindow, popping the NavigatorWindow in Windows 8 will no longer throw an exception.
* In PropertyGrid, the PrimitiveType Collections will now have their Editor.ItemType set when the collection is inheriting from a List<>.
* In AvalonDock, when a LayoutDocument is floating from a LayoutDocumentPane, docking it inside the same LayoutDocumentPane will now set its index to its last position saved instead of 0.
* In DateTimePicker, DateTimeUpDown, TimeSpanUpDown, TimePicker and DateTime, Parse is now used inside a try-catch, to prevent crashes for uncommon region settings.
* In PropertyGrid, Searching a property will not only be possible with properties "starting" with a set of letters, but also with properties "containing" a set of letters.
* In Wizard, with a binding to ItemsSource, it won't be necessary to set the CurrentPage, it will now be automatically set to the first WizardPage.
* In RichTextBox, the text selected containing more than 1 color won't disappear anymore.
* In MaskedTextBox, the property CharacterCasing is now taken into account when typing letters.
* In Chart, when using Axis.LabelsType as "Labels", the values won't be sorted anymore; they will be displayed in the order received.

We hope you love this release and decide to support the project.
-- Xceed Team

-----