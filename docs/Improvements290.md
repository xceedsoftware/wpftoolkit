{anchor:Community290}
## v2.9.0 Community Edition

_Released June 14, 2016._

**32 bug fixes and improvements**

* In ListBox, the addition of a range of items will now be supported resulting in less notifications when many items need to be added.
* In PropertyGrid, selectedObject implementing ICustomTypeDescriptor will now load correctly with a call to customTypeDescriptor.GetPropertyOwner().
* In AvalonDock, LayoutAutoHideWindowControl won’t throw an invalid handle exception anymore upon disconnecting-reconnecting from a Virtual Machine.
* In Toolkit Metro controls, setting a control’s height will not affect the data displayed in that control.
* In AvalonDock, deserialization of custom panes is now supported.
* In AvalonDock, the context menu of a LayoutAnchorableFloatingWindowControl will now always show the AnchorableContextMenu.
* In ColorPicker, 2 new events will now be raised when its popup is opened or closed.
* In DateTimePicker, the new property CalendarDisplayMode will now be available to modify the display of the Calendar.
* In DateTimePicker, DateTimeUpDown, TimePicker, CalculatorUpDown, FilePicker and TimespanUpDown, the IsTabStop property is now supported.
* In DateTimePicker, DateTimeUpDown and TimePicker, the selected date part will now remain selected after the Value property is changed via binding.
* In CollectionControl, a struct type can now be added as a new item.
* In AvalonDock, closing the last “Active” LayoutDocument will not set a new “Active” item anymore. This will prevent the AutoHide window popup to be shown as the new default “Active” item.
* In Calculator and CalculatorUpDown, pressing the Memory buttons while an “Error” is displayed in the calculator will not throw a Format exception anymore.
* In Calculator and CalculatorUpDown, the MR button can now be used into equations.
* In Magnifier, zooming with the mouseWheel will now be supported.
* In ListBox, adding Value type items is now supported.
* In ListBox, the Equals method override will now be supported for non-primitive type ListBoxItems.
* In controls using a ButtonSpinner, the “Enter” key will now be disabled when an arrow of the ButtonSpinner has the focus.
* In ListBox, doing a foreach loop on the ListBox.Items property will now be supported.
* In DateTimePicker, only one ValueChanged event will now be raised when selecting a new date.
* In SlideShow, modifying the collection of Items will now correctly set the Previous/Current/Next SlideShow Items.
* In AvalonDock, Deserializing a NodeType of type XmlNodeType.Whitespace will no longer throw an exception.
* In CollectionControlDialog, the “Cancel” button will now roll back the changes done in its CollectionControl’s PropertyGrid.
* In PropertyGrid, the interface ICustomTypeProvider is now supported for the SelectedObject.
* In PropertyGrid, the Resources properties will now display the correct icon when ShowAdvancedOptions is True.
* In DateTimePicker, CalculatorUpDown, ColorPicker, DropDownButton, MultiLineTextEditor and TimePicker, the Tooltip will now be used only in the collapsed part of the control; the popup will no longer display it.
* In PropertyGrid, the editor for properties of type FontFamily will now be a comboBox containing sorted Font names.
* In DateTimeUpDown, TimeSpanUpDown, TimePicker and DateTimePicker, the new property CurrentDateTimePart is now available to set the date time part that can be changed with the Up/Down buttons or the Up/Down keys.
* In DateTimeUpDown, TimeSpanUpDown, DateTimePicker and TimePicker, the new property “Step” can now be set to customize the increment/decrement value of DateTime controls.
* In PropertyGrid, the new event IsPropertyBrowsable will now be raised at the creation of each PropertyItem in order to use the callback and individually set the visibility of each propertyItem in the PropertyGrid.
* In NumericUpDowns, DateTimeUpDown, DateTimePicker, TimePicker and TimeSpanUpDown, the new event Spinned will now be raised when an Increment/Decrement action is initiated.
* The Metro theme Toolkit controls will now share the same height as the core metro theme controls.

{anchor:Plus290}
## v2.9.0 Plus Edition

_Released to registered users of the Plus Edition on February 10, 2016._

**43 improvements and bug fixes**

* 32 bug fixes and improvements from the Community Edition listed above, plus 11 more listed here.
* Setting the LicenseKey of a Toolkit Metro Theme in xaml and using a Toolkit control in MainWindow.Resources will no longer result in an exception.
* In Zoombox, when the property IsUsingScrollBars is true and one of its srollbars is moved, the event Scroll will now be raised.
* In TokenizedTextBox, the scrollBar’s thumb of the suggestion popup will now have a standard height.
* In PropertyGrid, when using multi-Selected Objects, the selected objects type and name will now be displayed at top of the PropertyGrid.
* The CollectionControlDialog will now be a StyleableWindow and will be themed when theming is defined in App.xaml.
* In StyleableWindow, when maximized, the header buttons will no longer be cropped.
* In ListBox, the Drag and Drop will now work after showing + adding a panel over the Drop area.
* In ListBox, creating a SelectionRange without a SortDescription’s list and using FromItem/ToItem is now possible. This will result in a selection based on the ListBox dataSource sort, not the ListBox visual sort.
* In SlideShow, using a binding on ItemsSource will now correctly set the CurrentItem.
* In TokenizedTextBox, the new event InvalidValueEntered will now be raised when resolving an invalid value. The InvalidValueEventArgs will give access to the invalid value through a Text property.
* The TokenizedTextBox size will now expand normally if there is enough space when adding many items.

{anchor:Plus300}
## v3.0.0 Plus Edition

_Released to registered users of the Plus Edition on April 5, 2016._

**New controls**

* MultiCalendar control. Lets you present and allow date selections on multiple calendars positioned in any number of rows and columns. (Plus Only)
* WatermarkPasswordBox control. Lets users enter a password that gets stored in a SecureString, and displays a watermark if no password is defined.

**41 improvements and bug fixes**

* In TokenizedTextBox, setting the focus will now be supported in code-behind. (Plus Only)
* In Calendar, when using the Metro theme, the “X” for the Blackout dates will now be visible. (Plus Only)
* In MaterialTextField, updating the Text property with a binding will now animate the Watermark accordingly. (Plus Only)
* In Chart, the new property IntersectMinValue will make it possible to move an axis on the other side of the chart. (Plus Only)
* In Chart, changing the collection of DataPoints will now update the axis so that the layout will fill the chart dimension. (Plus Only)
* In ListBox, the SelectedItems.Count will now be correctly updated when the ListBox is not visible. (Plus Only)
* In ListBox, the ItemsCount will now be updated when the ListBox is filtered. (Plus Only)
* In Chart, using a series.DataPointsSource binding will now refresh the series when the binding source is updated. (Plus Only)
* In ListBox, setting a Filtercontrol.DataSourceFilterExpression in code-behind will now update (if possible) the SearchTextBox with the provided filter. (Plus Only)
* In ListBox, clearing the ListBox’s source right after a FilterControl’s filter is set to null (without a layout pass between the 2 actions) will no longer throw a NullRefException. (Plus Only)
* In ListBox, the scrollbars won’t be displayed anymore when using a filter and adding new items when there is enough space to display all those items. (Plus Only)
* In TokenizedTextBox, the property IsValid will no longer have a setter. (Plus Only)
* In StyleableWindow, a new property TitleFontSize can now be used to set. (Plus Only)
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

We hope you love this release and decide to support the project.
-- Xceed Team

-----