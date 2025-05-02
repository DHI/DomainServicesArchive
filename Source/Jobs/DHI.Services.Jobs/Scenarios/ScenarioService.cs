namespace DHI.Services.Jobs.Scenarios
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using DHI.Services;
    using Jobs;
    using Argon;

    public class ScenarioService : BaseUpdatableDiscreteService<Scenario, string>
    {
        private readonly IScenarioRepository _repository;
        private readonly IJobRepository<Guid, string> _jobRepository;

        private static void TryFilterScenarioInfoData(ScenarioInfo scenarioInfo, string[] dataSelectors)
        {
            if (dataSelectors != null && dataSelectors.Length > 0)
            {
                scenarioInfo.Data = JsonConvert.SerializeObject(JObject.Parse(scenarioInfo.Data).Filter(dataSelectors));
            }
        }

        public ScenarioService(IScenarioRepository repository, IJobRepository<Guid, string> jobRepository = null) : base(repository)
        {
            _repository = repository;
            _jobRepository = jobRepository;
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IScenarioRepository>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IScenarioRepository>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers. If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IScenarioRepository>(path, searchPattern);
        }

        public IEnumerable<ScenarioInfo> Get(DateTime from, DateTime to, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            var scenarios = _repository.Get(from, to, user).ToArray();
            var allJobIds = scenarios.Where(s => s.LastJobId.HasValue).Select(s => s.LastJobId.Value);
            var result = new ConcurrentBag<ScenarioInfo>();
            Parallel.ForEach(scenarios, scenario =>
            {
                var scenarioInfo = new ScenarioInfo(scenario);
                TryFilterScenarioInfoData(scenarioInfo, dataSelectors);

                if (_jobRepository != null && scenario.LastJobId != null)
                {
                    var job = _jobRepository.Get(scenario.LastJobId.Value).Value;
                    if (job != null)
                    {
                        scenarioInfo.LastJobProgress = job.Progress;
                        scenarioInfo.LastJobStatus = job.Status;
                    }
                    else
                    {
                        scenarioInfo.LastJobId = scenario.LastJobId = null;
                        Update(scenario);
                    }
                }

                result.Add(scenarioInfo);
            });

            return result;
        }

        public IEnumerable<ScenarioInfo> Get(Query<Scenario> query, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            var scenarios = _repository.Get(query, user).ToArray();
            var allJobIds = scenarios.Where(s => s.LastJobId.HasValue).Select(s => s.LastJobId.Value);
            var result = new ConcurrentBag<ScenarioInfo>();
            Parallel.ForEach(scenarios, scenario =>
            {
                var scenarioInfo = new ScenarioInfo(scenario);
                TryFilterScenarioInfoData(scenarioInfo, dataSelectors);

                if (_jobRepository != null && scenario.LastJobId != null)
                {
                    var job = _jobRepository.Get(scenario.LastJobId.Value).Value;
                    if (job != null)
                    {
                        scenarioInfo.LastJobProgress = job.Progress;
                        scenarioInfo.LastJobStatus = job.Status;
                    }
                    else
                    {
                        scenarioInfo.LastJobId = scenario.LastJobId = null;
                        Update(scenario);
                    }
                }

                result.Add(scenarioInfo);
            });

            return result;
        }

        public ScenarioInfo Get(string id, string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            var maybe = _repository.Get(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"Scenario with id '{id}' was not found.");
            }

            var scenario = maybe.Value;
            var scenarioInfo = new ScenarioInfo(scenario);
            TryFilterScenarioInfoData(scenarioInfo, dataSelectors);

            if (_jobRepository == null || scenario.LastJobId == null)
            {
                return scenarioInfo;
            }

            var job = _jobRepository.Get(scenario.LastJobId.Value).Value;
            if (job != null)
            {
                scenarioInfo.LastJobProgress = job.Progress;
                scenarioInfo.LastJobStatus = job.Status;
            }
            else
            {
                scenarioInfo.LastJobId = scenario.LastJobId = null;
                Update(scenario);
            }

            return scenarioInfo;
        }

        /// <summary>
        ///     Try soft removing (mark as deleted) the scenario with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if scenario was successfully removed, <c>false</c> otherwise.</returns>
        public bool TrySoftRemove(string id, ClaimsPrincipal user = null)
        {
            try
            {
                var cancelEventArgs = new CancelEventArgs<string>(id);
                OnDeleting(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return false;
                }

                var scenario = _repository.Get(id, user).Value;
                if (scenario.Deleted is null)
                {
                    scenario.Deleted = DateTime.UtcNow;
                    _repository.Update(scenario, user);
                }

                OnDeleted(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}