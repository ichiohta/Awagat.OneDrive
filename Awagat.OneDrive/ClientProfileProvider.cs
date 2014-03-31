using Awagat.OneDrive.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Awagat.OneDrive
{
    public abstract class ClientProfileProvider
    {
        public static readonly ClientProfileProvider Default = new AppDataClientProfileProvcider();
        
        public abstract ClientProfile Profile { get; set; }
    }

    public class AppDataClientProfileProvcider : ClientProfileProvider
    {
        protected string ProfileFolder
        {
            get
            {
                string folderPath = Path.Combine(
                    Environment.GetEnvironmentVariable("LOCALAPPDATA"),
                    "Awagat.OneDrive"
                );

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                return folderPath;
            }
        }

        protected string ProfilePath
        {
            get
            {
                return Path.Combine(
                    ProfileFolder,
                    "profile.json"
                );
            }
        }

        public override ClientProfile Profile
        {
            get
            {
                if (!File.Exists(ProfilePath))
                    throw new InvalidOperationException("No client profile is stored.");

                using (var stream = File.Open(ProfilePath, FileMode.Open))
                {
                    return new DataContractJsonSerializer(typeof(ClientProfile))
                        .ReadObject(stream) as ClientProfile;
                }
            }

            set
            {
                using (var stream = File.Create(ProfilePath))
                {
                    new DataContractJsonSerializer(typeof(ClientProfile))
                        .WriteObject(stream, value);
                }
            }
        }
    }
}
