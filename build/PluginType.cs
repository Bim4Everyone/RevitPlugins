using System.ComponentModel;

using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<PluginType>))]
internal sealed class PluginType : Enumeration {
    public static readonly PluginType Default = new() {Value = nameof(Default)};
    public static readonly PluginType DevExpress = new() {Value = nameof(DevExpress)};
    public static readonly PluginType WpfUi = new() {Value = nameof(WpfUi) };
}
