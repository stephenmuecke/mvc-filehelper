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

### HtmlHelper methods

#### Sample Model

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

#### FileAttachmentEditorFor() method

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
- A `<textarea>` if the property is `string` and has the `[DataType(DataType.Multiline)]` attribute.
- An `<input type="checkbox" .. />` if the property is `bool`.
- A `<select>` if the property is `Nullable<bool>`. The text for the `null` option is defined by using the `[DisplayFormat(NullDisplayText = ".....")]` attribute.
- A `<select>` if the property is `enum` or `Nullable<enum>`. The text for the `null` option is defined by using the `[DisplayFormat(NullDisplayText = ".....")]` attribute.
- A `<select>` if the property has the `[DropDownList("xxx")]` attribute, where `xxx` is the name of a property in the parent view model that implements `IEnumerable<SelectListItem>`. The text for the `null` option is defined by using the `OptionLabel` property of `DropDownListAttribute`.
- An `<input type="text" .. />` if none of the above conditions are met. If the property has a `[DataList("xxx")]` attribute, where `xxx` is the name of a property in the parent view model that implements `IEnumerable<string>`, a `<datalist>` element is rendered to provide a basic auto-complete feature.

Client side validation is included for all form controls, included in dynamically added rows.

#### FileAttachmentDisplayFor() method

```c#
@model JobApplicationVM
....
@Html.FileAttachmentDisplayFor(m => m.Documents)
```

will generate the following view

<img src="/Images/file-attachment-readonly.png" />

### FileHelper methods

The `FileHelper` class contains static methods that enscapulate common code for saving and downloading files.

#### Save() method

Saves uploaded files to the server (updates the properties each `IEnumerable<IFileAttachement>`) and optionally deletes files marked for deletion.

```c#
[HttpPost]
public ActionResult Edit(JobApplicationVM model)
{
    string folder = "~/App_Data/Files"; // define folder to save files
    if (!ModelState.IsValid)
    {
        FileHelper.Save(model.Documents, folder, false); // save new files only
        return View(model);
    }      
    FileHelper.Save(modelDocuments, folder, true); // save and delete files
    // Get the data model, update from the view model, save and redirect
}
```

#### Download() method

Returns a `FileResult` to download a file.

```c#
public ActionResult DownloadAttachment(int ID)
{
    // .... Get the virtual path based on the ID (and optionally the display name if setting the ContentDisposition) 
    
    ContentDisposition disposition = new ContentDisposition { FileName = displayName, Inline = false };
    Response.AddHeader("Content-Disposition", disposition.ToString());
    
    // Return the file
    return FileHelper.Download(virtualPath);
}
```

### To Do
- Support for bootstrap
- Resource file for error messages, defaults etc.
- Render validation mesages in separate row so vertical alignment of form controls not affected
- Add client and server side validation for HttpPostedFileBase property (file size, file type)
- Re-parse the `$.validator` for each dynamically added form control, rather that re-parsing the whole form each time a file is added


