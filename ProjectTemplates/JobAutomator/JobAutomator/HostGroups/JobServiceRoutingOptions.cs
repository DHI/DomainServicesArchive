namespace JobAutomator.HostGroups
{
    public sealed class JobServiceRoutingOptions
    {
        public string DefaultKey { get; set; } = "wf-jobs-minion";

        public Dictionary<string, string> ContainsMap { get; set; } =
            new()
            {
                ["minion"] = "wf-jobs-minion",
                ["titan"] = "wf-jobs-titan"
            };
    }
}
