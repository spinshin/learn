// See https://aka.ms/new-console-template for more information

// QuestPDF.Settings.License = LicenseType.Community;
// QuestPDF.Settings.EnableDebugging = true;
// QuestPDF.Settings.EnableCaching = true;
//
// var doc = new PdfBuilder()
//     .UsingDocumentProperties("Audit1", "Audit Report")
//     .UsingLandscapeOrientation()
//     .UsingCoverPage(new Dictionary<string, string>
//     {
//         { "asd", "asd" },
//         { "asdf", "asd" }
//     })
//     .UsingEndPage("asd")
//     .Build(x =>
//     {
//         x.Column(c =>
//         {
//             c.Item()
//                 .Text(Placeholders.Paragraphs());
//         });
//     })
//     .Retrieve();
//
// // doc.GeneratePdf("test.pdf");
//
// await doc.ShowInCompanionAsync();

File.Delete(@"C:\Users\prathamesh\RiderProjects\learn\PdfTest\test.pdf");
