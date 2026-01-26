namespace WarriorsGuild.Email
{
    public record AttachmentDetail( string FileName, string ContentType, byte[] Bytes );
}