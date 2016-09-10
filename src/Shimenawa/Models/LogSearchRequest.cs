using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Medidata.Shimenawa.Models
{
    [DataContract]
    public class LogSearchRequest
    {
        [DataMember(Name = "request_uuid")]
        public Guid RequestUuid { get; set; }

        [DataMember(Name = "query")]
        [Required]
        public string Query { get; set; }

        [DataMember(Name = "from")]
        [Required]
        public DateTime? From { get; set; }

        [DataMember(Name = "to")]
        [Required]
        public DateTime? To { get; set; }

        [DataMember(Name = "callback_endpoint")]
        public string CallbackEndpoint { get; set; }

        public bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Query))
            {
                errorMessage = "Query value should not be null or empty";
                return false;
            }

            if (!From.HasValue)
            {
                errorMessage = "From value should not be null";
                return false;
            }

            if (!To.HasValue)
            {
                errorMessage = "To value should not be null";
                return false;
            }

            if (From > To)
            {
                errorMessage = "From value should be lesser or equal to To value";
                return false;
            }

            Uri callbackEndpointUri = null;
            if (!string.IsNullOrWhiteSpace(CallbackEndpoint) &&
                !Uri.TryCreate(CallbackEndpoint, UriKind.Absolute, out callbackEndpointUri))
            {   
                errorMessage = "CallbackEndpoint should be a valid absolute Uri string";
                return false;
            }
            else if (string.IsNullOrWhiteSpace(CallbackEndpoint))
            {
                CallbackEndpoint = string.Empty;
            }

            return true;
        }
    }
}