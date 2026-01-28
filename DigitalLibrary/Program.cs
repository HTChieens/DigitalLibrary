using DigitalLibrary.Data;
using DigitalLibrary.Services.Documents;
using DigitalLibrary.Services.SubmissionHistories;
using DigitalLibrary.Services.Submissions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DigitalLibraryContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DigitalLibrary"))
    );

builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<ISubmissionHistoryService, SubmissionHistoryService>();
builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
