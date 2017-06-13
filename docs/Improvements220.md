{anchor:Community221}
## v2.2.1 Community Edition service release (June 17, 2014)

**Special offer announcement**

* Price lowered to $99.95 until August 30, 2014 for the [Plus Edition](https://wpftoolkit.codeplex.com/wikipage?title=Compare%20Editions). [Please support the project](http://wpftoolkit.com/shop). We accept credit cards, PayPal and bitcoin.
**5 bug fixes**

* In DateTimePicker and DateTimeUpDown, if an invalid date is typed, on lostFocus, it will fall back to the last valid date displayed. E.g. Typing "April 99" will no longer revert back to "April 9", it will replace it with the last valid date that was displayed.
* In NumericUpDown, the Tab control can be used in propertyGrid to tab to and from NumericUpDown editors.
* In PropertyGrid, when using EditorTemplateDefinition.TargetProperties with a type, the Visual Studio designer won't throw a "Collection is ReadOnly" error. Note that to prevent an "Expecting IList" error, TargetProperties should be defined like this: <xctk:EditorTemplateDefinition.TargetProperties><xctk:TargetPropertyType Type="{x:Type local:ValuesClass}"/></xctk:EditorTemplateDefinition.TargetProperties>
* In PropertyGrid, if the ReadOnly attribute is used together with the ItemsSource attribute or Editor attribute, the editor will now be read only as expected.
* In CollectionControlDialog, collectionControlDialog will now return True if the 'OK' button is clicked and 'False' if the 'Cancel' button is clicked.

{anchor:Community220}
## v2.2.0 Community Edition (June 2, 2014)

**2 new free controls **

* +RangeSlider+. Lets users set a lower and upper value using a single integrated control. Includes features such as min/max values, steps, range widths,  and more.
* +TimeSpanUpDown+. Allows easy +/- input of time spans over 24 hours.

**15 bug fixes and improvements**

* In AvalonDock, only selected Document Tabs that are not visible will be moved to the extreme left upon a refresh.
* Added AvailableColorsSortingMode property to ColorPicker, allowing AvailableColors to be sorted alphabetically or by Hue, Saturation, Brightness.
* ColorPicker selection is performed on MouseUp instead of on MouseDown.
* Added TimePickerVisibility property to DateTimePicker, allowing you to show or hide the TimePicker beneath the calendar.
* In DateTimePicker, selecting a grayed out day (from a previous/next month) won't change the selected month twice.
* In NumericUpDown, MouseWheelActiveTrigger.Focused has been renamed to MouseWheelActiveTrigger.FocusedMouseOver to prevent a misunderstanding. Focus and mouseOver are needed in order to be able to do a mouseWheel when FocusedMouseOver is selected.
* In PropertyGrid, added property PropertyGrid.UpdateTextBoxSourceOnEnterKey that is True by default. When set to False, the bound source of the TextBox in a PropertyGridTextBoxEditor will only be updated after a lostFocus and not immediately when the user hits Enter.
* In MessageBox, WindowControl.BroderBrush and BorderThickness now work with the Windows 7 theme.
* In PropertyGrid, when PropertyGrid.SelectedObjects is only one object, it now behaves properly when BrowsableAttribute is False.
* PropertyGrid now has a TimeSpanUpDownEditor that supports a timeSpan greater than 24 hours.
* In ColorPicker, ShowDropDownButton's toggle button and borders now look and work properly in all themes.
* In PropertyGrid, the Char type is now supported. This was supposed to be in v2.1 but was omitted after the previous release notes were completed.
* In PropertyGrid, a maskedTextBoxEditor is used to support the GUID type.
* In PropertyGrid, added the ShowPreview property. When set to True and the SelectedObject is a UIElement, the control will display a small preview rectangle at the top.
* In NumericUpDown, IsTabStop now works.

{anchor:Plus220}
## v2.2.0 Plus Edition additional improvements (Feb. 20, 2014)

**4 new controls**

* +RadialGauge+. A a beautiful, styleable, radial gauge control with features such as Major/Minor ticks, portions of backgrounds and borders, independent pie pieces, and more.
* +RatingControl+. An interactive RatingControl, with features such as precise mode, support for star icons, continuous mode, tooltips and more.
* The 2 controls listed above that are in  [Community Edition v2.2.0](#Community220)

**18 bug fixes and improvements**

* In Ultimate ListBox (Plus Edition only), fixed an exception that was thrown when the data source was sorted on an Enum field.
* Fixed a bug in Ultimate ListBox (Plus Edition only) that caused it to freeze in some instances when dragging the scrollbar thumb up.
* In PropertyGrid (Plus Edition), added the ability to collapse specific categories with a given class attribute.
* The 15 bug fixes listed above for [Community Edition v2.2.0](#Community220).

{anchor:Plus230}
## v2.3.0 Plus Edition additional improvements (June 2, 2014)

_(Plus Edition is always at least release ahead of the Community Edition)_

**1 new control**

* ToggleSwitch. A highly customizable, and fully themed on/off switch.

**30 bug fixes and improvements**

* In PropertyGrid, users can now tab between PropertyItems without putting focus on the Name Part. Only the Editing Part will get the focus. ReadOnly properties will be skipped.
* In PropertyGrid, changing PropertyGrid.PropertyDefinitions in code-behind will update the visible PropertyItems in the PropertyGrid.
* In PropertyGrid, custom PropertyItems are now sorted alphabetically.
* In PropertyGrid, you can now set the ExpandableObjectAttribute parameter. A value of true will set PropertyItem to Expanded, while false will set PropertyItem to Collapsed.
* In PropertyGrid, Plus Edition only, the editor for collections of objects (CollectionEditor) is now themed in the Metro and Office2007 themes.
* In PropertyGrid, Plus Edition only, when using PropertyGrid.PropertiesSource, if a property is a collection of strings, the CollectionEditor now allows the collection to be edited.</li>
* In DataGrid, during design-time, the message "The specified view type must derive from ViewBase." will no longer appear.
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
* In Chart, Plus Edition only, in the designer, a value changed in a DataTemplate (located in the Resources) will now correctly refresh. E.g., changing the margin of a TextBlock in a DataTemplate.
* that's assigned to Axis.AxisTitleTemplate.
* In Chart, titles specified for the Y Axis are now vertically oriented.
* In Ultimate ListBox, the SearchTextBox now has a new 'IsCaseSensitive' property that is false by default. It determines if the search should be be case sensitive.
* In RichTextBox, when setting a transparent background, the Transparent color will be applied.
* In ZoomBox, when ZoomBox.Scale is bound to a property and has a DataContext, setting the dataContext to null will no longer cause a StackOverflow Exception.
* In ColorPicker, when ShowAdvanceButton is false, the separator above the AdvanceButton is removed (missed v2.2 release note).
* PrimitiveTypeCollectionControl and PrimitiveTypeCollectionEditor no longer get an IndexOutOfRange exception when checking for generic arguments.

{anchor:Plus231}
## v2.3.1 Plus Edition additional improvements (June 17, 2014)

**6 bug fixes and improvements**

* In PropertyGrid, Office2007 and Metro themes, expanded propertyItems will be stretched horizontally to take the full width. In Metro theme, PropertyGridComboBoxEditor will now contain correct data. In Metro theme, the height of a propertyItem will not be fixed at 29 but will instead use MinHeight = 29.
* The 5 bug fixes listed above for [Community Edition v2.2.1](#Community221).

[See Community Edition v2.1.0 improvements and bug fixes](Improvements210)

We hope you love this release.
-- Xceed Team