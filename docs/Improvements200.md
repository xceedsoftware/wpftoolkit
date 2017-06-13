## v2.0.0 Community Edition improvements and bug fixes

67 new improvements

**Notable new features**
* Our flexible docking window control, AvalonDock, is now part of the toolkit. Provides a system that allows developers to create customizable layouts using a dock system similar to what is found in many popular integrated development environments (IDEs). Aero, Metro, and VS2010 themes are provided for the control, in addition to the Office 2007 themes.
* WindowContainer has been added. This container can contain more than one ChildWindow at the same time.
* ChildWindow has several new properties that allow the window chrome to be styled. It also derives from the newly added WindowControl, which has various advantages over the old version.
* The MessageBox class also has several new properties that allow its chrome to be styled; furthermore, it can now be displayed in XBAP application when displayed in a WindowContainer. It also now derives from WindowControl.
* A Windows 8 theme is now supported by all of the controls in the toolkit.
* New 'Live Explorer' application with source code demonstrates all the features. See an online version [here](http://wpftoolkit.com/try-it/).

**Minor features and Issues resolved**
* PropertyGrid:
	* NewItemTypesAttributes class added.
	* PropertyContainerStyle property added.
	* PreparePropertyItem/ClearPropertyItem events added.
	* CreateFilter() method added. Allows you to override the filtering behavior.
	* Value editors display improved. Like VS2010, they will now display a border when focused or moused over.
	* Setting PropertyDefinitions or EditorDefinitions properties no longer causes a leak.
	* Like in VS2010, now all editors display their values aligned to the left by default.
	* SelectedProperty property added. Get/set the selected property in the PropertyGrid.
	* PropertyGrid.Background color adjusted to reflect VS2010 color.
	* PropertyGrid.Padding now working properly.
	* PropertyItem.Background now working properly.
	* PropertyItem.VerticalContentAlignment now working properly.
	* Setting PropertyItem.Height value no longer breaks the row lines positions.
	* ReceiveWeakEvent method removed, as it was never intended to be public.
	* PropertyItem no longer handles the PreviewMouseDown event, allowing the underlying editors to handle it.
	* Selected property will now be updated when the focused PropertyItem change.
	* An EditorAttribute that refer to a editor containing no public constructor will no longer throw an exception. It will be ignored.
	* Previous/next properties can now be selected with the Up/Down arrows.
* DataGrid: Copyright watermark removed.
* The SplitButton now has a standard look and behavior that integrates well with the other buttons.
* RichTextBox: No longer shows glitches when text is set. Setting the text will no longer prevent update to the Document property. Changing the TextFormatter when Text is not empty will no longer throw exceptions or display garbage content. Text property will now be updated when changing the TextFormatter.
* NumericUpDown: Initializing Value and DefaultValue to the same value will now display the appropriate value instead of an empty string.
* CalculatorUpDown: Maximum and Minimum values will now be respected when the value is entered using the calculator.
* ChildWindow: Memory leak fixed.
* DropDownButton: On popup, when the first element is not focusable, the first focusable element will be focused.
* DateTimePicker.AutoCloseCalendar property added. When true, the DatePicker calendar popup will close once the user clicks a new date.
* Added the following events to CollectionControl: ItemDeleting, ItemDeleted, ItemAdding, and ItemAdded.
* CollectionControl.IsReadOnly property added.
* CollectionControlDialog.IsReadOnly property added.
* Wizard.Next and Wizard.Previous events added.
* WizardPage.Enter and WizardPage.Leave events added.
* A RichTextBoxFormatBar not attached to a RichTextBox will no longer throw an exception.
* MultiLineTextEditor.IsReadOnly property added.
* NumericUpDown, DateTimeUpDown: BorderBrush and Padding properties now working properly.
* PrimitiveTypeCollectionControl.IsReadOnly property added.
* NumericUpDown: ClipValueToMinMax property added. When true, input values beyond accepted rage will be adjusted according the Minimum or Maximum.
* Mouse wheel now works with the ButtonSpinner class.
* CheckComboBox: Clearing a bound collection no longer throws a FatalExecutionEgineError exception.
* DateTimeUpDown.SelectAll method added.
* UpDownBase: Setting the focus on the control before applying the template will now focus the inner textbox properly.
* RichTextBox: TextFormatter property is now a dependency property.
* MessageBox: When using a WindowContainer, you can now display the MessageBox in a partial trust XBAP application.
* SingleUpDown is now working when no maximum/minimum values are set.
* The sample application has been reviewed. New look and faster compilation time.
* DateTimePicker: On the calendar popup, when the focus is on the TimePicker, Enter/Esc will now close the popup appropriately.
* TimePicker: Setting the TabIndex now works properly.
* TimePicker/DateTimePicker/DateTimeUpDown: Shift-Tab navigation now works properly. Keyboard input greatly improved. Navigation through the different date segments now works with the arrow keys.
* PropertyGrid: Update() method fixed. It will now update the display from the SelectedObject values.
* Xceed.Wpf.DataGrid.CancelRoutedEventArgs has been removed and replaced by Xceed.Wpf.Toolkit.CancelRoutedEventArgs version.
* DateTimePicker: DateTimePicker.Focus() method now works correctly.
* TimePicker: Setting Focusable to false works properly.
* DateTimeUpDown: Setting Focusable to false now works properly.
* DateTimePicker: Setting Focusable to false now works properly.
* NumericUpDown: Setting Focusable to false now works properly.
* Xceed.Wpf.DataGrid.Licenser no longer exist. Use Xceed.Wpf.Toolkit.Licenser to set your toolkit license.
* MessageBox.WindowThickness property added to control the thickness of the window chrome.
* Workaround for a WPF (.NET 4) bug when accessing Fonts.SystemFontFamilies added. (Issue 19552) 
* UpDownBase.Value: OneWay bindings will now work with this property, as long as the user does not input text.
* UpDownBase.MouseWheelActiveOnFocus behavior is now obsolete. Use MouseWheelActiveTrigger instead.
* UpdownBase.MouseWheelActiveTrigger property added. This property allows you to specify when a mouse wheel action affect the value.
* ColorPicker: SelectedColor and AvailableColors with an alpha lower than 255 will now display a background pattern to clearly identify the alpha effect.

**Interface and behavior changes (some may affect your projects)**
* PropertyGrid.Properties property is now of type IList instead of PropertyItemCollection.
* RichTextBox: In code-behind, when initializing the TextFormatter and Text properties, TextFormatter must be set first or use the ISupportInitialize API.

## v2.0.0 Plus Edition improvements and bug fixes

10 additional improvements

**PropertyGrid Plus-only features**
	* When using multiple selected objects, the validation red border will now be displayed on invalid input.
	* List source for properties added.
	* DefinitionKeyAttribute class added.
	* CustomPropertyItem class added.
	* PropertyItem.DefinitionKey property added. This property allows you to override the editor definition to be used by a property.

**New Plus-only controls**
* New WPF chart control.  100% lookless charting control  displays rich, configurable charts is able to display multiple areas with multiple charts at the same time. It supports area, column (bar), line, and pie charts, and also provides the ability to create custom charts. Developers can use the built-in, flexible legend or provide their own.
* PileFlowPanel. Contains PileFlowItem objects ("flow items"), which flow smoothly to the left and right of the central, selected element. The flow items can contain any FrameworkElement-derived class. An optional mirror-like reflection can be displayed in the PileFlowPanel beneath the content.
* Ultimate WPF listbox. Combines the streamlined form factor of a listbox—as well as unique “path” views—with lightning-fast remote data retrieval and absolute responsiveness. LiveExplorer, Metro, Office 2007 (blue, black, silver), and Windows Media Player 11 themes are provided for the control, in addition to the Office 2007 themes.
* StyleableWindow class has been added, which lets you style the chrome of a window. This class possesses all of the typical features of a normal window, but its elements can all be styled.
* ChildWindow: The window can also be resized and maximized (Maximize button added).

[See v1.9.0 improvements and bug fixes](Improvements190)

We hope you love this release.
-- Xceed Team