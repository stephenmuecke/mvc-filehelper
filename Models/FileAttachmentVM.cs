using System.Web;

namespace Sandtrap.Web.Models
{

    /// <summary>
    /// Sample concrete implementation of IFileAttachment.
    /// </summary>
    public class FileAttachment : IFileAttachment
    {
        public int? ID { get; set; }
        public string DisplayName { get; set; }
        public string VirtualPath { get; set; }
        public int? Size { get; set; }
        public FileAttachmentStatus Status { get; set; }
        public HttpPostedFileBase File { get; set; }
    }

}
