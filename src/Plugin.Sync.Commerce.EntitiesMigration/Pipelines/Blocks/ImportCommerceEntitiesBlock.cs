using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Plugin.Sync.Commerce.EntitiesMigration.Services;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Blocks
{
    [PipelineDisplayName("ImportCommerceEntitiesBlock")]
    public class ImportCommerceEntitiesBlock : PipelineBlock<ImportEntitiesArgument, bool, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        /// <summary>
        /// Find Entities In List Command
        /// </summary>
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
        /// <summary>
        /// Commerce Commander
        /// </summary>
        private readonly PersistEntityCommand _persistEntityCommand;
        //public CreateEntityCommand MyProperty { get; set; }

        /// <summary>
        /// Delete Entity Command
        /// </summary>
        private readonly DeleteEntityCommand _deleteEntityCommand;

        private readonly FindEntityCommand _findEntityCommand;
        private readonly AssociateCategoryToParentCommand _associateCategoryToParentCommand;
        private readonly AssociateSellableItemToParentCommand _associateSellableItemToParentCommand;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="commerceCommander">commerceCommander</param>
        /// <param name="findEntitiesInListCommand">findEntitiesInListCommand</param>
        public ImportCommerceEntitiesBlock(
            CommerceCommander commerceCommander,
            PersistEntityCommand persistEntityCommand,
            DeleteEntityCommand deleteEntityCommand,
            FindEntityCommand findEntityCommand,
            FindEntitiesInListCommand findEntitiesInListCommand,
            AssociateCategoryToParentCommand associateCategoryToParentCommand,
            AssociateSellableItemToParentCommand associateSellableItemToParentCommand)
        {
            _commerceCommander = commerceCommander;
            _persistEntityCommand = persistEntityCommand;
            _deleteEntityCommand = deleteEntityCommand;
            _findEntityCommand = findEntityCommand;
            _findEntitiesInListCommand = findEntitiesInListCommand;
            _associateCategoryToParentCommand = associateCategoryToParentCommand;
            _associateSellableItemToParentCommand = associateSellableItemToParentCommand;
            //_entityService = entityService;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns>flag if the process was sucessfull</returns>
        public override async Task<bool> Run(ImportEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            try
            {
                foreach (var commerceEntity in arg.EntityModel.Entities)
                {
                    var entity = Cast(commerceEntity, arg.EntityModel.EntityType);
                    var existingEntity = await _findEntityCommand.Process(context.CommerceContext, entity.GetType(), entity.Id);
                    //var existingEntity = existingEntities.Items.FirstOrDefault(item => item.Id.Equals(entity.Id));
                    if (existingEntity != null)
                    {
                        // Try to increase version count instead of delete
                        DeleteEntityArgument result = await _deleteEntityCommand.Process(context.CommerceContext, existingEntity.Id);
                        if (!result.Success)
                        {
                            Log.Error($"{this.GetType().Name}: Deletion of {existingEntity.Id} failed - new Entity was not imported");
                        }
                        else
                        {
                            entity.Version = 0;
                        }
                    }

                    //entity.EntityVersion = 1;
                    //entity.Version = 0;
                    entity.IsPersisted = false;
                    context.CommerceContext.AddUniqueEntityByType(entity);

                    
                    var persistResult = await this._persistEntityCommand.Process(context.CommerceContext,  entity);
                }

                if (arg.EntityModel.EntityType == typeof(Category))
                {
                    await AssociateCategoriesToParent(arg, context);
                }

                if (arg.EntityModel.EntityType == typeof(SellableItem))
                {
                    await AssociateSellableItemsToParent(arg, context);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            await Task.CompletedTask;

            return true;
        }

        private dynamic Cast(dynamic obj, Type castTo)
        {
            return Convert.ChangeType(obj, castTo);
        }

        private async Task AssociateSellableItemsToParent(ImportEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            CommerceList<Catalog> catalogs = _findEntitiesInListCommand.Process<Catalog>(context.CommerceContext, CommerceEntity.ListName<Catalog>(), 0, int.MaxValue).Result;
            CommerceList<Category> categories = _findEntitiesInListCommand.Process<Category>(context.CommerceContext, CommerceEntity.ListName<Category>(), 0, int.MaxValue).Result;
            foreach (var entity in arg.EntityModel.Entities)
            {
                var sellableItem = entity as SellableItem;
                if (!string.IsNullOrEmpty(sellableItem.ParentCatalogList) || !string.IsNullOrEmpty(sellableItem.ParentCategoryList))
                {
                    var parentCatalog = catalogs.Items.FirstOrDefault(i => i.SitecoreId.Equals(sellableItem.ParentCatalogList));
                    if (parentCatalog != null)
                    {
                        if (string.IsNullOrEmpty(sellableItem.ParentCategoryList))
                        {
                            await _associateSellableItemToParentCommand.Process(context.CommerceContext, parentCatalog.Id, parentCatalog.Id, sellableItem.Id);
                        }
                        else
                        {
                            foreach (string categorySitecoreId in sellableItem.ParentCategoryList.Split('|'))
                            {
                                var parentCategory = categories.Items.FirstOrDefault(i => i.SitecoreId.Equals(categorySitecoreId));
                                if (parentCategory != null)
                                {
                                    await _associateSellableItemToParentCommand.Process(context.CommerceContext, parentCatalog.Id, parentCategory.Id, sellableItem.Id);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task AssociateCategoriesToParent(ImportEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            CommerceList<Catalog> catalogs = _findEntitiesInListCommand.Process<Catalog>(context.CommerceContext, CommerceEntity.ListName<Catalog>(), 0, int.MaxValue).Result;
            CommerceList<Category> categories = _findEntitiesInListCommand.Process<Category>(context.CommerceContext, CommerceEntity.ListName<Category>(), 0, int.MaxValue).Result;
            foreach (var entity in arg.EntityModel.Entities)
            {
                var category = entity as Category;
                if (!string.IsNullOrEmpty(category.ParentCatalogList) || !string.IsNullOrEmpty(category.ParentCategoryList))
                {
                    var parentCatalog = catalogs.Items.FirstOrDefault(i => i.SitecoreId.Equals(category.ParentCatalogList));
                    if (parentCatalog != null)
                    {
                        if (string.IsNullOrEmpty(category.ParentCategoryList))
                        {
                            await _associateCategoryToParentCommand.Process(context.CommerceContext, parentCatalog.Id, parentCatalog.Id, category.Id);
                        }
                        else
                        {
                            foreach (string categorySitecoreId in category.ParentCategoryList.Split('|'))
                            {
                                var parentCategory = categories.Items.FirstOrDefault(i => i.SitecoreId.Equals(categorySitecoreId));
                                if (parentCategory != null)
                                {
                                    await _associateCategoryToParentCommand.Process(context.CommerceContext, parentCatalog.Id, parentCategory.Id, category.Id);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
