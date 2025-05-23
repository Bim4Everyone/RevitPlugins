using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

using DevExpress.Mvvm.Native;

using dosymep.SimpleServices;

namespace RevitPlatformSettings.Views;

public sealed partial class OpenSourceWindow {
    public OpenSourceWindow() {
    }

    public OpenSourceWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        string siteType = localizationService.GetLocalizedString("OpenSourceWindow.SiteType");
        string toolType = localizationService.GetLocalizedString("OpenSourceWindow.ToolType");
        string libraryType = localizationService.GetLocalizedString("OpenSourceWindow.LibraryType");

        string freeLicense = localizationService.GetLocalizedString("OpenSourceWindow.FreeLicense");
        string communityLicense = localizationService.GetLocalizedString("OpenSourceWindow.CommunityLicense");

        string mitLicense = localizationService.GetLocalizedString("OpenSourceWindow.MITLicense");
        string gnuLicense = localizationService.GetLocalizedString("OpenSourceWindow.GNULicense");
        string apacheLicense = localizationService.GetLocalizedString("OpenSourceWindow.ApacheLicense");
        string riderLicense = localizationService.GetLocalizedString("OpenSourceWindow.RiderLicense");
        string devExpressLicense = localizationService.GetLocalizedString("OpenSourceWindow.DevExpressLicense");
        string openSourceLicense = localizationService.GetLocalizedString("OpenSourceWindow.OpenSourceLicense");
        string onlineLicense = localizationService.GetLocalizedString("OpenSourceWindow.OnlineDocumentation");

        var items = new List<OpenSourceItem> {
            new() {
                ItemType = siteType,
                AuthorName = "Icons8",
                AuthorNavigationUrl = "https://icons8.com",
                SoftwareName = "Icons8",
                SoftwareNavigationUrl = "https://icons8.com",
                LicenseName = freeLicense,
                LicenseNavigationUrl = "https://icons8.com/license"
            },
            new() {
                ItemType = toolType,
                AuthorName = "JetBrains",
                AuthorNavigationUrl = "https://www.jetbrains.com",
                SoftwareName = "JetBrains Rider",
                SoftwareNavigationUrl = "https://www.jetbrains.com/rider",
                LicenseName = riderLicense,
                LicenseNavigationUrl = "https://www.jetbrains.com/rider"
            },
            new() {
                ItemType = toolType,
                AuthorName = "JetBrains",
                AuthorNavigationUrl = "https://www.jetbrains.com",
                SoftwareName = "JetBrains pyCharm",
                SoftwareNavigationUrl = "https://www.jetbrains.com/pycharm",
                LicenseName = communityLicense,
                LicenseNavigationUrl = "https://www.jetbrains.com/pycharm"
            },
            new() {
                ItemType = toolType,
                AuthorName = "Microsoft",
                AuthorNavigationUrl = "https://microsoft.com",
                SoftwareName = "MS Visual Studio",
                SoftwareNavigationUrl = "https://visualstudio.microsoft.com/vs",
                LicenseName = communityLicense,
                LicenseNavigationUrl = "https://visualstudio.microsoft.com/vs"
            },
            new() {
                ItemType = toolType,
                AuthorName = "PVS-Studio",
                AuthorNavigationUrl = "https://pvs-studio.com",
                SoftwareName = "PVS-Studio",
                SoftwareNavigationUrl = "https://pvs-studio.com/en/pvs-studio",
                LicenseName = openSourceLicense,
                LicenseNavigationUrl = "https://pvs-studio.com/en/order/open-source-license/"
            },
            new() {
                ItemType = toolType,
                AuthorName = "nuke-build",
                AuthorNavigationUrl = "https://nuke.build",
                SoftwareName = "Nuke",
                SoftwareNavigationUrl = "https://github.com/nuke-build/nuke",
                LicenseName = mitLicense,
                LicenseNavigationUrl = "https://github.com/nuke-build/nuke/blob/develop/LICENSE"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "dosymep",
                AuthorNavigationUrl = "https://dosymep.net/",
                SoftwareName = "dosymep.Revit",
                SoftwareNavigationUrl = "https://github.com/dosymep/dosymep.Revit",
                LicenseName = mitLicense,
                LicenseNavigationUrl = "https://github.com/dosymep/dosymep.Revit/blob/master/LICENSE.md"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "dosymep",
                AuthorNavigationUrl = "https://dosymep.net/",
                SoftwareName = "dosymep.Autodesk",
                SoftwareNavigationUrl = "https://github.com/dosymep/dosymep.Autodesk",
                LicenseName = mitLicense,
                LicenseNavigationUrl = "https://github.com/dosymep/dosymep.Autodesk/blob/master/LICENSE.md"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "dosymep",
                AuthorNavigationUrl = "https://dosymep.net/",
                SoftwareName = "dosymep.SimpleServices",
                SoftwareNavigationUrl = "https://github.com/dosymep/dosymep.SimpleServices",
                LicenseName = mitLicense,
                LicenseNavigationUrl = "https://github.com/dosymep/dosymep.SimpleServices/blob/master/LICENSE.md"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "dosymep",
                AuthorNavigationUrl = "https://dosymep.net/",
                SoftwareName = "Serilog.Enrichers.Autodesk.Revit",
                SoftwareNavigationUrl = "https://github.com/dosymep/Serilog.Enrichers.Autodesk.Revit",
                LicenseName = mitLicense,
                LicenseNavigationUrl =
                    "https://github.com/dosymep/Serilog.Enrichers.Autodesk.Revit/blob/master/LICENSE.md"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "dosymep",
                AuthorNavigationUrl = "https://dosymep.net/",
                SoftwareName = "Serilog.Sinks.Autodesk.Revit",
                SoftwareNavigationUrl = "https://github.com/dosymep/Serilog.Sinks.Autodesk.Revit",
                LicenseName = mitLicense,
                LicenseNavigationUrl =
                    "https://github.com/dosymep/Serilog.Sinks.Autodesk.Revit/blob/master/LICENSE.md"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "dosymep",
                AuthorNavigationUrl = "https://dosymep.net/",
                SoftwareName = "Autodesk.Revit.Sdk.Refs",
                SoftwareNavigationUrl = "https://github.com/dosymep/Autodesk.Revit.Sdk.Refs",
                LicenseName = mitLicense,
                LicenseNavigationUrl = "https://github.com/dosymep/Autodesk.Revit.Sdk.Refs/blob/master/LICENSE.md"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "dosymep",
                AuthorNavigationUrl = "https://dosymep.net/",
                SoftwareName = "dosymep.Nuke.RevitVersions",
                SoftwareNavigationUrl = "https://github.com/dosymep/dosymep.Nuke.RevitVersions",
                LicenseName = mitLicense,
                LicenseNavigationUrl =
                    "https://github.com/dosymep/dosymep.Nuke.RevitVersions/blob/master/LICENSE.md"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "JamesNK",
                AuthorNavigationUrl = "https://github.com/JamesNK",
                SoftwareName = "Newtonsoft.Json",
                SoftwareNavigationUrl = "https://github.com/JamesNK/Newtonsoft.Json",
                LicenseName = mitLicense,
                LicenseNavigationUrl = "https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "dahall",
                AuthorNavigationUrl = "https://github.com/dahall",
                SoftwareName = "vanara",
                SoftwareNavigationUrl = "https://github.com/dahall/vanara",
                LicenseName = mitLicense,
                LicenseNavigationUrl = "https://github.com/dahall/Vanara/blob/master/LICENSE"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "serilog",
                AuthorNavigationUrl = "https://github.com/serilog",
                SoftwareName = "serilog",
                SoftwareNavigationUrl = "https://github.com/serilog/serilog",
                LicenseName = apacheLicense,
                LicenseNavigationUrl = "https://github.com/serilog/serilog/blob/dev/LICENSE"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "ninject",
                AuthorNavigationUrl = "https://github.com/ninject",
                SoftwareName = "Ninject",
                SoftwareNavigationUrl = "https://github.com/ninject/Ninject",
                LicenseName = apacheLicense,
                LicenseNavigationUrl = "https://github.com/ninject/Ninject/blob/main/LICENSE.txt"
            },
            new() {
                ItemType = libraryType,
                AuthorName = "DevExpress",
                AuthorNavigationUrl = "https://www.devexpress.com",
                SoftwareName = "DevExpress",
                SoftwareNavigationUrl = "https://www.devexpress.com",
                LicenseName = devExpressLicense,
                LicenseNavigationUrl = "https://www.devexpress.com"
            },
            new() {
                ItemType = toolType,
                AuthorName = "jeremytammik",
                AuthorNavigationUrl = "https://github.com/jeremytammik",
                SoftwareName = "RevitLookup",
                SoftwareNavigationUrl = "https://github.com/jeremytammik/RevitLookup",
                LicenseName = mitLicense,
                LicenseNavigationUrl = "https://github.com/jeremytammik/RevitLookup/blob/dev/License.md"
            },
            new() {
                ItemType = siteType,
                AuthorName = "Gui Talarico",
                AuthorNavigationUrl = "https://gtalarico.com",
                SoftwareName = "revitapidocs",
                SoftwareNavigationUrl = "https://www.revitapidocs.com",
                LicenseName = onlineLicense,
                LicenseNavigationUrl = "https://www.revitapidocs.com"
            },
            new() {
                ItemType = toolType,
                AuthorName = "pyrevitlabs",
                AuthorNavigationUrl = "https://github.com/pyrevitlabs",
                SoftwareName = "pyRevit",
                SoftwareNavigationUrl = "https://github.com/pyrevitlabs/pyRevit",
                LicenseName = gnuLicense,
                LicenseNavigationUrl = "https://github.com/pyrevitlabs/pyRevit/blob/master/LICENSE.txt"
            }
        };

        Items = items
            .OrderBy(item => item.ItemType)
            .ToObservableCollection();

        InitializeComponent();
    }

    public override string PluginName => nameof(RevitPlatformSettings);
    public override string ProjectConfigName => nameof(OpenSourceWindow);

    public ObservableCollection<OpenSourceItem> Items { get; }

    private void OnHyperlinkClick(object sender, RoutedEventArgs e) {
        Uri navigateUri = ((Hyperlink) e.OriginalSource).NavigateUri;
        Process.Start(navigateUri.AbsoluteUri);
    }
}

public class OpenSourceItem {
    public string ItemType { get; set; }

    public string AuthorName { get; set; }
    public string AuthorNavigationUrl { get; set; }

    public string SoftwareName { get; set; }
    public string SoftwareNavigationUrl { get; set; }

    public string LicenseName { get; set; }
    public string LicenseNavigationUrl { get; set; }
}
