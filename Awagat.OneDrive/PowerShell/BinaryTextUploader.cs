using Awagat.OneDrive.Util;
using System;
using System.Collections;
using System.IO;
using System.Management.Automation.Provider;
using System.Text;

namespace Awagat.OneDrive.PowerShell
{
    public class BinaryTextUploader : IContentWriter
    {
        #region Constants

        private readonly Encoding UTF8Bomless = new UTF8Encoding(false);

        #endregion

        #region Properties

        private readonly MemoryStream Buffer = new MemoryStream();

        private readonly Client Client;

        private readonly string Path;

        private readonly Encoding Encoding;

        #endregion

        #region .ctor

        public BinaryTextUploader(Client client, string path, Encoding encoding)
        {
            Client   = client;
            Path     = path;
            Encoding = encoding;
        }

        #endregion

        #region IContentWriter

        public void Close()
        {
            Client.UploadByPath(Path, Buffer.ToArray());
            Buffer.Close();
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            Buffer.Seek(offset, origin);
        }

        public IList Write(IList content)
        {
            foreach (var item in content)
                if (item is byte)
                    Buffer.WriteByte((byte)item);
                else if (item is string && Encoding != null)
                {
                    byte[] data = (item as string).ToBytes(Encoding);
                    Buffer.Write(data, 0, data.Length);
                }
                else
                {
                    throw new InvalidDataException(
                        string.Format("Unable to write data of {0}", content.GetType()));
                }

            return content;
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }

        #endregion
    }
}
