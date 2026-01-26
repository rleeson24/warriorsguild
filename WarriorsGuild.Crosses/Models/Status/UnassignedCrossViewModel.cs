using System;

namespace WarriorsGuild.Crosses.Crosses.Status
{
    public class UnassignedCrossViewModel
    {
        public string Name { get; set; }
        public Guid CrossId { get; set; }
        public DateTime? ImageUploaded { get; set; }
        public string ImageExtension { get; set; }
        public bool HasImage
        {
            get
            {
                return ImageUploaded.HasValue;
            }
        }
        public string ImgSrcAttr => HasImage ? "/images/crosses/" + CrossId + ImageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png";
    }
}