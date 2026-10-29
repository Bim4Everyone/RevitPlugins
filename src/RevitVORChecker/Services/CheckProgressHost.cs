using System.Windows.Threading;

using RevitVORChecker.Views;

namespace RevitVORChecker.Services;

/// <summary>
/// Окно хода проверки в отдельном потоке: основной поток Revit занят проверкой и не перерисовывает окна.
/// </summary>
internal sealed class CheckProgressHost : IProgress<int>, IDisposable {
    private readonly CancellationTokenSource _cancellation = new();
    private readonly ManualResetEventSlim _windowReady = new(false);
    private Thread _thread;
    private Dispatcher _dispatcher;
    private CheckProgressWindow _window;

    private CheckProgressHost() {
    }

    /// <summary>
    /// Отмена, запрошенная кнопкой окна.
    /// </summary>
    public CancellationToken CancellationToken => _cancellation.Token;

    /// <summary>
    /// Показывает окно и ждёт, пока оно появится.
    /// </summary>
    public static CheckProgressHost Show(
        string title,
        string stepFormat,
        string cancelText,
        string cancellingText,
        int stepCount) {

        var host = new CheckProgressHost();
        host._thread = new Thread(() => {
            // необработанное исключение в этом потоке закрыло бы Revit — проверка идёт дальше без окна
            try {
                host._dispatcher = Dispatcher.CurrentDispatcher;
                host._window = new CheckProgressWindow(
                    title,
                    stepFormat,
                    cancelText,
                    cancellingText,
                    stepCount,
                    host._cancellation.Cancel);
                host._window.Closed += (_, _) => host._dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                host._window.Show();
                host._windowReady.Set();
                Dispatcher.Run();
            } catch(Exception) {
                host._window = null;
            } finally {
                host._windowReady.Set();
            }
        }) {
            IsBackground = true,
            Name = "RevitVORChecker progress"
        };
        host._thread.SetApartmentState(ApartmentState.STA);
        host._thread.Start();
        host._windowReady.Wait(TimeSpan.FromSeconds(10));
        return host;
    }

    /// <summary>
    /// Номер выполненного шага; передаётся в окно сразу (Progress&lt;T&gt; отложил бы его до освобождения потока Revit).
    /// </summary>
    public void Report(int step) {
        Invoke(window => window.SetStep(step));
    }

    public void ReportStatus(string status) {
        Invoke(window => window.SetStatus(status));
    }

    public void Dispose() {
        Invoke(window => window.CloseByHost());
        _thread?.Join(TimeSpan.FromSeconds(5));
        // _windowReady не освобождается: поток окна ставит его в finally и после Join с таймаутом
        _cancellation.Dispose();
    }

    private void Invoke(Action<CheckProgressWindow> action) {
        if(_dispatcher == null || _window == null || _dispatcher.HasShutdownStarted) {
            return;
        }

        _dispatcher.BeginInvoke(new Action(() => action(_window)));
    }
}
