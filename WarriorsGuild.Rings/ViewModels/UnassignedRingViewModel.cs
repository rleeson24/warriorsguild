using System;

namespace WarriorsGuild.Rings.ViewModels
{
    public class UnassignedRingViewModel
    {
        public string Name { get; internal set; }
        public Guid RingId { get; set; }
        public string Type { get; internal set; }
        public DateTime? ImageUploaded { get; internal set; }
        public string ImageExtension { get; internal set; }
        public bool HasImage { get; internal set; }
        public string ImgSrcAttr => HasImage ? "/images/rings/" + RingId + ImageExtension : "/images/logo/Warriors-Guild-icon-sm-wide.png";
    }
}