# ValueRangeTextBox
Derives from Xceed.Wpf.Toolkit.AutoSelectTextBox

Represents a control that limits the value extracted from the inputted text to be within the bounds determined by the MinValue and MaxValue properties.

## Properties
|| Property || Description
| BeepOnError | Gets or sets a value indicating if a system beep is raised when an inputted character is rejected.
| FormatProvider | Gets or sets the IFormatProvider that will be used to perform type validation.
| HasParsingError | Gets a value indicating that an error occurred while parsing the inputted text.
| HasValidationError | Gets a value indicating whether an error occurred during the validation of the **Value**.
| IsValueOutOfRange | Gets a value indicating if the value is out of range.
| MaxValue | Gets or sets the maximum accepted value.
| MinValue | Gets or sets the minimum accepted value.
| NullValue | Gets or sets a value representing the control's null value.
| Value | Gets or sets the control's value.
| ValueDataType | Gets or sets the type of the control's **Value**.

## Events
|| Event || Description
| QueryTextFromValue | Raised when a value is being queried to return its string representation.
| QueryValueFromText | Raised when inputted text is queried to return its corresponding value.

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---