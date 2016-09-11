using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Owin;

namespace Medidata.Shimenawa.Settings
{
    public class AppSettings : IAppSettings
    {
        public Uri BaseUri => new Uri(ConfigurationManager.AppSettings["BaseUri"]);

        public Uri SumoApiUri => new Uri(ConfigurationManager.AppSettings["SumoApiUri"]);

        public string SumoAccessId => ConfigurationManager.AppSettings["SumoAccessId"];

        public string SumoAccessKey => ConfigurationManager.AppSettings["SumoAccessKey"];

        public int SumoLogWaitTimeBeforeStartSearchMs => int.Parse(ConfigurationManager.AppSettings["SumoLogWaitTimeBeforeStartSearchMs"]);

        public int SumoLogIntervalWaitTimeBeforeSearchQueryMs => int.Parse(ConfigurationManager.AppSettings["SumoLogIntervalWaitTimeBeforeSearchQueryMs"]);

        public int SumoApiRequestRateLimit => int.Parse(ConfigurationManager.AppSettings["SumoApiRequestRateLimit"]);

        public int HangfireWorkerCount => int.Parse(ConfigurationManager.AppSettings["HangfireWorkerCount"]);

        private bool Bypass(IOwinRequest request, IList<string> whitelist)
        {
            return !request.Path.HasValue ||
                !whitelist.Any(uri => request.Path.Value.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase));
        }

        private static List<string> GetList(string key)
        {
            var result = new List<string>();

            var value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                result.AddRange(value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList());
            }

            return result;
        }
    }
}