namespace DHI.Services.Documents.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AutoFixture;
    using AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class AutoDocumentDataAttribute : AutoDataAttribute
    {
        public AutoDocumentDataAttribute(int repeatCount = 0)
            : base(() =>
            {
                var fixture = new Fixture();
                if (repeatCount > 0)
                {
                    fixture.RepeatCount = repeatCount;
                    var documentDictionary = new Dictionary<string, (Stream, Parameters)>();
                    var fullNames = fixture.CreateMany<FullName>();
                    foreach (var fullName in fullNames)
                    {
                        var document = fixture.Create<byte[]>();
                        var stream = new MemoryStream(document);
                        documentDictionary.Add(fullName.ToString(), (stream, null));
                    }

                    fixture.Register<IDocumentRepository<string>>(() => new FakeDocumentRepository<string>(documentDictionary));
                    fixture.Register<IGroupedDocumentRepository<string>>(() => new FakeGroupedDocumentRepository(documentDictionary));
                }
                else
                {
                    fixture.Register<IDocumentRepository<string>>(() => new FakeDocumentRepository<string>());
                    fixture.Register<IGroupedDocumentRepository<string>>(() => new FakeGroupedDocumentRepository());
                }

                return fixture;
            })
        {
        }
    }
}