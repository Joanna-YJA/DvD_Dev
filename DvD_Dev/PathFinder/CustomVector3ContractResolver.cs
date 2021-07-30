using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Numerics;
using System.Reflection;

namespace DvD_Dev
{
    class CustomVector3ContractResolver : DefaultContractResolver
    {
        public static readonly CustomVector3ContractResolver Instance = new CustomVector3ContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.DeclaringType == typeof(Vector3))
            {
                if (property.PropertyName.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    property.PropertyName = "x";
                } else if (property.PropertyName.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    property.PropertyName = "y";
                } else if (property.PropertyName.Equals("Z", StringComparison.OrdinalIgnoreCase))
                {
                    property.PropertyName = "z";
                }
            }
            return property;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType == typeof(Vector3))
            {
                contract.IsReference = false;
            }

            return contract;
        }
    }
}

