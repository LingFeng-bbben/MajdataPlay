using MajdataPlay.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net;
internal static class UriExtensions
{
    public static Uri Combine(this Uri baseUri, string relativeUri)
    {
        return new Uri(baseUri, relativeUri);
    }
    public static Uri Combine(this Uri baseUri, Uri relativeUri)
    {
        return new Uri(baseUri, relativeUri);
    }
    public static Uri Combine(this Uri baseUri, params string[] relativeUris)
    {
        if(relativeUris.Length == 1)
        {
            return new Uri(baseUri, relativeUris[0]);
        }
        else
        {
            var newBaseUri = baseUri;
            for (var i = 0; i < relativeUris.Length; i++)
            {
                var relativeUri = relativeUris[i];
                newBaseUri = newBaseUri.Combine(relativeUri);
            }
            return newBaseUri;
        }
    }
    public static Uri Combine(this Uri baseUri, params Uri[] relativeUris)
    {
        if (relativeUris.Length == 1)
        {
            return new Uri(baseUri, relativeUris[0]);
        }
        else
        {
            var newBaseUri = baseUri;
            for (var i = 0; i < relativeUris.Length; i++)
            {
                var relativeUri = relativeUris[i];
                newBaseUri = newBaseUri.Combine(relativeUri);
            }
            return newBaseUri;
        }
    }
}
