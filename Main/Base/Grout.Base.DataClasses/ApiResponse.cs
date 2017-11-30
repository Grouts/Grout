using System.Runtime.Serialization;

namespace Grout.Base.DataClasses
{
    public class ApiResponse
    {
        public bool ApiStatus { get; set; }

        public object Data { get; set; }
    }

    [DataContract]
    public class ItemDetailsApiResponse
    {
        [DataMember]
        public bool ApiStatus { get; set; }
        [DataMember]
        public EntityData<ItemDetail> Data { get; set; }
    }
}
