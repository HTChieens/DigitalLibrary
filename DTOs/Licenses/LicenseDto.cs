namespace DigitalLibrary.DTOs.Licenses
{
    public class LicenseDto
    {
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime AcceptedAt { get; set; }
    }

}
