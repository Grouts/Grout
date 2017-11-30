using System;

namespace Grout.Base.DataClasses
{
    public class ItemResponse
    {
        public bool Status { get; set; }

        public byte[] FileContent { get; set; }

        public object ResponseContent { get; set; }

        public string StatusMessage { get; set; }

        public string ItemName { get; set; }

        public ItemType ItemType { get; set; }

        public Guid PublishedItemId { get; set; }

        public SystemSettingsResponse SystemSettingsResponse { get; set; }

        public UserDetailResponse UserDetailResponse { get; set; }
    }

}
