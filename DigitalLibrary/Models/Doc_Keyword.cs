namespace DigitalLibrary.Models
{
    public class Doc_Keyword
    {
        public string DocumentID { get; set; } = null!;
        public string KeywordID { get; set; } = null!;
        public virtual Document Document { get; set; } = null!;
        public virtual Keyword Keyword { get; set; } = null!;
    }
}
