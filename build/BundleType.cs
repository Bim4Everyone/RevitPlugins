﻿using System.ComponentModel;

using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<BundleType>))]
class BundleType : Enumeration {
    public static readonly BundleType PushButton = new() {Value = nameof(PushButton)};
    public static readonly BundleType InvokeButton = new() {Value = nameof(InvokeButton)};

    public string ExtensionWithDot => "." + Value.ToLower();
}