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
                .OrderByDescending(r => r.CreatedAt) // Bình luận mới nhất lên đầu
                .Select(r => new ReviewDto
                {
                    Id = r.ID.ToString(),
                    DocumentId = r.DocumentID,
                    UserId = r.UserID,
                    // Lấy tên từ bảng Users thông qua liên kết
                    UserName = _context.Users.Where(u => u.ID == r.UserID).FirstOrDefault().Name,
                    Rating = r.Rating,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<object> GetAllAsync(
string? authorId,
string? collectionId,
string? communityId,
string? type,
string? keyword,
string sortBy,
int page,
int pageSize)
        {
            var query = _context.Documents
            .Where(d =>
            !d.IsDeleted &&
            d.Submissions.Any(s => s.Status == "Approved")
            )
            .AsQueryable();

            // -------- FILTERS --------
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

            // -------- SORTING --------
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

            // -------- PAGINATION --------
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
                .Where(d => d.DocumentId == Id && !d.IsDeleted && d.Submissions.Any(s => s.Status == "Approved"))
                .Select(d => new DocumentDetailDto
                {
                    Id = d.DocumentId,
                    Title = d.Title,
                    Description = d.Description,
                    DocumentType = d.DocumentType,
                    PageNum = d.PageNum,
                    PublicationDate = d.PublicationDate,
                    CoverPath = d.CoverPath,

                    // --- TÍNH TOÁN THỐNG KÊ TRỰC TIẾP TRÊN DATABASE ---
                    TotalReviews = d.Reviews.Count(),
                    // Tính trung bình cộng Rating, nếu không có ai đánh giá thì mặc định 0
                    AvgRating = d.Reviews.Any() ? Math.Round(d.Reviews.Average(r => (double)(r.Rating ?? 0)), 1) : 0,

                    // Đếm từ các bảng liên quan
                    TotalDownloads = _context.Downloads.Count(dl => dl.DocumentID == d.DocumentId),
                    TotalViews = _context.ReadingDocuments.Count(rd => rd.DocumentID == d.DocumentId),

                    // --- LẤY DANH SÁCH TÁC GIẢ VÀ ĐỊNH DANH ---
                    Authors = d.Authors.Select(a => new AuthorDto
                    {
                        Name = a.Name,
                        Email = a.Email,
                        Expertise = a.Expertise,
                    }).ToList(),

                    Keywords = d.Keywords.Select(k => k.Name).ToList(),

                    Identifiers = _context.Identifiers
                        .Where(i => i.DocumentID == d.DocumentId)
                        .Select(i => new IdentifierDto
                        {
                            Type = i.Type,
                            Value = i.Value
                        }).ToList(),

                    // --- CÁC KHỐI THÔNG TIN CHI TIẾT THEO LOẠI ---
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

        private void CreateSubTypeAsync(string documentId, CreateDocumentDto dto)
        {
            switch (dto.DocumentType)
            {
                case "InternalBook":
                    _context.InternalBooks.Add(new InternalBook
                    {
                        DocumentID = documentId,
                        Faculty = dto.InternalBook!.Faculty,
                        DocumentType = dto.InternalBook.DocumentType,
                        Version = dto.InternalBook.Version
                    });
                    break;

                case "ExternalBook":
                    _context.ExternalBooks.Add(new ExternalBook
                    {
                        DocumentID = documentId,
                        Publisher = dto.ExternalBook!.Publisher,
                        Version = dto.ExternalBook.Version
                    });
                    break;

                case "Thesis":
                    _context.Theses.Add(new Thesis
                    {
                        DocumentID = documentId,
                        DegreeLevel = dto.Thesis!.DegreeLevel,
                        Discipline = dto.Thesis.Discipline,
                        AdvisorName = dto.Thesis.AdvisorName,
                        Abstract = dto.Thesis.Abstract
                    });
                    break;

                case "Research":
                    _context.Researches.Add(new Research
                    {
                        DocumentID = documentId,
                        Abstract = dto.Research!.Abstract,
                        ResearchLevel = dto.Research.ResearchLevel
                    });
                    break;

                case "ResearchPublication":
                    _context.ResearchPublications.Add(new ResearchPublication
                    {
                        DocumentID = documentId,
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
                    ID = Guid.NewGuid().ToString("N")[..16],
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
                        ID = Guid.NewGuid().ToString("N")[..16],
                        Name = key
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
                    license = await _context.Licenses.FirstOrDefaultAsync(l => l.ID == dto.Id.Value)
                        ?? throw new Exception("License not found");
                }
                else
                {
                    var name = dto.Name.Trim().ToLower();

                    license = await _context.Licenses
                        .FirstOrDefaultAsync(l => l.Name.ToLower() == name)
                        ?? new License
                        {
                            ID = Guid.NewGuid(),
                            Name = dto.Name
                        };

                    if (_context.Entry(license).State == EntityState.Detached)
                        _context.Licenses.Add(license);
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
            var guid = Guid.NewGuid().ToString("N");

            return $"{guid}{extension}";
        }


        public async Task<string> CreateAsync(CreateDocumentDto dto)
        {
            if (_context.Identifiers.Any(i => i.Value.Trim().ToLower() == dto.Identifiers.First().Value.Trim().ToLower()))
            {
                throw new Exception("The document has already exsited!");
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var doc = new Models.Document
                {
                    DocumentId = Guid.NewGuid().ToString("N")[..16],
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

                CreateSubTypeAsync(doc.DocumentId, dto);

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
                    FilePath = dto.FilePath,
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

        private async Task UpsertSubtypeAsync(string documentId, UpdateDocumentDto dto)
        {
            switch (dto.DocumentType)
            {
                case "InternalBook":
                    {
                        var entity = await _context.InternalBooks.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                        if (entity == null)
                        {
                            entity = new InternalBook { DocumentID = documentId };
                            _context.InternalBooks.Add(entity);
                        }
                        entity.Faculty = dto.InternalBook!.Faculty;
                        entity.DocumentType = dto.InternalBook.DocumentType;
                        entity.Version = dto.InternalBook.Version;
                        break;
                    }

                case "ExternalBook":
                    {
                        var entity = await _context.ExternalBooks.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                        if (entity == null)
                        {
                            entity = new ExternalBook { DocumentID = documentId };
                            _context.ExternalBooks.Add(entity);
                        }
                        entity.Publisher = dto.ExternalBook!.Publisher;
                        entity.Version = dto.ExternalBook.Version;
                        break;
                    }

                case "Thesis":
                    {
                        var entity = await _context.Theses.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                        if (entity == null)
                        {
                            entity = new Thesis { DocumentID = documentId };
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
                        var entity = await _context.Researches.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                        if (entity == null)
                        {
                            entity = new Research { DocumentID = documentId };
                            _context.Researches.Add(entity);
                        }
                        entity.Abstract = dto.Research!.Abstract;
                        entity.ResearchLevel = dto.Research.ResearchLevel;
                        break;
                    }

                case "ResearchPublication":
                    {
                        var entity = await _context.ResearchPublications.FirstOrDefaultAsync(x => x.DocumentID == documentId);
                        if (entity == null)
                        {
                            entity = new ResearchPublication { DocumentID = documentId };
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
                            ID = Guid.NewGuid().ToString("N")[..16],
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

            // 1️⃣ Add / attach
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
                        ID = Guid.NewGuid().ToString("N")[..16],
                        Name = name.Trim()
                    };
                    _context.Keywords.Add(dbKeyword);
                }

                doc.Keywords.Add(dbKeyword);
            }

            // 2️⃣ Remove + delete orphan
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
                .Where(i => i.DocumentID == documentId)
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
                        ID = Guid.NewGuid(),
                        DocumentID = documentId,
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
                .Collection(d => d.Document_Licenses)
                .Query()
                .Include(dl => dl.License)
                .LoadAsync();

            var existing = doc.Document_Licenses
                .ToDictionary(dl => dl.LicenseID.ToString());

            var usedKeys = new HashSet<string>();

            // 1️⃣ Add / attach
            foreach (var dto in incoming)
            {
                License license;

                if (dto.Id.HasValue)
                {
                    license = await _context.Licenses
                        .FirstOrDefaultAsync(l => l.ID == dto.Id.Value)
                        ?? throw new Exception("License not found");

                    usedKeys.Add(dto.Id.Value.ToString());
                }
                else
                {
                    var nameKey = dto.Name!.Trim().ToLower();
                    usedKeys.Add($"NAME:{nameKey}");

                    license = await _context.Licenses
                        .FirstOrDefaultAsync(l => l.Name.ToLower() == nameKey);

                    if (license == null)
                    {
                        license = new License
                        {
                            ID = Guid.NewGuid(),
                            Name = dto.Name.Trim()
                        };
                        _context.Licenses.Add(license);
                    }
                }

                if (!existing.ContainsKey(license.ID.ToString()))
                {
                    doc.Document_Licenses.Add(new Document_License
                    {
                        DocumentID = doc.DocumentId,
                        LicenseID = license.ID,
                        AcceptedAt = DateTime.UtcNow
                    });
                }
            }

            // 2️⃣ Remove mapping
            foreach (var dl in existing.Values)
            {
                if (!usedKeys.Contains(dl.LicenseID.ToString()))
                {
                    doc.Document_Licenses.Remove(dl);
                }
            }
        }

        public async Task UpdateAsync(Guid submissionId, UpdateDocumentDto dto)
        {
            var documentId = _context.Submissions.Where(s => s.Id == submissionId).First().DocumentId;
            using var tx = await _context.Database.BeginTransactionAsync();

            var doc = await _context.Documents
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (doc == null)
                throw new Exception("Document not found");

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
