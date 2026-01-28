namespace DigitalLibrary.DTOs.Documents
{
    public class LicenseInputDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
