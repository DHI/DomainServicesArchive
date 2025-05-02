namespace DHI.Services.Jobs.WorkflowWorker.Test
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class GroupsCacheTests
    {
        [Fact]
        public void AddMemberIsOk()
        {
            var groupsCache = new SignalRHostRepository();

            var claims = new Dictionary<string, string>
            {
                { "HostGroup", "test" },
                { "Priority", "5" },
                { "RunningJobsLimit", "10" }
            };

            groupsCache.AddMember("servername", claims);

            var host = groupsCache.Get("servername").Value;

            Assert.NotNull(host);
            Assert.Equal("test", host.Group);
            Assert.Equal("servername", host.Name);
            Assert.Equal(5, host.Priority);
            Assert.Equal(10, host.RunningJobsLimit);
        }

        [Fact]
        public void AddDuplicateMemberIsOk()
        {
            var groupsCache = new SignalRHostRepository();

            var claims = new Dictionary<string, string>
            {
                { "HostGroup", "test" },
                { "Priority", "5" },
                { "RunningJobsLimit", "10" }
            };

            groupsCache.AddMember("servername", claims);
            groupsCache.AddMember("servername", claims);

            var host = groupsCache.Get("servername").Value;

            Assert.NotNull(host);
            Assert.Equal("test", host.Group);
            Assert.Equal("servername", host.Name);
            Assert.Equal(5, host.Priority);
            Assert.Equal(10, host.RunningJobsLimit);
        }

        [Fact]
        public void AddMemberNoGroupIsOk()
        {
            var groupsCache = new SignalRHostRepository();

            var claims = new Dictionary<string, string>
            {
                { "Priority", "5" },
                { "RunningJobsLimit", "10" }
            };

            groupsCache.AddMember("servername", claims);

            var host = groupsCache.Get("servername").Value;

            Assert.NotNull(host);
            Assert.Equal("servername", host.Name);
            Assert.Equal(5, host.Priority);
            Assert.Equal(10, host.RunningJobsLimit);
        }

        [Fact]
        public void AddMemberNoPriorityIsOk()
        {
            var groupsCache = new SignalRHostRepository();

            var claims = new Dictionary<string, string>
            {
                { "RunningJobsLimit", "10" }
            };

            groupsCache.AddMember("servername", claims);

            var host = groupsCache.Get("servername").Value;

            Assert.NotNull(host);
            Assert.Equal("servername", host.Name);
            Assert.Equal(1, host.Priority);
            Assert.Equal(10, host.RunningJobsLimit);
        }

        [Fact]
        public void AddMemberNoLimitIsOk()
        {
            var groupsCache = new SignalRHostRepository();

            var claims = new Dictionary<string, string>
            {
                { "Priority", "5" },
            };

            groupsCache.AddMember("servername", claims);

            var host = groupsCache.Get("servername").Value;

            Assert.NotNull(host);
            Assert.Equal("servername", host.Name);
            Assert.Equal(5, host.Priority);
            Assert.Equal(1, host.RunningJobsLimit);
        }

        [Fact]
        public void RemoveMemberIsOk()
        {
            var hosts = new ConcurrentDictionary<string, ConcurrentBag<Host>>
            {
                ["A"] = new ConcurrentBag<Host>() { new Host("one", "one") },
                ["B"] = new ConcurrentBag<Host>() { new Host("two", "two") },
            };
            var groupsCache = new SignalRHostRepository(hosts);

            groupsCache.RemoveMember("one", "A");

            var ordered = hosts.OrderBy(h => h.Key);

            Assert.Collection(ordered,
                a => Assert.Empty(a.Value),
                b => Assert.Collection(b.Value, c => Assert.Equal("two", c.Id))
                );
        }

        [Fact]
        public void RemoveMemberWithDuplicateIsOk()
        {
            var hosts = new ConcurrentDictionary<string, ConcurrentBag<Host>>
            {
                ["A"] = new ConcurrentBag<Host>() { new Host("one", "one") },
                ["B"] = new ConcurrentBag<Host>() { new Host("two", "two"), new Host("two", "two") }
            };
                
            var groupsCache = new SignalRHostRepository(hosts);

            groupsCache.RemoveMember("two", "B");
            var members = groupsCache.GetGroupMembers("B");
            
            Assert.Collection(members,
                test => Assert.Equal("two", test.Id)  
                
                );
        }

        [Fact]
        public void GetGroupMembersIsOk()
        {
            var hosts = new ConcurrentDictionary<string, ConcurrentBag<Host>>
            {
                ["A"] = new ConcurrentBag<Host>() { new Host("one", "one") },
                ["B"] = new ConcurrentBag<Host>() { new Host("two", "two") },
            };
            var groupsCache = new SignalRHostRepository(hosts);
            var members = groupsCache.GetGroupMembers("A");

            Assert.Collection(members,
                a => Assert.Equal("one", a.Id));
        }

        [Fact]
        public void GetHostsIsOk()
        {
            var hosts = new ConcurrentDictionary<string, ConcurrentBag<Host>>
            {
                ["A"] = new ConcurrentBag<Host>() { new Host("one", "one") },
                ["B"] = new ConcurrentBag<Host>() { new Host("two", "two"), new Host("two", "two") },
            };
            var groupsCache = new SignalRHostRepository(hosts);
            var members = groupsCache.GetGroupMembers("A");

            Assert.Collection(members,
                a => Assert.Equal("one", a.Id));
        }

        [Fact]
        public void HostCountIsOk()
        {
            var hosts = new ConcurrentDictionary<string, ConcurrentBag<Host>>
            {
                ["A"] = new ConcurrentBag<Host>() { new Host("one", "one") },
                ["B"] = new ConcurrentBag<Host>() { new Host("two", "two"), new Host("two", "two") },
            };
            var groupsCache = new SignalRHostRepository(hosts);
            var count = groupsCache.Count();

            Assert.Equal(2, count);
        }

        [Fact]
        public void HostExistsIsOk()
        {
            var hosts = new ConcurrentDictionary<string, ConcurrentBag<Host>>
            {
                ["A"] = new ConcurrentBag<Host>() { new Host("one", "one"), new Host("one", "one") },
                ["B"] = new ConcurrentBag<Host>() { new Host("two", "two") },
            };
            var groupsCache = new SignalRHostRepository(hosts);
            var exists = groupsCache.Contains("one");

            Assert.True(exists);

            var notexists = groupsCache.Contains("x");

            Assert.False(notexists);
        }
    }

}
