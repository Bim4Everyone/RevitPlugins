using System;
using System.Reflection;

using pyRevitLabs.Json.Serialization;

namespace RevitOpeningPlacement.Models.Configs {
    internal class OpeningSerializationBinder : ISerializationBinder {

        private readonly DefaultSerializationBinder _defaultBinder = new DefaultSerializationBinder();

        public void BindToName(Type serializedType, out string assemblyName, out string typeName) {
            if(serializedType.Assembly.GetName().Name.Equals(GetCurrentAssemblyName())) {
                assemblyName = GetCurrentAssemblyName();
                typeName = serializedType.FullName;
            } else {
                _defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        public Type BindToType(string assemblyName, string typeName) {
            if(assemblyName.Equals(GetCurrentAssemblyName())) {
                return Assembly.GetExecutingAssembly().GetType(typeName);
            } else {
                return _defaultBinder.BindToType(assemblyName, typeName);
            }
        }

        private string GetCurrentAssemblyName() {
            return Assembly.GetExecutingAssembly().GetName().Name;
        }
    }
}
