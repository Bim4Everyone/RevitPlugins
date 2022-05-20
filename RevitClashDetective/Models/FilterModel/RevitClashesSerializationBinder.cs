using System;
using pyRevitLabs.Json.Serialization;
using System.Reflection;

namespace RevitClashDetective.Models.FilterModel {
    public class RevitClashesSerializationBinder : ISerializationBinder {

        DefaultSerializationBinder defaultBinder = new DefaultSerializationBinder();

        public void BindToName(Type serializedType, out string assemblyName, out string typeName) {
            if(serializedType.Assembly.GetName().Name.Equals(nameof(RevitClashDetective))) {
                assemblyName = nameof(RevitClashDetective);
                typeName = serializedType.FullName;
            } else {
                defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        public Type BindToType(string assemblyName, string typeName) {
            if(assemblyName.Equals(nameof(RevitClashDetective))) {
                return Assembly.GetExecutingAssembly().GetType(typeName);
            } else {
                return defaultBinder.BindToType(assemblyName, typeName);
            }
        }
    }
}
