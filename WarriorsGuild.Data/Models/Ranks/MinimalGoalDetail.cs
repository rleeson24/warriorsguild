using System;

namespace WarriorsGuild.Data.Models.Ranks
{
    public abstract class MinimalGoalDetail
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public DateTime? ImageUploaded { get; set; }
        public string ImageExtension { get; set; }
        public bool HasImage
        {
            get
            {
                return ImageUploaded.HasValue;
            }
        }

        public abstract string ImgSrcAttr { get; }
    }

    public class MinimalRingDetail : MinimalGoalDetail
    {
        public Guid RingId { get { return Id; } }
        public override string ImgSrcAttr => HasImage ? "/images/rings/" + Id + ImageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png";
    }

    public class MinimalCrossDetail : MinimalGoalDetail
    {
        public Guid CrossId { get { return Id; } }
        public override string ImgSrcAttr => HasImage ? "/images/crosses/" + Id + ImageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png";
    }

    public class MinimalAttachmentDetail : MinimalGoalDetail
    {
        public override string ImgSrcAttr => string.Empty;
    }
}