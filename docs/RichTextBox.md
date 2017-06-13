# RichTextBox
Derives from System.Windows.Controls.RichTextBox

Extends the System.Windows.Control.RichTextBox control that represents a rich editing control which operates on FlowDocument objects.  The RichTextBox control has a Text dependency property which allows a user to data bind content to the RichTextBox.Document property.  The RichTextBox control introduces the concept of Text Formatters.  Text Formatters allows a user to format the content of the RichTextBox control into any format of their choice.  Three Text Formatters are included; PlainTextFormatter, RtfFormatter, and a XamlFormatter.  The RtfFormatter is the default Text Formatter.  A user can create their own custom Text Formatter by creating a class that inherits from ITextFormatter and implimenting the contract accordlingly.

* [Usage](#usage)
* [Formatters](#formatters)
* [Custom Formatters](#customformatters)
* [Gotchas](#gotchas)
* [Properties](#properties)
* [Methods](#methods)

{anchor:usage}
## Usage

When data binding to the Text property, you must use the Text Formatter that matches the format of the underlying data.  If your data is in RTF you must use the RTF formatter.  If your data is in plain text, you must use the PlainTextFormatter.

![](RichTextBox_ richtextbox_control.jpg)

This RichTextBox is bound to an object that has a Notes property.  The value of the Notes property is as follows in the RTF format:

{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Segoe UI;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs18\f2\cf0 \cf0\ql{\f2 {\ltrch This is the }{\b\ltrch RichTextBox}\li0\ri0\sa0\sb0\fi0\ql\par}}}

{{
<toolkit:RichTextBox x:Name="_richTextBox" Grid.Row="1" Margin="10" BorderBrush="Gray" Padding="10"
                                          Text="{Binding Notes}" 
                                          ScrollViewer.VerticalScrollBarVisibility="Auto" />
}}

{anchor:formatters}
## Using Formatters

To use a different Text Formatter than the default RtfFormatter use the following syntax:

**PlainTextFormatter**
{{
<toolkit:RichTextBox x:Name="_richTextBox" Grid.Row="1" Margin="10" BorderBrush="Gray" Padding="10"
                                     Text="{Binding Notes}" 
                                     ScrollViewer.VerticalScrollBarVisibility="Auto">
            <toolkit:RichTextBox.TextFormatter>
                <toolkit:PlainTextFormatter />
            </toolkit:RichTextBox.TextFormatter>
</toolkit:RichTextBox>
}}
+Plain Text Format:+  "This is the RichTextBox\r\n"

**RtfFormatter**
{{  
<toolkit:RichTextBox x:Name="_richTextBox" Grid.Row="1" Margin="10" BorderBrush="Gray" Padding="10"
                                     Text="{Binding Notes}" 
                                     ScrollViewer.VerticalScrollBarVisibility="Auto">
            <toolkit:RichTextBox.TextFormatter>
                <toolkit:RtfFormatter />
            </toolkit:RichTextBox.TextFormatter>
</toolkit:RichTextBox>
}}
+RTF Format:+  "{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Segoe UI;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs18\f2\cf0 \cf0\ql{\f2 {\ltrch This is the }{\b\ltrch RichTextBox}\li0\ri0\sa0\sb0\fi0\ql\par}}}"

**XamlFormatter**
{{
<toolkit:RichTextBox x:Name="_richTextBox" Grid.Row="1" Margin="10" BorderBrush="Gray" Padding="10"
                                     Text="{Binding Notes}" 
                                     ScrollViewer.VerticalScrollBarVisibility="Auto">
            <toolkit:RichTextBox.TextFormatter>
                <toolkit:XamlFormatter />
            </toolkit:RichTextBox.TextFormatter>
</toolkit:RichTextBox>
}}
+Xaml Format:+  "<Section xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xml:space=""preserve"" TextAlignment=""Left"" LineHeight=""Auto"" IsHyphenationEnabled=""False"" xml:lang=""en-us"" FlowDirection=""LeftToRight"" NumberSubstitution.CultureSource=""User"" NumberSubstitution.Substitution=""AsCulture"" FontFamily=""Segoe UI"" FontStyle=""Normal"" FontWeight=""Normal"" FontStretch=""Normal"" FontSize=""12"" Foreground=""#FF000000"" Typography.StandardLigatures=""True"" Typography.ContextualLigatures=""True"" Typography.DiscretionaryLigatures=""False"" Typography.HistoricalLigatures=""False"" Typography.AnnotationAlternates=""0"" Typography.ContextualAlternates=""True"" Typography.HistoricalForms=""False"" Typography.Kerning=""True"" Typography.CapitalSpacing=""False"" Typography.CaseSensitiveForms=""False"" Typography.StylisticSet1=""False"" Typography.StylisticSet2=""False"" Typography.StylisticSet3=""False"" Typography.StylisticSet4=""False"" Typography.StylisticSet5=""False"" Typography.StylisticSet6=""False"" Typography.StylisticSet7=""False"" Typography.StylisticSet8=""False"" Typography.StylisticSet9=""False"" Typography.StylisticSet10=""False"" Typography.StylisticSet11=""False"" Typography.StylisticSet12=""False"" Typography.StylisticSet13=""False"" Typography.StylisticSet14=""False"" Typography.StylisticSet15=""False"" Typography.StylisticSet16=""False"" Typography.StylisticSet17=""False"" Typography.StylisticSet18=""False"" Typography.StylisticSet19=""False"" Typography.StylisticSet20=""False"" Typography.Fraction=""Normal"" Typography.SlashedZero=""False"" Typography.MathematicalGreek=""False"" Typography.EastAsianExpertForms=""False"" Typography.Variants=""Normal"" Typography.Capitals=""Normal"" Typography.NumeralStyle=""Normal"" Typography.NumeralAlignment=""Normal"" Typography.EastAsianWidths=""Normal"" Typography.EastAsianLanguage=""Normal"" Typography.StandardSwashes=""0"" Typography.ContextualSwashes=""0"" Typography.StylisticAlternates=""0""><Paragraph><Run>This is the </Run><Run FontWeight=""Bold"">RichTextBox</Run></Paragraph></Section>"

{anchor:customformatters}
## Custom Formatters

To create a custom formatter create a class that inherits from ITextFormatter and implement accordingly.

{{
public class MyFormatter : ITextFormatter
{
        public string GetText(System.Windows.Documents.FlowDocument document)
        {
            return new TextRange(document.ContentStart, document.ContentEnd).Text;
        }

        public void SetText(System.Windows.Documents.FlowDocument document, string text)
        {
            new TextRange(document.ContentStart, document.ContentEnd).Text = text;
        }
 }
}}

Xaml:

{{
<toolkit:RichTextBox x:Name="_richTextBox" Grid.Row="1" Margin="10" BorderBrush="Gray" Padding="10"
                                     Text="{Binding Notes}" 
                                     ScrollViewer.VerticalScrollBarVisibility="Auto">
            <toolkit:RichTextBox.TextFormatter>
                <myCustomFormatter:MyFormatter />
            </toolkit:RichTextBox.TextFormatter>
</toolkit:RichTextBox>
}}

{anchor:gotchas}
## Gotchas

When using the RichTextBox with buttons to change the styles of your text such as bold, italics, etc...,; you will notice that the Text is not updated until the control losses focus.  Therefore when you leave focus on the RichTextBox and start manipulating test with buttons, those changes will not be propogated property.  This is because by default, the source is not update until the RichTextBox control loses focus.  To enable this behavior you must set the UpdateSourceTrigger to PropertyChanged on your Text property binding.  This will force any change to the text to be updated through data binding to the underlying data source.

Example:

![](RichTextBox_richtextbox_updatesourcetrigger.jpg)

{{
<Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition />
        </Grid.RowDefinitions>
        <StackPanel Orientation="Horizontal" Margin="10">
        <Button Command="EditingCommands.ToggleBold" 
                CommandTarget="{Binding ElementName=_richTextBox}"
                Content="B"
                FontWeight="Bold"
                MinWidth="25"/>
            <Button Command="EditingCommands.ToggleItalic" 
                CommandTarget="{Binding ElementName=_richTextBox}"
                Content="I"
                FontStyle="Italic"
                MinWidth="25"/>
        </StackPanel>
        <toolkit:RichTextBox x:Name="_richTextBox" Grid.Row="1" Margin="10" BorderBrush="Gray" Padding="10"
                                     Text="{Binding Notes, UpdateSourceTrigger=PropertyChanged}" 
                                     ScrollViewer.VerticalScrollBarVisibility="Auto">
            <RichTextBox.CommandBindings>
                <CommandBinding Command="EditingCommands.ToggleBold"/>
                <CommandBinding Command="EditingCommands.ToggleItalic"/>
            </RichTextBox.CommandBindings>
        </toolkit:RichTextBox>
</Grid>
}}

{anchor:properties}
## Properties
|| Property || Description
| Text | Gets or sets the text displayed in the RichTextBox.
| TextFormatter | Gets or sets the ITextFormatter used to format the contents for the RichTextBox.  RtfFormatter is Default.

{anchor:methods}
## Methods
|| Method || Description
| Clear | Clears the contents of the RichTextBox.

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---