using Esri.ArcGISRuntime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvD_Dev
{
    class SpatialReferenceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(SpatialReference));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Result into a JObject
            JObject jo = JObject.Load(reader);

            // Read the properties which will be used as constructor parameters
            string WkText = (string)jo["WkText"];

            // Construct the Result object using the non-default constructor
            SpatialReference result = new SpatialReference(WkText);

            // (If anything else needs to be populated on the result object, do that here)

            // Return the result
            return result;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
