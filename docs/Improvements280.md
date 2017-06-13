{anchor:Community280}
## v2.8.0 Community Edition (May 6, 2016)

**23 bug fixes and improvements**

* In BusyIndicator, the FocusAfterBusy control can now be focused more than once when BusyIndicator is no longer busy.
* In AvalonDock, when DockingManager.DocumentsSource changes, the private variable _suspendLayoutItemCreation will now be correctly set during the adding phase.
* In AvalonDock, removing a LayoutAnchorable will now null its Content before removing it from his Parent. This prevents memory leaks.
* In DropDownButton, Closing the DropDown will no longer throw a null reference exception.
* In PropertyGrid, the property AdvancedOptionsMenu is now set in the PropertyGrid's default style, preventing restricted access to propertyGrid in multithreading.
* In MaskTextBox, using a null mask will no longer clear the Text value on LostFocus.
* In RichTextBoxFormatBar, the correct adorner is now found when there are more than one, preventing a NullReference exception on a drag.
* In PropertyGrid, CollectionEditor now takes into account the NewItemTypes attribute.
* In DropDownButton, Setting the Focus via the Focus() method won't need a "Tab" to actually focus the DropDownButton.
* In AvalonDock, the focus of the active content is now set right away. This prevents the focus issues with comboBoxes in WinForms.
* In ZoomBox, The Fit and Fill options of the ZoomBox now centers correctly the ZoomBox content.
* In AvalonDock, Clicking the middle mouse button on LayoutDocumentItem header or LayoutAnchorableItem header will no longer throw a Null reference exception.
* In NumericUpDown, using a FormatString with "P" can now convert the typed and displayed value as its decimal value. For example, typing 10 will display "10%" and the property "Value" will be "0.1".
* In CheckComboBox, changing the CheckComboBox's Foreground now changes the Foreground Color of the ToggleButton it contains.
* In AvalonDock, pinning an AutoHideWindow, located in LayoutRoot.XXXSide, now has a correct width. User won't have to extend the ColumnSplitter to see the content. The Width used is the LayoutAnchorable.AutoHideMinWidth.
* In CheckComboBox, the control no longer loses its selection while being virtualized.
* In DateTimePicker, DateTimeUpDown, years of 2 or 4 digits are now handled. Also, entering a wrong format date will now default to current date, not to 1/1/0001.
* In PropertyGrid, the DescriptorPropertyDefinition is no longer ignoring the ConverterCulture in its Binding. This results in correct conversion for text numeric values in PropertyGrid's EditorTemplateDefinition
* In AvalonDock, if a floatingWindow is maximized and then the DockingManager is minimized, upon restoring the DockingManager, the floatingWindow will now remain maximized.
* In AvalonDock, the new property LayoutDocumentPane.ShowHeader is now available to Show/Hide the TabHeader.
* In TimePicker, the method CreateTimeItem is now proptected virtual.
* For all toolkit controls, an InvalidOperationException is no longer thrown when VisualTreeHelper gets its child.
* For themes different than Windows 8, the exception of type "Initialization of 'Xceed.Wpf.Toolkit.XXX' will no longer be thrown in Visual Studio 2015. This affects all controls of the toolkit.

{anchor:Plus280}
## v2.8.0 Plus Edition

_Shipped to registered users of the Plus Edition on Sept. 14, 2015._

**A total of 29 bug fixes and improvements**

* 23 bug fixes and improvements from the Community Edition listed above, plus 6 more listed here.
* In ListBox, Grouping a ListBox with an enum type property and selecting an item will now add the selected items in the SelectedItems collection.
* In ListBox, filtering a ListBox with a predicate defined through the ListBox.Filter event will now be considered in the SelectedItems.
* In ListBox, a filtered listbox using grouping will now return the correct SelectedItems by applying the Filter.
* In MaterialListBox, MaterialComboBox and MaterialTabControl, the MaterialControls style is now applied when using ItemsSource.
* In TokenizedTextBox, the TokenizedTextBox's popup will now always be displayed on the same monitor as the TokenizedTextBox.
* In TokenizedTextBox, if the control is placed in a popup, it will now display the tokens correctly.

{anchor:Plus290}
## v2.9.0 Plus Edition

_Shipped to registered users of the Plus Edition on February 10, 2016._

**A total of 44 improvements and bug fixes**

* Setting the LicenseKey of a Toolkit Metro Theme in xaml and using a Toolkit control in MainWindow.Resources will no longer result in an exception. (Plus Only)
* In Zoombox, when the property IsUsingScrollBars is true and one of its srollbars is moved, the event Scroll will now be raised. (Plus Only)
* In TokenizedTextBox, the scrollBar’s thumb of the suggestion popup will now have a standard height. (Plus Only)
* In PropertyGrid, when using multi-Selected Objects, the selected objects type and name will now be displayed at top of the PropertyGrid. (Plus Only)
* The CollectionControlDialog will now be a StyleableWindow and will be themed when theming is defined in App.xaml. (Plus Only)
* In StyleableWindow, when maximized, the header buttons will no longer be cropped. (Plus Only)
* In ListBox, the Drag and Drop will now work after showing + adding a panel over the Drop area. (Plus Only)
* In ListBox, creating a SelectionRange without a SortDescription’s list and using FromItem/ToItem is now possible. This will result in a selection based on the ListBox dataSource sort, not the ListBox visual sort. (Plus Only)
* In SlideShow, using a binding on ItemsSource will now correctly set the CurrentItem. (Plus Only)
* In TokenizedTextBox, the new event InvalidValueEntered will now be raised when resolving an invalid value. The InvalidValueEventArgs will give access to the invalid value through a Text property. (Plus Only)
* The TokenizedTextBox size will now expand normally if there is enough space when adding many items. (Plus Only)
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

{anchor:Plus300}
## v3.0.0 Plus Edition

_Shipped to registered users of the Plus Edition on April 5, 2016._

**New controls**

* MultiCalendar control. Lets you present and allow date selections on multiple calendars positioned in any number of rows and columns. (Plus Only)
* WatermarkPasswordBox control. Lets users enter a password that gets stored in a SecureString, and displays a watermark if no password is defined.

**A total of 43 improvements and bug fixes**

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
* In DropDownButton and the SplitButton, the new property DropDownPosition will now be available to set the position of the popup relative to the control.
* In RangeSlider, setting a LowerValue greater than HigherValue will no longer be possible. Setting a HigherValue smaller than LowerValue is also no longer possible.
* In TimeSpanUpDown, only numeric characters will now be accepted.
* In WatermarkComboBox, the toggleButton will now use the DisplayMemberPath when specified.
* In Modal ChildWindow and the MessageBox, Tab navigation will now remain inside the control. Also, the Menu shortcut keys will no longer be available.
* In ColorPicker, pressing the “Esc” Key will now reset the SelectedColor to the last selected color.


We hope you love this release and decide to support the project.
-- Xceed Team

-----