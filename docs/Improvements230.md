{anchor:Community230}
## v2.3 Community Edition (Oct 7, 2014)

**Special offer announcement**

* For a limited time, the price has been lowered from $199.95 to $149.95 for the [Plus Edition](https://wpftoolkit.codeplex.com/wikipage?title=Compare%20Editions). [Please support the project](http://wpftoolkit.com/shop). We accept credit cards, PayPal and bitcoin. The price of the Plus Edition with 1-year support and updates subscription has also been lowered for a limited time from $349.95 to $199.95.

**25 bug fixes and improvements**

* Major update to the datagrid control, with performance updates in many scenarios, bug fixes, and support for asynchronous binding.
* In PropertyGrid, users can now tab between PropertyItems without putting focus on the Name Part. Only the Editing Part will get the focus. ReadOnly properties will be skipped.
* In PropertyGrid, changing PropertyGrid.PropertyDefinitions in code-behind will update the visible PropertyItems in the PropertyGrid.
* In PropertyGrid, custom PropertyItems are now sorted alphabetically.
* In DataGrid, during design-time, the message "The specified view type must derive from ViewBase." will no longer appear. (There are too many bug fixes in the updated DataGrid to list, this particular one is mentionned specifically because an Extended WPF Toolkit user reported it.)
* In CollectionControl and PropertyGrid's CollectionControl, eliminated a crash when closing the control if it used arrays with an ItemsSource.
* In DateTimePicker, two new Properties have been added. TimePickerAllowSpin and TimePickerShowButtonSpinner, to allow for more flexibility to style the TimePicker subcontrol.
* In DateTimePicker, when an invalid date is entered, the displayed date won't be January 1st of the year 1 (DateTime.Min). It will be the last valid date, or DateTime.Now.
* In DateTimePicker, setting a Min/Max value will blackout unavailable dates in the calendar popup.
* In DateTimePicker, DateTimeUpDown, TimePicker, using a cultureInfo.DateTimeFormat.ShortDatePattern with a repetition of dateParts (like : ddd dd/MM/yyyy) will no longer throw an exception.
* In AvalonDock, users can use the Alt key adornments inside the AvalonDock DockingManager. E.g., to set an underline under the first letter of the content of a button.
* In AvalonDock, a 'File not found' exception (due to themes) will no longer happen when loading an application with AvalonDock controls.
* In AvalonDock, the Visual Studio 2010 theme will set the SelectedStyle when moving a tool window into the LayoutDocumentPane.
* In AvalonDock, debug output traces have been removed.
* In AvalonDock, LayoutAnchorables and LayoutDocuments are now removed when LayoutAnchorable.Close() or LayoutDocument.Close() is called and DocumentManager is null. Relevant when LayoutSerializationCallbackEventArgs.Cancel is true.
* In CheckComboBox, if a Delimiter different than coma is used while the checkComboBox is in a DataTemplate and has a SelectedValue, then SelectedValue will no longer be empty.
* In UpDown controls using UpDownBase, a new 'Focused' option is available for the MouseWheelActiveTrigger. It allows the user to use the mouseWheel over or outside the focused control to increment/decrement its value.
* In ChildWindow, a centered ChildWindow can be offset with its margin property.
* In WindowContainer, the Microsoft DatePicker control will now work when it is in a ChildWindow.
* In DropDownButton and SplitButton, styling with Background, Foreground, BorderThickness and BorderBrush will now work. 
* In Ultimate ListBox, the SearchTextBox now has a new 'IsCaseSensitive' property that is false by default. It determines if the search should be be case sensitive.
* In RichTextBox, when setting a transparent background, the Transparent color will be applied.
* In ZoomBox, when ZoomBox.Scale is bound to a property and has a DataContext, setting the dataContext to null will no longer cause a StackOverflow Exception.
* In ColorPicker, when ShowAdvanceButton is false, the separator above the AdvanceButton is removed (missed v2.2 release note).
* PrimitiveTypeCollectionControl and PrimitiveTypeCollectionEditor no longer get an IndexOutOfRange exception when checking for generic arguments.

{anchor:Plus230}
## v2.3.0 Plus Edition additional improvements 

_Shipped to registered users of the Plus Edition on June 2, 2014. (Plus Edition is always at least one release ahead of the Community Edition)_

**1 new control**

* ToggleSwitch. A highly customizable, and fully themed on/off switch.

**30 bug fixes and improvements**

* In PropertyGrid, the editor for collections of objects (CollectionEditor) is now themed in the Metro and Office2007 themes.
* In PropertyGrid, when using PropertyGrid.PropertiesSource, if a property is a collection of strings, the CollectionEditor now allows the collection to be edited.
* In PropertyGrid, you can now set the ExpandableObjectAttribute parameter. A value of true will set PropertyItem to Expanded, while false will set PropertyItem to Collapsed.
* In Chart, in the designer, a value changed in a DataTemplate (located in the Resources) will now correctly refresh. E.g., changing the margin of a TextBlock in a DataTemplate.
* In Chart, titles specified for the Y Axis are now vertically oriented.
* The 25 bug fixes listed above for [Community Edition v2.3.0](#Community230).

{anchor:Plus240}
## v2.4.0 Plus Edition additional improvements

_Shipped to registered users of the Plus Edition on Sept. 29, 2014. (Plus Edition is always at least one release ahead of the Community Edition)_

**1 new control**

* Tokenized TextBox. Lets you display a series of values as tokens (similar to Microsoft Outlook's "TO:" textbox that transforms an email address you type into a name token).

**44 bug fixes and improvements**

* In PropertyGrid, the property LocalizedCategory from the class LocalizedCategoryAttribute is now virtual and can be overriden. (Plus only)
* In PropertyGrid, the new "DependsOn" attribute is available. It can be placed over any property of the PropertyGrid's selected object. A list of strings can be passed to the "DependsOn" attribute. The properties with this attribute will have their editor re-evaluated when the properties listed in the "DependsOn" attribute are modified.  (Plus only)
* In MultiColumnComboBox, fixed a design-time bug.  (Plus only)
* In PropertyGrid, the EditorDefinitions will permit binding on properties. The Update of properties has been reviewed to update when values are different.  (Plus only)
* In PropertyGrid, TextBox editor appearance improved when set to readonly in Windows7 Theme.
* In RangeSlider, the clicking region to move the thumbs of the RangeSlider is now bigger in Metro Theme. (Plus only)
* In RangeSlider, Background, BorderThickness and BorderBrush properties are now working properly. When oriented verticaly the width is now working properly. (Plus only)
* In RangeSlider, the keyboard Navigation is now fixed. (Plus only)
* RangeSlider.Orientation property is now of type System.Windows.Control.Orientation instead of RangeSlider.OrientationEnum type, which has been removed.  (Plus only)
* In RangeSlider, it is not necessary to create a Style to be able to use RangeBackground, LowerRangeBackground and HigherRangeBackground. Using those properties directly on the RangeSlider will work.  (Plus only)
* In RangeSlider, Data Binding on LowerValue, HigherValue, Minimum and Maximum properties are now working properly. (Plus only)
* In RangeSlider, when oriented vertically, the thumbs of themes Office2007 and Metro are now perpendicular to the Track. (Plus only)
* In RangeSlider, the Metro Theme has the same look as the Slider control of the Metro Theme.  (Plus only)
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

We hope you love this release and decide to support the project.
-- Xceed Team

-----