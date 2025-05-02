namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Drawing;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Rasters;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoDataAttribute : AutoFixture.Xunit2.AutoDataAttribute
    {
        public AutoDataAttribute()
            : base(() =>
            {
                // Enable auto-mocking
                var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

                // Disregard circular references in the .NET Size type
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());
                fixture.Behaviors.Remove(new ThrowingRecursionBehavior());

                // Create fake images and add values
                var images = fixture.CreateMany<FakeRadarImage>().ToList();
                foreach (var image in images)
                {
                    image.Size = new Size(fixture.RepeatCount, fixture.RepeatCount);
                    fixture.AddManyTo(image.Values, image.Size.Width * image.Size.Height);
                }

                // Register fake types for abstractions
                fixture.Register<IRasterRepository<FakeRadarImage>>(() => new FakeRasterRepository<FakeRadarImage>(images));
                fixture.Register<IUpdatableRepository<Matrix, DateTime>>(() => new FakeRepository<Matrix, DateTime>());

                return fixture;
            })
        {

        }
    }
}