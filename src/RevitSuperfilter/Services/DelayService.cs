using System;
using System.Windows.Threading;

namespace RevitSuperfilter.Services;

internal sealed class DelayService : IDelayService, IDisposable {
    private readonly Action _action;
    private readonly DispatcherTimer _timer;

    public DelayService(int interval, Action action) {
        _action = action;
        _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(interval) };
        _timer.Tick += _timer_Tick;
    }

    private void _timer_Tick(object sender, EventArgs e) {
        _timer.Stop();
        _action();
    }

    public void Action() {
        _timer.Stop();
        _timer.Start();
    }

    public void Dispose() {
        _timer.Stop();
        _timer.Tick -= _timer_Tick;
    }
}
