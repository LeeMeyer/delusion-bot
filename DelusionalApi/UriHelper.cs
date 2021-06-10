using Microsoft.AspNetCore.Http;
using System;
using System.Web;
using Twilio.Http;

namespace DelusionalApi
{
    public static class UriHelper
    {
        public static Uri WithPath(this HttpRequest request, string relativeUri)
        {
            return new Uri("https://" + request.Host + relativeUri);
        }
    }
}
