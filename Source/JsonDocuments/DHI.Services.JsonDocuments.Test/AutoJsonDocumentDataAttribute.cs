namespace DHI.Services.JsonDocuments.Test
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Xunit2;
    using Logging;
    using Notifications;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoJsonDocumentDataAttribute : AutoDataAttribute
    {
        public AutoJsonDocumentDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
                var jsonDocuments = fixture.CreateMany<FakeJsonDocument>();
                fixture.Register<IJsonDocumentRepository<string>>(() => new FakeJsonDocumentRepository(jsonDocuments));
                fixture.Register<INotificationRepository>(() => new FakeNotificationRepository());
                return fixture;
            })
        {
        }
    }
}