﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Entities;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    public class ParseSellableItemJsonBlock : PipelineBlock<ImportSellableItemArgument, ImportSellableItemResponse, CommercePipelineExecutionContext>
    {
        public ParseSellableItemJsonBlock()
        {

        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportSellableItemResponse> Run(ImportSellableItemArgument arg, CommercePipelineExecutionContext context)
        {
            var mappingPolicy = context.CommerceContext.GetPolicy<SellableItemMappingPolicy>();
            arg.EntityData = new CatalogEntityData
            {
                Id = arg.JsonData.SelectValue<string>(mappingPolicy.IdPath),
                Name = arg.JsonData.SelectValue<string>(mappingPolicy.NamePath),
                DisplayName = arg.JsonData.SelectValue<string>(mappingPolicy.DisplayNamePath),
                CatalogName = arg.JsonData.SelectValue<string>(mappingPolicy.CatalogNamePath),
                Description = arg.JsonData.SelectValue<string>(mappingPolicy.DescriptionPath),
                ParentCategoryName = arg.JsonData.SelectValue<string>(mappingPolicy.ParentCategoryNamePath),
                ComposerFields = arg.JsonData.SelectMappedValues(mappingPolicy.ComposerFieldsPaths),
                CustomFields = arg.JsonData.SelectMappedValues(mappingPolicy.CustomFieldsPaths),
            };

            if (string.IsNullOrEmpty(arg.EntityData.CatalogName))
            {
                arg.EntityData.CatalogName = mappingPolicy.DefaultCatalogName;
            }

            if (string.IsNullOrEmpty(arg.EntityData.ParentCategoryName))
            {
                arg.EntityData.CatalogName = mappingPolicy.DefaultCategoryName;
            }

            await Task.CompletedTask;

            return new ImportSellableItemResponse
            {
                //TODO: clan up this 
            };
        }
    }
}