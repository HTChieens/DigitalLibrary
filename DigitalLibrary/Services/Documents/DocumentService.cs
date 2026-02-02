using DigitalLibrary.Data;
using DigitalLibrary.DTOs.Authors;
using DigitalLibrary.DTOs.Documents;
using DigitalLibrary.DTOs.Licenses;
using DigitalLibrary.Models;
using DigitalLibrary.Services.SubmissionHistories;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<CommunityTreeDto>> GetCommunities()
        {
            var communities = await _context.Communities.ToListAsync();
            var dict = communities.ToDictionary(
                c => c.Id,
                c => new CommunityTreeDto
                {
                    Id = c.Id,
                    Name = c.Name
                });

            var roots = new List<CommunityTreeDto>();

            foreach (var c in communities)
            {
                if (c.ParentCommunityId == null)
                {
                    roots.Add(dict[c.Id]);
                }
                else if (dict.ContainsKey((Guid)c.ParentCommunityId))
                {
                    dict[(Guid)c.ParentCommunityId].Children.Add(dict[c.Id]);
                }
            }

            return roots;
        }
        public async Task<List<Collection>> GetCollections()
        {
            return await _context.Collections.ToListAsync();
        }
        public async Task<List<Author>> GetAuthors()
        {
            return await _context.Authors
            .Where(a =>
                a.Documents.Any(d =>
                    !d.IsDeleted &&
                    d.Submissions.Any(s => s.Status == "Approved")
                )
            )
            .ToListAsync();
        }

        public async Task<List<DocumentFile>> GetFilesById(string Id)
        {
            return await _context.DocumentFiles
                .Where(d => d.DocumentId == Id)
                .OrderByDescending(d => d.Version).ToListAsync();
        }

        public async Task<List<ReviewDto>> GetReviews(string id)
        {
            var reviews = await _context.Reviews
                .Where(r => r.DocumentID == id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.ID.ToString(),
                    DocumentId = r.DocumentID,
                    UserId = r.UserID,
                    UserName = _context.Users.Where(u => u.ID == r.UserID).FirstOrDefault().Name,
                    Rating = r.Rating,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<object> GetAllAsync(string? authorId, string? collectionId, string? communityId, string? type, string? keyword, string sortBy, int page, int pageSize)
        {
            var query = _context.Documents
            .Where(d =>
            !d.IsDeleted &&
            d.Submissions.Any(s => s.Status == "Approved")
            )
            .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d =>
                    d.Title.Contains(keyword) ||
                    d.Description.Contains(keyword)
                );
            }

            if (!string.IsNullOrWhiteSpace(authorId))
            {
                query = query.Where(d =>
                    d.Authors.Any(a => a.ID == authorId)
                );
            }

            if (!string.IsNullOrWhiteSpace(collectionId))
            {
                query = query.Where(d =>
                    d.CollectionDocuments.Any(cd => cd.CollectionId.ToString() == collectionId)
                );
            }

            if (!string.IsNullOrWhiteSpace(communityId))
            {
                query = query.Where(d =>
                    d.Submissions.Any(s =>
                        s.Collection.CommunityId.ToString() == communityId
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(d => d.DocumentType == type);
            }

            query = sortBy switch
            {
                "trending" => query.OrderByDescending(d =>
                    d.ReadingDocuments.Count(r => r.IsCounted)
                ),
                "popular" => query.OrderByDescending(d =>
                    d.Downloads.Count
                ),
                _ => query.OrderByDescending(d => d.CreatedAt)
            };

            var totalItems = await query.CountAsync();

            var documents = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DocumentListDto
                {
                    Id = d.DocumentId,
                    Title = d.Title,
                    CoverPath = d.CoverPath,
                    DocumentType = d.DocumentType,
                    PublicationDate = d.PublicationDate
                })
                .ToListAsync();

            return new
            {
                Data = documents,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }

        public async Task<DocumentDetailDto?> GetByIdAsync(string Id)
        {
            var doc = await _context.Documents
                .Where(d => d.DocumentId == Id)
                .Select(d => new DocumentDetailDto
                {
                    Id = d.DocumentId,
                    Title = d.Title,
                    Description = d.Description,
                    DocumentType = d.DocumentType,
                    PageNum = d.PageNum,
                    PublicationDate = d.PublicationDate,
                    CoverPath = d.CoverPath,
                    IntroEndPage = d.IntroEndPage,
                    CollectionId = _context.Submissions.Where(s => s.DocumentId == Id).Select(s => s.CollectionId).FirstOrDefault(),


                    TotalReviews = d.Reviews.Count(),
                    AvgRating = d.Reviews.Any() ? Math.Round(d.Reviews.Average(r => (double)(r.Rating ?? 0)), 1) : 0,

                    TotalDownloads = _context.Downloads.Count(dl => dl.DocumentID == d.DocumentId),
                    TotalViews = _context.ReadingDocuments.Count(rd => rd.DocumentID == d.DocumentId),

                    Authors = d.Authors.Select(a => new AuthorDto
                    {
                        Name = a.Name,
                        Email = a.Email,
                        Expertise = a.Expertise,
                        OrcId = a.Orcid,
                        Image = a.Image,
                        Description = a.Description
                    }).ToList(),

                    Keywords = d.Keywords.Select(k => k.Name).ToList(),

                    Identifiers = _context.Identifiers
                        .Where(i => i.DocumentID == d.DocumentId)
                        .Select(i => new IdentifierDto
                        {
                            Type = i.Type,
                            Value = i.Value
                        }).ToList(),

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

                    Licenses = d.Document_Licenses.Select(dl => new LicenseDto
                    {
                        Id = dl.License.ID,
                        Name = dl.License.Name,
                        Content = dl.License.Content
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return doc;
        }

        public async Task<List<DocumentListDto>> SearchAsync(string keyword)
        {
            keyword = keyword.ToLower().Trim();

            var list = await _context.Documents
                .Where(d => !d.IsDeleted && d.Submissions.Any(s => s.Status == "Approved") &&
                    (
                        d.Title.ToLower().Trim().Contains(keyword) ||
                        d.Description!.ToLower().Trim().Contains(keyword)
                    )
                )
                .Select(d => new DocumentListDto
                {
                    Id = d.DocumentId,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    PublicationDate = d.PublicationDate,
                    CoverPath = d.CoverPath,
                })
                .ToListAsync();
            return list;
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

            if (author != null)
            {
                return author;
            }

            string? imagePathInDb = null;

            if (dto.ImageFile != null)
            {
                ValidateImage(dto.ImageFile);

                var imageFileName = GenerateUniqueFileName(dto.ImageFile.FileName);
                var imageFolder = Path.Combine("wwwroot", "uploads", "authors");
                Directory.CreateDirectory(imageFolder);

                var fullPath = Path.Combine(imageFolder, imageFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                imagePathInDb = $"uploads/authors/{imageFileName}";
            }

            author = new Author
            {
                ID = Guid.NewGuid().ToString("N")[..16],
                Name = dto.Name,
                Email = dto.Email,
                Orcid = dto.Orcid,
                Description = dto.Description,
                Expertise = dto.Expertise,
                Image = imagePathInDb
            };

            _context.Authors.Add(author);

            return author;
        }

        private async Task AttachKeywordsAsync(Models.Document doc, List<string> keywords)
        {
            await _context.Entry(doc).Collection(d => d.Keywords).LoadAsync();

            var incomingNames = keywords
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .Distinct()
                .ToList();

            var toRemove = doc.Keywords
                .Where(k => !incomingNames.Any(name => name.Equals(k.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var k in toRemove)
            {
                doc.Keywords.Remove(k);
            }

            foreach (var name in incomingNames)
            {
                if (doc.Keywords.Any(k => k.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var keyword = await _context.Keywords
                    .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

                if (keyword == null)
                {
                    keyword = new Keyword
                    {
                        ID = Guid.NewGuid().ToString("N")[..16],
                        Name = name
                    };
                    _context.Keywords.Add(keyword);
                }

                doc.Keywords.Add(keyword);
            }
        }

        private async Task AttachLicensesAsync(string documentId, List<LicenseInputDto> licenses)
        {
            foreach (var dto in licenses)
            {
                License license;

                if (dto.Id.HasValue)
                {
                    license = await _context.Licenses
                        .FirstOrDefaultAsync(l => l.ID == dto.Id.Value)
                        ?? throw new Exception("License not found");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(dto.Name) ||
                        string.IsNullOrWhiteSpace(dto.Content))
                    {
                        throw new Exception("License Name và Content là bắt buộc");
                    }

                    var name = dto.Name.Trim().ToLower();

                    license = await _context.Licenses
                        .FirstOrDefaultAsync(l => l.Name.ToLower() == name);

                    if (license == null)
                    {
                        license = new License
                        {
                            ID = Guid.NewGuid(),
                            Name = dto.Name.Trim(),
                            Content = dto.Content.Trim()
                        };

                        _context.Licenses.Add(license);
                    }
                }

                _context.Document_Licenses.Add(new Document_License
                {
                    DocumentID = documentId,
                    LicenseID = license.ID,
                    AcceptedAt = DateTime.UtcNow
                });
            }
        }

        private void AttachIdentifiers(string documentId, List<IdentifierDto> identifiers)
        {
            foreach (var i in identifiers)
            {
                _context.Identifiers.Add(new Identifier
                {
                    ID = Guid.NewGuid(),
                    DocumentID = documentId,
                    Type = i.Type,
                    Value = i.Value
                });
            }
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var guid = Guid.NewGuid().ToString("N");

            return $"{nameWithoutExt}_{guid}{extension}";
        }

        private void ValidatePdf(IFormFile file)
        {
            if (file.ContentType != "application/pdf")
                throw new Exception("Only PDF files are allowed.");

            if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid PDF file.");
        }

        private void ValidateImage(IFormFile file)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };

            if (!allowedTypes.Contains(file.ContentType))
                throw new Exception("Invalid image type.");
        }

        private void CreateSubTypeLogic(
            string documentId,
            string documentType,
            InternalBookDto? internalBook,
            ExternalBookDto? externalBook,
            ThesisDto? thesis,
            ResearchDto? research,
            ResearchPublicationDto? researchPublication)
        {
            switch (documentType)
            {
                case "InternalBook":
                    if (internalBook == null) throw new Exception("Thiếu thông tin Sách nội bộ");
                    _context.InternalBooks.Add(new InternalBook
                    {
                        DocumentID = documentId,
                        Faculty = internalBook.Faculty,
                        DocumentType = internalBook.DocumentType,
                        Version = internalBook.Version
                    });
                    break;
                case "ExternalBook":
                    if (externalBook == null) throw new Exception("Thiếu thông tin Sách xuất bản");
                    _context.ExternalBooks.Add(new ExternalBook
                    {
                        DocumentID = documentId,
                        Publisher = externalBook.Publisher,
                        Version = externalBook.Version
                    });
                    break;
                case "Thesis":
                    if (thesis == null) throw new Exception("Thiếu thông tin Khóa luận");
                    _context.Theses.Add(new Thesis
                    {
                        DocumentID = documentId,
                        DegreeLevel = thesis.DegreeLevel,
                        Discipline = thesis.Discipline,
                        AdvisorName = thesis.AdvisorName,
                        Abstract = thesis.Abstract
                    });
                    break;
                case "Research":
                    if (research == null) throw new Exception("Thiếu thông tin Nghiên cứu");
                    _context.Researches.Add(new Research
                    {
                        DocumentID = documentId,
                        Abstract = research.Abstract,
                        ResearchLevel = research.ResearchLevel
                    });
                    break;
                case "ResearchPublication":
                    if (researchPublication == null) throw new Exception("Thiếu thông tin Bài báo");
                    _context.ResearchPublications.Add(new ResearchPublication
                    {
                        DocumentID = documentId,
                        VenueName = researchPublication.VenueName,
                        PublicationType = researchPublication.PublicationType
                    });
                    break;
            }
        }

        private async Task UpsertSubtypeLogicAsync(
            string documentId,
            string documentType,
            InternalBookDto? internalBook,
            ExternalBookDto? externalBook,
            ThesisDto? thesis,
            ResearchDto? research,
            ResearchPublicationDto? researchPublication)
        {
            switch (documentType)
            {
                case "InternalBook":
                    var ib = await _context.InternalBooks.FirstOrDefaultAsync(x => x.DocumentID == documentId) ?? new InternalBook { DocumentID = documentId };
                    if (internalBook != null)
                    {
                        ib.Faculty = internalBook.Faculty; ib.DocumentType = internalBook.DocumentType; ib.Version = internalBook.Version;
                        if (_context.Entry(ib).State == EntityState.Detached) _context.InternalBooks.Add(ib);
                    }
                    break;
                case "ExternalBook":
                    var eb = await _context.ExternalBooks.FirstOrDefaultAsync(x => x.DocumentID == documentId) ?? new ExternalBook { DocumentID = documentId };
                    if (externalBook != null)
                    {
                        eb.Publisher = externalBook.Publisher; eb.Version = externalBook.Version;
                        if (_context.Entry(eb).State == EntityState.Detached) _context.ExternalBooks.Add(eb);
                    }
                    break;
                case "Thesis":
                    var th = await _context.Theses.FirstOrDefaultAsync(x => x.DocumentID == documentId) ?? new Thesis { DocumentID = documentId };
                    if (thesis != null)
                    {
                        th.DegreeLevel = thesis.DegreeLevel; th.Discipline = thesis.Discipline; th.AdvisorName = thesis.AdvisorName; th.Abstract = thesis.Abstract;
                        if (_context.Entry(th).State == EntityState.Detached) _context.Theses.Add(th);
                    }
                    break;
                case "Research":
                    var res = await _context.Researches.FirstOrDefaultAsync(x => x.DocumentID == documentId) ?? new Research { DocumentID = documentId };
                    if (research != null)
                    {
                        res.ResearchLevel = research.ResearchLevel; res.Abstract = research.Abstract;
                        if (_context.Entry(res).State == EntityState.Detached) _context.Researches.Add(res);
                    }
                    break;
                case "ResearchPublication":
                    var rp = await _context.ResearchPublications.FirstOrDefaultAsync(x => x.DocumentID == documentId) ?? new ResearchPublication { DocumentID = documentId };
                    if (researchPublication != null)
                    {
                        rp.VenueName = researchPublication.VenueName; rp.PublicationType = researchPublication.PublicationType;
                        if (_context.Entry(rp).State == EntityState.Detached) _context.ResearchPublications.Add(rp);
                    }
                    break;
            }
        }

        public async Task<string> CreateAsync(CreateDocumentDto dto)
        {
            ValidatePdf(dto.File);

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {

                var pdfFileName = GenerateUniqueFileName(dto.File.FileName);
                var pdfFolder = Path.Combine("wwwroot", "uploads", "documents");
                Directory.CreateDirectory(pdfFolder);

                var pdfFullPath = Path.Combine(pdfFolder, pdfFileName);
                using (var stream = new FileStream(pdfFullPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                var pdfPathInDb = $"uploads/documents/{pdfFileName}";


                string? coverPathInDb = null;

                if (dto.CoverFile != null)
                {
                    ValidateImage(dto.CoverFile);

                    var coverFileName = GenerateUniqueFileName(dto.CoverFile.FileName);
                    var coverFolder = Path.Combine("wwwroot", "uploads", "covers");
                    Directory.CreateDirectory(coverFolder);

                    var coverFullPath = Path.Combine(coverFolder, coverFileName);
                    using (var stream = new FileStream(coverFullPath, FileMode.Create))
                    {
                        await dto.CoverFile.CopyToAsync(stream);
                    }

                    coverPathInDb = $"uploads/covers/{coverFileName}";
                }


                var doc = new Models.Document
                {
                    DocumentId = Guid.NewGuid().ToString("N")[..16],
                    Title = dto.Title,
                    Description = dto.Description,
                    DocumentType = dto.DocumentType,
                    CoverPath = coverPathInDb,
                    PublicationDate = dto.PublicationDate,
                    PageNum = dto.PageNum,
                    IntroEndPage = dto.IntroEndPage,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Documents.Add(doc);


                CreateSubTypeLogic(doc.DocumentId, dto.DocumentType, dto.InternalBook, dto.ExternalBook, dto.Thesis, dto.Research, dto.ResearchPublication);

                foreach (var authorDto in dto.Authors)
                {
                    var author = await GetOrCreateAuthorAsync(authorDto);
                    doc.Authors.Add(author);
                }

                await AttachKeywordsAsync(doc, dto.Keywords);
                await AttachLicensesAsync(doc.DocumentId, dto.Licenses);
                AttachIdentifiers(doc.DocumentId, dto.Identifiers);


                _context.DocumentFiles.Add(new DocumentFile
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.DocumentId,
                    FilePath = pdfPathInDb,
                    Version = 1
                });

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return doc.DocumentId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }




        public async Task UploadNewVersionAsync(string documentId, UploadNewFileDto dto, string userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

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



        private async Task RemoveOldSubtypeAsync(string documentId, string oldType)
        {
            switch (oldType)
            {
                case "InternalBook":
                    var ib = await _context.InternalBooks.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                    if (ib != null) _context.InternalBooks.Remove(ib);
                    break;

                case "ExternalBook":
                    var eb = await _context.ExternalBooks.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                    if (eb != null) _context.ExternalBooks.Remove(eb);
                    break;

                case "Thesis":
                    var th = await _context.Theses.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                    if (th != null) _context.Theses.Remove(th);
                    break;

                case "Research":
                    var r = await _context.Researches.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                    if (r != null) _context.Researches.Remove(r);
                    break;

                case "ResearchPublication":
                    var rp = await _context.ResearchPublications.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                    if (rp != null) _context.ResearchPublications.Remove(rp);
                    break;
            }
        }


        private async Task UpdateAuthorsAsync(Models.Document doc, List<AuthorInputDto> incoming)
        {
            await _context.Entry(doc).Collection(d => d.Authors).LoadAsync();

            var currentAuthors = doc.Authors.ToList();
            doc.Authors.Clear();

            foreach (var dto in incoming)
            {
                var existingAuthor = await _context.Authors.FirstOrDefaultAsync(a =>
                    (!string.IsNullOrEmpty(dto.Orcid) && a.Orcid == dto.Orcid) ||
                    (!string.IsNullOrEmpty(dto.Email) && a.Email == dto.Email) ||
                    a.Name == dto.Name);

                if (existingAuthor != null)
                {
                    existingAuthor.Name = dto.Name;
                    existingAuthor.Expertise = dto.Expertise;
                    existingAuthor.Description = dto.Description;

                    if (dto.ImageFile != null)
                    {
                        existingAuthor.Image = await SaveFileLocal(dto.ImageFile, "authors");
                    }

                    doc.Authors.Add(existingAuthor);
                }
                else
                {
                    var newAuthor = new Author
                    {
                        ID = Guid.NewGuid().ToString("N")[..16],
                        Name = dto.Name,
                        Email = dto.Email,
                        Orcid = dto.Orcid,
                        Expertise = dto.Expertise,
                        Description = dto.Description
                    };
                    if (dto.ImageFile != null)
                    {
                        newAuthor.Image = await SaveFileLocal(dto.ImageFile, "authors");
                    }
                    _context.Authors.Add(newAuthor);
                    doc.Authors.Add(newAuthor);
                }
            }
        }

        public async Task UpdateAsync(Guid submissionId, UpdateDocumentDto dto, string userId) // Thêm userId vào đây
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var submission = await _context.Submissions
                    .Include(s => s.Document)
                    .FirstOrDefaultAsync(s => s.Id == submissionId);

                if (submission == null) throw new Exception("Không tìm thấy bản nộp (Submission).");

                var documentId = submission.DocumentId;

                var doc = await _context.Documents
                    .Include(d => d.Authors)
                    .Include(d => d.Keywords)
                    .Include(d => d.Document_Licenses)
                    .FirstOrDefaultAsync(d => d.DocumentId == documentId);

                if (doc == null) throw new Exception("Không tìm thấy tài liệu (Document).");

                doc.Title = dto.Title;
                doc.Description = dto.Description;
                doc.PublicationDate = dto.PublicationDate;
                doc.PageNum = dto.PageNum;
                doc.IntroEndPage = dto.IntroEndPage;

                submission.CollectionId = dto.CollectionId;

                bool hasNewFile = false;
                if (dto.File != null)
                {
                    ValidatePdf(dto.File);
                    var pdfPath = await SaveFileLocal(dto.File, "documents");
                    var lastVersion = await _context.DocumentFiles
                        .Where(f => f.DocumentId == documentId)
                        .MaxAsync(f => (int?)f.Version) ?? 0;

                    _context.DocumentFiles.Add(new DocumentFile
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        FilePath = pdfPath,
                        Version = lastVersion + 1,
                        ChangeNote = dto.RevisionComment
                    });
                    hasNewFile = true;
                }

                if (dto.CoverFile != null)
                {
                    ValidateImage(dto.CoverFile);
                    doc.CoverPath = await SaveFileLocal(dto.CoverFile, "covers");
                }


                string oldType = doc.DocumentType;

                if (oldType != dto.DocumentType)
                {
                    await RemoveOldSubtypeAsync(doc.DocumentId, oldType);
                    CreateSubTypeLogic(doc.DocumentId, dto.DocumentType, dto.InternalBook, dto.ExternalBook, dto.Thesis, dto.Research, dto.ResearchPublication);
                }
                else
                {
                    await UpsertSubtypeLogicAsync(doc.DocumentId, dto.DocumentType, dto.InternalBook, dto.ExternalBook, dto.Thesis, dto.Research, dto.ResearchPublication);
                }
                doc.DocumentType = dto.DocumentType;

                if (dto.Authors != null)
                {
                    await UpdateAuthorsAsync(doc, dto.Authors);
                }

                if (dto.Keywords != null)
                {
                    await AttachKeywordsAsync(doc, dto.Keywords);
                }

                var oldIds = await _context.Identifiers.Where(i => i.DocumentID == documentId).ToListAsync();
                _context.Identifiers.RemoveRange(oldIds);
                AttachIdentifiers(documentId, dto.Identifiers);

                var oldLics = await _context.Document_Licenses.Where(l => l.DocumentID == documentId).ToListAsync();
                _context.Document_Licenses.RemoveRange(oldLics);
                await AttachLicensesAsync(documentId, dto.Licenses);

                submission.UpdatedAt = DateTime.UtcNow;
                if (hasNewFile)
                {
                    submission.Status = "Submitt";
                }

                await _historyService.AddAsync(submissionId, userId, "Update", dto.RevisionComment ?? "Chỉnh sửa thông tin tài liệu");

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"UPDATE ERROR: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"INNER: {ex.InnerException.Message}");
                throw;
            }
        }


        private async Task<string> SaveFileLocal(IFormFile file, string subFolder) { var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName); var folderPath = Path.Combine("wwwroot/uploads", subFolder); if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath); var fullPath = Path.Combine(folderPath, fileName); using (var stream = new FileStream(fullPath, FileMode.Create)) { await file.CopyToAsync(stream); } return $"uploads/{subFolder}/{fileName}"; }


        public async Task<List<DocumentList2Dto>> GetByViewsAsync()
        {
            var result = await _context.ReadingDocuments
                .Where(r => r.IsCounted)
                .Where(d => _context.Submissions.Any(s => s.DocumentId == d.DocumentID && s.Status.Equals("Approved")))
                .GroupBy(r => r.Document)
                .Select(g => new
                {
                    Document = g.Key,
                    ViewCount = g.Count()
                })
                .OrderByDescending(x => x.ViewCount)
                .Select(x => new DocumentList2Dto
                {
                    Id = x.Document.DocumentId,
                    Title = x.Document.Title,
                    DocumentType = x.Document.DocumentType,
                    PublicationDate = x.Document.PublicationDate,
                    CoverPath = x.Document.CoverPath,
                    ViewCount = x.ViewCount
                })
                .ToListAsync();

            return result;
        }


        public async Task<List<DocumentPopularDto>> GetByDownloadsAsync()
        {
            var result = await _context.Downloads
                .Where(d => !d.Document.IsDeleted)
                .Where(d => _context.Submissions.Any(s => s.DocumentId == d.DocumentID && s.Status.Equals("Approved")))
                .GroupBy(d => d.Document)
                .Select(g => new
                {
                    Document = g.Key,
                    DownloadCount = g.Count()
                })
                .Where(x => x.DownloadCount > 0)
                .OrderByDescending(x => x.DownloadCount)
                .Select(x => new DocumentPopularDto
                {
                    Id = x.Document.DocumentId,
                    Title = x.Document.Title,
                    DocumentType = x.Document.DocumentType,
                    PublicationDate = x.Document.PublicationDate,
                    CoverPath = x.Document.CoverPath,
                    DownloadCount = x.DownloadCount
                })
                .ToListAsync();

            return result;
        }


    }
}
