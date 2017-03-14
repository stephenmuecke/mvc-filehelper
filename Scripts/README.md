### Usage
When using the `@Html.FileAttachmentEditorFor(m => m.SomeProperty)` method to generate a view for adding, editing and deleting files, the view must include the `sandtrap-fileupload-v1.0.js` script. To attach the plugin to the element

```js
<script src="~/Scripts/sandtrap-fileupload-v1.0.js"></script>
<script>
    $('#Documents').fileUpload();
</script>
```

### How the script works
