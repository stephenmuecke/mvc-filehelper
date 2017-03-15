using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Sandtrap.Web.Models;

// Resource file for error messages, label option
// Grouped SelectList
// DataAnotations keys

namespace Sandtrap.Web.Html
{

    /// <summary>
    /// Renders the html for a collection of files.
    /// </summary>
    public static class FileAttachmentHelper
    {

        #region .Declarations 

        private static string _SelectListKey = DataAnnotations.DropDownListAttribute.SelectListPropertyKey;
        private static string _OptionLabelKey = DataAnnotations.DropDownListAttribute.OptionLabelKey;
        private static string _DataListKey = DataAnnotations.DataListAttribute.DataListPropertyKey;

        // Error messages
        private const string _InvalidDataListPropertyName = "The model in the view does not contain a property for a DataList named {0}.";
        private const string _InvalidDataListType = "The property {0} used to define the DataList does not implement IEnumerable<string>";
        private const string _NullDataList = "The DataList for property {0} cannot be null";
        private const string _InvalidSelectListPropertyName = "The model in the view does not contain a property for a SelectList named {0}.";
        private const string _InvalidSelectListType = "The property {0} used to define the SelectList does not implement IEnumerable<SelectListItem>";
        private const string _NullSelectList = "The SelectList for property {0} cannot be null";
         
        #endregion

        #region .Properties 

        // TODO: Put this is separate class (common to other extension methods)
        public enum ButtonType
        {
            Add,
            Delete
        }

        #endregion

        #region .Methods 

        /// <summary>
        /// Returns the html to display file properties and download links.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to render.
        /// </param>
        /// <param name="actionName">
        /// The name of the action method that returns a FileResult to download the file.
        /// </param>
        public static MvcHtmlString FileAttachmentDisplayFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<IFileAttachment>>> expression, string actionName = "DownloadAttachment")
        {
            return FileAttachmentDisplayFor(helper, expression, actionName, null);
        }

        /// <summary>
        /// Returns the html to display file properties and links.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to render.
        /// </param>
        /// <param name="actionName">
        /// The name of the action method that returns a FileResult to download the file.
        /// </param>
        /// <param name="controllerName">
        /// The name of the controller containing the <paramref name="actionName"/> method.
        /// </param>
        public static MvcHtmlString FileAttachmentDisplayFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<IFileAttachment>>> expression, string actionName, string controllerName)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the attachments
            IEnumerable<IFileAttachment> attachments = metaData.Model as IEnumerable<IFileAttachment>;
            if (attachments == null)
            {
                // TODO: add single row with "No files selected"?
                throw new ArgumentException("The collection contains no elements");
            }
            // Get the fully qualified name of the property
            string propertyName = ExpressionHelper.GetExpressionText(expression);
            // Get the type metadata
            Type modelType = metaData.ModelType.GetGenericArguments()[0];
            // Get properties included in the model but not the IFileAttachment
            IEnumerable<ModelMetadata> extraProperties = GetExtraProperties(modelType);
            // Build the html
            StringBuilder html = new StringBuilder();
            IEnumerable<string> extraColumns = extraProperties.Select(x => x.GetDisplayName());
            html.Append(DisplayHeader(extraColumns));
            extraColumns = extraProperties.Select(x => x.PropertyName);
            html.Append(DisplayBody(helper, modelType, attachments, extraColumns, actionName, controllerName));
            TagBuilder table = new TagBuilder("table");
            table.AddCssClass("readonly-table");
            table.AddCssClass("file-attachments");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(propertyName));
            table.InnerHtml = html.ToString();
            return MvcHtmlString.Create(table.ToString());
        }

        /// <summary>
        /// Returns the html for an editable control to add and delete files.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to render.
        /// </param>
        public static MvcHtmlString FileAttachmentEditorFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<IFileAttachment>>> expression)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the attachments
            IEnumerable<IFileAttachment> attachments = metaData.Model as IEnumerable<IFileAttachment>;
            // Get the fully qualified name of the property
            string propertyName = ExpressionHelper.GetExpressionText(expression);
            // Get the type metadata
            Type modelType = metaData.ModelType.GetGenericArguments()[0];
            // Get the metadata for new instance
            object instance = Activator.CreateInstance(modelType);
            ModelMetadata itemMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => instance, modelType);
            // Get any selectlists or datalists 
            Dictionary<string, object> optionLists = GetOptionLists(itemMetadata, helper.ViewData.Model);
            // Get properties included in the model but not the IFileAttachment
            IEnumerable<ModelMetadata> extraProperties = GetExtraProperties(modelType);
            // Build html
            StringBuilder html = new StringBuilder();
            IEnumerable<string> extraColumns = extraProperties.Select(x => x.GetDisplayName());
            string header = EditHeader(extraColumns);
            html.Append(header);
            extraColumns = extraProperties.Select(x => x.PropertyName);
            if (attachments == null)
            {
                TagBuilder tbody = new TagBuilder("tbody");
                html.Append(tbody.ToString());
            }
            else
            {
                string body = EditBody(helper, modelType, attachments, propertyName, extraColumns, optionLists);
                html.Append(body);
            }
            string hiddenBody = HiddenBody(helper, itemMetadata, propertyName, extraColumns, optionLists);
            html.Append(hiddenBody);
            string footer = EditFooter(propertyName, extraProperties.Count());
            html.Append(footer);
            TagBuilder table = new TagBuilder("table");
            table.AddCssClass("edit-table");
            table.AddCssClass("file-attachments");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(propertyName));
            table.InnerHtml = html.ToString();
            // Add any datalists
            var datalists = optionLists.Where(x => x.Value is IEnumerable<string>);
            if (datalists.Any())
            {
                html = new StringBuilder();
                html.Append(table.ToString());
                foreach (var item in datalists)
                {
                    string id = String.Format("{0}-datalist", item.Key).ToLower();
                    string datalist = DataList(id, item.Value as IEnumerable<string>);
                    html.Append(datalist);
                }
                return MvcHtmlString.Create(html.ToString());
            }
            return MvcHtmlString.Create(table.ToString());
        }

        #endregion

        #region .Helper methods 

        // Gets the data used to generate datalists and selectlists 
        private static Dictionary<string, object> GetOptionLists(ModelMetadata itemMetadata, object parentModel)
        {
            // Get datalists
            var dataLists = itemMetadata.Properties.Where(x => x.AdditionalValues.ContainsKey(_DataListKey)).Select(x => new
            {
                propertyName = x.PropertyName,
                optionsPropertyName = x.AdditionalValues[_DataListKey]
            });
            // Get selectlists
            var selectLists = itemMetadata.Properties.Where(x => x.AdditionalValues.ContainsKey(_SelectListKey)).Select(x => new
            {
                propertyName = x.PropertyName,
                optionsPropertyName = x.AdditionalValues[_SelectListKey],
                optionLabel = x.AdditionalValues[_OptionLabelKey]
            });
            if (selectLists.Any() || dataLists.Any())
            {
                Dictionary<string, object> optionsLists = new Dictionary<string, object>();
                Type parentType = parentModel.GetType();
                var parentMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => parentModel, parentType);
                foreach(var item in dataLists)
                {
                    var optionsMetadata = parentMetadata.Properties.FirstOrDefault(x => x.PropertyName == (string)item.optionsPropertyName);
                    if (optionsMetadata == null)
                    {
                        string message = String.Format(_InvalidDataListPropertyName, item.optionsPropertyName);
                        throw new MissingFieldException(message); // TODO: What is the correct exception

                    }
                    if (optionsMetadata.Model == null)
                    {
                        string message = String.Format(_NullDataList, item.optionsPropertyName);
                        throw new NullReferenceException(message);
                    }
                    IEnumerable<string> options = optionsMetadata.Model as IEnumerable<string>;
                    if (options == null)
                    {
                        string message = String.Format(_InvalidDataListType, item.optionsPropertyName);
                        throw new InvalidCastException(message); // TODO: What is the correct exception
                    }
                    optionsLists.Add(item.propertyName, options);
                }
                foreach(var item in selectLists)
                {
                    var optionsMetadata = parentMetadata.Properties.FirstOrDefault(x => x.PropertyName == (string)item.optionsPropertyName);
                    if (optionsMetadata == null)
                    {
                        string message = String.Format(_InvalidSelectListPropertyName, item.optionsPropertyName);
                        throw new MissingFieldException(message); // TODO: What is the correct exception

                    }
                    if (optionsMetadata.Model == null)
                    {
                        string message = String.Format(_NullSelectList, item.optionsPropertyName);
                        throw new NullReferenceException(message);
                    }
                    IEnumerable<SelectListItem> options = optionsMetadata.Model as IEnumerable<SelectListItem>;
                    if (options == null)
                    {
                        string message = String.Format(_InvalidSelectListType, item.optionsPropertyName);
                        throw new InvalidCastException(message); // TODO: What is the correct exception
                    }
                    // Build new option list
                    List<SelectListItem> selectList = new List<SelectListItem>();
                    if (item.optionLabel != null)
                    {
                        selectList.Add(new SelectListItem() { Value = "", Text = (string)item.optionLabel });
                    }
                    foreach (var option in options)
                    {
                        selectList.Add(new SelectListItem() { Value = option.Value, Text = option.Text });
                    }
                    optionsLists.Add(item.propertyName, selectList);
                }
                return optionsLists;
            }
            return new Dictionary<string,object>();
        }

        // Gets the collection of properties implemented in the model, but not the interface.
        private static IEnumerable<ModelMetadata> GetExtraProperties(Type type)
        {
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // Get properties not included in IFileAttachment and that are not complex types
            IEnumerable<string> intefaceNames = typeof(IFileAttachment).GetProperties().Select(x => x.Name);
            return metadata.Properties.Where(x => !intefaceNames.Contains(x.PropertyName) && !x.IsComplexType);
        }

        // Generates the thead element
        private static string EditHeader(IEnumerable<string> extraColumns)
        {
            // Generate the cells
            StringBuilder html = new StringBuilder();
            string fileNameHeader = HeaderCell("File name");
            
            html.Append(fileNameHeader);
            foreach(string heading in extraColumns)
            {
                string headerCell = HeaderCell(heading);
                html.Append(headerCell);
            }
            string fileSizeHeader = HeaderCell("File size");
            string buttonHeader = HeaderCell(string.Empty, "button-header-cell");
            string hiddenHeader = HeaderCell(string.Empty, "hidden-header-cell");
            html.Append(fileSizeHeader);
            html.Append(buttonHeader);
            html.Append(hiddenHeader);
            // Generate the row
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = html.ToString();
            // Generate the header
            TagBuilder head = new TagBuilder("thead");
            head.InnerHtml = row.ToString();
            return head.ToString();
        }

        // Generates the visible tbody element containing details of existing attachments
        private static string EditBody(HtmlHelper helper, Type modelType, IEnumerable<IFileAttachment> attachments, string propertyName, IEnumerable<string> extraProperties, Dictionary<string, object> optionLists)
        {
            StringBuilder html = new StringBuilder();
            int rowNumber = 0;
            foreach (IFileAttachment attachment in attachments)
            {
                string tableRow = EditRow(helper, modelType, attachment, propertyName, rowNumber, extraProperties, optionLists);
                html.Append(tableRow);

                string validationRow = ValidationRow(helper, propertyName, rowNumber.ToString(), extraProperties);
                html.Append(validationRow);

                rowNumber++;
            }
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            return body.ToString();
        }

        // Generates the row for editing data in the visible tbody element
        private static string EditRow(HtmlHelper helper, Type modelType, IFileAttachment attachment, string propertyName, int index, IEnumerable<string> extraColumns, Dictionary<string, object> optionLists)
        {
            // Get the ModelMetadata for the attachment
            ModelMetadata itemMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => attachment, modelType);
            // Generate table cells
            StringBuilder html = new StringBuilder();
            string displayName = TableCell(attachment.DisplayName);
            html.Append(displayName);
            string prefix = String.Format("{0}[{1}]", propertyName, index);
            string formControls = EditRowControlCells(helper, itemMetadata, prefix, extraColumns, optionLists);
            html.Append(formControls);
            string size = String.Format("{0} kB", attachment.Size);
            if (attachment.Size >= 1024)
            {
                size = String.Format("{0:0.00} MB", attachment.Size / 1024F);
            }
            string fileSize = TableCell(size);
            string button = ButtonCell(ButtonType.Delete);
            string inputs = EditRowHiddenInputs(attachment, propertyName, index);
            html.Append(fileSize);
            html.Append(button);
            html.Append(inputs);
            // Generate table row
            TagBuilder row = new TagBuilder("tr");
            row.AddCssClass("edit-row");
            if (attachment.Status == FileAttachmentStatus.Deleted)
            {
                row.AddCssClass("archived");
            }
            row.InnerHtml = html.ToString();
            return row.ToString();
        }

        // Generates the editable form controls in a tr element
        private static string EditRowControlCells(HtmlHelper helper, ModelMetadata fileMetatdata, string prefix, IEnumerable<string> extraColumns, Dictionary<string, object> optionLists)
        {
            StringBuilder html = new StringBuilder();
            foreach (string column in extraColumns)
            {
                // TODO: Add data list
                ModelMetadata metaData = fileMetatdata.Properties.FirstOrDefault(x => x.PropertyName == column);
                string name = String.Format("{0}.{1}", prefix, column);
                Type type = metaData.ModelType;
                if (Nullable.GetUnderlyingType(type) != null)
                {
                    type = Nullable.GetUnderlyingType(type);
                }
                if (metaData.DataTypeName == "MultilineText")
                {
                    string textAreaCell = TextAreaCell(helper, name, metaData);
                    html.Append(textAreaCell);
                }
                else if (metaData.ModelType == typeof(bool))
                {
                    string checkBoxCell = CheckboxCell(helper, name, metaData);
                    html.Append(checkBoxCell);
                }
                else if (metaData.ModelType == typeof(bool?))
                {
                    bool? defaultValue = (bool?)metaData.Model;
                    List<SelectListItem> selectList = new List<SelectListItem>()
                    {
                        new SelectListItem(){ Value = "", Text = "", Selected = !defaultValue.HasValue }, // TODO: Text from Resource File
                        new SelectListItem(){ Value = "true", Text = "Yes", Selected = defaultValue.HasValue && defaultValue.Value },
                        new SelectListItem(){ Value = "false", Text = "No", Selected = defaultValue.HasValue && !defaultValue.Value }
                    };
                    string selectCell = SelectCell(helper, name, metaData, selectList);
                    html.Append(selectCell);
                }
                else if (type.IsEnum)
                {
                    string defaultValue = Convert.ToString(metaData.Model);
                    List<SelectListItem> selectList = new List<SelectListItem>();
                    var nullText = metaData.NullDisplayText;
                    selectList.Add(new SelectListItem() { Value = "", Text = nullText });
                    foreach (var item in Enum.GetNames(type))
                    {
                        selectList.Add(new SelectListItem() { Value = item, Text = item, Selected = (item == defaultValue) });
                    }
                    string selectCell = SelectCell(helper, name, metaData, selectList);
                    html.Append(selectCell);
                }
                else if (metaData.AdditionalValues.ContainsKey(_SelectListKey))
                {
                    IEnumerable<SelectListItem> options = optionLists[column] as IEnumerable<SelectListItem>;
                    List<SelectListItem> selectList = new List<SelectListItem>();
                    string defaultValue = Convert.ToString(metaData.Model);
                    foreach (SelectListItem item in options)
                    {
                        item.Selected = (item.Value != null) ? item.Value == defaultValue : item.Text == defaultValue;
                        selectList.Add(item);
                    }
                    string selectCell = SelectCell(helper, name, metaData, selectList);
                    html.Append(selectCell);
                }
                else if (metaData.AdditionalValues.ContainsKey(_DataListKey))
                {
                    IEnumerable<string> options = optionLists[column] as IEnumerable<string>;
                    string dataListCell = DataListCell(helper, name, metaData, options);
                    html.Append(dataListCell);
                }
                else
                {
                    string textBoxCell = TextBoxCell(helper, name, metaData);
                    html.Append(textBoxCell);
                }
            }
            return html.ToString();
        }

        // Generates the cell containing the hidden inputs for binding
        private static string EditRowHiddenInputs(IFileAttachment attachment, string propertyName, int index)
        {
            StringBuilder html = new StringBuilder();
            // Generate the input for the collection indexer
            TagBuilder indexer = new TagBuilder("input");
            indexer.MergeAttribute("type", "hidden");
            indexer.MergeAttribute("value", index.ToString());
            indexer.MergeAttribute("name", string.Format("{0}.Index", propertyName));
            html.Append(indexer.ToString());
            // Generate the input for the file ID
            TagBuilder id = new TagBuilder("input");
            id.MergeAttribute("value", attachment == null ? string.Empty : attachment.ID.ToString());
            id.MergeAttribute("type", "hidden");
            id.MergeAttribute("name", string.Format("{0}[{1}].ID", propertyName, index));
            html.Append(id.ToString());
            // Generate the input for the file name and path
            TagBuilder fileName = new TagBuilder("input");
            fileName.MergeAttribute("value", attachment == null ? string.Empty : attachment.VirtualPath);
            fileName.MergeAttribute("type", "hidden");
            fileName.MergeAttribute("name", string.Format("{0}[{1}].FilePath", propertyName, index));
            html.Append(fileName.ToString());
            // Generate the input for the files display name
            TagBuilder displayName = new TagBuilder("input");
            displayName.MergeAttribute("value", attachment == null ? string.Empty : attachment.DisplayName);
            displayName.MergeAttribute("type", "hidden");
            displayName.MergeAttribute("name", string.Format("{0}[{1}].DisplayName", propertyName, index));
            html.Append(displayName.ToString());
            // Generate the input for the file size
            TagBuilder size = new TagBuilder("input");
            size.MergeAttribute("value", attachment == null ? string.Empty : attachment.Size.ToString());
            size.MergeAttribute("type", "hidden");
            size.MergeAttribute("name", string.Format("{0}[{1}].Size", propertyName, index));
            html.Append(size.ToString());
            // Generate the input for the file status
            TagBuilder status = new TagBuilder("input");
            status.AddCssClass("file-status");
            status.MergeAttribute("value", attachment == null ? string.Empty : ((int)attachment.Status).ToString());
            status.MergeAttribute("type", "hidden");
            status.MergeAttribute("name", string.Format("{0}[{1}].Status", propertyName, index));
            html.Append(status.ToString());
            // Generate the table cell
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = html.ToString();
            // Return the html
            return cell.ToString();
        }

        // Generates the alternate row containing validation messages for form controls
        private static string ValidationRow(HtmlHelper helper, string propertyName, string index, IEnumerable<string> extraColumns)
        {
            string prefix = String.Format("{0}[{1}]", propertyName, index);
            TagBuilder emptyCell = new TagBuilder("td");
            // Build the html
            StringBuilder html = new StringBuilder();
            html.Append(emptyCell.ToString()); // file name
            // Add validation cells
            string validationCells = ValidationCells(helper, prefix, extraColumns);
            html.Append(validationCells);
            html.Append(emptyCell.ToString()); // file size
            TagBuilder buttonCell = new TagBuilder("td");
            buttonCell.AddCssClass("button-cell");
            html.Append(buttonCell.ToString()); // buttons
            html.Append(emptyCell.ToString()); // hidden inputs
            TagBuilder row = new TagBuilder("tr");
            row.AddCssClass("validation-row");
            row.InnerHtml = html.ToString();
            return row.ToString();
        }

        // Generates the validation message cells associated with form controls
        private static string ValidationCells(HtmlHelper helper, string prefix, IEnumerable<string> extraColumns)
        {
            StringBuilder html = new StringBuilder();
            foreach (string column in extraColumns)
            {
                string name = String.Format("{0}.{1}", prefix, column);
                MvcHtmlString validation = helper.ValidationMessage(name);
                TagBuilder cell = new TagBuilder("td");
                cell.InnerHtml = validation.ToString();
                html.Append(cell.ToString());
            }
            return html.ToString();
        }

        // Generates the hidden tbody element that is cloned when adding new files
        private static string HiddenBody(HtmlHelper helper, ModelMetadata itemMetadata, string propertyName, IEnumerable<string> extraColumns, Dictionary<string, object> optionLists)
        {
            StringBuilder html = new StringBuilder();

            string editRow = HiddenRow(helper, itemMetadata, propertyName, extraColumns, optionLists);
            string validationRow = ValidationRow(helper, propertyName, "#", extraColumns);

            html.Append(editRow);
            html.Append(validationRow);


            // Generate the table body
            TagBuilder body = new TagBuilder("tbody");
            body.MergeAttribute("style", "display:none;");
            body.InnerHtml = html.ToString();
            // Return the html
            return body.ToString();


            //return null;
        }

        // Generates the hidden row that is cloned when adding new files
        private static string HiddenRow(HtmlHelper helper, ModelMetadata itemMetadata, string propertyName, IEnumerable<string> extraColumns, Dictionary<string, object> optionLists)
        {
            // Generate table cells
            StringBuilder html = new StringBuilder();
            string cell = TableCell(string.Empty);
            html.Append(cell);
            string prefix = String.Format("{0}[#]", propertyName);
            string formControls = EditRowControlCells(helper, itemMetadata, prefix, extraColumns, optionLists);
            html.Append(formControls);
            html.Append(cell);
            string button = ButtonCell(ButtonType.Delete);
            string input = HiddenRowInputs(propertyName);
            html.Append(button);
            html.Append(input);
            // Generate the table row
            TagBuilder row = new TagBuilder("tr");
            row.AddCssClass("edit-row");
            row.InnerHtml = html.ToString();
            // Return the html
            return row.ToString();




            //// Generate the table body
            //TagBuilder body = new TagBuilder("tbody");
            //body.MergeAttribute("style", "display:none;");
            //body.InnerHtml = row.ToString();
            //// Return the html
            //return body.ToString();
        }

        // Generates the cell containing the input for the collection indexer
        private static string HiddenRowInputs(string propertyName)
        {
            TagBuilder indexer = new TagBuilder("input");
            indexer.MergeAttribute("type", "hidden");
            indexer.MergeAttribute("name", string.Format("{0}.Index", propertyName));
            indexer.MergeAttribute("value", "#");
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = indexer.ToString();
            return cell.ToString();
        }

        // Generates the tfoot element
        private static string EditFooter(string propertyName, int extraProperties)
        {
            // Generate the cells
            StringBuilder html = new StringBuilder();
            string cell = FooterCell(string.Empty);
            string button = ButtonCell(ButtonType.Add);
            string inputs = FooterRowInputs(propertyName);
            html.Append(cell);
            html.Append(cell);
            for (int i = 0; i < extraProperties; i++)
            {
                html.Append(cell);
            }
            html.Append(button);
            html.Append(inputs);
            // Generate the row
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = html.ToString();
            // Generate the header
            TagBuilder footer = new TagBuilder("tfoot");
            footer.InnerHtml = row.ToString();
            return footer.ToString();
        }

        // Generates the cell containing the file input for selecting a file
        private static string FooterRowInputs(string propertyName)
        {
            TagBuilder fileInput = new TagBuilder("input");
            fileInput.MergeAttribute("type", "file");
            fileInput.MergeAttribute("name", string.Format("{0}[#].File", propertyName));
            fileInput.MergeAttribute("style", "display:none");
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = fileInput.ToString();
            return cell.ToString();
        }

        // Generates the thead element
        private static string DisplayHeader(IEnumerable<string> extraColumns)
        {
            // Generate the cells
            StringBuilder html = new StringBuilder();
            string fileNameHeader = HeaderCell("File name");
            html.Append(fileNameHeader);
            foreach (string heading in extraColumns)
            {
                string headerCell = HeaderCell(heading);
                html.Append(headerCell);
            }
            string fileSizeHeader = HeaderCell("File size");
            html.Append(fileSizeHeader);
            // Generate the row
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = html.ToString();
            // Generate the header
            TagBuilder head = new TagBuilder("thead");
            head.InnerHtml = row.ToString();
            return head.ToString();
        }

        // Generates the tbody element
        private static string DisplayBody(HtmlHelper helper, Type modelType, IEnumerable<IFileAttachment> attachments, IEnumerable<string> extraColumns, string actionName, string controllerName)
        {
            StringBuilder html = new StringBuilder();
            // Add table rows
            foreach (IFileAttachment attachment in attachments)
            {
                html.Append(DisplayRow(helper, modelType, attachment, extraColumns, actionName, controllerName));
            }
            // Generate tbody
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            return body.ToString();
        }

        // Generates a row in the tbody element
        private static string DisplayRow(HtmlHelper helper, Type modelType, IFileAttachment attachment, IEnumerable<string> extraColumns, string actionName, string controllerName)
        {
            // Get the ModelMetadata for the attachment
            ModelMetadata itemMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => attachment, modelType);

            StringBuilder html = new StringBuilder();
            // Generate link
            object routeValues = new { ID = attachment.ID.Value };
            object htmlAttributes = new { target = "_blank" };
            var link = helper.ActionLink(attachment.DisplayName, actionName, controllerName, routeValues, htmlAttributes);
            // Add table cells
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = link.ToString();
            html.Append(cell.ToString());
            foreach (string propertyName in extraColumns)
            {
                ModelMetadata metaData = itemMetadata.Properties.FirstOrDefault(x => x.PropertyName == propertyName);
                if (metaData.Model == null)
                {
                    cell.InnerHtml = metaData.NullDisplayText;
                }
                else if (metaData.ModelType == typeof(bool) || metaData.ModelType == typeof(Nullable<bool>))
                {
                    cell.InnerHtml = (bool)metaData.Model ? "Yes" : "No";
                }
                //else if (metaData.AdditionalValues.ContainsKey(_SelectListKey))
                //{
                //    // This would only work if we get the IEnumerable<SelectListItem> and find the corresponding text
                      // Would be better to use a separate view model for display vd edit
                //}
                else
                {
                    string formatString = metaData.DisplayFormatString ?? "{0}";
                    cell.InnerHtml = String.Format(formatString, metaData.Model);
                }
                html.Append(cell.ToString());
            }
            cell.InnerHtml = string.Format("{0} KB", attachment.Size);
            html.Append(cell.ToString());
            // Generate table row
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = html.ToString();
            return row.ToString();
        }

        // Geneates a th element
        private static string HeaderCell(string text, string className = null)
        {
            TagBuilder cell = new TagBuilder("th");
            if (className != null)
            {
                cell.AddCssClass(className);
            }
            cell.InnerHtml = text;
            return cell.ToString();
        }

        // Geneates a td element
        private static string TableCell(string text)
        {
            TagBuilder div = new TagBuilder("div");
            div.AddCssClass("table-text");
            div.InnerHtml = text;
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = div.ToString();
            return cell.ToString();
        }

        // Generates a td element containing a textbox
        private static string TextBoxCell(HtmlHelper helper, string name, ModelMetadata metadata)
        {
            // Build html attributes
            IDictionary<string, object> htmlAttributes = helper.GetUnobtrusiveValidationAttributes(name, metadata);
            htmlAttributes.Add("id", null);
            htmlAttributes.Add("class", "table-control");
            // Build html
            MvcHtmlString textBox = helper.TextBox(name, metadata.Model, htmlAttributes);
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = textBox.ToString();
            return cell.ToString();
        }

        // Generates a td element containing a textarea
        private static string TextAreaCell(HtmlHelper helper, string name, ModelMetadata metadata)
        {
            // Build html attributes
            IDictionary<string, object> htmlAttributes = helper.GetUnobtrusiveValidationAttributes(name, metadata);
            htmlAttributes.Add("id", null);
            htmlAttributes.Add("class", "table-control");
            // Build html
            MvcHtmlString textArea = helper.TextArea(name, (metadata.Model ?? string.Empty).ToString(), htmlAttributes);
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = textArea.ToString();
            return cell.ToString();
        }

        // Generates a td element containing a checkbox
        private static string CheckboxCell(HtmlHelper helper, string name, ModelMetadata metadata)
        {
            // Build html attributes
            IDictionary<string, object> htmlAttributes = helper.GetUnobtrusiveValidationAttributes(name, metadata);
            htmlAttributes.Add("id", null);
            htmlAttributes.Add("class", "table-control");
            // Build html
            MvcHtmlString checkbox = helper.CheckBox(name, (bool)metadata.Model, htmlAttributes);
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = checkbox.ToString();
            return cell.ToString();
        }

        // Generates a td element containing a select
        private static string SelectCell(HtmlHelper helper, string name, ModelMetadata metadata, IEnumerable<SelectListItem> selectList)
        {
            // Build html attributes
            IDictionary<string, object> htmlAttributes = helper.GetUnobtrusiveValidationAttributes(name, metadata);
            htmlAttributes.Add("id", null);
            htmlAttributes.Add("class", "table-control");
            // Build html
            MvcHtmlString select = helper.DropDownList(name, selectList, htmlAttributes);
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = select.ToString();
            return cell.ToString();
        }

        // Generates a td element containing an input with a datalist
        private static string DataListCell(HtmlHelper helper, string name, ModelMetadata metadata, IEnumerable<string> options)
        {
            string id = String.Format("{0}-datalist", name.Split('.').Last()).ToLower();
            // Build html attributes
            IDictionary<string, object> htmlAttributes = helper.GetUnobtrusiveValidationAttributes(name, metadata);
            htmlAttributes.Add("id", null);
            htmlAttributes.Add("class", "table-control");
            htmlAttributes.Add("list", id);
            MvcHtmlString textBox = helper.TextBox(name, metadata.Model, htmlAttributes);
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = textBox.ToString();
            return cell.ToString();
        }

        // Generates a datalist element
        private static string DataList(string id, IEnumerable<string> options)
        {
            if (options == null)
            {
                return null;
            }
            StringBuilder html = new StringBuilder();
            foreach (string item in options)
            {
                TagBuilder option = new TagBuilder("option");
                option.MergeAttribute("value", item);
                html.Append(option.ToString(TagRenderMode.StartTag));
            }
            TagBuilder dataList = new TagBuilder("datalist");
            dataList.MergeAttribute("id", id);
            dataList.InnerHtml = html.ToString();
            return dataList.ToString();
        }

        // Geneates a td element
        private static string FooterCell(string text)
        {
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = text;
            return cell.ToString();
        }

        // Geneates a td element containing a button to add and delete table rows
        private static string ButtonCell(ButtonType type)
        {
            TagBuilder button = new TagBuilder("button");
            button.AddCssClass("table-button");
            button.AddCssClass(string.Format("{0}-button", type.ToString().ToLower()));
            button.MergeAttribute("type", "button");
            TagBuilder cell = new TagBuilder("td");
            cell.AddCssClass("button-cell");
            cell.InnerHtml = button.ToString();
            return cell.ToString();
        }

        #endregion

    }

}