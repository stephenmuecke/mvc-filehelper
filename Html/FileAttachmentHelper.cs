using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Sandtrap.Web.Models;

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
        /// Returns the html to display file properties and links.
        /// </summary>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to render.
        /// </param>
        public static MvcHtmlString FileAttachmentDisplayFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<IFileAttachment>>> expression, string downloadActionName = "DownloadAttachment")
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
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            StringBuilder html = new StringBuilder();
            html.Append(DisplayHeader());
            html.Append(DisplayBody(helper, attachments, downloadActionName));
            TagBuilder table = new TagBuilder("table");
            table.AddCssClass("readonly-table");
            table.AddCssClass("file-attachments");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(fieldName));
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
            StringBuilder html = new StringBuilder();
            html.Append(EditHeader());
            if (attachments != null)
            {
                html.Append(EditBody(attachments, propertyName));
            }
            html.Append(HiddenRow(propertyName));
            html.Append(EditFooter(propertyName));
            TagBuilder table = new TagBuilder("table");
            table.AddCssClass("edit-table");
            table.AddCssClass("file-attachments");
            table.MergeAttribute("id", HtmlHelper.GenerateIdFromName(propertyName));
            table.InnerHtml = html.ToString();
            return MvcHtmlString.Create(table.ToString());
        }

        #endregion

        #region .Helper methods 

        // Generates the thead element
        public static string EditHeader()
        {
            // Generate the cells
            StringBuilder html = new StringBuilder();
            string fileNameHeader = HeaderCell("File name");
            string fileSizeHeader = HeaderCell("File size");
            string buttonHeader = HeaderCell(string.Empty, "button-header-cell");
            string hiddenHeader = HeaderCell(string.Empty, "hidden-header-cell");
            html.Append(fileNameHeader);
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
        public static string EditBody(IEnumerable<IFileAttachment> attachments, string propertyName)
        {
            StringBuilder html = new StringBuilder();
            int rowNumber = 0;
            foreach (IFileAttachment attachment in attachments)
            {
                string tableRow = EditRow(attachment, propertyName, rowNumber);
                html.Append(tableRow);
                rowNumber++;
            }
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            return body.ToString();
        }

        // Generates a row in the visible tbody element
        public static string EditRow(IFileAttachment attachment, string prefix, int index)
        {
            // Generate table cells
            StringBuilder html = new StringBuilder();
            string displayName = TableCell(attachment.DisplayName);
            string fileSize = TableCell(string.Format("{0} KB", attachment.Size));
            string button = ButtonCell(ButtonType.Delete);
            string inputs = EditRowInputs(attachment, prefix, index);
            html.Append(displayName);
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
        public static string EditRowInputs(IFileAttachment attachment, string propertyName, int index)
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
        public static string EditFooter(string propertyName)
        {
            // Generate the cells
            StringBuilder html = new StringBuilder();
            string cell = FooterCell(string.Empty);
            string button = ButtonCell(ButtonType.Add);
            string inputs = FooterRowInputs(propertyName);
            html.Append(cell);
            html.Append(cell);
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
        public static string FooterRowInputs(string propertyName)
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
        public static string HiddenRow(string propertyName)
        {
            // Generate table cells
            StringBuilder html = new StringBuilder();
            string cell = TableCell(string.Empty);
            string button = ButtonCell(ButtonType.Delete);
            string input = HiddenRowInputs(propertyName);
            html.Append(cell);
            html.Append(cell);
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
        public static string HiddenRowInputs(string propertyName)
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
        public static string DisplayHeader()
        {
            // Generate the cells
            StringBuilder html = new StringBuilder();
            string fileNameHeader = HeaderCell("File name");
            string fileSizeHeader = HeaderCell("File size");
            html.Append(fileNameHeader);
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
        public static string DisplayBody(HtmlHelper helper, IEnumerable<IFileAttachment> attachments, string downloadActionName)
        {
            StringBuilder html = new StringBuilder();
            // Add table rows
            foreach (IFileAttachment attachment in attachments)
            {
                html.Append(DisplayRow(helper, attachment, downloadActionName));
            }
            // Generate tbody
            TagBuilder body = new TagBuilder("tbody");
            body.InnerHtml = html.ToString();
            return body.ToString();
        }

        // Generates a row in the tbody element
        public static string DisplayRow(HtmlHelper helper, IFileAttachment attachment, string downloadActionName)
        {
            StringBuilder html = new StringBuilder();
            // Generate link
            object routeValues = new { ID = attachment.ID.Value };
            object htmlAttributes = new { target = "_blank" };
            var link = helper.ActionLink(attachment.DisplayName, downloadActionName, routeValues, htmlAttributes);
            // Add table cells
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = link.ToString();
            html.Append(cell.ToString());
            cell.InnerHtml = string.Format("{0} KB", attachment.Size);
            html.Append(cell.ToString());
            // Generate table row
            TagBuilder row = new TagBuilder("tr");
            row.InnerHtml = html.ToString();
            return row.ToString();
        }

        // Geneates a th element
        public static string HeaderCell(string text, string className = null)
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
        public static string TableCell(string text)
        {
            TagBuilder div = new TagBuilder("div");
            div.AddCssClass("table-text");
            div.InnerHtml = text;
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = div.ToString();
            return cell.ToString();
        }

        // Geneates a td element
        public static string FooterCell(string text)
        {
            TagBuilder cell = new TagBuilder("td");
            cell.InnerHtml = text;
            return cell.ToString();

        }

        // Geneates a td element containing a button to add and delete table rows
        public static string ButtonCell(ButtonType type)
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
