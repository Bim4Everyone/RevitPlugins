using System;
using System.Reflection;

using pyRevitLabs.Json.Serialization;

namespace RevitParamsChecker.Services;

internal class JsonSerializationBinder : ISerializationBinder {
    private readonly DefaultSerializationBinder _defaultBinder = new();

    public void BindToName(Type serializedType, out string assemblyName, out string typeName) {
        if(serializedType.Assembly.GetName().Name.Equals(GetCurrentAssemblyName())) {
            assemblyName = GetCurrentAssemblyName();
            typeName = serializedType.FullName;
        } else {
            _defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
        }
    }

    public Type BindToType(string assemblyName, string typeName) {
        return assemblyName?.Equals(GetCurrentAssemblyName()) ?? false
            ? Assembly.GetExecutingAssembly().GetType(typeName)
            : _defaultBinder.BindToType(assemblyName, typeName);
    }

    private string GetCurrentAssemblyName() {
        return Assembly.GetExecutingAssembly().GetName().Name;
    }
}
