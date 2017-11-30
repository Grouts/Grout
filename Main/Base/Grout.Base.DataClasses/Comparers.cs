using System.Collections.Generic;

namespace Grout.Base.DataClasses
{
    public class ItemComparer : IEqualityComparer<ItemDetail>
    {
        public bool Equals(ItemDetail itemX, ItemDetail itemY)
        {
            return itemX.Id == itemY.Id;
        }

        public int GetHashCode(ItemDetail item)
        {
            return item.Id.GetHashCode();
        }
    }
}
