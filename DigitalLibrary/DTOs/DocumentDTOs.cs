namespace DigitalLibrary.DTOs
{
    public class LicenseCreateDto
    {
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
    }

    public class DocumentLicenseCreateDto
    {
        public string DocumentID { get; set; } = null!;
        public Guid LicenseID { get; set; }
    }

    public class DocKeywordCreateDto
    {
        public string DocumentID { get; set; } = null!;
        public string KeywordID { get; set; } = null!;
    }

    public class IdentifierCreateDto
    {
        public string DocumentID { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class IdentifierUpdateDto
    {
        public string Type { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class ThesisCreateDto
    {
        public string DocumentID { get; set; } = null!;
        public string DegreeLevel { get; set; } = null!;
        public string Discipline { get; set; } = null!;
        public string AdvisorName { get; set; } = null!;
        public string? Abstract { get; set; }
    }

    public class ThesisUpdateDto
    {
        public string DegreeLevel { get; set; } = null!;
        public string Discipline { get; set; } = null!;
        public string AdvisorName { get; set; } = null!;
        public string? Abstract { get; set; }
    }

    public class ExternalBookCreateDto
    {
        public string DocumentID { get; set; } = null!;
        public string Publisher { get; set; } = null!;
        public int Version { get; set; }
    }
    

    public class ExternalBookUpdateDto
    {
        public string Publisher { get; set; } = null!;
        public int Version { get; set; }
    }
    
    public class InternalBookCreateDto
    {
        public string DocumentID { get; set; } = null!;
        public string Faculty { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public int Version { get; set; }
    }
   
    public class InternalBookUpdateDto
    {
        public string Faculty { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public int Version { get; set; }
    }

    public class ResearchCreateDto
    {
        public string DocumentID { get; set; } = null!;
        public string? Abstract { get; set; }
        public string ResearchLevel { get; set; } = null!;
    }
    public class ResearchUpdateDto
    {
        public string? Abstract { get; set; }
        public string ResearchLevel { get; set; } = null!;
    }
   
    public class ResearchPublicationCreateDto
    {
        public string DocumentID { get; set; } = null!;
        public string VenueName { get; set; } = null!;
        public string PublicationType { get; set; } = null!;
    }
 
    public class ResearchPublicationUpdateDto
    {
        public string VenueName { get; set; } = null!;
        public string PublicationType { get; set; } = null!;
    }

    public class DocumentUpdateDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string DocumentType { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateOnly PublicationDate { get; set; }
        public int PageNum { get; set; }
        public int? IntroEndPage { get; set; }
        public string CoverPath { get; set; } = null!;
    }
}
