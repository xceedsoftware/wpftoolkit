{anchor:Community300}
## v3.0.0 Community Edition

_Released December 13, 2016._

**New controls**

* WatermarkPasswordBox control. Lets users enter a password that gets stored in a SecureString, and displays a watermark if no password is defined.

**28 bug fixes and improvements**

* In NumericUpDown, DateTimeUpDown, DateTimePicker, TimePicker, TimeSpanUpDown and CalulatorUpDown, when the property UpdateValueOnEnterKey is True, the ButtonSpinners, the keyboard arrows and the mouse wheel will no longer update the Value property. The Value property will now only be updated on a “Enter” key press or a lost focus of the UpDown control.
* In CollectionControl, objects of type ICollection and IList are now be supported.
* In UpDown controls, when the Value property is bound to a property which coerce the received value, the Text property will now be refreshed accordingly.
* In TimeSpanUpDown, selecting the minus sign(“-“) with the mouse and incrementing/decrementing the control will no longer reset the Value to null.
* In ListBox, doing a ListBoxItem drag while its drop fade out animation isn’t completed will no longer result in a Null reference exception.
* In CollectionControl, the objects of type IDictionary are now supported.
* In NumericUpDowns, when the property ClipValueToMinMax is True and the property Minimum becomes greater than Value (or the property Maximum becomes lower than Value), the Value property will now be re-evaluated to respect the Minimum – Maximum range.
* In SplitButton, the BorderThickness will now be modifiable.
* In PropertyGrid, when trying to edit a List of T, the CollectionControlDialog will no longer crash if the T class doesn’t include a default Constructor.
* In PropertyGrid, registering to the IsPropertyBrowsable event and not doing anything in the callback, will no longer display the properties with BrowsableAttribute(false).
* In CollectionControl, the ListBoxItem will now be updated when a property value is modified in the PropertyGrid.
* In PropertyGrid, the DisplayAttribute will now be supported for PropertyItems.
* In Wizard, the Finish event will now be a CancelRoutedEventHandler.
* In NumericUpDowns, the FormatString property will now accept strings like the BindingBase.StringFormat. Ex : “{}{0:N2} ms”.
* In CheckListBox, the selectedItems will not be cleared anymore if the ItemSource’s filter is removed.
* The PropertyGrid will now support the Range attribute to set the Maximum/Minimum properties on NumericUpDown/DateTimeUpDown and TimeSpanUpDown editors.
* In AvalonDock, DockingManager.LogicalChildren will now be correctly updated when a LayoutDocument/LayoutAnchorable is removed by modifying the DockingManager.DocumentsSource/AnchorablesSource properties or by closing a LayoutDocument/LayoutAnchorable.
* In PropertyGrid, the attribute TypeConverter (of type ExpandableObjectConverter) will now be supported to expand a PropertyItem.
* In PropertyGrid, TypeConverter.GetStandardValues() will now be supported to display options in a ComboBox editor.
* In NumericUpDowns, DatetimePicker, SplitButton, RichTextBoxFormatBar, MultiLineTextEditor, DropDownButton, ColorPicker, CollectionControl, CalculatorUpDown, and TimePicker, the arrows will not look pixelated anymore in high DPI.
* In PropertyGrid, the setter of an expandable propertyItem will now be called when one of its sub-propertyItem is modified.
* In DateTime controls, using a Custom format with no dateTime separators will now correctly display the selected dateTime.
* In DropDownButton and SplitButton, the new property DropDownPosition will now be available to set the position of the popup relative to the control.
* In RangeSlider, setting a LowerValue greater than HigherValue will no longer be possible. Setting a HigherValue smaller than LowerValue is also no longer possible.
* In TimeSpanUpDown, only numeric characters will now be accepted.
* In WatermarkComboBox, the toggleButton will now use the DisplayMemberPath when specified.
* In the modal ChildWindow and MessageBox, Tab navigation will now remain inside the control. Also, the Menu shortcut keys will no longer be available.
* In ColorPicker, pressing the “Esc” Key will now reset the SelectedColor to the last selected color.

{anchor:Plus300}
## v3.0.0 Plus Edition

_Released to registered users of the Plus Edition on April 5, 2016._

**New controls**

* MultiCalendar control. Lets you present and allow date selections on multiple calendars positioned in any number of rows and columns. (Plus Only)
* The WatermarkPasswordBox control from the Community Edition listed above.

**41 improvements and bug fixes**

* 28 bug fixes and improvements from the Community Edition listed above, plus 13 more listed here.
* In TokenizedTextBox, setting the focus will now be supported in code-behind.
* In Calendar, when using the Metro theme, the “X” for the Blackout dates will now be visible.
* In MaterialTextField, updating the Text property with a binding will now animate the Watermark accordingly.
* In Chart, the new property IntersectMinValue will make it possible to move an axis on the other side of the chart.
* In Chart, changing the collection of DataPoints will now update the axis so that the layout will fill the chart dimension.
* In ListBox, the SelectedItems.Count will now be correctly updated when the ListBox is not visible.
* In ListBox, the ItemsCount will now be updated when the ListBox is filtered.
* In Chart, using a series.DataPointsSource binding will now refresh the series when the binding source is updated.
* In ListBox, setting a Filtercontrol.DataSourceFilterExpression in code-behind will now update (if possible) the SearchTextBox with the provided filter.
* In ListBox, clearing the ListBox’s source right after a FilterControl’s filter is set to null (without a layout pass between the 2 actions) will no longer throw a NullRefException.
* In ListBox, the scrollbars won’t be displayed anymore when using a filter and adding new items when there is enough space to display all those items.
* In TokenizedTextBox, the property IsValid will no longer have a setter.
* In StyleableWindow, a new property TitleFontSize can now be used to set.

{anchor:Plus310}
## v3.1.0 Plus Edition

_Released to registered users of the Plus Edition on July 12, 2016._

**56 improvements and bug fixes**

* All controls in the Toolkit now have a Windows 10 theme. (Plus Only)
* In Chart, the new property Axis.LabelDecimalCount can now be used to specify the decimal count for axis labels. (Plus Only)
* In Chart, axes will no longer duplicate DateTime labels. (Plus Only)
* In Chart, the column bars will now have a MinWidth of 5 pixels, preventing very thin bars for series with a large range. (Plus Only)
* In ListBox, combining SelectionRanges with predicates along with SelectionRanges without predicates will now correctly update the SelectedItems.Count. (Plus Only)
* In MaterialTabItem, the property CornerRadius is now available to customize the corners the of TabItems. (Plus Only)
* In MaterialTabControl, the BorderThickness property will now affect the header part. (Plus Only)
* In MaterialSlider, using a binding on the Value property will no longer freeze the thumb's movement. (Plus Only)
* In RadialGauge, a binding can now be set on the Maximum property. (Plus Only)
* In RadialGauge, the GaugePortion.Portion property is now a DP and binding is available. (Plus Only)
* In RadialGauge, the marker's position are now updated when the Marker.Value is changed. (Plus Only)
* In SlideShow, modifying the properties CurrentIndex or CurrentItem will now play the sliding animation. (Plus Only)
* In SlideShow, the VerticalContentAlignment and HorizontalContentAlignment will now align the content correctly. (Plus Only)
* In StyleableWindow, MaterialToast can now be shown. (Plus Only)
* In ToggleSwith, clicking on the thumb will now also toggle the switch. (Plus Only)
* In TokenizedTextBox, defaulting with the property IsDropDownOpen true will now open the suggestion popup if the Text property can be found in the ItemsSource.  (Plus Only)
* In TokenizedTextBox, the performance of filtering is now improved for large number of TokenizedTextBoxItems. (Plus Only)
* In PropertyGrid, the PrimitiveTypeCollectionEditor will now set its Editor.ItemType based on any generic type objects. (Plus Only)
* In PropertyGrid, the new property IsExpandingNonPrimitiveTypes is now available to expand and edit non-primitive type properties. The Collections/List of objects can now be expanded to edit their sub-items. (Plus Only)
* In PropertyGrid, expandable PropertyItems will now be able to specify the sub-PropertyItems that will be displayed.
* In PropertyGrid, new methods will now be available to set the vertical scrolling position.
* In PropertyGrid, modifying a PropertyItem many times will not longer create a memory leak since the PropertyItem's subscription to the PropertyChanged event will now be cleared when unneeded.
* In PropertyGrid, editing a List or Collection of objects, containing a List or Collection of sub-objects,  will no longer cause a crash.
* In PropertyGrid, the Reset Value command will now be available when the property ShowAdvancedOptions is set to True.
* In PropertyGrid, new methods are now available to expand/collapse, from code-behind, all the PropertyItems or specific propertyItems. 
* In PropertyGrid, doing a mouse over the SearchTextBox will no longer resize the PropertyGrid's controls.
* In PropertyGrid, an expandable propertyItem of type Array will now always be ordered by their index.
* In AvalonDock, closing a LayoutDocument by clicking the "X" button, when it is not the current one, will no longer cause a crash.
* In AvalonDock, dragging a maximized floating window will now update its IsMaximized property.
* In AvalonDock, unhiding an auto-hidden LayoutAnchorable will no longer cause a crash.
* In AvalonDock, Having the focus on a LayoutDocumentFloatingWindow or LayoutAnchorableFloatingWindow will now set the Highlight brush on its border.
* In CheckComboBox, the method UpdateText is now virtual. A user can override it to prevent automatic updates of the Text property based on SelectedItems.
* In ChildWindow, modifying the Content when the WindowStartupLocation is centered will now always pop the ChildWindow in the center of its parent.
* In CheckComboBox and CheckListBox, the SelectedMemberPath property now supports nested paths.
* In CheckComboBox, when the DisplayMemberPath property is used for a nested path, the SelectedValue string will now display selected strings based on the DisplayMemberPath.
* In CheckComboBox, the checkBoxes will no longer become disabled when the Text property is set by the user.
* In ColorPicker, setting the property DisplayColorAndName to True and using new ColorItems with specific color names as the available colors will no longer display color "hex" names. The specific color names will now be used.
* In ColorPicker, the new property MaxDropDownWidth is now available to customize the width of the popup.
* In CollectionControl, when a collection item A has a property of type collection (or list) B, adding an item to B will no longer add a null collection (or list).
* In DateTimePicker, the new property CalendarWidth is now available to set the size of the Calendar inside the popup.
* In DateTimePicker, the included TimePicker.Step property will now be bound to DateTimePicker.Step.
* In DateTimePicker, TimePicker and DateTimeUpDown, the UpdateValueOnEnterKey is now True by default. This will let user completely type the DateTime before the validation occurs. Also, the key inputs ".,/:" will no longer move the focus to another DateTime part.
* In DateTimePicker, clicking the TimePicker's TextBox, Spinners or ToggleButton while the TimePicker popup is opened will now close the TimePicker popup.
* In TimeSpanUpDown, DateTimeUpDown and FilePicker, the Tab navigation is now available.
* In MaterialSlider, the TickFrequency is now respected when the ShowTicks property is set to False.
* In MaskedTextBox, removing the Mask will now correctly remove the underlined characters from the Text property.
* In MaterialToast, the slide in of toasts can now be done from left or right edges.
* In NumericUpDowns, DateTimeUpDown, DateTimePicker, TimePicker, TimeSpanUpDown, CalculatorUpDown, PrimitiveTypeCollectionControl, ColorPicker, FilePicker and MultiLineTextEditor, calling the Focus method on these controls will now give them the focus.
* In RangeSlider, the properties TickPlacement and TickFrequency will now be available to display the Ticks.
* In RangeSlider, the property IsSnapToTickEnabled will now be available to snap the dragging thumb to the next tick mark.
* In RangeSlider, the new properties AutoToolTipPlacement and AutoToolTipPrecision will now be available to position and set the precision for the ToolTip of a Thumb.
* In StyleableWindow, ChildWindow and MessageBox, when setting the properties WindowThickness and WindowBorderThickness to 0, there will no longer remain a 1 pixel border.
* In StyleableWindow, an implicit style for StyleableWindowKey is now defined.
* In TimeSpanUpDown, the new property ShowSeconds is now available.
* In TokenizedTextBox, removing a SelectedItem will no longer cause a crash.
* In ValueRangeTextBox, when the property BeepOnError is true, the beep will now be heard when the value is out of range or of the wrong type.

{anchor:Plus320}
## v3.2.0 Plus Edition

_Released to registered users of the Plus Edition on November 21, 2016._

**New controls**

* IconButton control. Lets you easily add an icon and some data to a button’s content. Also makes it easier to customize the “Background”, “BorderBrush” and “Foreground” properties on “MouseOver” and “MousePressed” events.
* MaterialHamburger control. Hamburger menu control for building apps with the Material Design look.

**44 improvements and bug fixes**

* In MaterialSwitch, when unchecked, the "CheckBackground" will no longer be visible behind the thumb. (Plus Only)
* In Chart, when "loaded" and "displayed", adding dataPoints will now correctly align the dataPoints and the grid lines. (Plus Only)
* In AvalonDock, the MetroAccent theme can now be set from XAML. (Plus Only)
* In ChildWindow, double-clicking on the caption bar will no longer maximize it when the "ResizeMode" is set to "NoResize" or "CanMinimize". (Plus Only)
* In MaterialToast, the MaterialToast defined in XAML will no longer throw an exception when its property "IsOpen" becomes true. (Plus Only)
* In MaterialToast, if the MaterialToast is defined in XAML and its "IsOpen" property is changed while the sliding in/out animation is still active, the MaterialToast will no longer become invisible. (Plus Only)
* In MaterialToast, calling the "HideToast()" and "ShowToast()" methods will no longer destroy a binding on the "IsOpen" property. (Plus Only)
* In MaterialToast, the close button will now always be enabled when calling the default constructor. (Plus Only)
* In MaterialToast, the new events "Showing" and "Hiding" are now raised when the control starts to show/hide. (Plus Only)
* In DateTimeUpDown, DateTimePicker and TimePicker, an exception will now be raised to remind the user to set a "FormatString" when the format is set to custom. (Plus Only)
* In MaterialToast, setting the "VerticalContentAlignment" and "HorizontalContentAlignment" properties are now templatebound properly. (Plus Only)
* In MaterialTabControl, starting with a null "SelectedItem" and clicking on a new MaterialTabItem will no longer cause a crash. (Plus Only)
* In MaterialTabControl, when the "SelectedItem" is null, the first MaterialTabItem will no longer have a null Foreground. (Plus Only)
* In Magnifier, when the mouse is not moving, modifying the "ZoomFactor" property will now keep its content centered. (Plus Only)
* In MaterialTextField, setting a "Text" without a "Watermark" will no longer cause a crash upon loading. (Plus Only)
* In ChildWindow, MessageBox, StyleableWindow and WindowControl, the new event "Activated" will now be raised when the "IsActive" property becomes true. (Plus Only)
* In Expander, when using with "ExpanderDirection" set to  "Left" or "Right" under the Metro/Office2007 themes, the arrow and the text will now correctly be aligned. (Plus Only)
* In TokenizedTextBox, using the "SelectedItemsOverride" property while the TokenizedTextBox is placed inside a DataTemplate will no longer crash. (Plus Only)
* In TokenizedTextBox, when the "ItemsSource" is a CollectionViewSource and this CollectionViewSource is modified, typing(or removing text) in the TokenizedTextBox will no longer cause a crash. (Plus Only)
* In DataGridControl, the DataGridCheckBoxes will no longer have a blue Background. (Plus Only)
* In AvalonDock, dragging a FloatingWindow to the top of the screen will now maximize it.
* In all controls, when a ControlTemplate is redefined with a missing templated part name, the application will no longer cause a crash.
* In all UpDown controls, setting the "MouseWheelActiveTrigger" property to "MouseOver" will now work as expected.
* In ChildWindow, MessageBox, StyleableWindow and WindowControl, the new event "Activated" will now be raised when the "IsActive" property becomes true.
* In DateTimePicker, TimePicker and DateTimeUpDown, the modification of "UpdateValueOnEnterKey" property will no longer be supported. It will always be "True" to simplify input validation.
* In the TimeSpanUpDown, typing a large value for hours, minutes or seconds will no longer reset the "Value" property.
* In the Zoombox, when using the scrollBars, clicking on the scrollBar's tracks will now scroll by a page.
* In AvalonDock, the new property "AllowDuplicateContent" will now prevent from having 2 identical LayoutDocuments/LayoutAnchorables in a single pane.
* In the PropertyGrid, using the property "PropertyDefinitions" on an expanded PropertyItem(to display specific sub-propertyItems) will now work properly.
* In AvalonDock, serializing a custom LayoutDocument (or LayoutAnchorable) in a LayoutFloatingWindow will now be possible.
* In DateTimePicker, when modifying the template of the DateTimePicker to use a MaskedTextBox as the "PART_TextBox" templated part, the caret position will now be correctly updated while typing.
* In PropertyGrid, a horizontal scrollBar will now be displayed when needed.
* In SplitButton and DropDownButton, the property "IsDefault" is now available.
* In PropertyGrid, defining a custom editor as an attribute in another assembly will now be found properly.
* In PropertyGrid, when using grouping, the outOfRangeException will no longer be thrown when typing text in the SearchTextBox.
* In SplitButton and DropDownButton, using an implicit style on type "Button" will no longer affect them.
* In PropertyGrid, the "DataContext" of a custom editor will no longer be overwritten.
* In PropertyGrid, the attribute "DefaultValue" will now set in bold the "Value" of the corresponding PropertyItem when equal.
* In PropertyGrid, the "Update()" method will no longer throw a null reference exception.
* In MaskedTextBox, if the "Mask" property is "&gt;"( or "&lt;"), all the input characters will be converted to upper case(or lower case).
* In PropertyGrid, the "DisplayName" attribute of a fake type, used by an ICustomTypeProvider, will now be displayed as the SelectedObject's type.
* In PropertyGrid, the new property "IsVirtualizing" can now be used to virtualize the PropertyItems.
* In Zoombox, the property ViewFinder will now have a setter to add your own Zoombox's ViewFinder.
* In ColorPicker, tabs are now used to switch from "standard" to "advanced" view.

We hope you love this release and decide to support the project.
-- Xceed Team

-----