using System.ComponentModel;
using System.Windows;

namespace RevitVORChecker.Views;

/// <summary>
/// Окно хода проверки: шаг, прогресс-бар, текущее действие и отмена.
/// </summary>
/// <remarks>
/// Работает в отдельном потоке (основной поток Revit занят проверкой), поэтому это обычное окно WPF
/// без сервисов платформы. Все тексты передаются готовыми.
/// </remarks>
public partial class CheckProgressWindow {
    private readonly string _title;
    private readonly string _stepFormat;
    private readonly string _cancellingText;
    private readonly Action _cancel;
    private bool _isClosingByHost;

    public CheckProgressWindow(
        string title,
        string stepFormat,
        string cancelText,
        string cancellingText,
        int stepCount,
        Action cancel) {

        InitializeComponent();
        _title = title;
        _stepFormat = stepFormat;
        _cancellingText = cancellingText;
        _cancel = cancel;
        CancelButton.Content = cancelText;
        StepProgress.Maximum = stepCount;
        SetStep(0);
    }

    public void SetStep(int step) {
        StepProgress.Value = Math.Min(step, StepProgress.Maximum);
        StepText.Text = string.Format(_stepFormat, Math.Min(step, (int) StepProgress.Maximum), (int) StepProgress.Maximum);
        // ход проверки виден на кнопке панели задач, когда окно свёрнуто
        TaskbarProgress.ProgressValue = StepProgress.Maximum > 0 ? StepProgress.Value / StepProgress.Maximum : 0;
        Title = string.Format("{0:0}% — {1}", TaskbarProgress.ProgressValue * 100, _title);
    }

    public void SetStatus(string status) {
        StatusText.Text = status;
    }

    /// <summary>
    /// Закрывает окно по окончании проверки.
    /// </summary>
    public void CloseByHost() {
        _isClosingByHost = true;
        Close();
    }

    /// <summary>
    /// Крестик окна — та же отмена, что и кнопка: окно закроется, когда проверка остановится.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e) {
        if(!_isClosingByHost) {
            e.Cancel = true;
            RequestCancel();
        }

        base.OnClosing(e);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) {
        RequestCancel();
    }

    private void RequestCancel() {
        if(!CancelButton.IsEnabled) {
            return;
        }

        CancelButton.IsEnabled = false;
        StatusText.Text = _cancellingText;
        _cancel();
    }
}
