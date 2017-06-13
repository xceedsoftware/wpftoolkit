## v1.9.0 improvements and bug fixes

70 new improvements

* When [PropertyGrid](PropertyGrid).IsEnabled property is set to False, all properties will now appear grayed out.
* The magnified element in the [Magnifier](Magnifier) control is no longer offset when it is not the first element of its parent Panel.
* The RichTextBox will now fade out as the cursor moves away from the selected text. Moving the mouse slightly beyond the RichTextBoxFormat will no longer close the format bar.
* On UpDownBase.InputValidationError event, you can now set the "ThrowException" property to true to  refuse any invalid input.
* Selection with the space key now works in [CheckListBox](CheckListBox).
* Fixed [ButtonSpinner](ButtonSpinner) padding.
* [ButtonSpinner](ButtonSpinner).AllowSpin property now disables the spinner button when set to false.
* Selecting a Time in the [DateTimePicker](DateTimePicker) drop-down popup will no longer close the popup.
* Pressing "Enter" on properties edited by a [NumericUpDown](NumericUpDown) will now update the source item.
* Resetting a PropetyItem value through the context menu will now update the [PropertyGrid](PropertyGrid)'s property icon appropriately.
* Updating one of the following properties will now have an immediate effect in the display position of the PropertyItem within the [PropertyGrid](PropertyGrid): Category, PropertyOrder, DisplayName.
* [DoubleUpDown](DoubleUpDown) and [SingleUpDown](SingleUpDown) will no longer throw exceptions by default when Infinity is assigned to the Value property.
* [NumericUpDown](NumericUpDown): The DefaultValue will be set as Value when the user empties the TextBox.
* [NumericUpDown](NumericUpDown), [DateTimeUpDown](DateTimeUpDown): Value property will now be updated while user is typing, unless the actual input content is invalid.
* [DateTimeUpDown](DateTimeUpDown).ConvertTextToValue(string text) method will now receive the actual text input as parameter instead of a string representation of an already parsed value.
* UpDownBase: When both Value and Text properties are specified in XAML, Value has precedence over Text.
* [NumericUpDown](NumericUpDown), [DateTimeUpDown](DateTimeUpDown): DefaultValue now works. When the text input is null or empty, Value property will be set to DefaultValue instead of null.
* [NumericUpDown](NumericUpDown): Setting DefaultValue to null will no longer break spinner buttons.
* [DoubleUpDown](DoubleUpDown), [SingleUpDown](SingleUpDown): Spin button status fixed when the value is Infinity.
* Properties with NaN values now display properly in the [PropertyGrid](PropertyGrid).
* When UpDownBase.InputValidateError is raised, the invalid Text value will still be present on the instance.
* UpDownBase: Value property will now update as the user types instead of on lost focus.
* [NumericUpDown](NumericUpDown): No longer throws an exception on mouse wheel when value is out of Min/Max ranges.
* [PropertyGrid](PropertyGrid): Input error will now display a tooltip containing the error description.
* PropertyItem will now display the red validation error frame when invalid values are specified for a property.
* The "Cancel" button of the [CollectionControlDialog](CollectionControlDialog) (formerly CollectionEditorDialog) now closes the window.
* [DateTimePicker](DateTimePicker): Text input fixed.
* CaluclatorUpDown, [CheckComboBox](CheckComboBox), [ColorPicker](ColorPicker), [DateTimePicker](DateTimePicker), [TimePicker](TimePicker), DropDownButton, SplitButton, MultiLineTextEditor: The focus will no longer reach the drop-down toggle button when using "Tab".
* {Alt + Down, Alt+Up}, and F4 will now display the DropDown popup.
* The popup content will now be focused when displayed.
* [CheckComboBox](CheckComboBox), [ColorPicker](ColorPicker), [DateTimePicker](DateTimePicker), [TimePicker](TimePicker): Pressing "Escape" when popup is open will revert the selected content to the value present when the popup was opened and close the popup. 'Enter' will close the popup and keep the selected value.
* CalculatorUpDown: When "EnterCloseCalculator" property is "true," "Escape" will revert the control value to the value present when the popup was opened and close the popup.
* [TimePicker](TimePicker): Opening the popup will now select the appropriate item based on the current value.
* [NumericUpDown](NumericUpDown): When current input is not parsable, and not empty, up/down spinner will be disabled.
* DataGrid: GroupByControl top-right watermark will no longer be displayed over the display text of the control.
* NaN is no longer an accepted value for Increment, Maximum, and Minimum.
* Null value for Increment disables the spin buttons.
* Updates to "Value" and "Text" properties made from code behind are no longer limited to Mininum/Maximum constraint. Only end-user input will be restricted by thoses limits.
* [ButtonSpinner](ButtonSpinner).BorderThickness property fixed.
* Pressing "Enter" on the CalculatorUpDown control's calculator now works.
* Magnifier.ZoomFactor: You can now set a value greater than 1 to have a de-magnify effect.
* [NumericUpDown](NumericUpDown): Spinner buttons will now be disabled when ReadOnly is true.
* CalculatorUpDown: Background property now working.
* SelectAllOnGotFocus property now working.
* EnterClosesCalculator now working.
* Calculator will now pop up in a "clean" state, only keeping the memory store and clearing any incompleted operations from the previous usage.
* [PropertyGrid](PropertyGrid).Instance property is now initialized correctly.
* DropDownButton no longer crashes when DropDownContent is empty or contains no focusable elements.
* [ColorPicker](ColorPicker) RGBA values no longer display decimals next the sliders.
* ColorCanvas: Sliders now work using the keyboard.
* [CheckComboBox](CheckComboBox).MaxDropDownHeight proprty added.
* InputValidationErrorEventArgs.ThrowException property added.
* InputValidationErrorEventArgs.Exception property added, which replaces removed InputValidationErrorEventArgs.ExceptionMessage.
* InputValidationErrorEventArgs.ExceptionMessage property removed.
* PropertyItem.HasChildProperties property renamed to PropertyItem.IsExpandable.
* PropertyItem no longer implements INotifyPropertyChanged. Properties that were notified by this interface are now Dependency Properties.
* ValueSourceToImagePathConverter and ValueSourceToToolitpConverter removed. They are no longer used by the PropertyItem template.
* PropertyItem.ResetValueCommand property removed, as it was never initialized and never used.
* The IsDataBound, IsDynamicResource, HasResourceApplied, and ValueSource PropertyItem properties have been replaced by the writable AdvancedOptionsIcon and AdvancedOptionsTooltip properties, which now allow customization of the display of AdvancedOptionsIcon and tooltip.
* [DoubleUpDown](DoubleUpDown), [SingleUpDown](SingleUpDown): AllowInputSpecialValues property added to control. Decides whether the user is allowed to input of "Infinity", "-Infinity" and "NaN" values. Default: None. (In v1.8 and lower these values were simply accepted.)
* [NumericUpDown](NumericUpDown): SelectAllOnGotFocus is now true by default.
* [NumericUpDown](NumericUpDown), [DateTimeUpDown](DateTimeUpDown): DefaultValue property moved up to the UpDownBase base class.
* Xceed.Wpf.Toolkit.PrimitiveTypeCollectionEditor renamed to Xceed.Wpf.Toolkit.PrimitiveTypeCollectionControl to avoid any confusion with Xceed.Wpf.Toolkit.[PropertyGrid](PropertyGrid).Editors.PrimitiveTypeCollectionEditor.
* Xceed.Wpf.Toolkit.CollectionEditor renamed to Xceed.Wpf.Toolkit.CollectionControl to avoid any confusion with the Xceed.Wpf.Toolkit.[PropertyGrid](PropertyGrid). Editors.CollectionEditor class.
* Xceed.Wpf.Toolkit. CollectionEditorDialog renamed to Xceed.Wpf.Toolkit.[CollectionControlDialog](CollectionControlDialog).
* Common[NumericUpDown](NumericUpDown): ParsingNumberStyle property added. This will allow finer control of acceptable input values. Users will be able to support hexadecimal input.
* [TimePicker](TimePicker).TextAlignment property added.
* [DateTimePicker](DateTimePicker) now has a BorderBrush.
* Copy support added to MessageBox.
* WPFToolkit.Extended.dll renamed to Xceed.Wpf.Toolkit.dll.

## Support this impressive amount of work!


* **Please rate +this release+ and write something positive. It's at the bottom of the [downloads page](http://wpftoolkit.codeplex.com/releases/view/96972#ReviewsAnchor)**
* **Consider purchasing the [Plus Edition](Extended-WPF-Toolkit-Plus) to support this impressive amount of work.**

Thanks
Xceed Team