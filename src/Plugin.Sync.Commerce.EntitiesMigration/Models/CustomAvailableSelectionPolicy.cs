using System.Collections.Generic;

namespace Plugin.Sync.Commerce.EntitiesMigration.Models
{
    public class CustomAvailableSelectionPolicy
    {
        public string PolicyId { get; set; }

        public IList<CustomSelection> Selection { get; set; }

        public bool AllowMultiSelect { get; set; }
    }
}
