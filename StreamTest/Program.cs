using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();


var todosApi = app.MapGroup("/files");

todosApi.MapGet("/stream", () =>
{
    var stream = File.OpenRead("data.pdf");
    var memoryStream = new MemoryStream();
    stream.CopyTo(memoryStream);
    stream.Seek(0, SeekOrigin.Begin);
    return Results.File(stream, "applicationb/pdf", "data.pdf");
});

todosApi.MapGet("/bytes", () =>
{
    var data = File.ReadAllBytes("data.pdf");
    return Results.File(data, "application/pdf", "data.pdf");
});

todosApi.MapGet("/gc", () =>
{
    GC.Collect();
    return Results.NoContent();
});

app.Run();
