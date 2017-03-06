using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Sandtrap.Web.Models;

namespace Sandtrap.Web.Utilities
{

    /// <summary>
    /// Utility methods for saving, deleting and downloading files on the server.
    /// </summary>
    public static class FileHelper
    {

        #region .Declarations 

        private const string nullAttachment = "The attachment cannot be null.";
        private const string nullPath = "The file path cannot be null.";

        #endregion

        #region .Methods 

        /// <summary>
        /// Saves a files to the server.
        /// </summary>
        /// <param name="attachments">
        /// A collection of `IFileAttachment` objects.
        /// </param>
        /// <param name="virtualPath">
        /// The virtual path to save the files.
        /// </param>
        /// <param name="deleteFiles">
        /// A value indicating if files marked for deletion should be deleted from the server.
        /// </param>
        public static void Save(IEnumerable<IFileAttachment> attachments, string virtualPath, bool deleteFiles)
        {
            if (attachments == null)
            {
                return;
            }
            // Get the physical path
            string physicalPath = HttpContext.Current.Server.MapPath(virtualPath);
            // string physicalPath = HostingEnvironment.MapPath(path);
            if (!Directory.Exists(physicalPath))
            {
                Directory.CreateDirectory(physicalPath);
            }
            foreach (IFileAttachment attachment in attachments)
            {
                if (attachment.File != null && attachment.File.ContentLength > 0)
                {
                    // Save the file
                    HttpPostedFileBase file = attachment.File;
                    string fileName = file.FileName;
                    string extension = Path.GetExtension(fileName);
                    // Create unique file name
                    fileName = string.Format("{0}{1}", Guid.NewGuid(), extension);
                    // Update properties
                    attachment.DisplayName = file.FileName;
                    attachment.VirtualPath = Path.Combine(physicalPath, fileName);
                    attachment.Size = (int)Math.Round(file.ContentLength / 1024F);
                    attachment.Status = FileAttachmentStatus.Added;
                    // Save to server
                    file.SaveAs(Path.Combine(physicalPath, fileName));
                }
                else if (attachment.Status == FileAttachmentStatus.Deleted && deleteFiles)
                {
                    // Delete the file from the server
                    File.Delete(HttpContext.Current.Server.MapPath(attachment.VirtualPath));
                }
            }
        }

        /// <summary>
        /// Returns a FileResult to send the contents of a file to the response.
        /// </summary>
        /// <param name="attachment">
        /// An IFileAttachment containing the properties of the file to download.
        /// </param>
        public static FileResult Download(IFileAttachment attachment)
        {
            // Validate
            if (attachment.VirtualPath == null)
            {
                throw new ArgumentNullException(nullPath);
            }
            if (attachment.DisplayName == null)
            {
                attachment.DisplayName = Path.GetFileName(attachment.VirtualPath);
            }
            // Return FileResult
            return Download(attachment.VirtualPath, attachment.DisplayName);
        }

        /// <summary>
        /// Returns a FileResult to send the contents of a file to the response.
        /// </summary>
        /// <param name="path">
        /// The virtual path of the file.
        /// </param>
        /// <param name="displayName">
        /// The files display name to be returned to the browser.
        /// </param>
        public static FileResult Download(string path, string displayName)
        {
            // Validate
            if (attachment == null)
            {
                throw new ArgumentNullException(nullAttachment);
            }
            if (path == null)
            {
                throw new ArgumentNullException(nullPath);
            }
            if (displayName == null)
            {
                displayName = Path.GetFileName(path);
            }
            // Get the physical path
            string physicalPath = HttpContext.Current.Server.MapPath(path);
            if (!File.Exists(physicalPath))
            {
                throw new FileNotFoundException(); // TODO: error message
            }
            // Get the files mime type
            string mineType = MimeMapping.GetMimeMapping(path);
            // Return FileResult
            return new FilePathResult(physicalPath, mineType);
        }

        #endregion

    }

}
