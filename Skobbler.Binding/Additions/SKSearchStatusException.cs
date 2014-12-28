using Skobbler.Ngx.Search;
using System;
using System.Runtime.Serialization;

namespace Skobbler.Additions
{
    [Serializable]
    public class SKSearchStatusException : Exception
    {
        public SKSearchStatus SearchStatus { get; private set; }

        public SKSearchStatusException(SKSearchStatus searchStatus)
        {
            SearchStatus = searchStatus;
        }

        public SKSearchStatusException(string message) : base(message) { }
        public SKSearchStatusException(string message, Exception inner) : base(message, inner) { }
        protected SKSearchStatusException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}