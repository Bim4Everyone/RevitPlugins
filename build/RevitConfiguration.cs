using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<RevitConfiguration>))]
class RevitConfiguration : Enumeration {
    static RevitConfiguration D2020 = new RevitConfiguration {Value = nameof(D2020), Version = 2020};
    static RevitConfiguration D2021 = new RevitConfiguration {Value = nameof(D2021), Version = 2021};

    static RevitConfiguration D2022 = new RevitConfiguration {Value = nameof(D2022), Version = 2022};
    // static RevitConfiguration D2023 => new RevitConfiguration {Value = nameof(D2023), Version = 2023};

    static RevitConfiguration R2020 = new RevitConfiguration {Value = nameof(R2020), Version = 2020};
    static RevitConfiguration R2021 = new RevitConfiguration {Value = nameof(R2021), Version = 2021};

    static RevitConfiguration R2022 = new RevitConfiguration {Value = nameof(R2022), Version = 2022};
    // static RevitConfiguration R2023 => new RevitConfiguration {Value = nameof(R2023), Version = 2023};

    public int Version { get; protected set; }

    public static implicit operator string(RevitConfiguration configuration) {
        return configuration.Value;
    }

    public static IEnumerable<RevitConfiguration> GetDebugConfiguration() {
        return typeof(RevitConfiguration).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(item => item.Name.StartsWith("D"))
            .Select(item => item.GetValue(null))
            .OfType<RevitConfiguration>();
    }

    public static IEnumerable<RevitConfiguration> GetReleaseConfiguration() {
        return typeof(RevitConfiguration).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(item => item.Name.StartsWith("R"))
            .Select(item => item.GetValue(null))
            .OfType<RevitConfiguration>();
    }
}