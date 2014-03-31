using System;
using System.Runtime.Serialization;

namespace Awagat.OneDrive.Json
{
    [DataContract]
    public class Item
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "from")]
        public Person From { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "parent_id")]
        public string ParentId { get; set; }

        [DataMember(Name = "size")]
        public long Size { get; set; }

        [DataMember(Name = "upload_location")]
        public string UploadLocation { get; set; }

        [DataMember(Name = "comments_count")]
        public int CommentsCount { get; set; }

        [DataMember(Name = "comments_enabled")]
        public bool CommentsEnabled { get; set; }

        [DataMember(Name = "is_embeddable")]
        public bool IsEmbeddable { get; set; }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "source", IsRequired = false)]
        public string Source { get; set; }

        [DataMember(Name = "link")]
        public string Link { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "shared_with")]
        public SharedWith SharedWith { get; set; }

        [DataMember(Name = "create_time")]
        private string created_time_raw { get; set; }

        [DataMember(Name = "updated_time")]
        private string updated_time_raw { get; set; }

        [DataMember(Name = "client_updated_time_raw")]
        private string client_updated_time_raw { get; set; }

        private DateTimeOffset TryParseDateTimeOffset(string text)
        {
            DateTimeOffset result = default(DateTimeOffset);

            if (!string.IsNullOrEmpty(text))
                DateTimeOffset.TryParse(text, out result);

            return result;
        }

        public DateTimeOffset CreatedTime
        {
            get
            {
                return TryParseDateTimeOffset(created_time_raw);
            }
        }

        public DateTimeOffset UpdatedTime
        {
            get
            {
                return TryParseDateTimeOffset(updated_time_raw);
            }
        }

        public DateTimeOffset ClientUpdatedTime
        {
            get
            {
                return TryParseDateTimeOffset(client_updated_time_raw);
            }
        }

        public bool IsContainer
        {
            get
            {
                return string.Equals(Type, "album", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Type, "folder", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
