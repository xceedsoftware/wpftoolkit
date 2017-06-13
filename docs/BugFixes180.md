**Issues resolved in v1.8.0**

* PropertyOrder attribute is now also considered when the sort is alphabetical.
* In PropertyGrid, a "<no name>" title is now only displayed when the selected object is a FrameworkElement and the Name property is null or empty string.
* Setting ShowTitle/ShowSortOptions/ShowSearchBox no longer leaves empty gray margins on the property grid.
* Setting PropertyGrid.NameColumnWidth in XAML no longer causes an exception to be thrown.
* RichTextBox.Text property will now be updated when there is no binding defined on the property.
* Dropping a Wizard control on a design surface will no longer throw an exception.
* Changes within the Selector.SelectedItems collection (add/remove) will now be reflected on the UI.
* Entering an invalid value in the ColorCanvas Hexadecimal value field will no longer throw an exception. The file will reset to its previous value instead.
* Selector: Changing the SelectedMemberPath property now updates the selection.
* Selector: Changes to any value referenced by SelectedMemberPath now updates the selection.
* Selector: Changes to any value referenced by ValueMemberPath now updates the SelectedValue.
* CheckComboBox: Changes to any value referenced by DisplayMemberPath now updates the Combobox display text.
* Adding an element to Selector.SelectedItems collection will select it.
* Selector.SelectedItem property will no longer contain the last unselected item and its behavior will now be consistent with thoses of the standard ListBox and ComboBox.
* MessageBox: The "Enter" key is now marked as handled when closing the window.
* Added the following properties to the MessageBox class: YesButtonStyle, NoButtonStyle, CancelButtonStyle, OkButtonStyle, ButtonRegionBackground.
* MessageBox: Bottom corners have been rounded.
* MessageBox: Pressing "Esc" will now close the message box in all case except "Yes No", just like the standard MessageBox.
* MessageBox: All MessageBox return values will now be the same as those of the standard MessageBox in all cases.
* The following PropertyItem properties no longer exist: BindingPath and PropertyGrid. These were only used for internal logic.
* PropertyGrid.SelectedPropertyItem is now reset when the selected object changes.
* PropertyGrid.Filter will no longer be cleared when the SelectedObject changes.
* PropertyGrid.AutoGenerateProperties will refresh displayed properties when modified at runtime.
* PropertyGrid.Properties will never be null. Only the collection content will change for now on. The collection itself will remain the same instance. Registering to INotifyCollectionChanged works.
* PropertyGrid.PropertyValueChanged routed event is now raised starting from the PropertyItem itself instead of from the PropertyGrid.
* PropertyItem's hint icons will now work on sub-properties.
* PropertyItem: IsDataBound, ValueSource, IsDynamicResource, and HasResourceApplied properties will now be valid on expanded sub-properties.
* PropertyGrid: Except when an appropriate TypeConverter is defined, an unknown property type will no longer be considered editable with a TextBox. A TextBlock will be used instead.
* Setting MaskedTextBox.Value property will now work properly when set before the template is applied.
* MaskedTextBox will no longer prevent the Default button to trigger on the enter key
* MaskedTextBox will now work properly in a DataGridTemplateColumn (Enter key will work).
* MaskedTextBox.InsertKeyMode property allows the user to overwrite when characters are typed.