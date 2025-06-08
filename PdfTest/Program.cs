// See https://aka.ms/new-console-template for more information

using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var document = Document
    .Create(document =>
    {
        document.Page(page =>
        {
            page.Size(PageSizes.Postcard);
            page.Margin(0.3f, Unit.Inch);

            page.Header()
                .Text("Hello PDF!")
                .FontSize(28)
                .Bold()
                .FontColor(Colors.Blue.Darken2);

            
            page.Content()
                .PaddingVertical(8)
                .Column(column =>
                {
                    column.Item().ShowEntire().Column(async c =>
                    {
                        await Task.Delay(12);
                    });
                    
                    column.Item()
                        .Text(Placeholders.LoremIpsum())
                        .Justify();

                    column.Item()
                        .AspectRatio(16 / 9f)
                        .Image(Placeholders.Image);
                });

            page.Footer()
                .AlignCenter()
                .Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                });
        });
    });


document.GeneratePdf();

document.ShowInCompanion();
