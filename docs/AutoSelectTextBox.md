# AutoSelectTextBox

Derives from TextBox

AutoSelectTextBox is a control whose content is selected when it receives the focus. It also performs automatic focus navigation when the caret reaches the extremities of the text range.

## Usage

**XAML**

{{
<sample:DemoView x:Class="Samples.Modules.Text.Views.AutoSelectTextBoxView"
                 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:sample="clr-namespace:Samples.Infrastructure.Controls;assembly=Samples.Infrastructure"
                 xmlns:xctk="http://schemas.xceed.com/wpf/xaml/toolkit"
                 Title="AutoSelectTextBox"
                 Description="The AutoSelectTextBox allows the text content to be selected when the control get the focus. It also allows the Focus navigation behavior within the control to be affected.">
   <StackPanel>
      <StackPanel.Resources>
         <Style TargetType="{x:Type xctk:AutoSelectTextBox}">
            <Setter Property="Margin" Value="5"/>
            <Setter Property="AutoSelectBehavior" Value="{Binding SelectedItem, ElementName=_autoSelectBehavior}"/>
            <Setter Property="AutoMoveFocus" Value="{Binding IsChecked, ElementName=_autoMoveFocus}"/>
         </Style>
      </StackPanel.Resources>

      <!-- FEATURES GROUP BOX -->
      <GroupBox Header="Features" >
         <Grid Margin="5">
            <Grid.ColumnDefinitions>
               <ColumnDefinition />
               <ColumnDefinition />
            </Grid.ColumnDefinitions>
            <StackPanel Orientation="Horizontal" Grid.Column="0">
               <TextBlock Grid.Row="0" Grid.Column="0" Text="AutoSelectBehavior: " VerticalAlignment="Center" />
               <ComboBox Grid.Row="0" Grid.Column="1" x:Name="_autoSelectBehavior" SelectedIndex="1" Width="100" VerticalAlignment="Center">
                  <x:StaticExtension Member="xctk:AutoSelectBehavior.Never" />
                  <x:StaticExtension Member="xctk:AutoSelectBehavior.OnFocus" />
               </ComboBox>
            </StackPanel>
            <StackPanel Orientation="Horizontal" Grid.Column="1">
               <TextBlock Grid.Row="0" Grid.Column="2" Text="AutoMoveFocus:  " VerticalAlignment="Center" />
               <CheckBox Grid.Row="0" Grid.Column="3" x:Name="_autoMoveFocus" IsChecked="True" VerticalAlignment="Center"/>
            </StackPanel>
         </Grid>
      </GroupBox>
      
      <StackPanel>
         <TextBlock Text="Usage:" Style="{StaticResource Header}"/>
         <RichTextBox IsReadOnly="True" BorderThickness="0">
            <FlowDocument>
               <Paragraph>
                  <Bold>AutoSelectBehavior:</Bold>
                  <LineBreak/>
                  The value of the "AutoSelectBehavior" property determines whether the content of the AutoSelectTextBox will be selected or not when the control gets the focus.
                  <LineBreak/>
                  <LineBreak/>
                  <Bold>AutoMoveFocus:</Bold>
                  <LineBreak/>
                  <Italic>Effect with "MaxLength" property:</Italic>
                  <LineBreak/>
                  <LineBreak/>
                  Setting the "MaxLength" of the text box allows the focus to move from the AutoSelectTextBox once the max length has been reached.
               In the following "Telephone Number" fields, the "MaxLength" properties of the controls have been set to 3, 3, and 4.
               </Paragraph>
            </FlowDocument>
         </RichTextBox>
         
         <!-- PHONE NUMBER FIELDS -->         
         <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
            <TextBlock Text="(" VerticalAlignment="Center"/>
            <xctk:AutoSelectTextBox MaxLength="3" Width="30" Text="555"/>
            <TextBlock Text=")" VerticalAlignment="Center"/>
            <xctk:AutoSelectTextBox MaxLength="3" Width="30" Text="555"/>
            <TextBlock Text="-" VerticalAlignment="Center"/>
            <xctk:AutoSelectTextBox MaxLength="4" Width="40" Text="5555"/>
         </StackPanel>
         <RichTextBox IsReadOnly="True" BorderThickness="0">
            <FlowDocument>
               <Paragraph>
                  <Italic>Effect with Arrow keys</Italic>
                  <LineBreak/>
                  <LineBreak/>
                  Setting "AutoMoveFocus" to true also allows navigating the focus through the controls using the arrow keys to move the focus up, down, left, or right. 
               You are no longer limited to the "left-right" navigation of the "Tab, Shift-Tab" keys.
               </Paragraph>
            </FlowDocument>
         </RichTextBox>

         <!-- TEXTBOX MATRIX -->
         <StackPanel Orientation="Vertical" HorizontalAlignment="Center">
            <StackPanel Orientation="Horizontal">
               <xctk:AutoSelectTextBox Text="Text1" Width="100" />
               <xctk:AutoSelectTextBox Text="Text2" Width="100"/>
               <xctk:AutoSelectTextBox Text="Text3" Width="100"/>
            </StackPanel>
            <StackPanel Orientation="Horizontal">
               <xctk:AutoSelectTextBox Text="Text4" Width="100"/>
               <xctk:AutoSelectTextBox Text="Text5" Width="100"/>
               <xctk:AutoSelectTextBox Text="Text6" Width="100"/>
            </StackPanel>
            <StackPanel Orientation="Horizontal">
               <xctk:AutoSelectTextBox Text="Text7" Width="100"/>
               <xctk:AutoSelectTextBox Text="Text8" Width="100"/>
               <xctk:AutoSelectTextBox Text="Text9" Width="100"/>
            </StackPanel>
         </StackPanel>
      </StackPanel>
   </StackPanel>
</sample:DemoView>
}}
## Properties
|| Property || Description
| AutoMoveFocus |  Gets or sets a value indicating if the focus can navigate in the appropriate flow direction (e.g., from one cell to another when a cell is being edited) when the cursor is at the beginning or end of the auto-select text box.
| AutoSelectBehavior | Gets or sets a value indicating how the content of the auto-select text box is selected (Never or OnFocus). By default, Never.

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---