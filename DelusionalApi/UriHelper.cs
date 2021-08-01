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
#if DEBUG
            return new Uri("http://lee337.ngrok.io/" + relativeUri);
#else
            return new Uri("https://" + request.Host + "/" + relativeUri);
#endif
        }

        public static Uri WithPath(this HttpRequest request, string relativeUri, string file)
        {
#if DEBUG
            return new Uri("http://lee337.ngrok.io/" + Path.Combine(relativeUri, file).Replace("\\", "/"));
#else
            return new Uri("https://" + request.Host + "/" + Path.Combine(relativeUri, file).Replace("\\", "/"));
#endif

        }
    }
}