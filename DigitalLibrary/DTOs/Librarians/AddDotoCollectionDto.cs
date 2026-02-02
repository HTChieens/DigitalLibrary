namespace DigitalLibrary.DTOs.Librarians
{
    public class AddDotoCollectionDto
    {
        public string DocumentId { get; set; } = null!;
        public Guid CollectionId { get; set; }
    }
}
