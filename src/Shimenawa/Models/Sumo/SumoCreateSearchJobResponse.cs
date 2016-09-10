using System;
using System.Net;

namespace Medidata.Shimenawa.Models.Sumo
{
    public class SumoCreateSearchJobResponse
    {
        public Uri Location { get; set; }
        public CookieContainer CookieContainer { get; set; }
    }
}