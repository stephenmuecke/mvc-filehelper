# mvc-filehelper
### Description
A set of `HtmlHelper` extension methods, a jquery plug-in and utility methods to simplify the process of uploading, saving and download files.

#### Pre-requisites
- The database must contain fields for a PK (`int`) and the files display name and virtual path
- The view model used to create, edit and display files must implement `IFileAttachment`

#### Aims
- To provide an improved UI for dynamically adding, editing and deleting one or more files
- To allow additional properties associated with a file to be displayed and edited
- To provide client side validation for dynamically added files and their associated properties
- To allow the view to be returned in a `[HttpPost]` method (when `ModelState` is invalid) without having to re-select files

### Example

#### Model

```c#
// Describes a job application where a user can upload documents associated with the application
public class JobApplicationVM
{
    // .... other properties of JobApplicationVM
    
    public IEnumerable<ApplicationDocumentVM> Documents { get; set; }
}
public class ApplicationDocumentVM : IFileAttachment
{
    //  .... implemented properties of IFileAttachment
    
    [Required(ErrorMessage = "Please enter a description")]
    public string Description { get; set; }
    
    [Display(Name = "Category")]
    [Required(ErrorMessage = "Select a category")]
    [DisplayFormat(NullDisplayText = "Please select")]
    public DocumentType? DocumentType { get; set; }
    
    [Display(Name = "Confidential")]
    public bool IsConfidential { get; set; }
}
public enum DocumentType
{
    Qualification,
    Reference
}
```

### View

```c#
@model JobApplicationVM
....
@using (Html.BeginForm())
{
    ....
    @Html.FileAttachmentEditorFor(m => m.Documents)
```

will generate the following view

<img src="/Images/file-attachment-edit.png" />

The green '+' footer button opens the dialog to select a new file. The red '-' button marks an uploaded file for deletion (the button becomes a blue '+' button to toggle the action), of if the file has not been saved, removes the row.

For additional properties in the model, the following form controls are generated:
- A `<textarea>` for `string` if the property is decorated with `[DataType(DataType.Multiline)]` attribute.
- A `<input type="checkbox" .. />` if the property is `bool`.
- A `<select>` if the property is `Nullable<bool>`. The text for the `null` option is defined by using the `[DisplayFormat(NullDisplayText = ".....")]` attribute.
- A `<select>` if the property is `enum` or `Nullable<enum>`. The text for the `null` option is defined by using the `[DisplayFormat(NullDisplayText = ".....")]` attribute.


### To Do
- Support for bootstrap
- Resource file for error messages, defaults etc.
- Render validation mesages in separate row so vertical alignment of form controls not affected
- Add client and server side validation for HttpPostedFileBase property (file size, file type)



<img src="/Images/file-attachment-display.png" />


