using System.Collections.Generic;
using Sitecore.Commerce.Core;

namespace Plugin.Sync.Commerce.EntitiesMigration.Models
{
    public class CustomComposerTemplate
    {
        public string Id { get; set; }

        public int Version { get; set; }

        public int EntityVersion { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public IList<string> LinkedEntities { get; set; }

        public IList<Tag> Tags { get; set; }

        public CustomEntityView ChildView { get; set; }
    }
}
