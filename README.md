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




### To Do
- Support for bootstrap
- Resource file for error messages, defaults etc.
- Render validation mesages in separate row so vertical alignment of form controls not affected
- Add client and server side validation for HttpPostedFileBase property (file size, file type)



<img src="/Images/file-attachment-display.png" />

<img src="/Images/file-attachment-edit.png" />
