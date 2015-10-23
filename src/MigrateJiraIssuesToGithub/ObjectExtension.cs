using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace MigrateJiraIssuesToGithub
{
    public static class ObjectExtension
    {
        public static String ToJson(this Object instance)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            return instance == null ? String.Empty : JsonConvert.SerializeObject(instance, settings);
        }
    }
}