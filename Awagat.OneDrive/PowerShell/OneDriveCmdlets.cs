using Awagat.OneDrive.Json;
using Awagat.OneDrive.OAuth2;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization.Json;

namespace Awagat.OneDrive.PowerShell
{
    public class OneDriveCmdletBase : PSCmdlet
    {
        #region Helper methods

        protected string ResolvePath(string path)
        {
            return Path.IsPathRooted(path) ?
                path :
                Path.Combine(SessionState.Path.CurrentLocation.ProviderPath, path);
        }

        public static void Save<T>(string path, T entity) where T : class
        {
            using (var stream = File.OpenWrite(path))
            {
                new DataContractJsonSerializer(typeof(T))
                    .WriteObject(stream, entity);
            }
        }

        public static T Load<T>(string path) where T : class
        {
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    return new DataContractJsonSerializer(typeof(T))
                        .ReadObject(stream) as T;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }

    [Cmdlet(VerbsData.Save, "OdSession")]
    public class SaveSession : OneDriveCmdletBase
    {
        #region Properties

        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 1)]
        public Session Session { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            Save<Session>(ResolvePath(Path), Session);
        }
    }

    [Cmdlet(VerbsData.Restore, "OdSession")]
    public class RestoreSession : OneDriveCmdletBase
    {
        #region Properties

        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            WriteObject(Load<Session>(ResolvePath(Path)));
        }
    }

    [Cmdlet(VerbsCommon.Get, "OdSession")]
    public class GetSession : OneDriveCmdletBase
    {
        #region Properties

        [Parameter(Mandatory = false, Position = 0)]
        public string Name { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            var sessions = SessionState.Drive.GetAll()
                    .Select(info => info as OneDriveInfo)
                    .Where(info => info != null)
                    .Where(info => Name == null || Name.Equals(info.Name, StringComparison.OrdinalIgnoreCase))
                    .Select(info => info.Client.Session);

            WriteObject(sessions, true);
        }
    }

    [Cmdlet(VerbsCommon.Get, "OdItem")]
    public class GetItem : OneDriveCmdletBase
    {
        #region

        public const string IdForRootFolder = "me/skydrive";

        #endregion

        #region Properties

        [Parameter(Mandatory = true, Position = 0)]
        public Session Session { get; set; }

        [Parameter(Mandatory = false, Position = 1)]
        public string Id { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            var client = new Client(Session);
            WriteObject(client.GetItemById(Id ?? IdForRootFolder));
        }
    }

    [Cmdlet(VerbsCommon.Get, "OdChildItem")]
    public class GetChildItem : GetItem
    {
        protected override void ProcessRecord()
        {
            var client = new Client(Session);
            var received = client.GetChildItemsById(Id ?? IdForRootFolder);

            if (received != null)
                WriteObject(received.Data, true);
        }
    }

    [Cmdlet(VerbsCommon.Get, "OdContent")]
    public class GetContent : GetItem
    {
        #region Properties

        [Parameter(Mandatory = true, Position = 2)]
        public string OutputPath { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            var client = new Client(Session);
            byte[] data = client.DownloadById(Id);

            using (var stream = File.Create(ResolvePath(OutputPath)))
            {
                stream.Write(data, 0, data.Length);
            }
        }
    }

    [Cmdlet(VerbsCommon.Add, "OdContent")]
    public class AddContent : GetItem
    {
        #region Properties

        [Parameter(Mandatory = true, Position = 2)]
        public string FileName { get; set; }

        [Parameter(Mandatory = true, Position = 4)]
        public byte[] Data { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            var client = new Client(Session);
            client.UploadById(Id, FileName, Data);
        }
    }

    [Cmdlet(VerbsCommon.Set, "OdClientProfile")]
    public class SetClientProfile : Cmdlet
    {
        #region Properties

        [Parameter(Mandatory = true)]
        public string Id { get; set; }

        [Parameter(Mandatory = true)]
        public string Secret { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            ClientProfileProvider.Default.Profile = new ClientProfile()
            {
                Id = Id,
                Secret = Secret
            };
        }
    }

    [Cmdlet(VerbsCommon.Move, "OdItem")]
    public class MoveItem : Cmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true, Position = 1)]
        public Session Session { get; set; }

        [Parameter(Mandatory = true, Position = 2)]
        public string SourcePath { get; set; }

        [Parameter(Mandatory = true, Position = 3)]
        public string TargetPath { get; set; }

        #endregion

        protected override void ProcessRecord()
        {
            var client = new Client(Session);

            client.MoveCopyItemByPath(SourcePath, TargetPath, "MOVE");
        }
    }

}
