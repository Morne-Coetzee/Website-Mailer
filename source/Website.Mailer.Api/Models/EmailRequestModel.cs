using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Website.Mailer.Api.Models
{
    public class EmailRequestModel
    {
        [Required]
        public string[] To { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public string AttachmentsJson { get; set; }

        [JsonIgnore]
        public List<(byte[] Data, string MimeType, string FileName)> Attachments
        {
            get
            {
                if (string.IsNullOrEmpty(AttachmentsJson))
                {
                    return new List<(byte[] Data, string MimeType, string FileName)>();
                }
                return JsonConvert.DeserializeObject<List<(byte[] Data, string MimeType, string FileName)>>(AttachmentsJson);
            }
        }
    }
}
