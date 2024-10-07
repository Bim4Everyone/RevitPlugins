using System.Windows;

using RevitValueModifier.ViewModels;

using TextBox = System.Windows.Controls.TextBox;

namespace RevitValueModifier.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitValueModifier);
        public override string ProjectConfigName => nameof(MainWindow);
        
        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        /// <summary>
        /// Обработчик события выполняется в момент, когда TextBox с маской значения теряет фокус.
        /// Это происходит, когда пользователь поставил каретку TextBox в нужное положение и 
        /// перевел курсор на ComboBox с выбором параметра для добалвения.
        /// Обработчик реализует запись индекса каретки TextBox (вертикальная черта внутри TextBox) в свойство ViewModel,
        /// для последующего использования в методе команды для добавления параметра в маску.
        /// Попытки выполнить передачу значения индекса через CommandParameter команды, которая выполняется при выборе
        /// параметра для добавления не увенчались успехом (всегда передается 0). 
        /// Тестирование передачи значения через Binding между размещенными контролами также не имело эффекта.
        /// Чтение форумов показало, что данная ошибка распространена.
        /// Единственный найденный подход, когда значения передавались - при помощи обработчика события.
        /// </summary>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox) sender;
            MainViewModel mainViewModel = (MainViewModel) DataContext;
            mainViewModel.ParamValueMaskCaretIndex = textBox.CaretIndex;
        }
    }
}
