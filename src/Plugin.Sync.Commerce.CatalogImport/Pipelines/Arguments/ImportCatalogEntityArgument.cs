﻿using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Sitecore.Commerce.Core;
using System;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments
{
    public class ImportCatalogEntityArgument : PipelineArgument
    {
        public ImportCatalogEntityArgument(JObject request, MappingPolicyBase mappingPolicy, Type commerceEntityType)
        {
            Request = request;
            MappingPolicy = mappingPolicy;
            CommerceEntityType = commerceEntityType;
        }
        public JObject Request { get; set; }
        public MappingPolicyBase MappingPolicy { get; set; }
        public Type CommerceEntityType { get; set; }
    }
}