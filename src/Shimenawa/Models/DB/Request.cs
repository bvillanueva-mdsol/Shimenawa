using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Medidata.Shimenawa.Models.DB
{
    public class Request
    {
        [Key]
        public Guid RequestUuid { get; set; }
        
        [Required]
        public string Query { get; set; }
        
        [Required]
        public DateTime From { get; set; }
        
        [Required]
        public DateTime To { get; set; }
        
        [Required]
        public bool Success { get; set; }
        
        public string StatusMessage { get; set; }
        
        [Required]
        public DateTime RequestTime { get; set; }
        
        public DateTime? CompletedRequestTime { get; set; }

        [DataMember(Name = "apps")]
        [NotMapped]
        public IEnumerable<string> Apps { get; set; }
        
        public string AppsSerialized
        {
            get
            {
                return JsonConvert.SerializeObject(Apps);
            }
            set
            {
                Apps = string.IsNullOrEmpty(value)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(value);
            }
        }
        
        [NotMapped]
        public IEnumerable<string> ExceptionApps { get; set; }

        public string ExceptionAppsSerialized
        {
            get
            {
                return JsonConvert.SerializeObject(ExceptionApps);
            }
            set
            {
                ExceptionApps = string.IsNullOrEmpty(value)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(value);
            }
        }
        
        public string CallbackEndpoint { get; set; }

        public virtual List<Log> Logs { get; set; }
    }
}