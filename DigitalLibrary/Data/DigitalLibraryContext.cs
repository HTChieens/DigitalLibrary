using System;
using System.Collections.Generic;
using DigitalLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Data
{
    public partial class DigitalLibraryContext : DbContext
    {
        public DigitalLibraryContext(DbContextOptions<DigitalLibraryContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Author> Authors { get; set; }

        public virtual DbSet<Collection> Collections { get; set; }

        public virtual DbSet<Collection_Document> Collection_Documents { get; set; }

        public virtual DbSet<Collection_Permission> Collection_Permissions { get; set; }

        public virtual DbSet<Community> Communities { get; set; }

        public virtual DbSet<Document> Documents { get; set; }

        public virtual DbSet<Document_License> Document_Licenses { get; set; }
        public virtual DbSet<Doc_Keyword> Doc_Keywords { get; set; }

        public virtual DbSet<Download> Downloads { get; set; }

        public virtual DbSet<ExternalBook> ExternalBooks { get; set; }

        public virtual DbSet<Identifier> Identifiers { get; set; }

        public virtual DbSet<InternalBook> InternalBooks { get; set; }

        public virtual DbSet<Keyword> Keywords { get; set; }

        public virtual DbSet<License> Licenses { get; set; }

        public virtual DbSet<Permission> Permissions { get; set; }

        public virtual DbSet<ReadingDocument> ReadingDocuments { get; set; }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        public virtual DbSet<Research> Researches { get; set; }

        public virtual DbSet<ResearchPublication> ResearchPublications { get; set; }

        public virtual DbSet<Review> Reviews { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<SavedDocument> SavedDocuments { get; set; }

        public virtual DbSet<Submission> Submissions { get; set; }

        public virtual DbSet<Submission_History> Submission_Histories { get; set; }

        public virtual DbSet<Thesis> Theses { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<User_Author> User_Authors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Authors__3214EC27E6E29E6B");

                entity.Property(e => e.ID).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Email).HasMaxLength(50);
                entity.Property(e => e.Expertise).HasMaxLength(50);
                entity.Property(e => e.Image).HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Collection>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Collecti__3214EC27238F7B5E");

                entity.Property(e => e.ID).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.Community).WithMany(p => p.Collections)
                    .HasForeignKey(d => d.CommunityID)
                    .HasConstraintName("FK_Collection_Community");
            });

            modelBuilder.Entity<Collection_Document>(entity =>
            {
                entity.HasKey(e => new { e.CollectionID, e.DocumentID }).HasName("PK_Collection_Documents");

                entity.ToTable("Collection_Document");

                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasOne(d => d.Collection).WithMany(p => p.Collection_Documents)
                    .HasForeignKey(d => d.CollectionID)
                    .HasConstraintName("FK_CD_Collection");

                entity.HasOne(d => d.Document).WithMany(p => p.Collection_Documents)
                    .HasForeignKey(d => d.DocumentID)
                    .HasConstraintName("FK_CD_Document");
            });

            modelBuilder.Entity<Collection_Permission>(entity =>
            {
                entity.HasKey(e => new { e.CollectionID, e.RoleID, e.PermissionID });

                entity.ToTable("Collection_Permission");

                entity.Property(e => e.RoleID).HasMaxLength(20);
                entity.Property(e => e.PermissionID).HasMaxLength(20);

                entity.HasOne(d => d.Collection).WithMany(p => p.Collection_Permissions)
                    .HasForeignKey(d => d.CollectionID)
                    .HasConstraintName("FK_CP_Collection");

                entity.HasOne(d => d.Permission).WithMany(p => p.Collection_Permissions)
                    .HasForeignKey(d => d.PermissionID)
                    .HasConstraintName("FK_CP_Permission");

                entity.HasOne(d => d.Role).WithMany(p => p.Collection_Permissions)
                    .HasForeignKey(d => d.RoleID)
                    .HasConstraintName("FK_CP_Role");
            });

            modelBuilder.Entity<Community>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Communit__3214EC27494E0471");

                entity.Property(e => e.ID).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.ParentCommunity).WithMany(p => p.InverseParentCommunity)
                    .HasForeignKey(d => d.ParentCommunityID)
                    .HasConstraintName("FK_Community_Parent");
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Document__3214EC273A84990A");

                entity.HasIndex(e => e.PublicationDate, "IX_Documents_PublicationDate");

                entity.HasIndex(e => e.Title, "IX_Documents_Title");

                entity.Property(e => e.ID).HasMaxLength(20);
                entity.Property(e => e.CoverPath).HasMaxLength(255);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DocumentType)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.FilePath).HasMaxLength(255);
                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasMany(d => d.Authors).WithMany(p => p.Documents)
                    .UsingEntity<Dictionary<string, object>>(
                        "Doc_Author",
                        r => r.HasOne<Author>().WithMany()
                            .HasForeignKey("AuthorID")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__Doc_Autho__Autho__7E37BEF6"),
                        l => l.HasOne<Document>().WithMany()
                            .HasForeignKey("DocumentID")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__Doc_Autho__Docum__7F2BE32F"),
                        j =>
                        {
                            j.HasKey("DocumentID", "AuthorID").HasName("PK__Doc_Auth__4DB340AEFFA624F6");
                            j.ToTable("Doc_Author");
                            j.HasIndex(new[] { "AuthorID" }, "IX_Doc_Author_AuthorID");
                            j.IndexerProperty<string>("DocumentID").HasMaxLength(20);
                            j.IndexerProperty<string>("AuthorID").HasMaxLength(20);
                        });

            });
            modelBuilder.Entity<Doc_Keyword>(entity =>
            {
                entity.ToTable("Doc_Keyword");

                // Composite Primary Key
                entity.HasKey(e => new { e.DocumentID, e.KeywordID });

                entity.Property(e => e.DocumentID)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.KeywordID)
                      .HasMaxLength(20)
                      .IsRequired();

                // FK -> Document
                entity.HasOne(e => e.Document)
                      .WithMany(d => d.Doc_Keywords)
                      .HasForeignKey(e => e.DocumentID)
                      .OnDelete(DeleteBehavior.Cascade);

                // FK -> Keyword
                entity.HasOne(e => e.Keyword)
                      .WithMany(k => k.Doc_Keywords)
                      .HasForeignKey(e => e.KeywordID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Document_License>(entity =>
            {
                entity.HasKey(e => new { e.DocumentID, e.LicenseID });

                entity.ToTable("Document_License");

                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.AcceptedAt).HasDefaultValueSql("(sysdatetime())");

                entity.HasOne(d => d.Document).WithMany(p => p.Document_Licenses)
                    .HasForeignKey(d => d.DocumentID)
                    .HasConstraintName("FK_DL_Document");

                entity.HasOne(d => d.License).WithMany(p => p.Document_Licenses)
                    .HasForeignKey(d => d.LicenseID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DL_License");
            });

            modelBuilder.Entity<Download>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Download__3214EC27CB4694C1");

                entity.HasIndex(e => e.DocumentID, "IX_Downloads_DocumentID");

                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.DownloadedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.UserID).HasMaxLength(20);

                entity.HasOne(d => d.Document).WithMany(p => p.Downloads)
                    .HasForeignKey(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Downloads__Docum__03F0984C");

                entity.HasOne(d => d.User).WithMany(p => p.Downloads)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Downloads__UserI__04E4BC85");
            });

            modelBuilder.Entity<ExternalBook>(entity =>
            {
                entity.HasKey(e => e.DocumentID).HasName("PK__External__1ABEEF6F5509FDB5");

                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.Publisher).HasMaxLength(100);
                entity.Property(e => e.Version).HasDefaultValue(1);

                entity.HasOne(d => d.Document).WithOne(p => p.ExternalBook)
                    .HasForeignKey<ExternalBook>(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ExternalB__Docum__05D8E0BE");
            });

            modelBuilder.Entity<Identifier>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Identifi__3214EC27BDCEE150");

                entity.ToTable("Identifier");

                entity.HasIndex(e => new { e.Type, e.Value }, "UQ_Document_Identifier").IsUnique();

                entity.Property(e => e.ID).HasDefaultValueSql("(newid())");
                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Value).HasMaxLength(255);

                entity.HasOne(d => d.Document).WithMany(p => p.Identifiers)
                    .HasForeignKey(d => d.DocumentID)
                    .HasConstraintName("FK_DI_Document");
            });

            modelBuilder.Entity<InternalBook>(entity =>
            {
                entity.HasKey(e => e.DocumentID).HasName("PK__Internal__1ABEEF6F1246C7D9");

                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.DocumentType).HasMaxLength(50);
                entity.Property(e => e.Faculty).HasMaxLength(100);
                entity.Property(e => e.Version).HasDefaultValue(1);

                entity.HasOne(d => d.Document).WithOne(p => p.InternalBook)
                    .HasForeignKey<InternalBook>(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__InternalB__Docum__07C12930");
            });

            modelBuilder.Entity<Keyword>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Keywords__3214EC2726E53CF3");

                entity.HasIndex(e => e.Name, "UQ__Keywords__737584F6F1A80B99").IsUnique();

                entity.Property(e => e.ID).HasMaxLength(20);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<License>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Licenses__3214EC27DD960D9F");

                entity.Property(e => e.ID).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Permissi__3214EC27D2D1ECCB");

                entity.Property(e => e.ID).HasMaxLength(20);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<ReadingDocument>(entity =>
            {
                entity.HasKey(e => new { e.UserID, e.DocumentID, e.FirstReadAt });

                entity.Property(e => e.UserID).HasMaxLength(20);
                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.FirstReadAt).HasColumnType("datetime");
                entity.Property(e => e.IsCounted).HasDefaultValue(false);
                entity.Property(e => e.LastReadAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Document).WithMany(p => p.ReadingDocuments)
                    .HasForeignKey(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ReadingDo__Docum__08B54D69");

                entity.HasOne(d => d.User).WithMany(p => p.ReadingDocuments)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ReadingDo__UserI__09A971A2");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07BE6F68F2");

                entity.HasIndex(e => e.UserId, "IX_RefreshTokens_UserId");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
                entity.Property(e => e.TokenHash).HasMaxLength(255);
                entity.Property(e => e.UserId).HasMaxLength(20);

                entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_RefreshToken_User");
            });

            modelBuilder.Entity<Research>(entity =>
            {
                entity.HasKey(e => e.DocumentID).HasName("PK__Research__1ABEEF6F69D3637D");

                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.Abstract).HasMaxLength(1000);
                entity.Property(e => e.ResearchLevel)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("FACULTY");

                entity.HasOne(d => d.Document).WithOne(p => p.Research)
                    .HasForeignKey<Research>(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Researche__Docum__0B91BA14");
            });

            modelBuilder.Entity<ResearchPublication>(entity =>
            {
                entity.HasKey(e => e.DocumentID).HasName("PK__Research__1ABEEF6F902A0846");

                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.PublicationType).HasMaxLength(50);
                entity.Property(e => e.VenueName).HasMaxLength(150);

                entity.HasOne(d => d.Document).WithOne(p => p.ResearchPublication)
                    .HasForeignKey<ResearchPublication>(d => d.DocumentID)
                    .HasConstraintName("FK__ResearchP__Docum__0C85DE4D");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Reviews__3214EC27D5A902CC");

                entity.HasIndex(e => e.DocumentID, "IX_Reviews_DocumentID");

                entity.HasIndex(e => new { e.UserID, e.DocumentID }, "UQ_Reviews_User_Document").IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.UserID).HasMaxLength(20);

                entity.HasOne(d => d.Document).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reviews_Document");

                entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reviews_User");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Roles__3214EC27114EA946");

                entity.Property(e => e.ID).HasMaxLength(20);
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<SavedDocument>(entity =>
            {
                entity.HasKey(e => new { e.UserID, e.DocumentID }).HasName("PK__SavedDoc__F623225AF4002D96");

                entity.Property(e => e.UserID).HasMaxLength(20);
                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.SavedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Document).WithMany(p => p.SavedDocuments)
                    .HasForeignKey(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SavedDocu__Docum__0F624AF8");

                entity.HasOne(d => d.User).WithMany(p => p.SavedDocuments)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SavedDocu__UserI__10566F31");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Submissi__3214EC278AF54BF9");

                entity.Property(e => e.ID).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
                entity.Property(e => e.CurrentStep).HasDefaultValue(1);
                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.SubmitterID).HasMaxLength(20);

                entity.HasOne(d => d.Collection).WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.CollectionID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Submission_Collection");

                entity.HasOne(d => d.Document).WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Submission_Document");

                entity.HasOne(d => d.Submitter).WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.SubmitterID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Submission_User");
            });

            modelBuilder.Entity<Submission_History>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Submissi__3214EC27E1C4C742");

                entity.ToTable("Submission_History");

                entity.Property(e => e.ID).HasDefaultValueSql("(newid())");
                entity.Property(e => e.Action).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
                entity.Property(e => e.PerformedBy).HasMaxLength(20);

                entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.Submission_Histories)
                    .HasForeignKey(d => d.PerformedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SH_User");

                entity.HasOne(d => d.Submission).WithMany(p => p.Submission_Histories)
                    .HasForeignKey(d => d.SubmissionID)
                    .HasConstraintName("FK_SH_Submission");
            });

            modelBuilder.Entity<Thesis>(entity =>
            {
                entity.HasKey(e => e.DocumentID).HasName("PK__Theses__1ABEEF6F724342FF");

                entity.Property(e => e.DocumentID).HasMaxLength(20);
                entity.Property(e => e.Abstract).HasMaxLength(1000);
                entity.Property(e => e.AdvisorName).HasMaxLength(100);
                entity.Property(e => e.DegreeLevel).HasMaxLength(50);
                entity.Property(e => e.Discipline).HasMaxLength(100);

                entity.HasOne(d => d.Document).WithOne(p => p.Thesis)
                    .HasForeignKey<Thesis>(d => d.DocumentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Theses__Document__160F4887");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__Users__3214EC27BB84BC01");

                entity.HasIndex(e => e.Email, "UQ__Users__A9D1053411FCC852").IsUnique();

                entity.Property(e => e.ID).HasMaxLength(20);
                entity.Property(e => e.Class).HasMaxLength(50);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(15);
                entity.Property(e => e.RoleID).HasMaxLength(20);
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Role).WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Users__RoleID__18EBB532");
            });

            modelBuilder.Entity<User_Author>(entity =>
            {
                entity.HasKey(e => e.UserID).HasName("PK__User_Aut__1788CCAC9E6D1B17");

                entity.ToTable("User_Author");

                entity.HasIndex(e => e.AuthorID, "UQ__User_Aut__70DAFC1553161ED4").IsUnique();

                entity.Property(e => e.UserID).HasMaxLength(20);
                entity.Property(e => e.AuthorID).HasMaxLength(20);

                entity.HasOne(d => d.Author).WithOne(p => p.User_Author)
                    .HasForeignKey<User_Author>(d => d.AuthorID)
                    .HasConstraintName("FK_UA_Author");

                entity.HasOne(d => d.User).WithOne(p => p.User_Author)
                    .HasForeignKey<User_Author>(d => d.UserID)
                    .HasConstraintName("FK_UA_User");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        public DbSet<DigitalLibrary.Models.Doc_Keyword> Doc_Keyword { get; set; } = default!;
    }

}
