namespace DHI.Services.Jobs.Automations
{
    using DHI.Services.Scalars;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class AutomationScalarResolver
    {
        private readonly Dictionary<string, Scalar<string, int>> _scalarDict;

        public AutomationScalarResolver(IEnumerable<Scalar<string, int>> scalars)
        {
            _scalarDict = scalars.ToDictionary(s => s.Id);
        }

        public bool TryResolveIsMet(string automationId, string hostGroup, out bool isMet)
        {
            var match = _scalarDict
                .Where(kvp => kvp.Key.EndsWith("/Is Met"))
                .FirstOrDefault(kvp => kvp.Key.Contains($"/{automationId}/") && kvp.Key.Contains($"/{hostGroup}/"));

            isMet = false;

            if (match.Value is not null)
            {
                var val = match.Value.GetData().Value?.Value?.ToString();
                return bool.TryParse(val, out isMet);
            }

            return false;
        }

        public bool TryResolveLastJobId(string automationId, string hostGroup, out Guid jobId)
        {
            jobId = Guid.Empty;

            var match = _scalarDict
                .Where(kvp => kvp.Key.EndsWith("/Last Job Id"))
                .FirstOrDefault(kvp => kvp.Key.Contains($"/{automationId}/") && kvp.Key.Contains($"/{hostGroup}/"));

            if (match.Value is not null)
            {
                var val = match.Value.GetData().Value?.Value?.ToString();
                return Guid.TryParse(val, out jobId);
            }

            return false;
        }
    }
}
