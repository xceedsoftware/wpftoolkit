# MaskedTextBox (obsolete version)

The MaskedTextBox control lets you display and edit values based on a mask. The mask is specified via its Mask property.

Note: This version of MaskedTextBox is obsolete and has been replaced by a new one, which provides a much more complete API. The original control is still available in the Xceed.Wpf.Toolkit.Obselete namespace.

## Usage

To add a mask to the MaskTextBox set the MaskedTextBox.Mask property to a supported mask string.

{{
<toolkit:MaskedTextBox Mask="(000) 000-0000" />
}}

You can Get/Set the value of the MaskedTextBox by using the MaskedTextBox.Value property.  The MaskedTextBox.Value property supports binding to any data type, but it is a String by default.

{{
<toolkit:MaskedTextBox Mask="(000) 000-0000" Value="5551234567" />
}}

In this example the Value is a String and does not include the literals of the mask.  You can choose to include the literals of the mask by setting the MaskedTextBox.IncludeLiterals to true.  If set to true your Value would look like (555) 123-4567.  You can also choose to include the prompt characters.  If you set the MaskedTextBox.IncludePrompt to true, the value would store any prompt characters in the mask.  For instance, the masked may be incomplete, (555) 12{"_"}-{"____"}, when set, the Value would also include the {"_"} prompt characters.  If the MaskedTextBox.IncludePrompt property is set to false, the value would simply be (555) 12 -    .  By default the MaskedTextBox.IncludeLiterals and MaskedTextBox.IncludePrompt properties are false.

{{
<toolkit:MaskedTextBox Mask="(000) 000-0000" Value="(555) 123-4567" IncludeLiterals="True" />
}}

## Changing Value Types
Lets say you want to apply a mask to a value, but not store the mask literals or prompt with it, and your data type is not a string.  To support different data types you must set the MaskedTextBox.ValueType property to your required data type.

First add a new namespace to the top of your XAML file to mscorlib.  This contains the different data types.

{{
xmlns:sys="clr-namespace:System;assembly=mscorlib"...
}}

Next, specify the MaskedTextBox.ValueType to the required data type. In this example the underlying value is an Int64.

{{
<toolkit:MaskedTextBox Mask="(000) 000-0000" Value="5551234567" ValueType="{x:Type sys:Int64}" />
}}

You can still obtain the formatted mask text by using the MaskedTextBox.Text property.

## Supported Mask Strings
|| Masking Element || Description
| 0 | Digit, required. This element will accept any single digit between 0 and 9. 
| 9 | Digit or space, optional. 
| # | Digit or space, optional. If this position is blank in the mask, it will be rendered as a space in the Text property. Plus (+) and minus (-) signs are allowed. 
| L | Letter, required. Restricts input to the ASCII letters a-z and A-Z. This mask element is equivalent to {"[a-zA-Z](a-zA-Z)"} in regular expressions. 
| ? | Letter, optional. Restricts input to the ASCII letters a-z and A-Z. This mask element is equivalent to {"[a-zA-Z](a-zA-Z)?"} in regular expressions. 
| & | Character, required. If the AsciiOnly property is set to true, this element behaves like the "L" element. 
| C | Character, optional. Any non-control character. If the AsciiOnly property is set to true, this element behaves like the "?" element. 
| A | Alphanumeric, optional. If the AsciiOnly property is set to true, the only characters it will accept are the ASCII letters a-z and A-Z. 
| a | Alphanumeric, optional. If the AsciiOnly property is set to true, the only characters it will accept are the ASCII letters a-z and A-Z. 
| . | Decimal placeholder. The actual display character used will be the decimal symbol appropriate to the format provider, as determined by the control's FormatProvider property. 
| , | Thousands placeholder. The actual display character used will be the thousands placeholder appropriate to the format provider, as determined by the control's FormatProvider property. 
| : | Time separator. The actual display character used will be the time symbol appropriate to the format provider, as determined by the control's FormatProvider property. 
| / | Date separator. The actual display character used will be the date symbol appropriate to the format provider, as determined by the control's FormatProvider property. 
| $ | Currency symbol. The actual character displayed will be the currency symbol appropriate to the format provider, as determined by the control's FormatProvider property. 
| < | Shift down. Converts all characters that follow to lowercase 
| > | Shift up. Converts all characters that follow to uppercase. 
|   | Disable a previous shift up or shift down. 
| \ | Escape. Escapes a mask character, turning it into a literal. "\\" is the escape sequence for a backslash. 
All other characters Literals. All non-mask elements will appear as themselves within MaskedTextBox. Literals always occupy a static position in the mask at run time, and cannot be moved or deleted by the user.
## Properties / Events
|| Property || Description
| IncludeLiterals | Gets or sets a value indicating whether literals in Mask are included in Value.
| IncludePrompt | Gets or sets a value indicating whether prompt characters in Mask are included in Value.
| Mask | Gets or sets the mask.
| MaskProvider | Gets or sets the MaskedTextProvider of the MaskedTextBox.
| PromptChar | Gets or sets the prompt character.
| SelectAllOnGotFocus | Gets or sets a value indicating whether the entire text displayed in the MaskedTextBox will be selected when it receives focus.
| Value | Gets or sets the value of the MaskedTextBox.
| ValueType | Gets or sets the Type of Value.

|| Event || Description
| ValueChanged | Raised when Value changes.
Use ValueType if you want to apply a mask to a value, but not store the mask literals or prompt with it, and your data type is not a string. To support different data types you must set the MaskedTextBox.ValueType property to your required data type.

First add a new namespace to the top of your XAML file to mscorlib:

{{
xmlns:sys="clr-namespace:System;assembly=mscorlib"...
}}

Then set the MaskedTextBox.ValueType to the required data type, for example:

{{
<xctk:MaskedTextBox Mask="(000) 000-0000" Value="5551234567" ValueType="{x:Type sys:Int64}" />
}}