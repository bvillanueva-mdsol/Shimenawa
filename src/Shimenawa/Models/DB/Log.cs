using System;
using System.ComponentModel.DataAnnotations;

namespace Medidata.Shimenawa.Models.DB
{
    public class Log
    {
        [Key]
        public long LogId { get; set; }

        [Required]
        public string RawLog { get; set; }

        [StringLength(100)]
        public string ComponentName { get; set; }

        public bool HasException { get; set; }

        public Guid RequestUuid { get; set; }

        public virtual Request Request { get; set; }
}
}