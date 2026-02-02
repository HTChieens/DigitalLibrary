using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace DigitalLibrary.DTOs.Documents
{
    public class ResearchPublicationDto
    {
        public string VenueName { get; set; } = null!;
        public string PublicationType { get; set; } = null!;
    }
}
