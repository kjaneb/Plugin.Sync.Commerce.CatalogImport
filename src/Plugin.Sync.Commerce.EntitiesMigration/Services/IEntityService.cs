using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using System.Collections.Generic;

namespace Plugin.Sync.Commerce.EntitiesMigration.Services
{
    /// <summary>
    /// Composer Template Service
    /// </summary>
    public interface IEntityService
    {
        /// <summary>
        /// Gets all Composer Templates
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>List of all composer templates</returns>
        EntityCollectionModel GetAllEntities<T>(CommerceContext context) where T: CommerceEntity;
    }
}
