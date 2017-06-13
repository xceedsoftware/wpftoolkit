{anchor:Community210}
## v2.1.0 Community Edition improvements and bug fixes

**70+ bug fixes and improvements**

* Side-by-side version loading of the Xceed.Wpf.Toolkit.dlls should now work properly when using v2.1 and later.
* Office2007 and Metro Themes have been added to Chart, Pie and AvalonDock.
* In AvalonDock, added the Metro Dark/Light Theme that supports accent colors.
* In AvalonDock, the appropriate Closing and Closed events are fired when a LayoutContent is closed in code-behind.
* In AvalonDock, fixed first button click not working on a deactivated panel.
* In AvalonDock, the Close command from the LayoutAnchorable context menu is now replaced by the Hide command, to be consistent with Visual Studio.
* In AvalonDock, clicking on a control in an auto hidden window will now correctly focus the clicked control, and clicking on a button in an unfocused auto hidden window will now trigger the button click event correctly.
* In AvalonDock, LayoutContent.Title is now a DependencyProperty.
* In AvalonDock, maximized floating window states are now saved/restored when the layout is serialized.
* In AvalonDock, setting the default style on Window.SizeToContent to anything other than Manual will no longer break pane resizing.
* In AvalonDock, the MetroDark and MetroLight themes can now be used.
* In AvalonDock, fixed binding Errors while changing the theme from Generic to another theme.
* In DateTimeUpDown, DateTimePicker and TimePicker, added the Minimum/Maximum and ClipValueToMinMax properties and updated Office2007 and Metro themes support.
* In DateTimeUpDown, DateTimePicker and TimePicker, changing themes will not erase the current date.
* In DateTimeUpDown, DateTimeParser will manage all DateTime separators.
* In DateTimeUpDown, milliseconds can now be incremented/decremented in Custom format.
* In DateTimePicker, validation of Date editing now works, and milliseconds are taken into account.
* In DateTimePicker, AutoCloseCalendar will no longer close the calendar when choosing a month or a year. It will close only on a day selection.
* In DateTimePicker, the ValueChanged event will no longer fire twice when a date in the calendar is selected with the mouse.
* WPFToolkit.dll is now provided with the .NET 3.5 version of the Xceed.Wpf.Toolkit.dll
* In TimePicker, Padding now works.
* RichTextFormatBar now suppoers the Aero2 (Windows 8) theme.
* In PropertyGrid, PropertyOrderAttribute now takes a new parameter (UsageContext) which is used to apply the PropertyOrder on Categorized/Alphabetical or Both sorting mode.
* In PropertyGrid, Assigning an enum to an object type property in a now displays the property.
* In PropertyGrid, the PropertyGrid/Editors/DefaultEditor (the ColorPicker editor) sample doesn't crash anymore on non-Windows 8 systems.
* In PropertyGrid, using EditorComboBoxDefinition with a CustomPropertyItem now binds with the default "Value" property like other EditorDefinitions do. 
* In PropertyGrid, modifying a Collection Property from an object used as the PropertyGrid.SelectedObject now updates the relative PropertyGrid's PropertyItem.
* In PropertyGrid, all the PropertyGrid Editors now have an implicit style in XAML. They can be themed.
* In PropertyGrid, CustomPropertyItem can now be added to the CustomPropertyItem.Properties collection.
* In PropertyGrid, fixed a crash with PrimitiveTypeCollectionEditor when receiving an array.
* In PropertyGrid, themes now load faster when many properties are used.
* In PropertyGrid, all the PropertyGrid Editor controls have an implicit style. They are redefined in the Metro and Office2007 themes.
* In PropertyGrid, no error is thrown if the Business object don't have a Name property.
* In PropertyGrid, CategoryOrdering is now also available in the Community Edition. 
* In PropertyGrid, added support for CategoryOrdering with multiple selected objects.
* In PropertyGrid, when an expandable property contains a null value, it will no longer display the expansion arrow.
* In PropertyGrid, Char type now supported.
* In PropertyGrid, AdvancedIcon for PropertyItems now set in XAML to allow use of different icons relative to the current theme. The Metro Themes use different icons.
* In BusyIndicator, added new property FocusAfterBusy that can be set to a control what will gain focus when BusyIndicator becomes unbusy.
* In DataGrid, removed the blue background when editing a CheckBox.
* In DataGrid, SelectAllOnGotFocus property is now obsolete. It is replaced by the AutoSelectBehavior property. This affects the following classes and derived classes: WatermarkTextBox, NumericUpDown, EditorNumericUpDownDefinitionBase.
* In DataGrid, when editing a numeric value, the up/down arrow will no longer "spin" the value but will allow navigation between rows and cells.
* In WindowContainer, a control that is clicked inside a ChildWindow that is in a WindowContainer will obtain the focus.
* In WindowContainer, closed Modal ChildWindows won't block inputs anymore.
* In WindowControl with the Office2007 theme, WindowStyle = None now displays properly.
* In WindowControl, the title font's size and alignment has been fixed when running on Windows 8.
* In CheckComboBox, binding SelectedValue or SelectedItem when placed in a DataTemplate now works.
* In CheckComboBox, clicking anywhere on the CheckcomboBox will now open the CheckComboBox's popup.
* In CheckListBox and CheckComboBox, ItemTemplate is not ignored anymore.
* In ChildWindow, you can no longer expand a window beyond childWindow.MaxWidth and childWindow.MaxHeight.
* In ChildWindow, resizing a Window to its MaxWidth or MaxHeight won't hide the resize border.
* In ChildWindow, a newly displayed ChildWindow in a window container will now be displayed in front if an insertion order is not specified.
* In ChildWindow, fixed hidden buttons when there is more than one modal ChildWindow in the same WindowContainer.
* In CollectionControl, fixed an exception when using a  fixed size array.
* In CollectionControl, the ListBox can now be styled.
* In CollectionEditor, when a ToString() method is defined on an object, it will be used in the CollectionEditor to identify the object in its ListBox.
* In WizardPage, the Next button is now enabled when the binding on "AllowNext" doesn't come from an input.
* In ColorPicker, replaced ToggleButton in the Popup by a Button. Allows changing from ColorPicker to ColorCanvas, without causing the button to become checked.
* In ColorPicker, added ColorMode property. Allows you to specify which palette or color canvas to display on the color picker. Allows starting with the canvas displayed first.
* In ColorPicker, the Office 2007 theme ColorPicker.UsingAlphaChannel setting now works properly.
* In ColorPicker, RecentColors are now updated when Color is chosen from ColorCanvas.
* In Chart, LayoutMode of Series can be set in XAML with SideBySide or Stacked option.
* In Chart, the following properties are now Dependency Properties and can be used to set the default implicit style of the chart Legend: DefaultSeriesItemTemplate, DefaultDataPointItemTemplate, DefaultAreaTitleDataTemplate and DefaultSeriesTitleDataTemplate.
* In Ultimate Listbox, fixed an exception that was thrown when the data source was sorted on an Enum field.
* New 'Remote oData' sample added to the LiveExplorer collection of sample applications.
* StyleableWindow and ChildWindow's Close button now display properly under Windows 8 when the ChildWindow.WindowStyle or StyleableWindow.WindowStyleOverride is "ToolWindow".
* In NumericUpDown, when there's no value in NumericUpDown and the new Property "DisplayDefaultValueOnEmptyText" is True, the default value will be displayed.
* In NumericTextBox, AutoSelectBehavior and AutoMoveFocus properties have been added.
* In ButtonSpinner, Key.Up and Key.Down key events are no longer handled when AllowSpin is "False".
* In WatermarkTextBox, the class now derives from AutoSelectTextBox insteand of TextBox. This adds the AutoSelectBehavior and AutoMoveFocus properties to the control.
* In SearchTextBox, fixed the magnifying glass icon for the Metro themes.
* In DoubleUpDown, added missing word "Invalid" in the warning message.
* In MessageBox, CloseButton was not using an image with rounded corners on mouseOver in Windows 7.
* The legacy MaskedTextBox (Xceed.Wpf.Toolkit.Obselete.MaskedTextBox) will no longer compile due to its Obsolete "error" attribute. Use Xceed.Wpf.Toolkit.MaskedTextBox control instead.
* In RichTextBoxFormatBar, the Office2007 theme is now supported.
* In RichTextBoxFormatBar, it no longer disappears while selecting a font in the dropdown combobox.
* ColorCavas.UsingAlphaChannel now works properly in the Office2007 theme.

**Breaking changes**

* StaticResourceKey no longer exists. Declarations in the ResourceKeys class now use the standard ComponentResourceKey.

{anchor:Plus210}
## Additional improvements in v2.1.0 Plus Edition

**77+ bug fixes and improvements**

* New Metro theme.
* In PropertyGrid, using PropertyGrid.CategoryDefinitions property, you can now set (or override) the category ordering without having to add CategoryOrderAttributes to your selected object.
* In PropertyGrid, expandable properties are now supported when multiple objects are selected.
* In PropertyGrid, validation now supported within the PropertyGrid.SelectedObjects collection. It cannot contain any null entries.
* In ChildWindow, the maximized child window will now adjust to its parent WindowContainer when the latter is resized.
* In Ultimate ListBox, fixed a freeze that occurred when scrolling up.
* In Ultimate Listbox, added design-time dlls.
* The 70+ bug fixes listed above for [Community Edition v2.1.0](#Community210).
## Previous versions

[See v2.0.0 improvements and bug fixes](Improvements200)

We hope you love this release.
-- Xceed Team