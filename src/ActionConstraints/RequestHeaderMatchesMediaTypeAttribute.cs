using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionConstraints
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly MediaTypeCollection _mediaTypes = new MediaTypeCollection();
        private readonly string _requestHeaderToMatch;
        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
            string mediaType, params string[] otherMediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch ??
                throw new ArgumentNullException(nameof(requestHeaderToMatch));

            // Validate inputted media types
            if (MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                _mediaTypes.Add(parsedMediaType);
            }
            else
            {
                throw new ArgumentException(nameof(mediaType));
            }

            foreach (var otherMediaType in otherMediaTypes)
            {
                // Validate inputted media types
                if (MediaTypeHeaderValue.TryParse(mediaType,
                    out MediaTypeHeaderValue parsedOtherMediaType))
                {
                    _mediaTypes.Add(parsedOtherMediaType);
                }
                else
                {
                    throw new ArgumentException(nameof(otherMediaTypes));
                }
            }
        }

        /// <summary>
        /// Order is used by the framework to decide the execution order of constraints.
        /// 0 is the default value that all framework level constraints execute on.
        /// </summary>
        public int Order => 0;
        public bool Accept(ActionConstraintContext context)
        {
            IHeaderDictionary requestHeaders = context.RouteContext.HttpContext.Request.Headers;
            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            MediaType parsedRequestMediaType = new MediaType(requestHeaders[_requestHeaderToMatch]);

            // if one of the media types matches, return true
            foreach (var mediaType in _mediaTypes)
            {
                MediaType parsedMatchingMediaType = new MediaType(mediaType);
                if (parsedRequestMediaType.Equals(parsedMatchingMediaType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}