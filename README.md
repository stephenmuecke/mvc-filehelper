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
    ....
    public IEnumerable<ApplicationDocumentVM> Documents { get; set; }
}
public class ApplicationDocumentVM : IFileAttachment
{
    .... // implemented properties of IFileAttachment
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


### To Do
- Support for bootstrap
- Resource file for error messages, defaults etc.
- Render validation mesages in separate row so vertical alignment of form controls not affected
- Add client and server side validation for HttpPostedFileBase property (file size, file type)



<img src="/Images/file-attachment-display.png" />


