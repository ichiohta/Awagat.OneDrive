using System;

namespace Awagat.OneDrive
{
    public class OneDriveException : Exception
    {
        public OneDriveException(string format, params object[] arguments)
            : base(string.Format(format, arguments))
        {
        }
    }

    public class InvalidFolderException : OneDriveException
    {
        public InvalidFolderException(string name) :
            base("{0} does not exist for is not a folder.", name)
        {
        }
    }

    public class NoItemException : OneDriveException
    {
        public NoItemException(string name) :
            base("{0} does not exist.", name)
        {
        }
    }
}
