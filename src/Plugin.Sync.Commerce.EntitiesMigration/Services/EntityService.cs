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

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="findEntitiesInListCommand">findEntitiesInListCommand</param>
        public EntityService(
            FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        /// <summary>
        /// Gets all Composer Templates
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>List of all composer templates</returns>
        public IList<CommerceEntity> GetAllEntities<T>(CommerceContext context) where T: CommerceEntity
        {
            CommerceList <T> commerceList = _findEntitiesInListCommand.Process<T>(context, CommerceEntity.ListName<T>(), 0, int.MaxValue).Result;
            List<CommerceEntity> composerTemplateList;
            if (commerceList == null)
            {
                composerTemplateList = null;
            }
            else
            {
                return commerceList?.Items?.Cast<CommerceEntity>().ToList();
                //composerTemplateList = items != null ? items.ToList() : null;
            }
            if (composerTemplateList == null)
            {
                composerTemplateList = new List<CommerceEntity>();
            }

            return composerTemplateList;
        }
    }
}
