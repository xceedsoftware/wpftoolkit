# PropertyGrid Plus
Derives from Control

A version of the [PropertyGrid](PropertyGrid) with 15 additional Features:
{anchor:feature1}
**1) Custom Properties**
Create PropertyItems by specifying their characteristics (DisplayName, Value, Category, Description, Editor…). These CustomPropertyItems should be added to the PropertyGrid.Properties collection. The PropertyGrid will then display this collection of custom properties. The display of each property can be individually controlled.
* LiveExplorer sample : PropertyGrid/Using a CustomList/CustomProperties
{anchor:feature2}
**2) DefinitionKey Attribute (For Editors)**
The DefinitionKey attribute can be assigned to properties of your selected object to define which EditorDefinition to be applied to the property. As an alternative to the Editor attribute, this allows you separate the UI-specific code from your business model code. It can also be used to specify a specific default editor when a property type does not resolve to a valid editor (e.g., Object).

In the following example, the properties FirstName and LastName will use a TextEditor.

{{
    [DefinitionKey( "nameType" )](DefinitionKey(-_nameType_-))     
    public string FirstName { get; set; }      
    [DefinitionKey( "nameType" )](DefinitionKey(-_nameType_-))     
    public string LastName { get; set; }
    public object ReferenceNo { get; set; }

    <xctk:EditorTextDefinition TargetProperties="nameType" />
}}
* LiveExplorer sample : PropertyGrid/UsingSelectedObjects/DefinitionKeyAttribute
{anchor:feature3}
**3) Expandable properties when multiple objects are selected**
Expandable properties will work when using multi-selected object in a PropertGrid. The selectedObjects must have common expandable properties.
{anchor:feature4}
**4) Ability to collapse specific categories with a given class attribute**
You can use the ExpandedCategory attribute to make a specific category Collapsed or Expanded by default.

In the following example, the Category “Connections” is Collapsed by default.

{{
    [ExpandedCategory( "Conections", false )](ExpandedCategory(-_Conections_,-false-))
    Public class Person
    {
       [Category( "Conections" )](Category(-_Conections_-))
       public List<Person> Friends { get; set; }
       [Category( "Conections" )](Category(-_Conections_-))
       public string LastName { get; set; }
       [Category( "Information" )](Category(-_Information_-))
       public string FirstName { get; set; }
    }
}}
* LiveExplorer sample : PropertyGrid/UsingSelectedObjects/UsingAttributes
{anchor:feature5}
**5) Attributes for localization**
LocalizedDisplayName, LocalizedDescription, LocalizedCategory: Theses attributes allow to easily localize the DisplayName, Description, and Category attributes values using standard Resx resource files.

{{
    public class Person    
    {      
       [LocalizedDisplayName( "FirstName", typeof( DisplayLocalizationRes ) )](LocalizedDisplayName(-_FirstName_,-typeof(-DisplayLocalizationRes-)-))     
       [LocalizedDescription( "FirstNameDesc", typeof( DisplayLocalizationRes ) )](LocalizedDescription(-_FirstNameDesc_,-typeof(-DisplayLocalizationRes-)-)) 
       [LocalizedCategory( "InfoCategory", typeof( DisplayLocalizationRes ) )](LocalizedCategory(-_InfoCategory_,-typeof(-DisplayLocalizationRes-)-))  
       public string FirstName { get; set; }
    }
    // DisplayLocalizationRes is a resx file.
}}

* LiveExplorer sample : PropertyGrid/UsingSelectedObjects/Localization
{anchor:feature6}
**6) Override property's editor definitions**
The EditorDefinition classes can be overridden to fits your needs.
{anchor:feature7}
**7) PropertiesSource string collections are editable**
When setting the PropertiesSource or Properties property, you can provide your own data to be displayed in the PropertyGrid. This allows you to easily insert and remove properties at runtime. The usage scheme is similar to the one used for the standard ItemsControl.

The following example uses the MyData object to create a collection of 2 properties to fill the PropertyGrid.

{{
    <xctk:PropertyGrid x:Name="_propertyGrid"
                        PropertiesSource="{Binding}"  
                        PropertyNameBinding="{Binding MyName}" 
                        PropertyValueBinding="{Binding MyValue}" > 

    var list = new ObservableCollection<object>();  
    list.Add( new MyData( "string", "First text" ) );    
    list.Add( new MyData( "Second string", "Second text" ) );
    this.DataContext = list;

    private class MyData    
    {     
       public MyData( string name, object value )  
       {      
          this.MyName = name;   
          this.MyValue = value;  
       }     
       public string MyName { get; set; }
       public object MyValue { get; set; }
    }
}}
* LiveExplorer sample : PropertyGrid/Using a CustomList
{anchor:feature8}
**8) List source for properties added**
Same as 7. A list or collection can be provided to the PropertyGrid using its PropertiesSource property.
{anchor:feature9}
**9) Multi-Selected Objects**
When many objects are assigned to a PropertyGrid, the PropertyGrid will show all the common properties so that they can be changed simultaneously on each object.
* LiveExplorer sample : PropertyGrid/UwingSelectedObject/Multi-Selected Objects
{anchor:feature10}
**10) Custom properties list**
CustomPropertyItems can be added to the PropertyGrid.Properties like in point 1.
A list of CustomProperties can be passed to the PropertyGrid.PropertiesSource like in point 7.
{anchor:feature11}
**11) within the PropertyGrid.SelectedObjects collection**
The Validation of wrong inputs will be done when using Multi-Selected Objects on a PropertyGrid.
{anchor:feature12}
**12) Category ordering without attributes**
You can set the category ordering without having to add CategoryOrderAttributes to selected objects.
{anchor:feature13}
**13) Validation display for multi-selected objects**
When using multiple selected objects, the validation red border will now be displayed on invalid input.
When a wrong value is typed in a propertyGrid having muli-selected objects, the red border will appear around the propertyItem in the PropertyGrid.
* Live Explorer sample : PropertyGrid/Using SelectedObject/Multi-Selected Objects
{anchor:feature14}
**14) DependsOn Attribute**
The "DependsOn" attribute can be placed over any property of the PropertyGrid's selected object. A list of strings can be passed to the "DependsOn" attribute. The properties with this attribute will have their editor re-evaluated when the properties listed in the "DependsOn" attribute are modified.
{anchor:feature15}

In the following example, the property "FirstName" has its own editor. If the properties "IsMale" or "LastName" are modified, the editor of property "FirstName" will be re-evaluated. This allows a user to modify the editor to change its background, ItemsSource, or create a new editor.

{{
    [Editor( typeof( FirstNameEditor ), typeof( FirstNameEditor ) )](Editor(-typeof(-FirstNameEditor-),-typeof(-FirstNameEditor-)-))
    [DependsOn( "IsMale", "LastName" )](DependsOn(-_IsMale_,-_LastName_-))
    public string FirstName { get; set; }
}}
It is also possible to have different properties with the same "DependsOn". Here the editors of properties "FirstName" and "LastName" will be re-evaluated if the property "IsMale" is changed.

{{
    [Editor( typeof( FirstNameEditor ), typeof( FirstNameEditor ) )](Editor(-typeof(-FirstNameEditor-),-typeof(-FirstNameEditor-)-))
    [DependsOn( "IsMale" )](DependsOn(-_IsMale_-))
    public string FirstName { get; set; }

    [Editor( typeof( LastNameEditor ), typeof( LastNameEditor ) )](Editor(-typeof(-LastNameEditor-),-typeof(-LastNameEditor-)-))
    [DependsOn( "IsMale" )](DependsOn(-_IsMale_-))
    public string LasttName { get; set; }
}}

**15) FilePicker editor**
You can use the FilePicker editor to select files in the Property grid.

## Properties
|| Property || Description
| * | All the Properties from [PropertyGrid](PropertyGrid)
| CategoryDefinition.IsBrowsable | Gets or sets a value indicating whether the category and its properties will be shown in the property grid.
| CategoryDefinitions | Gets or sets the CategoryDefinition collection sed to set the categories.
| CategoryGroupDefinitions | Gets or sets the GroupDescription to be applied on the source items in order to define the groups when the PropertyGrid is Categorized.
| IsMiscCategoryLabelHidden | Gets or sets a value indicating whether the "Misc" category expander should be hidden.  
| PropertyNameBinding | Gets or sets the Binding to be used on the property's underlying item to get the name of the property to display.
| PropertyValueBinding | Gets or sets the Binding to be used on the property's underlying item to get the value of the property to display.
| DefaultEditorDefinition | Gets or sets the default editor definition to use when the property value type is not supported.
| PropertiesSource | Gets or sets the items source for the properties of the PropertyGrid.
| PropertyContainerStyle | Gets or sets the style that will be applied to all PropertyItemBase instances displayed in the property grid.
| SelectedObjects | Gets the currently selected objects the PropertyGrid is inspecting.
| SelectedObjectsOverride | Gets or sets the list of selected objects.

## Methods
|| Method || Description
| * | All the Methods from [PropertyGrid](PropertyGrid)
| CollapseAllCategories | Will collapse all categories in the propertyGrid.
| ExpandAllCategories | Will expand all categories in the propertyGrid.
| CollapseCategory | Will collapse a specific category in the PropertyGrid.
| ExpandCategory | Will expand a specific category in the PropertyGrid.
---