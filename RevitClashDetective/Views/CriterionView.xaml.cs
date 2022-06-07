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

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for CriterionView.xaml
    /// </summary>
    public partial class CriterionView : UserControl {
        public CriterionView() {
            InitializeComponent();
        }

        private void ComboBox_Drop(object sender, DragEventArgs e) {
            int i = 0;
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e) {
            base.OnKeyDown(e);
            var c = sender as ComboBox;

            if(c.IsEditable &&
                c.IsDropDownOpen == false &&
                c.StaysOpenOnEdit) {
                c.IsDropDownOpen = true;
            }
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e) {
            int i = 0;
            var c = sender as ComboBox;
            c.IsDropDownOpen = true;
        }
    }
}
