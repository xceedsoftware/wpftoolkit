# CheckComboBox
Derives from Xceed.Wpf.Toolkit.Primitives.Selector

CheckComboBox is a combo box in which the items in the dropdown are preceded by a checkbox. As items are checked or unchecked, the Text property displayed above the dropdown is updated using the concatenated string representations of the checked items. The text displayed is formated using the value of the Delimiter property to separate the individual strings.

![](CheckComboBox_checkcombobox.jpg)

{{
         <xctk:CheckComboBox x:Name="_combo" 
                             HorizontalAlignment="Center" 
                             VerticalAlignment="Center" 
                             DisplayMemberPath="Color"
                             ValueMemberPath="Level"
                             SelectedValue="{Binding SelectedValue}"
                             SelectedItems="{Binding SelectedItems}" />
}}


## Properties
|| Property || Description
| Command | Gets or sets the command to execute when an item is checked/unchecked. (Inherited from Selector)
| Delimiter | Gets or sets the string used to separate the concatenated string representations of the checked items. (Inherited from Selector)
| IsDropDownOpen | Gets or sets a value indicating whether the combo box drop-down is currently open.
| IsEditable | Gets or sets a value that enables or disables editing of the text in the Textbox of the CheckComboBox. Values entered must be seperated by the **Delimiter**. Values entered that are not in the list or that are duplicates will be removed. Selection will be active on LostFocus. Default is False.
| MaxDropDownHeight | Gets or sets the maximum height of the popup.
| SelectedItem | Gets or sets the last checked item. (Inherited from Selector)
| SelectedItems | Gets the collection of checked items. (Inherited from Selector)
| SelectedItemsOverride | Gets or sets the list of **SelectedItems**. (Inherited from Selector)
| SelectedMemberPath | Gets or sets a path to a value on the source object used to determine whether an item is selected. (Inherited from Selector)
| SelectedValue | Gets or sets a string containing the selected items separated by the value of Delimiter (ex., "Item1, Item2, Item3"). (Inherited from Selector)
| Text | Gets or sets the formated text of the currently checked items.
| ValueMemberPath | Gets or sets a path to a value on the source object representing the value to use. (Inherited from Selector)

## Events
|| Event || Description
| ItemSelectionChanged | Raised when an item's selection is changed. (Inherited from Selector)

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---