namespace DHI.Services.Jobs.Workflows
{
    using System;
    using System.Linq;

    [Obsolete("This type will eventually be removed.")]
    public class ActivityService : BaseDiscreteService<Activity, string>
    {
        public ActivityService(IActivityRepository repository)
            : base(repository)
        {
        }

        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IActivityRepository>(path);
        }

        public Type[] GetTypes()
        {
            return GetAll().Select(activity => activity.Type).ToArray();
        }
    }
}