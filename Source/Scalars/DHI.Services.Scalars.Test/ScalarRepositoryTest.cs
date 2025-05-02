namespace DHI.Services.Scalars.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class ScalarRepositoryTest : IDisposable
    {
        private readonly string _filePath;
        private readonly ScalarRepository _repository;

        public ScalarRepositoryTest()
        {
            _filePath = Path.Combine(Path.GetTempPath(), "scalars.json");
            File.Copy(@"..\..\..\Data\scalars.json", _filePath, true);
            new FileInfo(_filePath).IsReadOnly = false;
            _repository = new ScalarRepository(_filePath);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ScalarRepository(null));
        }

        [Fact]
        public void AddWithNoGroupThrows()
        {
            var scalar = new Scalar<string, int>(Guid.NewGuid().ToString(), "MyScalar", "System.Double");
            Assert.Throws<ArgumentException>(() => _repository.Add(scalar));
        }

        [Fact]
        public void GetWithInvalidGroupedIdThrows()
        {
            Assert.Throws<ArgumentException>(() => _repository.Get("InvalidScalarId"));
        }

        [Fact]
        public void RemoveWithInvalidGroupedIdThrows()
        {
            Assert.Throws<ArgumentException>(() => _repository.Remove("InvalidScalarId"));
        }

        [Fact]
        public void GetNonExistingReturnsEmpty()
        {
            Assert.False(_repository.Get("NonExistingGroup/NonExistingName").HasValue);
        }

        [Theory, AutoScalarData]
        public void GetNonExistingFromExistingGroupReturnsEmpty(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            var id = $"{scalar.Group}/NonExistingName";
            Assert.False(_repository.Get(id).HasValue);
        }

        [Theory, AutoScalarData]
        public void AddAndGetIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            var actual = _repository.Get(scalar.FullName).Value;
            Assert.Equal(scalar.Id, actual.Id);
        }
        
        
        [Fact]
        public void AddAndGetWithGuidDataIsOk()
        {
            var data = new ScalarData(Guid.NewGuid().ToString(), new DateTime(2000, 01, 01));
            var scalar = new Scalar<string, int>(Guid.NewGuid().ToString(), "MyScalar", "System.String", "Group", data);
            
            _repository.Add(scalar);
            var actual = _repository.Get(scalar.FullName).Value;
            Assert.Equal(scalar.Id, actual.Id);
            Assert.Equal((string)data.Value, (string)actual.GetData().Value.Value);
        }

        [Theory, AutoScalarData]
        public void ContainsIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            Assert.True(_repository.Contains(scalar.FullName));
        }

        [Theory, AutoScalarData]
        public void DoesNotContainIsOk(Scalar<string, int> scalar)
        {
            Assert.False(_repository.Contains(scalar.FullName));
        }

        [Theory, AutoScalarData]
        public void ContainsGroupIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            Assert.True(_repository.ContainsGroup(scalar.Group));
        }

        [Theory, AutoScalarData]
        public void DoesNotContainGroupIsOk(Scalar<string, int> scalar)
        {
            Assert.False(_repository.ContainsGroup(scalar.Group));
        }

        [Fact]
        public void ContainsWithInvalidGroupedIdReturnFalse()
        {
            Assert.False(_repository.Contains("InvalidScalarId"));
        }

        [Theory, AutoScalarData]
        public void CountIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoScalarData]
        public void GetAllIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            Assert.Single(_repository.GetAll());
        }


        [Theory, AutoScalarData]
        public void GetByGroupIsOk(Scalar<string, int> scalar1, Scalar<string, int> scalar2)
        {
            _repository.Add(scalar1);
            _repository.Add(scalar2);
            var scalar3 = new Scalar<string, int>(Guid.NewGuid().ToString(), "MyScalar", "System.Double", scalar1.Group);
            _repository.Add(scalar3);
            Assert.Equal(2, _repository.GetByGroup(scalar1.Group).Count());
            Assert.Single(_repository.GetByGroup(scalar2.Group));
        }

        [Theory, AutoScalarData]
        public void GetFullNamesByGroupIsOk(Scalar<string, int> scalar1, Scalar<string, int> scalar2)
        {
            _repository.Add(scalar1);
            _repository.Add(scalar2);
            var scalar3 = new Scalar<string, int>(Guid.NewGuid().ToString(), "MyScalar", "System.Double", scalar1.Group);
            _repository.Add(scalar3);
            Assert.Equal(2, _repository.GetFullNames(scalar1.Group).Count());
            Assert.Single(_repository.GetFullNames(scalar2.Group));
            Assert.Equal(scalar2.FullName, _repository.GetFullNames(scalar2.Group).First());
        }

        [Theory, AutoScalarData]
        public void GetFullNamesIsOk(Scalar<string, int> scalar1, Scalar<string, int> scalar2)
        {
            _repository.Add(scalar1);
            _repository.Add(scalar2);
            var scalar3 = new Scalar<string, int>(Guid.NewGuid().ToString(), "MyScalar", "System.Double", scalar1.Group);
            _repository.Add(scalar3);
            Assert.Equal(3, _repository.GetFullNames().Count());
        }

        [Theory, AutoScalarData]
        public void GetIdsIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            Assert.Equal(scalar.Id, _repository.GetIds().First());
        }

        [Theory, AutoScalarData]
        public void RemoveIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            _repository.Remove(scalar.FullName);
            Assert.False(_repository.Contains(scalar.FullName));
            Assert.Equal(0, _repository.Count());
        }

        [Theory, AutoScalarData]
        public void RemoveUsingPredicateIsOk(Scalar<string, int> scalar1, Scalar<string, int> scalar2)
        {
            _repository.Add(scalar1);
            _repository.Add(scalar2);
            _repository.Remove(e => e.Id == scalar1.Id);
            Assert.False(_repository.Contains(scalar1.FullName));
            Assert.Equal(1, _repository.Count());
        }

        [Theory, AutoScalarData]
        public void UpdateIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            scalar.Description = "Scalar description";
            _repository.Update(scalar);
            Assert.Equal(scalar.Description, _repository.Get(scalar.FullName).Value.Description);
        }

        [Theory, AutoScalarData]
        public void SetDataIsOk(Scalar<string, int> scalar, ScalarData<int> data)
        {
            _repository.Add(scalar);
            _repository.SetData(scalar.FullName, data);
            Assert.Equal(data.Value, _repository.Get(scalar.FullName).Value.GetData().Value.Value);
        }

        [Theory, AutoScalarData]
        public void SetLockedIsOk(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            _repository.SetLocked(scalar.FullName, true);
            Assert.True(_repository.Get(scalar.FullName).Value.Locked);
        }

        [Theory, AutoScalarData]
        public void GetAllReturnsClones(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            foreach (var e in _repository.GetAll())
            {
                e.Metadata.Add("Description", "A description");
            }

            Assert.Empty(_repository.Get(scalar.FullName).Value.Metadata);
        }

        [Fact]
        public void GetAllForEmptyRepositoryIsOk()
        {
            Assert.Empty(_repository.GetAll());
        }

        [Theory, AutoScalarData]
        public void GetReturnsClone(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            var e = _repository.Get(scalar.FullName).Value;
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(scalar.FullName).Value.Metadata);
        }

        [Theory, AutoScalarData]
        public void GetByPredicateReturnsClones(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            var e = _repository.Get(ent => ent.Id == scalar.Id).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(scalar.FullName).Value.Metadata);
        }

        [Theory, AutoScalarData]
        public void GetByGroupReturnsClones(Scalar<string, int> scalar)
        {
            _repository.Add(scalar);
            var e = _repository.GetByGroup(scalar.Group).First();
            e.Metadata.Add("Description", "A description");

            Assert.Empty(_repository.Get(scalar.FullName).Value.Metadata);
        }

        [Fact]
        public void CreateRepositoryViaReflectionIsOk()
        {
            var repository = Activator.CreateInstance(typeof(ScalarRepository), string.Empty);
            Assert.NotNull(repository);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            File.Delete(_filePath);
        }
    }
}
