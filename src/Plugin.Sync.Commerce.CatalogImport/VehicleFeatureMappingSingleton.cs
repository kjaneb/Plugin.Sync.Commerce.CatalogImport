using System.Collections.Generic;

namespace Plugin.Sync.Commerce.CatalogImport
{
    //TODO: don't use Singleton - use Policy instead
    public sealed class VehicleFeatureMappingSingleton
    {
        private static VehicleFeatureMappingSingleton _instance;
        private static readonly object readlock = new object();
        private readonly Dictionary<string, string> _jsonPropertyMapping;

        public Dictionary<string, string> JsonPropertyMapping
        {
            get { return _jsonPropertyMapping; }
        }

        public static VehicleFeatureMappingSingleton MappingInstance
        {
            get
            {
                lock (readlock)
                {
                    return _instance ?? (_instance = new VehicleFeatureMappingSingleton());
                }
            }
        }

        private VehicleFeatureMappingSingleton()
        {
            _jsonPropertyMapping = new Dictionary<string, string>();
            _jsonPropertyMapping.Add("CatalogId", "CatalogId");
            _jsonPropertyMapping.Add("CategoryId", "CategoryId");
            _jsonPropertyMapping.Add("VehicleId", "VehicleId");
            _jsonPropertyMapping.Add("Name", "Name");
            _jsonPropertyMapping.Add("DisplayName", "DisplayName");
            _jsonPropertyMapping.Add("Description", "Description");
            _jsonPropertyMapping.Add("BrandName", "BrandName");
            _jsonPropertyMapping.Add("Manufacturer", "Manufacturer");
            _jsonPropertyMapping.Add("TypeOfGood", "TypeOfGood");
            _jsonPropertyMapping.Add("ListPrice", "ListPrice");
            _jsonPropertyMapping.Add("SamClass", "SamClass");
            _jsonPropertyMapping.Add("AcceptedDate", "$.Features.AcceptedDate");
            _jsonPropertyMapping.Add("SoldDate", "$.Features.SoldDate");
            _jsonPropertyMapping.Add("HighestPriorityChangeDate", "$.Features.HighestPriorityChangeDate");
            _jsonPropertyMapping.Add("SeatType", "$.Features.SeatType");
            _jsonPropertyMapping.Add("CurbSideDoor", "$.Features.CurbSideDoor");
            _jsonPropertyMapping.Add("RoadSideDoor", "$.Features.RoadSideDoor");
            _jsonPropertyMapping.Add("Etrac", "$.Features.Etrac");
            _jsonPropertyMapping.Add("LogisticPosts", "$.Features.LogisticPosts");
            _jsonPropertyMapping.Add("CurtainSideDoor", "$.Features.CurtainSideDoor");
            _jsonPropertyMapping.Add("AirConditioner", "$.Features.AirConditioner");
        }
    }
}