using System.Management.Automation;

namespace Awagat.OneDrive.PowerShell
{
    public class OneDriveInfo : PSDriveInfo
    {
        public OneDriveInfo(PSDriveInfo driveInfo, Client client) :
            base(driveInfo.Name, driveInfo.Provider, driveInfo.Root, driveInfo.Description, null)
        {
            Client = client;
        }

        public Client Client
        {
            get;
            private set;
        }
    }

}
