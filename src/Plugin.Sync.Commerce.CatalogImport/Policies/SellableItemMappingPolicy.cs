﻿using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;


namespace Plugin.Sync.Commerce.CatalogImport.Policies
{
    public class SellableItemMappingPolicy : MappingPolicyBase
    {
        public List<CustomComponentPolicy> CustomComponentPaths { get; set; }
    }
}
