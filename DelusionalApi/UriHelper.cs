using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Web;
using Twilio.Http;

namespace DelusionalApi
{
    public static class UriHelper
    {
        public static Uri WithPath(this HttpRequest request, string relativeUri)
        {
            return new Uri("https://" + request.Host + "/" + relativeUri);
        }

        public static Uri WithPath(this HttpRequest request, string relativeUri, string file)
        {
            return new Uri("https://" + request.Host + "/" + Path.Combine(relativeUri, file).Replace("\\", "/"));
        }
    }
}
