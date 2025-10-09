using System;
using System.Windows;

using DevExpress.Xpf.Grid;

using RevitClashDetective.Models;
using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Views;
internal partial class NavigatorView {
    public NavigatorView(SettingsConfig config) {
        if(config is null) {
            throw new ArgumentNullException(nameof(config));
        }

        InitializeComponent();
        string[] paramNames = config.ParamNames;
        int startFirst = 9;
        int startSecond = 16;
        for(int i = 0; i < paramNames.Length; i++, startFirst++, startSecond += 2) {
            var firstElementColumn = new GridColumn() {
                Header = $"{paramNames[i]} (1)",
                VisibleIndex = startFirst,
                FieldName = $"{nameof(IClashViewModel.FirstElementParams)}.{ClashViewModel.ElementParamFieldName}{i}"
            };
            var secondElementColumn = new GridColumn() {
                Header = $"{paramNames[i]} (2)",
                VisibleIndex = startSecond,
                FieldName = $"{nameof(IClashViewModel.SecondElementParams)}.{ClashViewModel.ElementParamFieldName}{i}"
            };
            _dg.Columns.Add(firstElementColumn);
            _dg.Columns.Add(secondElementColumn);
        }
    }

    public override string PluginName => nameof(RevitClashDetective);
    public override string ProjectConfigName => nameof(NavigatorView);

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
