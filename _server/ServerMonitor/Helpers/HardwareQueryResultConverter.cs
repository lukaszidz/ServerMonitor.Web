using System;
using System.Linq;
using Newtonsoft.Json;
using ServerMonitor.Models;

namespace ServerMonitor.Helpers
{
    public class HardwareQueryResultConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var hardwareResult = value as HardwareQueryResult;

            if (hardwareResult.HardwareViewList.Count() == 1)
            {
                HardwareQueryResult.HardwareView hardwareObj = hardwareResult.HardwareViewList.FirstOrDefault();
                serializer.Serialize(writer, hardwareObj);
                return;
            }
            
            writer.WriteStartArray();

            foreach (var item in hardwareResult.HardwareViewList)
            {
                serializer.Serialize(writer, item);
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return false;
        }
    }
}