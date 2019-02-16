using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServerMonitor.Helpers;
using static ServerMonitor.Helpers.HardwareManager;

namespace ServerMonitor.Models
{
    [JsonConverter(typeof(HardwareQueryResultConverter))]
    public class HardwareQueryResult
    {
        public HardwareQueryResult(IList<Hardware> hardwareList)
        {
            HardwareViewList = hardwareList.Select(h => new HardwareView { Name = h.Name, Data = h.Data });
        }

        public IEnumerable<HardwareView> HardwareViewList { get; }

        public class HardwareView
        {
            [JsonProperty("key")]
            public string Name { get; set; }
            [JsonProperty("data")]
            public List<Data<double>> Data { get; set; }
        }
    }
}