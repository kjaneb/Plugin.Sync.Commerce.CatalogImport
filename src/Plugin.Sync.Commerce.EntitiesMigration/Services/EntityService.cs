using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.Sync.Commerce.EntitiesMigration.Services
{
    /// <summary>
    /// Composer Template Service
    /// </summary>
    public class EntityService : IEntityService
    {
        /// <summary>
        /// Find Entities In List Command
        /// </summary>
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
        ///// <summary>
        ///// Commerce Commander
        ///// </summary>
        //private readonly PersistEntityCommand _persistEntityCommand;

        ///// <summary>
        ///// Delete Entity Command
        ///// </summary>
        //private readonly DeleteEntityCommand _deleteEntityCommand;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="findEntitiesInListCommand">findEntitiesInListCommand</param>
        public EntityService(
            FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        //public bool ImportEntities<T>(CommerceContext context, EntityCollectionModel entitiesModel) where T : CommerceEntity
        //{

        //    CommerceList<T> existingEntities = _findEntitiesInListCommand.Process<T>(context, CommerceEntity.ListName<T>(), 0, int.MaxValue).Result;
        //    foreach (var entity in entitiesModel.Entities)
        //    {

        //        var existingEntity = existingEntities.Items.FirstOrDefault(item => item.Id.Equals(entity.Id));
        //        if (existingEntity != null)
        //        {
        //            // Try to increase version count instead of delete
        //            DeleteEntityArgument result = await _deleteEntityCommand.Process(context.CommerceContext, existingComposerTemplate.Id);
        //            if (!result.Success)
        //            {
        //                Log.Error($"OverrideComposerTemplatesBlock: Deletion of {newComposerTemplate.Id} failed - new Template was not imported");
        //            }
        //        }

        //    }

        //    return true;
        //}

        /// <summary>
        /// Gets all Composer Templates
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>List of all composer templates</returns>
        public EntityCollectionModel GetAllEntities<T>(CommerceContext context) where T : CommerceEntity
        {
            CommerceList<T> commerceList = _findEntitiesInListCommand.Process<T>(context, CommerceEntity.ListName<T>(), 0, int.MaxValue).Result;
            List<CommerceEntity> entityList;
            if (commerceList == null)
            {
                entityList = null;
            }
            else
            {
                entityList = commerceList?.Items?.Cast<CommerceEntity>().ToList();
            }
            if (entityList == null)
            {
                entityList = new List<CommerceEntity>();
            }

            return new EntityCollectionModel
            {
                EntityType = typeof(T),
                Entities = entityList
            };
        }
    }
}
