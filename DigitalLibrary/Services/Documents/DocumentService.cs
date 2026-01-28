using DigitalLibrary.Data;
using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.DTOs.Licenses;
using DigitalLibrary.Models;
using DigitalLibrary.Services.SubmissionHistories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection.Metadata;

namespace DigitalLibrary.Services.Documents
{
    public class DocumentService : IDocumentService
    {
        private readonly DigitalLibraryContext _context;
        private readonly ISubmissionHistoryService _historyService;

        public DocumentService(DigitalLibraryContext context, ISubmissionHistoryService historyService)
        {
            _context = context;
            _historyService = historyService;
        }

        public async Task<List<DocumentListDto>> GetAllAsync()
        {
            return await _context.Documents
                .Where(d => !d.IsDeleted)
                .Where(d => _context.Submissions.Any(s => s.DocumentId == d.Id && s.Status.Equals("Accept")))
                .Select(d => new DocumentListDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    PublicationDate = d.PublicationDate,
                    CoverPath = d.CoverPath
                })
                .ToListAsync();
        }

        public async Task<DocumentDetailDto?> GetByIdAsync(string Id)
        {
            var doc = await _context.Documents
                .Where(d => d.Id == Id && !d.IsDeleted && d.Submissions.Any(s => s.Status == "Approved"))
                .Select(d => new DocumentDetailDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    DocumentType = d.DocumentType,
                    PageNum = d.PageNum,
                    PublicationDate = d.PublicationDate,
                    Authors = d.Authors.Select(a => a.Name).ToList(),
                    Keywords = d.Keywords.Select(k => k.Name).ToList(),
                    Identifiers = _context.Identifiers
                        .Where(i => i.DocumentId == d.Id)
                        .Select(i => i.Type + ": " + i.Value)
                        .ToList(),
                    InternalBook = d.InternalBook == null ? null : new InternalBookDto
                    {
                        Faculty = d.InternalBook.Faculty,
                        DocumentType = d.InternalBook.DocumentType,
                        Version = d.InternalBook.Version
                    },

                    Thesis = d.Thesis == null ? null : new ThesisDto
                    {
                        DegreeLevel = d.Thesis.DegreeLevel,
                        Discipline = d.Thesis.Discipline,
                        AdvisorName = d.Thesis.AdvisorName,
                        Abstract = d.Thesis.Abstract
                    },

                    Research = d.Research == null ? null : new ResearchDto
                    {
                        ResearchLevel = d.Research.ResearchLevel,
                        Abstract = d.Research.Abstract
                    },

                    ExternalBook = d.ExternalBook == null ? null : new ExternalBookDto
                    {
                        Publisher = d.ExternalBook.Publisher,
                        Version = d.ExternalBook.Version
                    },

                    ResearchPublication = d.ResearchPublication == null ? null : new ResearchPublicationDto
                    {
                        VenueName = d.ResearchPublication.VenueName,
                        PublicationType = d.ResearchPublication.PublicationType
                    },

                    Licenses = d.DocumentLicenses.Select(dl => new LicenseDto
                    {
                        Name = dl.License.Name,
                        Content = dl.License.Content,
                        AcceptedAt = dl.AcceptedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return doc;
        }

        public async Task<List<DocumentListDto>> SearchAsync(string keyword)
        {
            keyword = keyword.ToLower().Trim();

            var list =  await _context.Documents
                .Where(d => !d.IsDeleted && d.Submissions.Any(s => s.Status == "Approved") &&
                    (
                        d.Title.ToLower().Trim().Contains(keyword) ||
                        d.Description!.ToLower().Trim().Contains(keyword)
                    )
                )
                .Select(d => new DocumentListDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    PublicationDate = d.PublicationDate,
                    CoverPath = d.CoverPath,
                })
                .ToListAsync();
            return list;
        }

        private void CreateSubTypeAsync(string documentId, CreateDocumentDto dto)
        {
            switch (dto.DocumentType)
            {
                case "InternalBook":
                    _context.InternalBooks.Add(new InternalBook
                    {
                        DocumentId = documentId,
                        Faculty = dto.InternalBook!.Faculty,
                        DocumentType = dto.InternalBook.DocumentType,
                        Version = dto.InternalBook.Version
                    });
                    break;

                case "ExternalBook":
                    _context.ExternalBooks.Add(new ExternalBook
                    {
                        DocumentId = documentId,
                        Publisher = dto.ExternalBook!.Publisher,
                        Version = dto.ExternalBook.Version
                    });
                    break;

                case "Thesis":
                    _context.Theses.Add(new Thesis
                    {
                        DocumentId = documentId,
                        DegreeLevel = dto.Thesis!.DegreeLevel,
                        Discipline = dto.Thesis.Discipline,
                        AdvisorName = dto.Thesis.AdvisorName,
                        Abstract = dto.Thesis.Abstract
                    });
                    break;

                case "Research":
                    _context.Researches.Add(new Research
                    {
                        DocumentId = documentId,
                        Abstract = dto.Research!.Abstract,
                        ResearchLevel = dto.Research.ResearchLevel
                    });
                    break;

                case "ResearchPublication":
                    _context.ResearchPublications.Add(new ResearchPublication
                    {
                        DocumentId = documentId,
                        VenueName = dto.ResearchPublication!.VenueName,
                        PublicationType = dto.ResearchPublication.PublicationType
                    });
                    break;

                default:
                    throw new Exception("Invalid DocumentType");
            }
        }

        private async Task<Author> GetOrCreateAuthorAsync(AuthorInputDto dto)
        {
            Author? author = null;

            if (!string.IsNullOrWhiteSpace(dto.Orcid))
            {
                author = await _context.Authors.FirstOrDefaultAsync(a => a.Orcid == dto.Orcid);
            }

            if (author == null && !string.IsNullOrWhiteSpace(dto.Email))
            {
                author = await _context.Authors.FirstOrDefaultAsync(a => a.Email == dto.Email);
            }

            if (author == null)
            {
                author = new Author
                {
                    Id = Guid.NewGuid().ToString("N")[..16],
                    Name = dto.Name,
                    Email = dto.Email,
                    Orcid = dto.Orcid,
                    Description = dto.Description,
                    Image = dto.Image,
                    Expertise = dto.Expertise
                };

                _context.Authors.Add(author);
            }

            return author;
        }

        private async Task AttachKeywordsAsync(Models.Document doc, List<string> keywords)
        {
            await _context.Entry(doc).Collection(d => d.Keywords).LoadAsync();

            foreach (var k in keywords)
            {
                var key = k.Trim();
                var keyword = await _context.Keywords.FirstOrDefaultAsync(x => x.Name == key);

                if (keyword == null)
                {
                    keyword = new Keyword
                    {
                        Id = Guid.NewGuid().ToString("N")[..16],
                        Name = key
                    };
                    _context.Keywords.Add(keyword);
                }

                doc.Keywords.Add(keyword);
            }
        }

        private async Task AttachLicensesAsync(string documentId, List<Guid> licenseIds, List<LicenseInputDto>? licenses)
        {
            if (licenseIds.Any())
            {
                foreach (var id in licenseIds)
                {
                    var license = await _context.Licenses
                        .FirstOrDefaultAsync(l => l.Id == id)
                        ?? throw new Exception("License not found");

                    _context.DocumentLicenses.Add(new DocumentLicense
                    {
                        DocumentId = documentId,
                        LicenseId = license.Id,
                        AcceptedAt = DateTime.UtcNow
                    });
                }
            }

            if (licenses!.Any())
            {
                foreach (var dto in licenses)
                {
                    var license = new License
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.Name,
                        Content = dto.Content
                    };

                    _context.Licenses.Add(license);

                    _context.DocumentLicenses.Add(new DocumentLicense
                    {
                        DocumentId = documentId,
                        LicenseId = license.Id,
                        AcceptedAt = DateTime.UtcNow
                    });
                }
            }
        }


        private void AttachIdentifiers(string documentId, List<IdentifierDto> identifiers)
        {
            foreach (var i in identifiers)
            {
                _context.Identifiers.Add(new Identifier
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    Type = i.Type,
                    Value = i.Value
                });
            }
        }

        public async Task<string> CreateAsync(CreateDocumentDto dto)
        {
            if(_context.Identifiers.Any(i => i.Value.Trim().ToLower() == dto.Identifiers.First().Value.Trim().ToLower()))
            {
                throw new Exception("The document has already exsited!");
            }
            
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var doc = new Models.Document
                {
                    Id = Guid.NewGuid().ToString("N")[..16],
                    Title = dto.Title,
                    Description = dto.Description,
                    DocumentType = dto.DocumentType,
                    CoverPath = dto.CoverPath,
                    PublicationDate = dto.PublicationDate,
                    PageNum = dto.PageNum,
                    IntroEndPage = dto.IntroEndPage,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Documents.Add(doc);

                CreateSubTypeAsync(doc.Id, dto);

                foreach (var authorDto in dto.Authors)
                {
                    var author = await GetOrCreateAuthorAsync(authorDto);
                    doc.Authors.Add(author);
                }


                await AttachKeywordsAsync(doc, dto.Keywords);

                await AttachLicensesAsync(doc.Id, dto.LicenseIds, dto.Licenses);

                AttachIdentifiers(doc.Id, dto.Identifiers);

                _context.DocumentFiles.Add(new DocumentFile
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    FilePath = dto.FilePath,
                    Version = 1
                });


                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return doc.Id;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task UploadNewVersionAsync(UploadNewFileDto dto, string userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var documentId = _context.Submissions.Where(s => s.Id == dto.SubmissionId).First().DocumentId;

            var currentFile = await _context.DocumentFiles
                .Where(f => f.DocumentId == documentId)
                .OrderByDescending(f => f.Version)
                .FirstOrDefaultAsync();

            int newVersion = 1;

            if (currentFile != null)
            {
                newVersion = currentFile.Version + 1;
            }

            var newFile = new DocumentFile
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                FilePath = dto.FilePath,
                Version = newVersion,
                ChangeNote = dto.ChangeNote
            };

            _context.DocumentFiles.Add(newFile);

            await _historyService.AddAsync(dto.SubmissionId, userId, "Submit", "Newer version");

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        private async Task UpdateSubTypeAsync(string documentId, string oldType, UpdateDocumentDto dto)
        {
            if (oldType != dto.DocumentType)
            {
                await RemoveOldSubtypeAsync(documentId, oldType);
            }

            await UpsertSubtypeAsync(documentId, dto);
        }

        private async Task RemoveOldSubtypeAsync(string documentId, string oldType)
        {
            switch (oldType)
            {
                case "InternalBook":
                    var ib = await _context.InternalBooks.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                    if (ib != null) _context.InternalBooks.Remove(ib);
                    break;

                case "ExternalBook":
                    var eb = await _context.ExternalBooks.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                    if (eb != null) _context.ExternalBooks.Remove(eb);
                    break;

                case "Thesis":
                    var th = await _context.Theses.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                    if (th != null) _context.Theses.Remove(th);
                    break;

                case "Research":
                    var r = await _context.Researches.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                    if (r != null) _context.Researches.Remove(r);
                    break;

                case "ResearchPublication":
                    var rp = await _context.ResearchPublications.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                    if (rp != null) _context.ResearchPublications.Remove(rp);
                    break;
            }
        }

        private async Task UpsertSubtypeAsync(string documentId, UpdateDocumentDto dto)
        {
            switch (dto.DocumentType)
            {
                case "InternalBook":
                    {
                        var entity = await _context.InternalBooks.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                        if (entity == null)
                        {
                            entity = new InternalBook { DocumentId = documentId };
                            _context.InternalBooks.Add(entity);
                        }
                        entity.Faculty = dto.InternalBook!.Faculty;
                        entity.DocumentType = dto.InternalBook.DocumentType;
                        entity.Version = dto.InternalBook.Version;
                        break;
                    }

                case "ExternalBook":
                    {
                        var entity = await _context.ExternalBooks.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                        if (entity == null)
                        {
                            entity = new ExternalBook { DocumentId = documentId };
                            _context.ExternalBooks.Add(entity);
                        }
                        entity.Publisher = dto.ExternalBook!.Publisher;
                        entity.Version = dto.ExternalBook.Version;
                        break;
                    }

                case "Thesis":
                    {
                        var entity = await _context.Theses.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                        if (entity == null)
                        {
                            entity = new Thesis { DocumentId = documentId };
                            _context.Theses.Add(entity);
                        }
                        entity.DegreeLevel = dto.Thesis!.DegreeLevel;
                        entity.Discipline = dto.Thesis.Discipline;
                        entity.AdvisorName = dto.Thesis.AdvisorName;
                        entity.Abstract = dto.Thesis.Abstract;
                        break;
                    }

                case "Research":
                    {
                        var entity = await _context.Researches.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                        if (entity == null)
                        {
                            entity = new Research { DocumentId = documentId };
                            _context.Researches.Add(entity);
                        }
                        entity.Abstract = dto.Research!.Abstract;
                        entity.ResearchLevel = dto.Research.ResearchLevel;
                        break;
                    }

                case "ResearchPublication":
                    {
                        var entity = await _context.ResearchPublications.FirstOrDefaultAsync(x => x.DocumentId == documentId);
                        if (entity == null)
                        {
                            entity = new ResearchPublication { DocumentId = documentId };
                            _context.ResearchPublications.Add(entity);
                        }
                        entity.VenueName = dto.ResearchPublication!.VenueName;
                        entity.PublicationType = dto.ResearchPublication.PublicationType;
                        break;
                    }

                default:
                    throw new Exception("Invalid DocumentType");
            }
        }

        private async Task UpdateAuthorsAsync(Models.Document doc, List<AuthorInputDto> incoming)
        {
            await _context.Entry(doc)
                .Collection(d => d.Authors)
                .LoadAsync();

            string KeyEntity(Author a) =>
                !string.IsNullOrWhiteSpace(a.Orcid) ? $"ORCID:{a.Orcid.ToLower()}" :
                !string.IsNullOrWhiteSpace(a.Email) ? $"EMAIL:{a.Email.ToLower()}" :
                $"NAME:{a.Name.ToLower()}";

            string KeyDto(AuthorInputDto a) =>
                !string.IsNullOrWhiteSpace(a.Orcid) ? $"ORCID:{a.Orcid.ToLower()}" :
                !string.IsNullOrWhiteSpace(a.Email) ? $"EMAIL:{a.Email.ToLower()}" :
                $"NAME:{a.Name.ToLower()}";

            var existing = doc.Authors.ToDictionary(KeyEntity);
            var usedKeys = new HashSet<string>();

            // 1️⃣ Update / Add
            foreach (var dto in incoming)
            {
                var key = KeyDto(dto);
                usedKeys.Add(key);

                if (existing.TryGetValue(key, out var author))
                {
                    author.Name = dto.Name;
                    author.Orcid = dto.Orcid;
                    author.Email = dto.Email;
                    author.Description = dto.Description;
                    author.Image = dto.Image;
                    author.Expertise = dto.Expertise;
                }
                else
                {
                    var dbAuthor = await _context.Authors.FirstOrDefaultAsync(a =>
                        (!string.IsNullOrEmpty(dto.Orcid) && a.Orcid == dto.Orcid) ||
                        (!string.IsNullOrEmpty(dto.Email) && a.Email == dto.Email) ||
                        a.Name.ToLower() == dto.Name.Trim().ToLower()
                    );

                    if (dbAuthor == null)
                    {
                        dbAuthor = new Author
                        {
                            Id = Guid.NewGuid().ToString("N")[..16],
                            Name = dto.Name,
                            Orcid = dto.Orcid,
                            Email = dto.Email,
                            Description = dto.Description,
                            Image = dto.Image,
                            Expertise = dto.Expertise
                        };
                        _context.Authors.Add(dbAuthor);
                    }

                    doc.Authors.Add(dbAuthor);
                }
            }

            // 2️⃣ Remove + delete orphan
            var toRemove = existing
                .Where(e => !usedKeys.Contains(e.Key))
                .Select(e => e.Value)
                .ToList();

            foreach (var author in toRemove)
            {
                doc.Authors.Remove(author);

                await _context.Entry(author)
                    .Collection(a => a.Documents)
                    .LoadAsync();

                if (!author.Documents.Any())
                {
                    _context.Authors.Remove(author);
                }
            }
        }

        private async Task UpdateKeywordsAsync(Models.Document doc, List<string> incoming)
        {
            await _context.Entry(doc)
                .Collection(d => d.Keywords)
                .LoadAsync();

            string Normalize(string s) => s.Trim().ToLower();

            var existing = doc.Keywords
                .ToDictionary(k => Normalize(k.Name));

            var used = new HashSet<string>();

            foreach (var name in incoming)
            {
                var key = Normalize(name);
                used.Add(key);

                if (existing.ContainsKey(key))
                    continue;

                var dbKeyword = await _context.Keywords
                    .FirstOrDefaultAsync(k => k.Name.ToLower() == key);

                if (dbKeyword == null)
                {
                    dbKeyword = new Keyword
                    {
                        Id = Guid.NewGuid().ToString("N")[..16],
                        Name = name.Trim()
                    };
                    _context.Keywords.Add(dbKeyword);
                }

                doc.Keywords.Add(dbKeyword);
            }

            var toRemove = existing
                .Where(k => !used.Contains(k.Key))
                .Select(k => k.Value)
                .ToList();

            foreach (var keyword in toRemove)
            {
                doc.Keywords.Remove(keyword);

                await _context.Entry(keyword)
                    .Collection(k => k.Documents)
                    .LoadAsync();

                if (!keyword.Documents.Any())
                {
                    _context.Keywords.Remove(keyword);
                }
            }
        }

        private async Task UpdateIdentifiersAsync(string documentId, List<IdentifierDto> incoming)
        {
            var existing = await _context.Identifiers
                .Where(i => i.DocumentId == documentId)
                .ToListAsync();

            foreach (var dto in incoming)
            {
                var match = existing.FirstOrDefault(x =>
                    x.Type == dto.Type && x.Value == dto.Value);

                if (match != null)
                {
                    existing.Remove(match);
                }
                else
                {
                    _context.Identifiers.Add(new Identifier
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        Type = dto.Type,
                        Value = dto.Value
                    });
                }
            }

            _context.Identifiers.RemoveRange(existing);
        }

        private async Task UpdateLicensesAsync(Models.Document doc, List<LicenseInputDto> incoming)
        {
            await _context.Entry(doc)
                .Collection(d => d.DocumentLicenses)
                .Query()
                .Include(dl => dl.License)
                .LoadAsync();

            string Normalize(string s) => s.Trim().ToLower();

            var existing = doc.DocumentLicenses
                .ToDictionary(dl => Normalize(dl.License.Name));

            var used = new HashSet<string>();

            foreach (var dto in incoming)
            {
                var key = Normalize(dto.Name!);
                used.Add(key);

                if (existing.ContainsKey(key))
                    continue;

                var license = await _context.Licenses
                    .FirstOrDefaultAsync(l => l.Name.ToLower() == key);

                if (license == null)
                {
                    license = new License
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.Name.Trim(),
                        Content = dto.Content
                    };
                    _context.Licenses.Add(license);
                }

                doc.DocumentLicenses.Add(new DocumentLicense
                {
                    DocumentId = doc.Id,
                    LicenseId = license.Id,
                    AcceptedAt = DateTime.UtcNow
                });
            }

            var toRemove = existing
                .Where(k => !used.Contains(k.Key))
                .Select(k => k.Value)
                .ToList();

            foreach (var dl in toRemove)
            {
                doc.DocumentLicenses.Remove(dl);

                await _context.Entry(dl.License)
                    .Collection(l => l.DocumentLicenses)
                    .LoadAsync();

                if (!dl.License.DocumentLicenses.Any())
                {
                    _context.Licenses.Remove(dl.License);
                }
            }
        }

        public async Task UpdateAsync(Guid submissionId, UpdateDocumentDto dto)
        {
            var documentId = _context.Submissions.Where(s => s.Id == submissionId).First().DocumentId;
            using var tx = await _context.Database.BeginTransactionAsync();

            var doc = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (doc == null)
                throw new Exception("Document not found");

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);
            if (submission!.Status == "Accept" || submission.Status == "Reject")
                throw new Exception("This submission cannot be changed");

            doc.Title = dto.Title;
            doc.Description = dto.Description;
            doc.PublicationDate = dto.PublicationDate;
            doc.PageNum = dto.PageNum;
            doc.IntroEndPage = dto.IntroEndPage;
            doc.CoverPath = dto.CoverPath;

            await UpdateSubTypeAsync(documentId, doc.DocumentType, dto);

            if (dto.Authors != null)
                await UpdateAuthorsAsync(doc, dto.Authors);

            if (dto.Keywords != null)
                await UpdateKeywordsAsync(doc, dto.Keywords);

            if (dto.Identifiers != null)
                await UpdateIdentifiersAsync(documentId, dto.Identifiers);

            if (dto.Licenses != null)
                await UpdateLicensesAsync(doc, dto.Licenses);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
