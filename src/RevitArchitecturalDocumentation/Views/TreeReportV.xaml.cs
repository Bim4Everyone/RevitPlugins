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
using System.Windows.Shapes;

namespace RevitArchitecturalDocumentation.Views
{
    /// <summary>
    /// Логика взаимодействия для TreeReportV.xaml
    /// </summary>
    public partial class TreeReportV
    {
        public TreeReportV()
        {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitArchitecturalDocumentation);
        public override string ProjectConfigName => nameof(TreeReportV);
    }
}
