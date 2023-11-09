using System;
using System.Reflection;

using pyRevitLabs.Json.Serialization;

namespace RevitClashDetective.Models.FilterModel {
    public class RevitClashesSerializationBinder : ISerializationBinder {

        DefaultSerializationBinder defaultBinder = new DefaultSerializationBinder();

        public void BindToName(Type serializedType, out string assemblyName, out string typeName) {
            if(serializedType.Assembly.GetName().Name.Equals(GetCurrentAssemblyName())) {
                assemblyName = GetCurrentAssemblyName();
                typeName = serializedType.FullName;
            } else {
                defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        public Type BindToType(string assemblyName, string typeName) {
            if(assemblyName.Equals(GetCurrentAssemblyName())) {
                return Assembly.GetExecutingAssembly().GetType(typeName);
            } else {
                return defaultBinder.BindToType(assemblyName, typeName);
            }
        }

        private string GetCurrentAssemblyName() {
            return Assembly.GetExecutingAssembly().GetName().Name;
        }
    }
}
