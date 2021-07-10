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
            return new Uri("https://" + "71b961ae2680.ngrok.io/" + relativeUri);
        }

        public static Uri WithPath(this HttpRequest request, string relativeUri, string file)
        {
            return new Uri("https://" + "71b961ae2680.ngrok.io/" + Path.Combine(relativeUri, file).Replace("\\", "/"));
        }
    }
}
