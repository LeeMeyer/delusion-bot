using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DelusionalApi
{
    public static class UriHelper
    {
        public static Uri WithQuery(this Uri uri, string key, object value)
        {
            var uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[key] = value.ToString();
            return uriBuilder.Uri;
        }

        public static Uri WithQuery(this string uri, string key, object value)
        {
            return WithQuery(new Uri(uri), key, value);
        }

        public static string Query(this Uri uri, string key)
        {
            var uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            return query[key];
        }

        public static string Query(this string uri, string key)
        {
            return Query(new Uri(uri), key);
        }
    }
}
