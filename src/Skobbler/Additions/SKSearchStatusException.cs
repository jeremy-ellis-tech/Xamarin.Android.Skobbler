using System;
using System.Runtime.Serialization;

namespace Skobbler.Ngx.Search
{
    [Serializable]
    public sealed class SKSearchStatusException : Exception
    {
        private readonly SKSearchStatus _searchStatus;

        public SKSearchStatus SearchStatus
        {
            get { return _searchStatus; }
        }

        internal SKSearchStatusException(SKSearchStatus searchStatus)
        {
            _searchStatus = searchStatus;
        }

        internal SKSearchStatusException(SKSearchStatus searchStatus, string message)
            : base(message)
        {
            _searchStatus = searchStatus;
        }

        internal SKSearchStatusException(SKSearchStatus searchStatus, string message, Exception inner)
            : base(message, inner)
        {
            _searchStatus = searchStatus;
        }

        //Serializer invokes this through reflection -> can be private.
        private SKSearchStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _searchStatus = (SKSearchStatus)info.GetValue("SearchStatus", typeof(SKSearchStatus));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");

            info.AddValue("SearchStatus", SearchStatus, typeof(SKSearchStatus));

            base.GetObjectData(info, context);
        }
    }
}