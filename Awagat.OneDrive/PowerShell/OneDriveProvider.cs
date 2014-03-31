using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Awagat.OneDrive.OAuth2;
using Awagat.OneDrive.Json;
using Awagat.OneDrive.Util;
using Microsoft.PowerShell.Commands;

namespace Awagat.OneDrive.PowerShell
{
    [CmdletProvider("OneDrive", ProviderCapabilities.None)]
    public class OneDriveProvider : NavigationCmdletProvider, IContentCmdletProvider
    {
        #region Constants

        private static readonly byte[] EmptyFile = new byte[0];

        #endregion

        #region Helper types

        public class NewOneDriveParameters
        {
            [Parameter(Mandatory = false)]
            public Session Session { get; set; }
        }

        public class NewItemParameters
        {
            [Parameter(Mandatory = false)]
            public string Description { get; set; }
        }

        public class ContentParameters
        {
            [Parameter(Mandatory = false)]
            public FileSystemCmdletProviderEncoding Encoding { get; set; }
        }

        #endregion

        #region Properties

        protected Client Client
        {
            get
            {
                return (PSDriveInfo as OneDriveInfo).Client;
            }
        }

        #endregion

        #region DriveCmdletProvider

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            Session session = (DynamicParameters as NewOneDriveParameters).Session;

            return new OneDriveInfo(
                drive,
                new Client(session ?? SessionUtility.StartSession()));
        }

        protected override object NewDriveDynamicParameters()
        {
            return new NewOneDriveParameters();
        }

        #endregion

        #region ItemCmdletProvider

        // Clear-Item
        protected override void ClearItem(string path)
        {
            // Not supported
            base.ClearItem(path);
        }

        // Get-Item
        protected override void GetItem(string path)
        {
            var item = Client.GetItemByPath(path);

            if (item == null)
                new NoItemException(path);

            WriteItemObject(item, path, item.IsContainer);
        }

        // Invoke-Item
        protected override void InvokeDefaultAction(string path)
        {
            // Not supported
            base.InvokeDefaultAction(path);
        }

        protected override bool IsValidPath(string path)
        {
            return !path.Any(c => Path.GetInvalidFileNameChars().Contains(c));
        }

        protected override bool ItemExists(string path)
        {
            return Client.GetItemByPath(path) != null;
        }

        // Set-Item
        protected override void SetItem(string path, object value)
        {
            // Not supported
            base.SetItem(path, value);
        }

        #endregion

        #region ContainerCmdletProvider

        // Copy-Item
        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            // Only supports the following scenarios (for now)
            // 1. copyPath is an existing folder

            Client.MoveCopyItemByPath(path, copyPath, "COPY");
        }

        // Get-ChildItem
        protected override void GetChildItems(string path, bool recurse)
        {
            Bundle list = Client.GetChildItemsByPath(path);

            foreach (Item item in list.Data)
            {
                string fullPath = NormalizeRelativePath(item.Name, path);

                WriteItemObject(
                    item,
                    fullPath,
                    item.IsContainer);

                if (item.IsContainer && recurse)
                    GetChildItems(fullPath, recurse);
            }
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            foreach (Item item in Client.GetChildItemsByPath(path).Data)
                WriteItemObject(
                    item,
                    NormalizeRelativePath(item.Name, path),
                    item.IsContainer);
        }

        protected override bool HasChildItems(string path)
        {
            var item = Client.GetItemByPath(path);
            return item != null && item.IsContainer && item.Count > 0;
        }

        // New-Item
        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            switch (itemTypeName.ToLower())
            {
                case "directory":
                case "folder":

                    var description = (DynamicParameters as NewItemParameters).Description;
                    Client.CreateFolder(path, description);
                
                    break;

                case "file":

                    if (!(newItemValue is byte[] || newItemValue is string))
                        throw new ArgumentException("Only byte[] and string is supported for new item's value.");

                    byte[] data = newItemValue is byte[] ?
                        newItemValue as byte[] :
                        (newItemValue as string).ToBytes();

                    Client.UploadByPath(path, data);

                    break;

                default:
                    throw new ArgumentException(
                        string.Format("Invalid type name: {0}", itemTypeName));
            }
        }

        protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
        {
            return new NewItemParameters();
        }

        // Remove-Item
        protected override void RemoveItem(string path, bool recurse)
        {
            Client.RemoveItemByPath(path, recurse);
        }

        // Rename-Item
        protected override void RenameItem(string path, string newName)
        {
            Client.UpdateItemByPath(path, newName, null);
        }

        #endregion

        #region NavigationCmdletProvider

        protected override string GetChildName(string path)
        {
            return base.GetChildName(path);
        }

        protected override string GetParentPath(string path, string root)
        {
            return base.GetParentPath(path, root);
        }

        protected override bool IsItemContainer(string path)
        {
            return Client.GetItemByPath(path).IsContainer;
        }

        protected override string MakePath(string parent, string child)
        {
            return base.MakePath(parent, child);
        }

        // Move-Item
        protected override void MoveItem(string path, string destination)
        {
            // Only supports the first scenario (for now)
            // 1. Destination is existing folder
            // 2. Destination doesn't exist but its parent exists (move with new name)
            // 3. Destination exists but it's a file

            Client.MoveCopyItemByPath(path, destination, "MOVE");
        }

        protected override string NormalizeRelativePath(string path, string basePath)
        {
            return basePath != null ? Path.Combine(basePath, path) : path;
        }

        #endregion

        #region IContentCmdletProvider

        // Clear-Content
        public void ClearContent(string path)
        {
            Client.UploadByPath(path, EmptyFile);
        }

        public object ClearContentDynamicParameters(string path)
        {
            return null;
        }

        // Get-Content
        public IContentReader GetContentReader(string path)
        {
            Encoding encoding =
                (DynamicParameters as ContentParameters).Encoding.ToTextEncoding();

            return encoding != null ?
                new TextDownLoader(Client, path, encoding) as IContentReader :
                new BinaryDownloader(Client, path);
        }

        public object GetContentReaderDynamicParameters(string path)
        {
            return new ContentParameters();
        }

        // Set-Content
        public IContentWriter GetContentWriter(string path)
        {
            Encoding encoding =
                (DynamicParameters as ContentParameters).Encoding.ToTextEncoding();

            return new BinaryTextUploader(Client, path, encoding);
        }

        public object GetContentWriterDynamicParameters(string path)
        {
            return new ContentParameters();
        }

        #endregion
    }
}
