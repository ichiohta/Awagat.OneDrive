using Awagat.OneDrive.Util;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation.Provider;
using System.Text;
using System.Threading.Tasks;

namespace Awagat.OneDrive.PowerShell
{

    public class BinaryDownloader : IContentReader
    {
        #region Properties

        private readonly MemoryStream Buffer;

        #endregion

        #region .ctor

        public BinaryDownloader(Client client, string path)
        {
            Buffer = new MemoryStream(client.DownloadByPath(path));
        }

        #endregion

        public void Close()
        {
            Buffer.Close();
        }

        public IList Read(long readCount)
        {
            var buffer = new byte[readCount];

            return Buffer.Read(buffer, 0, (int)readCount) > 0 ?
                new ArrayList(buffer) :
                null;
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            Buffer.Seek(offset, origin);
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
