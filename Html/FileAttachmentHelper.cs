using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Sandtrap.Web.Models;

using System.Linq;

namespace Sandtrap.Web.Html
{

    /// <summary>
    /// Renders the html for a collection of files.
    /// </summary>
    public static class FileAttachmentHelper
    {

        #region .Declarations 

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
            // Get properties included in the model but not the IFileAttachment
            IEnumerable<ModelMetadata> extraProperties = GetExtraProperties(modelType);
            // Build html
            StringBuilder html = new StringBuilder();
            IEnumerable<string> extraColumns = extraProperties.Select(x => x.GetDisplayName());
            html.Append(EditHeader(extraColumns));
            extraColumns = extraProperties.Select(x => x.PropertyName);
            if (attachments == null)
            {
                TagBuilder tbody = new TagBuilder("tbody");
                html.Append(tbody.ToString());
            }
            else
            {
                html.Append(EditBody(helper, modelType, attachments, propertyName, extraColumns));
            }
            html.Append(HiddenRow(helper, modelType, propertyName, extraColumns));
            html.Append(EditFooter(propertyName, extraProperties.Count()));
            TagBuilder table = new TagBuilder("table");
            table.AddCssClass("edit-table");
            table.AddCssClass("file-attachments");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(propertyName));
            table.InnerHtml = html.ToString();
            return MvcHtmlString.Create(table.ToString());
        }

        #endregion

        #region .Helper methods 

        private static IEnumerable<ModelMetadata> GetExtraProperties(Type type)
        {
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, type);
            // Get properties not included in IFileAttachment
            IEnumerable<string> intefaceNames = typeof(IFileAttachment).GetProperties().Select(x => x.Name);
            return metadata.Properties.Where(x => !intefaceNames.Contains(x.PropertyName));
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
        private static string EditBody(HtmlHelper helper, Type modelType, IEnumerable<IFileAttachment> attachments, string propertyName, IEnumerable<string> extraProperties)
        {
            StringBuilder html = new StringBuilder();
            int rowNumber = 0;
            foreach (IFileAttachment attachment in attachments)
            {
                string tableRow = EditRow(helper, modelType, attachment, propertyName, rowNumber, extraProperties);
                html.Append(tableRow);
                rowNumber++;
            }
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            return body.ToString();
        }

        // Generates a row in the visible tbody element
        private static string EditRow(HtmlHelper helper, Type modelType, IFileAttachment attachment, string propertyName, int index, IEnumerable<string> extraColumns)
        {
            // Get the ModelMetadata for the attachment
            ModelMetadata itemMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => attachment, modelType);
            // Generate table cells
            StringBuilder html = new StringBuilder();
            string displayName = TableCell(attachment.DisplayName);
            html.Append(displayName);
            string prefix = String.Format("{0}[{1}]", propertyName, index);
            string formControls = EditableFormControls(helper, itemMetadata, prefix, extraColumns);
            html.Append(formControls);
            string fileSize = TableCell(string.Format("{0} KB", attachment.Size));
            string button = ButtonCell(ButtonType.Delete);
            string inputs = EditRowInputs(attachment, propertyName, index);
            html.Append(fileSize);
            html.Append(button);
            html.Append(inputs);
            // Generate table row
            TagBuilder row = new TagBuilder("tr");
            if (attachment.Status == FileAttachmentStatus.Deleted)
            {
                row.AddCssClass("archived");
            }
            row.InnerHtml = html.ToString();
            return row.ToString();
        }

        // Generates the cell containing the inputs for binding
        private static string EditRowInputs(IFileAttachment attachment, string propertyName, int index)
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

        // Generates the hidden tbody element that is cloned when adding new files
        private static string HiddenRow(HtmlHelper helper, Type modelType, string propertyName, IEnumerable<string> extraColumns)
        {
            // Create an instance of the type so default values are rendered
            object instance = Activator.CreateInstance(modelType);
            ModelMetadata itemMetadata = ModelMetadataProviders.Current
                .GetMetadataForType(() => instance, modelType);
            // Generate table cells
            StringBuilder html = new StringBuilder();
            string cell = TableCell(string.Empty);
            html.Append(cell);
            string prefix = String.Format("{0}[#]", propertyName);
            string formControls = EditableFormControls(helper, itemMetadata, prefix, extraColumns);
            html.Append(formControls);
            html.Append(cell);
            string button = ButtonCell(ButtonType.Delete);
            string input = HiddenRowInputs(propertyName);
            html.Append(button);
            html.Append(input);
            // Generate the table row
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = html.ToString();
            // Generate the table body
            TagBuilder body = new TagBuilder("tbody");
            body.MergeAttribute("style", "display:none;");
            body.InnerHtml = row.ToString();
            // Return the html
            return body.ToString();
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
                else if (metaData.ModelType == typeof(Nullable<bool>) || metaData.ModelType == typeof(bool))
                {
                    cell.InnerHtml = (bool)metaData.Model ? "Yes" : "No";
                }
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

        // Generates the editable form controls in a tr element
        private static string EditableFormControls(HtmlHelper helper, ModelMetadata fileMetatdata, string prefix, IEnumerable<string> extraColumns)
        {
            StringBuilder html = new StringBuilder();
            foreach (string column in extraColumns)
            {
                ModelMetadata metaData = fileMetatdata.Properties.FirstOrDefault(x => x.PropertyName == column);
                string name = String.Format("{0}.{1}", prefix, column);
                object value = metaData.Model;
                // TODO: If ModelType is Nullable<bool>, generate select
                // TODO: If [Select] attribute, generate select
                if (metaData.DataTypeName == "MultilineText")
                {
                    string textAreaCell = TextAreaCell(helper, name, value);
                    html.Append(textAreaCell);
                }
                else if (metaData.ModelType == typeof(bool))
                {
                    string checkBoxCell = CheckboxCell(helper, name, (bool)metaData.Model);
                    html.Append(checkBoxCell);
                }
                else
                {
                    string textBoxCell = TextBoxCell(helper, name, value);
                    html.Append(textBoxCell);
                }
            }
            return html.ToString();
        }

        // Generates a td element containing a textbox
        private static string TextBoxCell(HtmlHelper helper, string name, object value)
        {
            // TODO: Add ValidationMessageFor()
            var editor = helper.TextBox(name, value, new { id = "", @class = "table-control" });
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = editor.ToString();
            return cell.ToString();
        }

        // Generates a td element containing a textarea
        private static string TextAreaCell(HtmlHelper helper, string name, object value)
        {
            // TODO: Add ValidationMessageFor()
            MvcHtmlString editor = helper.TextArea(name, (value ?? string.Empty).ToString(), new { id = "", @class = "table-control" });
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = editor.ToString();
            return cell.ToString();
        }

        // Generates a td element containing a checkbox
        private static string CheckboxCell(HtmlHelper helper, string name, bool isChecked)
        {
            // TODO: Add ValidationMessageFor()
            MvcHtmlString editor = helper.CheckBox(name, isChecked, new { id = "", @class = "table-control" });
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = editor.ToString();
            return cell.ToString();
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