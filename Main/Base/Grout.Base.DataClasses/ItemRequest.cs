using System;
using System.Collections.Generic;

namespace Grout.Base.DataClasses
{

    public class ItemRequest
    {
        public Guid ItemId { get; set; }

        public DataSourceMappingInfo DatasourceDetails { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid? CategoryId { get; set; }

        public byte[] ItemContent { get; set; }

        public ItemType ItemType { get; set; }

        public DataSourceDefinition DataSourceDefinition { get; set; }

        public string UploadedReportName { get; set; }

        public List<DataSourceMappingInfo> DataSourceMappingInfo { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string VersionComment { get; set; }

        public string ServerPath { get; set; }
    }
}
