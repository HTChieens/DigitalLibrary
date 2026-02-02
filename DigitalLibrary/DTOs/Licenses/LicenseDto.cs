namespace DigitalLibrary.DTOs.Licenses
{
    public class LicenseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

}
