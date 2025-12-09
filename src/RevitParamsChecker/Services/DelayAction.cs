using System;
using System.Windows.Threading;

namespace RevitParamsChecker.Services;

internal class DelayAction {
    private readonly Action _action;
    private readonly DispatcherTimer _timer;

    public DelayAction(int ticks, Action action) {
        _action = action;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ticks) };
        _timer.Tick += OnTimerTick;
    }

    private void OnTimerTick(object sender, EventArgs e) {
        _timer.Stop();
        _action();
    }

    public void Action() {
        _timer.Stop();
        _timer.Start();
    }
}
