using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Grout.Base.DataClasses
{   
    [DataContract]
   public class EntityData<T>
    {
        [DataMember]
       public IEnumerable<T> result { get; set; }
        [DataMember]
       public int count { get; set; }
          
    }
}
