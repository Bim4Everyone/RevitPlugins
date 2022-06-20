using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using dosymep.WPF.Views;

namespace RevitLintelPlacement.Views {
    /// <summary>
    /// Логика взаимодействия для LintelsTabView.xaml
    /// </summary>
    public partial class LintelsView {

        public LintelsView() {
            InitializeComponent();
            _gridControl.GroupBy(_gridControl.Columns.Last());
        }

        public override string PluginName => nameof(RevitLintelPlacement);
        public override string ProjectConfigName => nameof(LintelsView);
    }
}
