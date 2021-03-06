### DropDownListAttribute

When applied to to a property of a class that implements `IFileAttachment`, a `<select>` element is generated by the `FileAttachmentEditorFor()` method.

The `SelectListProperty` property defines the name of a property in the parent model that is used for generating the `<option>` elements. The property must implement `IEnumerable<SelectListItem>`.

The `OptionLabel` property defines the text used for generating the `null` option. The default is an empty string. To omit the `null` option, set the value of the property to `null`.

### DataListAttribute

When applied to to a `string` property of a class that implements `IFileAttachment`, an `<input type="text" list=".." />` element and a  `<datalist>` element is generated by the `FileAttachmentEditorFor()` method that is used to provide a basic auto-complete feature for the textbox. 

The `DataListProperty` property defines the name of a property in the parent model that is used for generating the `<datalist>` element. The property must implement `IEnumerable<string>`.


### Usage

```c#
// Describes a job application where a user can upload documents associated with the application
public class JobApplicationVM
{
    // .... other properties of JobApplicationVM
    public IEnumerable<SelectListItem> CategoryList { get; set; }
    public IEnumerable<string> NameSuggestions { get; set; }
    public IEnumerable<ApplicationDocumentVM> Documents { get; set; }
}
public class ApplicationDocumentVM : IFileAttachment
{
    //  .... implemented properties of IFileAttachment
    
    [Required(ErrorMessage = "Please enter a description")]
    [DropDownList(SelectListProperty = "CategoryList", OptionLabel = "Please select")] // generates a select element
    public int? SelectedCategory { get; set; }

    [DataList(DataListProperty = "NameSuggestions"] // generates a textbox with auto-complete feature
    public string Name { get; set; }
}
```

