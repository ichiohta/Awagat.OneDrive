using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Provider;

namespace Awagat.OneDrive.PowerShell
{
    public class TextDownLoader : IContentReader
    {
        #region Properties

        private readonly TextReader Buffer;

        #endregion

        #region .ctor

        public TextDownLoader(Client client, string path, Encoding encoding)
        {
            Buffer = new StreamReader(
                new MemoryStream(client.DownloadByPath(path)), encoding);
        }

        #endregion

        #region IContextReader

        public void Close()
        {
            Buffer.Close();
        }

        public IList Read(long readCount)
        {
            var result = new ArrayList();
            string line = null;

            for (int i = 0; i++ < readCount && (line = Buffer.ReadLine()) != null; i++)
                result.Add(line);

            return result;
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            if (offset != 0 || origin != SeekOrigin.Begin)
                throw new ArgumentException("Only accepts offset = 0 and origin = Begin.");
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }

        #endregion
    }
}
