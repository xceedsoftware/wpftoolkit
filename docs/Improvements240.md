{anchor:Community240}
## v2.4 Community Edition (Feb. 13, 2015)

_Shipped to registered users of the Plus Edition on Sept. 29, 2014. (Plus Edition is always at least one release ahead of the Community Edition)_ 

**37 bug fixes and improvements**

* In PropertyGrid, TextBox editor appearance improved when set to readonly in Windows7 Theme.
* In all UpDown controls (Numeric, Date, Time), the property AllowTextInput has been added to class InputBase. Users can now use it to control if the editing part of the control can be edited. Note that if property IsReadOnly is set, the editable part will be ReadOnly, as well as the Button Spinners.
* In TimePicker, a new property was added : MaxDropDownHeight. This propertly allows you to modify the height of the popup of the TimePicker.
* In DateTimePicker and DateTimeUpDown, when a date is typed with incomplete parts, the missing date parts will be auto-completed to 0 or 1 depending on the part.
* In AvalonDock, the Metro Theme will not throw warnings about undefined brushes.
* In CollectionControlDialog, collectionControlDialog will now return True if the 'OK' button is clicked and 'False' if the 'Cancel' button is clicked.
* In Chart, when using a LineLayout Chart with a Series.MarkerTemplate being empty, the dots won't be calculated and drawn. This will improve the performance of the control.
* In CollectionControl, a new property has been added on CollectionControl to get its PropertyGrid. Users will be able to be notifyed on a PropertyGrid.PropertyValueChanged event.
* In CollectionControlDialog, a new property is available in CollectionControlDialog : CollectionControl. This property returns the collectionControl from the CollectionControlDialog to let user access the CollectionControl's callbacks and events.
* In ColorPicker, When the ColorPicker is used inside a ListView, doing a MouseDown in the ColorCanvas of the ColorPicker will no longer closes the ColorPicker.
* In ColorPicker, a single tab is now needed to move to the next control from ColorPicker.
* In AvalonDock, when FocusElementManager.SetFocusOnLastElement is called through a Dispatcher.BeginInvoke, the priority has been changed from DispatcherPriority.Background to DispatcherPriority.Input.
* In AvalonDock, the AnchorablePaneControlStyle has a binding for the Background and Foreground, allowing the LayoutAnchorable to be transparent.
* In NumericUpDown, UpDownBase has a new property : ButtonSpinnerLocation. It can be set to Left or Right. It will position the Up/Down buttons on the left or on the right of the control.
* In MessageBox, starting a MessageBox from a different thread no longer throws an exception.
* In Chart, When the Chart uses a series with the property DataPointsSource, modifying a property in a DataPoint will update the chart. calling Area.Invalidate will no longer be necessary.
* In MultiLineTextEditor, opening the popup will set the focus in the editable part of the MultiLineTextEditor's popup.
* In PropertyGrid, EditorTemplateDefinition.TargetProperties will accept the special character {"**"}. When found, all the propertyItems with the name starting or ending with the string containing the special character will be affected by the Template defined in the EditorTemplateDefinition. ex : if {TargetProperties="Prop**"}, the PropertyItems named "Prop1", "Prop2", "Prop3"...will be affected by the Template.
* In PropertyGrid, if the PropertyGrid is categorized, the expandable PropertyItems will also be categorized.
* In StyleableWindow, StyleableWindow has been renamed to StyleableWindow.
* In ListBox, if a selection is done and something is typed in the SearchTextBox, the selection will no longer be cleared.
* In Wizard, if a Wizard has items that are not WizardPages, an exception will be raised.
* In ZoomBox, the default Foreground color of the ZoomBox is now Black instead of White.
* In ZoomBox, the ZoomBox will no longer have a Width/Height of 0 if its Width/Height is not defined. Instead, it will use its available space.
* In ZoomBox, setting ZoomBox.Zoom.Scale when starting the application will no longer throw a NullReferenceException.
* In DateTimePicker, TimePicker and DateTimeUpDown, the new property "Kind" let's user set if the timeZone is UTC, Local or UnSpecified. Setting this property fixes a bug where loosing focus on the DateTimePicker was incrementing time in UTC. It also fixes a bug where specifying a UTC Date was changing the date's kind to UnSpecified or Local.
* In DateTimePicker, the calendar display date is now updated when editing the date using the textbox.
* In NumericUpDowns, no PropertyChanged will be raised on lost focus when the value remains the same.
* In NumericUpDown, the displayed text won't be empty when initializing the NumericUpDown with Value equal to DefaultValue.
* In RichTextBox, the RichTextBoxFormatBar won't appear if the RichTextBox's IsReadOnly property is set to True.
* The AutoFilterControl items are now visible when using the MetroDark theme.
* The datagrid's FixedColumnSpliter now correctly updates its position while dragging it in the presence of scrolled columns.
* In RangeSlider, Background, BorderThickness and BorderBrush properties are now working properly. When oriented verticaly the width is now working properly.
* In RangeSlider, the keyboard Navigation is now fixed.
* RangeSlider.Orientation property is now of type System.Windows.Control.Orientation instead of RangeSlider.OrientationEnum type, which has been removed.
* In RangeSlider, it is not necessary to create a Style to be able to use RangeBackground, LowerRangeBackground and HigherRangeBackground. Using those properties directly on the RangeSlider will work.
* In RangeSlider, Data Binding on LowerValue, HigherValue, Minimum and Maximum properties are now working properly.

{anchor:Plus240}
## v2.4.0 Plus Edition additional improvements

**1 new control**

* Tokenized TextBox. Lets you display a series of values as tokens (similar to Microsoft Outlook's "TO:" textbox that transforms an email address you type into a name token).

**7 additional bug fixes and improvements**

* In PropertyGrid, the property LocalizedCategory from the class LocalizedCategoryAttribute is now virtual and can be overriden.
* In PropertyGrid, the new "DependsOn" attribute is available. It can be placed over any property of the PropertyGrid's selected object. A list of strings can be passed to the "DependsOn" attribute. The properties with this attribute will have their editor re-evaluated when the properties listed in the "DependsOn" attribute are modified.
* In MultiColumnComboBox, fixed a design-time bug.
* In PropertyGrid, the EditorDefinitions will permit binding on properties. The Update of properties has been reviewed to update when values are different.
* In RangeSlider, the clicking region to move the thumbs of the RangeSlider is now bigger in Metro Theme.
* In RangeSlider, when oriented vertically, the thumbs of themes Office2007 and Metro are now perpendicular to the Track.
* In RangeSlider, the Metro Theme has the same look as the Slider control of the Metro Theme.
* The 32 bug fixes listed above for [Community Edition v2.4.0](#Community240).

{anchor:Plus250}
## v2.5.0 Plus Edition

_Shipped to registered users of the Plus Edition on Nov. 24, 2014. (Plus Edition is always at least one release ahead of the Community Edition)_

**3 new controls**

* WatermarkComboBox, a ComboBox that lets you display text, graphics or other content while nothing has yet been selected.
* FilePicker, a full-featured file picker textbox that lets end users select one or more files and see selected files in textbox format. (Plus Only)
* SlideShow, a full featured slideshow control, with animated transitions and a variety of navigation options. Can display live WPF content. (Plus Only)

**49 bug fixes and improvements**

* In Chart, the Tick labels will be displayed with any small values. There is no more rounding to 2 decimals. (Plus Only)
* In PropertyGrid, CategoryDefinition has a new Property : IsBrowsable. This property will control wether you wish to display the entire category with all its properties. (Plus Only)
* In TokenizedTextBox, the position of the popup in X will be at the caret X position. The margin of the TokenizedTextBox will no longer be added. (Plus Only)
* In PropertyGrid, using PropertyGrid.PropertiesSource along with CategoryOrderAttribute and PropertyOrderAttribute will now sort Categories and propertyItems properly. (Plus Only)
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

{anchor:Plus260}
## v2.6.0 Plus Edition

_Shipped to registered users of the Plus Edition on Feb. 9, 2015. (Plus Edition is always at least one release ahead of the Community Edition)_

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

We hope you love this release and decide to support the project.
-- Xceed Team

-----