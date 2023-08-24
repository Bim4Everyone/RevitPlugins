using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<RevitConfiguration>))]
class RevitConfiguration : Enumeration {
    public static readonly RevitConfiguration D2020 = new() {Value = nameof(D2020), Version = 2020};
    public static readonly RevitConfiguration D2021 = new() {Value = nameof(D2021), Version = 2021};
    public static readonly RevitConfiguration D2022 = new() {Value = nameof(D2022), Version = 2022};
    public static readonly RevitConfiguration D2023 = new() {Value = nameof(D2023), Version = 2023};
    public static readonly RevitConfiguration D2024 = new() {Value = nameof(D2024), Version = 2024};

    public static readonly RevitConfiguration R2020 = new() {Value = nameof(R2020), Version = 2020};
    public static readonly RevitConfiguration R2021 = new() {Value = nameof(R2021), Version = 2021};
    public static readonly RevitConfiguration R2022 = new() {Value = nameof(R2022), Version = 2022};
    public static readonly RevitConfiguration R2023 = new() {Value = nameof(R2023), Version = 2023};
    public static readonly RevitConfiguration R2024 = new() {Value = nameof(R2024), Version = 2024};

    public int Version { get; protected set; }

    public static implicit operator string(RevitConfiguration configuration) {
        return configuration.Value;
    }

    public static IEnumerable<RevitConfiguration> GetDebugConfiguration() {
        return typeof(RevitConfiguration).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(item => item.Name.StartsWith("D"))
            .Select(item => item.GetValue(null))
            .OfType<RevitConfiguration>();
    }

    public static IEnumerable<RevitConfiguration> GetReleaseConfiguration() {
        return typeof(RevitConfiguration).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(item => item.Name.StartsWith("R"))
            .Select(item => item.GetValue(null))
            .OfType<RevitConfiguration>();
    }
}