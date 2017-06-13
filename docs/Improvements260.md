{anchor:Community260}
## v2.6 Community Edition (January 8, 2016)

**15 bug fixes and improvements**

* In DateTimePicker, DateTimeUpDown and TimePicker, when TimeFormat has a foreign culture, it will no longer cause a crash.
* In PropertyGrid, PropertyItem now has a new read-only property: PropertyName. It returns the name of the property referenced by the PropertyItem.
* In WatermarkComboBox, the BorderBrush can now be set when IsEditable is true.
* In ColorPicker, when ShowRecentColors is True, selecting a color from the popup will select the proper color.
* In ColorPicker, the RecentColors property can now be bound.
* In MessageBox, when a user is not using Windows 8, changing the FontSize won't affect the Caption anymore, it will only affect the content.
* In WatermarkTextBox, when a user is not in Windows 8 and IsEnabled is set to false, the Background and BorderBrush will no longer be gray.
* In BusyIndicator, width is now based on its content.
* In DataGrid, groups should now be properly displayed when grouping is activated for data sources other than a DataGridCollectionView.
* In AvalonDock, AutoHideWindow will now be disposed when DockingManager is unloaded to prevent memory leaks.
* In RangeSlider, the RangeSlider now has a new property : IsDeferredUpdateValues. When set to True, the update of the LowerValue/HigherValue will be done on the Thumbs DragCompleted events, not on the DragDelta events. Default value is False. 
* In DateTimePicker/TimePicker, a new property ShowDropDownButton property as been added. When set to false, the dropDownButton of the DateTimePicker/TimePicker will be removed. Default value is true.
* In PropertyGrid, when using EditorTemplateDefinition TargetProperties with something else than a type or propertyName (like an interface), the EditorTemplateDefinition will now be applied. PropertyDefinitionBaseCollection<T> now has a protected constructor and a virtual indexer.
* In TimePicker, when setting the Format to Custom and setting a FormatString, pressing the ButtonSpinner will no longer cause a crash.
* In TimeSpanUpDown, negative values are now supported.

{anchor:Plus260}
## v2.6.0 Plus Edition

_Shipped to registered users of the Plus Edition on Feb. 9, 2015._

**2 additional bug fixes and improvements**

* In TokenizedTextBox, the Foreground and caret colors can now be set.
* In TokenizedTextBox, alias values are now supported for tokens. Theses values will not appear in the suggested list.

{anchor:Plus270}
## v2.7.0 Plus Edition

_Shipped to registered users of the Plus Edition on June 8, 2015._

**New controls**

* A set of 15 new controls for building apps with the Material Design look, to give your WPF apps a modern look and feel that blends in with the latest Web applications.

**41 bug fixes and improvements**

* In PropertyGrid, using the DependsOn attribute on properties will now update the validation on the depending properties. (Plus Only)
* In ListBox, the SearchTextBox's Background, BorderThickness and BorderBrush can now be set when styling the ListBox's SearchTextBox. (Plus Only)
* In ToggleSwitch, the ToggleSwitch can now stretch in all the available space. (Plus Only)
* In TokenizedTextBox, the event ItemSelectionChanged is now fired when TokenizedTextBox.SelectedItems adds or removes items in code-behind. (Plus Only)
* In PropertyGrid, EditorNumericUpDownDefinition now supports the property UpdateValueOnEnterKey. (Plus Only)
* In PropertyGrid, using CustomPropertyItems with customEditors will now have the correct DataContext. (Plus Only)
* In ListBox, the Drag and Drop will now be available when using a ListBox inside a WinForm's ElementHost. (Plus Only)
* In ListBox, binding on ListBox.Items.Count now works when ItemsSource is used. (Plus Only)
* In PropertyGrid, the categories can now be collapsed/expanded in code-behind with 4 new methods :CollapseAllCategories(), ExpandAllCategories(), CollapseCategory(string) and ExpandCategory(string). (Plus Only)
* In ToggleSwitch, the properties ThumbLeftContent and ThumbRightContent are now of type "object" instead of "string". So any content can be put in the ToggleSwitch. The property ThumbStyle has been added to redefine the Style of the Thumb. Please note that using ThumbStyle will disable the following properties : ThumbLeftContent, ThumbRightContent, ThumbForeground, ThumbBackground, ThumbBorderBrush, ThumbBorderThickness, ThumbHoverBackground, ThumbHoverBorderBrush, ThumbPressedBackground, ThumbPressedBorderBrush. (Plus Only)
* The ZoomBox will no longer throw an exception at startup when its Content is null.
* In WatermarkTextBox, the new property KeepWatermarkOnGotFocus makes it possible to keep or remove the watermark on GotFocus.
* In PropertyGrid, you can now only display the SelectedObject properties, hidding the inherited ones.
* In PropertyGrid, the RefreshPropertiesAttribute is now supported.
* In PropertyGrid, Removing the Misc category label is now possible in any language.
* In DropDownButton, if KeyboardFocusWithin becomes false, the DropDownButton will now close.
* In PropertyGrid, expandable PropertyItems are now updated when their Sub-PropertyItems are modified.
* In PropertyGrid, Nullable are now supported.
* In DropDownButton, SplitButtons : Under Windows7, Button's Style will now follow Windows 7 and have their bottom half background painted in gray.
* In ColorCanvas, the ColorCanvas's SpectrumSlider value will no longer be updated while selecting a color in the ShadingColor rectangle.
* In ColorPicker, when the property DisplayColorAndName is False, the Tooltip displaying the Color name will no longer appear in the ColorPicker popup.
* In Calculator, the Calculator buttons content won't be overriden with default values anymore. Also, The new property CalculatorButtonPanelTemplate can now be set to define and position the calculator buttons.
* In AvalonDock , The BorderBrush, BorderThickness and Background can now be modified when styling a LayoutAnchorableFloatingWindowControl or LayoutDocumentFloatingWindowControl.
* In PropertyGrid, Properties Array type, with ExpandableObject attribute, are now displayed in a numerical order instead of alphabetical order to have the indexes displayed correctly.
* In AvalonDock, The LayoutDocument and LayoutAnchorable can now be Enabled/Disabled with the new property LayoutContent.IsEnabled.
* In MessageBox, the MessageBox owner is now assigned correctly, whether the MessageBox is created normally or from another thread.
* In AvalonDock, with a LayoutFloatingWindow active, switching Windows Theme won't cause an invalid Window Handle Exception anymore.
* In AvalonDock, the LayoutDocuments repositionning in the LayoutDocumentPane (or the LayoutAnchorable repositionning in the LayoutAnchorablePane) can now be blocked with the new property LayoutPositionableGroup.CanRepositionItems.
* In CheckComboBox, CheckComboBox now has the IsEditable property. When True, the TextBox of the CheckComboBox can be edited to specify the SelectedItems.
* In CheckComboBox, when using Windows Classic Theme, the CheckMark will no longer disappear when Checked and MouseOver are true.
* In DropDownButton and SplitButton, a new property MaxDropDownContent has been created, allowing a maximum height for the popup in the control.
* In AvalonDock, using a dark Background Brush for the DockingManager in the System Theme won't hide the icons "DropDownMenu", "Pin" and "Close" anymore.
* In NavigationWindow, popping the NavigatorWindow in Windows 8 will no longer throw an exception.
* In PropertyGrid, the PrimitiveType Collections will now have their Editor.ItemType set when the collection is inheriting from a List<>.
* In AvalonDock, when a LayoutDocument is floating from a LayoutDocumentPane, docking it inside the same LayoutDocumentPane will now set its index to its last position saved instead of 0.
* In DateTimePicker, DateTimeUpDown, TimeSpanUpDown, TimePicker and DateTime, Parse is now used inside a try-catch, to prevent crashes for uncommon region settings.
* In PropertyGrid, Searching a property will not only be possible with properties "starting" with a set of letters, but also with properties "containing" a set of letters.
* In Wizard, with a binding to ItemsSource, it won't be necessary to set the CurrentPage, it will now be automatically set to the first WizardPage.
* In RichTextBox, the text selected containing more than 1 color won't disappear anymore.
* In MaskedTextBox, the property CharacterCasing is now taken into account when typing letters.
* In Chart, when using Axis.LabelsType as "Labels", the values won't be sorted anymore; they will be displayed in the order received.

{anchor:Plus280}
## v2.8.0 Plus Edition

_Shipped to registered users of the Plus Edition on Sept. 14, 2015._

**29 bug fixes and improvements**

* In ListBox, Grouping a ListBox with an enum type property and selecting an item will now add the selected items in the SelectedItems collection. (Plus Only)
* In ListBox, filtering a ListBox with a predicate defined through the ListBox.Filter event will now be considered in the SelectedItems. (Plus Only)
* In ListBox, a filtered listbox using grouping will now return the correct SelectedItems by applying the Filter. (Plus Only)
* In MaterialListBox, MaterialComboBox and MaterialTabControl, the MaterialControls style is now applied when using ItemsSource. (Plus Only)
* In TokenizedTextBox, the TokenizedTextBox's popup will now always be displayed on the same monitor as the TokenizedTextBox. (Plus Only)
* In TokenizedTextBox, if the control is placed in a popup, it will now display the tokens correctly. (Plus Only)
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

We hope you love this release and decide to support the project.
-- Xceed Team

-----