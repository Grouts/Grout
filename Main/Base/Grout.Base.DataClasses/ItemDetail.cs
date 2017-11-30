using System;
using System.Runtime.Serialization;

namespace Grout.Base.DataClasses
{
    [DataContract]
    public class ItemDetail
    {
        [DataMember]
        public bool CanRead { get; set; }
        [DataMember]
        public bool CanWrite { get; set; }
        [DataMember]
        public bool CanDelete { get; set; }
        [DataMember]
        public bool CanSchedule { get; set; }
        [DataMember]
        public bool CanOpen { get; set; }
        [DataMember]
        public bool CanMove { get; set; }
        [DataMember]
        public bool CanCopy { get; set; }
        [DataMember]
        public bool CanClone { get; set; }
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public ItemType ItemType { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string TrimmedName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Name) == false)
                {
                    return Name.Length > 18 ? Name.Substring(0, 18) + "..." : Name;
                }
                return String.Empty;
            }
            set
            {

            }
        }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string TrimmedDescription
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Description) == false)
                {
                    return Description.Length > 40 ? Description.Substring(0, 40) + "..." : Description;
                }
                return String.Empty;
            }
            set
            {
                
            }
        }
        [DataMember]
        public int CreatedById { get; set; }
        [DataMember]
        public string CreatedByDisplayName { get; set; }
        [DataMember]
        public string TrimmedCreatedByDisplayName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(CreatedByDisplayName) == false)
                {
                    return CreatedByDisplayName.Length > 18 ? CreatedByDisplayName.Substring(0, 18) + "..." : CreatedByDisplayName;
                }
                return String.Empty;
            }
            set
            {

            }
        }
        [DataMember]
        public int ModifiedById { get; set; }
        [DataMember]
        public string ModifiedByFullName { get; set; }
        [DataMember]
        public string TrimmedModifiedByFullName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(ModifiedByFullName) == false)
                {
                    return ModifiedByFullName.Length > 18 ? ModifiedByFullName.Substring(0, 18) + "..." : ModifiedByFullName;
                }
                return String.Empty;
            }
            set
            {

            }
        }
        [DataMember]
        public Guid? CategoryId { get; set; }
        [DataMember]
        public string CategoryName { get; set; }
        [DataMember]
        public string TrimmedCategoryName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(CategoryName) == false)
                {
                    return CategoryName.Length > 18 ? CategoryName.Substring(0, 18) + "..." : CategoryName;
                }
                return String.Empty;
            }
            set
            {

            }
        }
        [DataMember]
        public string CategoryDescription { get; set; }
        [DataMember]
        public string CreatedDate { get; set; }
        [DataMember]
        public string ModifiedDate { get; set; }
        [DataMember]
        public bool IsCreatedByActive { get; set; }
        [DataMember]
        public bool IsModifiedByActive { get; set; }
        [DataMember]
        public DateTime ItemModifiedDate { set; get; }
        [DataMember]
        public DateTime ItemCreatedDate { set; get; }
        [DataMember]
        public Guid? CloneOf { get; set; }
        [DataMember]
        public string Extension { get; set; }
    }
}
