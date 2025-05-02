using WUApiLib;

namespace DHI.Workflow.Host.Example
{
    public class WindowsSystemInformation : WindowsUpdate.ISystemInformationWrapper
    {
        public bool RebootRequired()
        {
            var systemInfo = new SystemInformation();
            return systemInfo.RebootRequired;
        }
    }

}
