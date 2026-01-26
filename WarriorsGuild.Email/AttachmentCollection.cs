using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsGuild.Email
{
    public record AttachmentCollection : IEnumerable<AttachmentDetail>
    {
        private readonly IEnumerable<AttachmentDetail> detail;

        public AttachmentCollection( IEnumerable<AttachmentDetail> detail)
        {
            this.detail = detail;
        }

        public AttachmentDetail this[int index] { get { return detail.ElementAtOrDefault( index ); } }

        public IEnumerator<AttachmentDetail> GetEnumerator()
        {
            return detail.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return detail.GetEnumerator();
        }
    }
}