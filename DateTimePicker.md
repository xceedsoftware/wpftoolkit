# DateTimePicker
Derives from Xceed.Wpf.Toolkit.DateTimeUpDown

Get the best of both worlds: The user can increment or decrement the DateTime using button spinners, up/down keys, or the mouse wheel, or can modify the DateTime in the dropdown calendar.

![](DateTimePicker_DateTimePicker.jpg)

## Properties
|| Property || Description
| AllowSpin | Gets or sets the ability to perform increment/decrement operations via the keyboard, button spinners, or mouse wheel. (Inherited from UpDownBase)
| AutoCloseCalendar | Gets or sets if the Calendar should close on a selection.
| ClipValueToMinMax | Gets or sets if the value should be clipped when minimum/maximum is reached. (Inherited from UpDownBase)
| CultureInfo | Gets or sets the current CultureInfo. (Inherited from InputBase)
| DefaultValue | Gets or sets the value to use when the **Value** is null and an increment/decrement operation is performed. (Inherited from UpDownBase)
| DisplayDefaultValueOnEmptyText | Gets or sets if the **DefaultValue** should be displayed when the **Text** is empty. (Inherited from UpDownBase)
| Format | Gets or sets a DateTimeFormat value representing the format to be used (FullDateTime, LongDate, etc.). (Inherited from DateTimeUpDown).
| FormatString | Gets or sets the display format to use when **Format** is set to Custom (e.g., "hh:mm tt") (Inherited from DateTimeUpDown).
| IsOpen | Gets or sets a value indicating whether the DateTimePicker is open.
| IsReadOnly | Gets or sets if the control is read only. (Inherited from InputBase)
| Kind | Gets or sets a value indicating whether a DateTime object represents a local time, a Coordinated Universal Time (UTC), or is not specified as either local time or UTC. NOTE: Setting this property fixes a bug where losing focus on the DateTimePicker was incrementing time in UTC. It also fixes a bug where specifying a UTC Date was changing the date's Kind property to UnSpecified or Local.
| Maximum | Gets or sets the maximum allowed value. (Inherited from UpDownBase)
| Minimum | Gets or sets the minimum allowed value. (Inherited from UpDownBase)
| MouseWheelActiveTrigger | Gets or sets when the MouseWheel is active (Focused, FocusedMouseOver, MouseOver, Disabled). By default FocusedMouseOver. (Inherited from UpDownBase).
| ShowButtonSpinner | Gets or sets if the ButtonSpinners are visible. (Inherited from UpDownBase)
| Text | Gets or sets the formatted string representation of the value. (Inherited from InputBase)
| TextAlignment | Gets or sets the alignment of the Text. (Left, Right, Center, Justify). By default Left. (Inherited from InputBase)
| TimeFormat | Gets or sets the time format.
| TimeFormatString | Gets or sets the time format string used when **TimeFormat** is set to Custom.
| TimePickerAllowSpin | Gets or sets if the TimePicker in the DateTimePicker can Spin.
| TimePickerShowButtonSpinner | Gets or sets if the ButtonSpinners of the TimePicker in the DateTimePicker are shown.
| TimePickerVisibility | Gets or sets if the TimePicker in the DateTimePicker is visible.
| TimeWatermark | Gets or sets the time watermark.
| TimeWatermarkTemplate | Gets or sets the time watermark's data template.
| UpdateValueOnEnterKey | Gets or sets a value indicating whether the synchronization between "Value" and "Text" should be done only on the Enter key press (and lost focus). 
| Value | Gets or sets the numeric value. (Inherited from UpDownBase)
| Watermark | Gets or sets the object to use as a watermark if **Value** is null. (Inherited from InputBase)
| WatermarkTemplate | Gets or sets the DataTemplate to use for the **Watermark**. (Inherited from InputBase)

## Events
|| Event || Description
| InputValidationError | Raised when the **Text** cannot be converted to a valid **Value**. (Inherited from UpDownBase)
| ValueChanged | Raised when the **Value** changes. (Inherited from UpDownBase)

## Methods
|| Method || Description
| SelectAll | Select all the Text from the TextBox in the DateTimePicker. (Inherited from DateTimeUpDownBase)

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---