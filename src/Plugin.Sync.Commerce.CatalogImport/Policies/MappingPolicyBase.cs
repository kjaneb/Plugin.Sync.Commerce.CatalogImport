﻿using System.Collections.Generic;
using Sitecore.Commerce.Core;


namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class MappingPolicyBase: Policy
    {
        public bool ClearFieldValues { get; set; }
        public string IdPath { get; set; }
        public string NamePath { get; set; }
        public string DisplayNamePath { get; set; }
        public string DescriptionPath { get; set; }
        public string ParentCategoryNamePath { get; set; }
        public string CatalogNamePath { get; set; }
        public List<string> ComposerFieldsRootPaths { get; set; }
        public Dictionary<string, string> ComposerFieldsPaths { get; set; }
        public List<string> CustomFieldsRootPaths { get; set; }
        public Dictionary<string, string> CustomFieldsPaths { get; set; }
        public string UpdatedItemsList { get; set; }
        public string DefaultCatalogName { get; set; }
        public string DefaultCategoryName { get; set; }

    }
}
