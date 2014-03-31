using Awagat.OneDrive.Json;
using Awagat.OneDrive.OAuth2;
using Awagat.OneDrive.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Awagat.OneDrive
{
    public class Client
    {
        #region Contants

        private const string UriItemTemplate = "https://apis.live.net/v5.0/{0}?access_token={1}";

        private const string UriListTemplate = "https://apis.live.net/v5.0/{0}/files?access_token={1}";

        private static readonly char[] Separators = new char[] { '\\' };

        #endregion

        #region Properties

        public Session Session { get; private set; }

        private Cache<string, Item> cachedItems = new Cache<string, Item>();

        private Cache<string, Bundle> cachedLists = new Cache<string, Bundle>();

        #endregion

        #region .ctor
        public Client(Session session)
        {
            Session = session;
        }

        #endregion

        #region Helper methods

        protected T GetObjectById<T>(string template, string id)
        {
            EnsureAccessToken();

            string uri = string.Format(template, id, Session.AccessToken);

            using (var stream = new WebClient().OpenRead(uri))
            {
                return (T)new DataContractJsonSerializer(typeof(T))
                    .ReadObject(stream);
            }
        }

        protected void InvalidateCache(string key)
        {
            cachedItems.Invalidate(key);
            cachedLists.Invalidate(key);
        }

        #endregion

        #region Public methods

        #region Token management

        public void EnsureAccessToken()
        {
            if (Session.IsExpired)
                Session.RefreshSession();
        }

        #endregion

        #region Item retrieval

        public Item GetItemById(string id)
        {
            return cachedItems.Get(id) ?? cachedItems.Add(id, GetObjectById<Item>(UriItemTemplate, id));
        }

        public Bundle GetChildItemsById(string id)
        {
            return cachedLists.Get(id) ?? cachedLists.Add(id, GetObjectById<Bundle>(UriListTemplate, id));
        }

        public Item GetItemByPath(string path)
        {
            IEnumerator<string> components = 
                (path.Split(Separators, StringSplitOptions.RemoveEmptyEntries) as IEnumerable<string>)
                .GetEnumerator();

            Item current = GetItemById("me/skydrive");

            while (current != null && components.MoveNext())
                current = GetChildItemsById(current.Id)
                   .Data
                   .SingleOrDefault(item => string.Equals(item.Name, components.Current, StringComparison.OrdinalIgnoreCase));

            return current;
        }

        public Bundle GetChildItemsByPath(string path)
        {
            Item item = GetItemByPath(path);

            if (item == null || !item.IsContainer)
                throw new InvalidFolderException(path);

            return GetChildItemsById(item.Id);
        }

        #endregion

        #region Item creation

        public Item CreateFolder(string path, string description = null)
        {
            string folderPath = Path.GetDirectoryName(path);
            string folderName = Path.GetFileName(path);

            Item parentFolder = GetItemByPath(folderPath);

            if (parentFolder == null || !parentFolder.IsContainer)
                throw new InvalidFolderException(folderPath);

            var data = new UpdateOperation()
            {
                Name = folderName,
                Description = description ?? string.Empty
            };

            string uri = string.Format(
                UriItemTemplate,
                parentFolder.Id,
                Session.AccessToken);

            Item created = data.SendAndReceive<UpdateOperation, Item>(uri);

            InvalidateCache(parentFolder.Id);

            return created;
        }

        public void UpdateItemById(string id, string newName, string newDescription)
        {
            var item = GetItemById(id);

            var data = new UpdateOperation()
            {
                Name = newName ?? item.Name,
                Description = newDescription ?? item.Description
            };

            string uri = string.Format(
                UriItemTemplate,
                item.Id,
                Session.AccessToken);

            data.Send(uri, "PUT");

            InvalidateCache(item.Id);
            InvalidateCache(item.ParentId);
        }

        public void UpdateItemByPath(string path, string newName, string newDescription)
        {
            var item = GetItemByPath(path);
            UpdateItemById(item.Id, newName, newDescription);
        }

        #endregion

        #region Item removal

        public void RemoveItemById(string id, bool recurse)
        {
            Item item = GetItemById(id);

            if (item.IsContainer && item.Count > 0)
            {
                if (!recurse)
                    throw new InvalidOperationException(
                        string.Format("Directory '{0}' is not empty.", item.Name));

                foreach (var child in GetChildItemsById(id).Data)
                    RemoveItemById(child.Id, recurse);
            }

            string uri = string.Format(
                UriItemTemplate,
                id,
                Session.AccessToken);

            HttpClientUtility.Send(uri, "DELETE");

            InvalidateCache(id);
            InvalidateCache(item.ParentId);
        }

        public void RemoveItemByPath(string path, bool recurse)
        {
            Item item = GetItemByPath(path);

            if (item == null)
                new NoItemException("path");

            RemoveItemById(item.Id, recurse);
        }

        #endregion

        #region File upload & download

        public byte[] DownloadById(string id)
        {
            EnsureAccessToken();

            var item = GetItemById(id);
            return new WebClient().DownloadData(item.Source);
        }

        public byte[] DownloadByPath(string path)
        {
            var item = GetItemByPath(path);

            if (item == null || item.IsContainer)
                throw new NoItemException(path);

            return DownloadById(item.Id);
        }

        public void UploadById(string folderId, string fileName, byte[] data)
        {
            EnsureAccessToken();

            var item = GetItemById(folderId);

            if (item == null || !item.IsContainer)
                throw new InvalidFolderException(folderId);

            string uri = string.Format("{0}?access_token={1}", item.UploadLocation, Session.AccessToken);

            data.Send(uri, fileName, "POST");

            InvalidateCache(folderId);
        }

        public void UploadByPath(string path, byte[] data)
        {
            string folderPath = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            var folderItem = GetItemByPath(folderPath);

            if (folderItem == null || !folderItem.IsContainer)
                throw new InvalidFolderException(folderPath);

            UploadById(folderItem.Id, fileName, data);
        }

        #endregion

        #region Item move and copy

        public void MoveCopyItemById(string id, string targetId, string method)
        {
            string uri = string.Format(
                UriItemTemplate,
                id,
                Session.AccessToken);

            var data = new CopyMoveOperation()
            {
                Destination = targetId
            };

            data.Send(uri, method);

            var item = GetItemById(id);

            InvalidateCache(item.ParentId);
            InvalidateCache(targetId);
        }

        public void MoveCopyItemByPath(string path, string folderPath, string method)
        {
            Item item = GetItemByPath(path);

            if (item == null)
                throw new NoItemException(path);

            Item folder = GetItemByPath(folderPath);

            if (folder == null || !folder.IsContainer)
                throw new InvalidFolderException(path);

            MoveCopyItemById(item.Id, folder.Id, method);
        }

        #endregion

        #endregion
    }
}
