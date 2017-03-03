using System.Web;

namespace Sandtrap.Web.Models
{

    /// <summary>
    /// Describes the status of a file.
    /// </summary>
    public enum FileAttachmentStatus
    {
        Added = 1,
        Deleted = -1
    }

    /// <summary>
    /// Describes the properties of a file.
    /// </summary>
    public interface IFileAttachment
    {
        /// <summary>
        /// Gets or sets the ID of the file.
        /// </summary>
        int? ID { get; set; }
        /// <summary>
        /// Gets of sets the name.
        /// </summary>
        string FileName { get; set; }
        /// <summary>
        /// Gets or sets the file directory.
        /// </summary>
        string Directory { get; set; }
        /// <summary>
        /// Gets or sets the file display name
        /// </summary>
        string DisplayName { get; set; }
        /// <summary>
        /// Gets of sets the file size (in bytes).
        /// </summary>
        int? Size { get; set; }
        /// <summary>
        /// Gest or sets the file status.
        /// </summary>
        FileAttachmentStatus Status { get; set; }
        /// <summary>
        /// Gets or sets the file uploaded by the client.
        /// </summary>
        HttpPostedFileBase File { get; set; }
    }

    /// <summary>
    /// Sample concrete implementation of IFileAttachment.
    /// </summary>
    public class FileAttachment : IFileAttachment
    {
        public int? ID { get; set; }
        public string FileName { get; set; }
        public string Directory { get; set; }
        public string DisplayName { get; set; }
        public int? Size { get; set; }
        public FileAttachmentStatus Status { get; set; }
        public HttpPostedFileBase File { get; set; }
    }

}
