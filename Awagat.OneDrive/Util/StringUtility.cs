using System.IO;
using System.Text;
using Microsoft.PowerShell.Commands;
using System;

namespace Awagat.OneDrive.Util
{
    public static class StringUtility
    {
        #region Constannts

        public static readonly Encoding UTF8Bomless = new UTF8Encoding(false);

        #endregion

        public static byte[] ToBytes(this string text, Encoding encoding = null)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, encoding ?? UTF8Bomless))
            {
                writer.Write(text);
                writer.Flush();
                return stream.ToArray();
            }
        }

        public static Encoding ToTextEncoding(this FileSystemCmdletProviderEncoding encoding)
        {
            switch (encoding)
            {
                case FileSystemCmdletProviderEncoding.Ascii:
                    return Encoding.ASCII;
                case FileSystemCmdletProviderEncoding.BigEndianUnicode:
                    return Encoding.BigEndianUnicode;

                case FileSystemCmdletProviderEncoding.Unknown:
                    return Encoding.Default;

                case FileSystemCmdletProviderEncoding.Unicode:
                    return Encoding.Unicode;

                case FileSystemCmdletProviderEncoding.UTF7:
                    return Encoding.UTF7;

                case FileSystemCmdletProviderEncoding.UTF8:
                    return Encoding.UTF8;
                default:
                    return null;
            }
        }
    }
}
