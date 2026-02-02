namespace DigitalLibrary.DTOs.Documents
{
    public class CommunityTreeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public List<CommunityTreeDto> Children { get; set; } = new();
    }

}
