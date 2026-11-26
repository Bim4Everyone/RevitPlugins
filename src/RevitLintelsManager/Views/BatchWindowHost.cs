using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using dosymep.SimpleServices;

using Ninject;

using pyRevitLabs.Json;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Views;

/// <summary>Показывает окно прогресса Dosymep в собственном WPF-процессе.</summary>
internal static class BatchWindowHost {
    /// <summary>Сообщает исходной сессии о готовности окна и запросе отмены.</summary>
    public static void Run(string json, string channel, int parentId) {
        AppDomain.CurrentDomain.AssemblyResolve += dosymep.AppDomainExtensions.CurrentDomain_AssemblyResolve;
        using var parent = Process.GetProcessById(parentId);
        using var mapping = MemoryMappedFile.OpenExisting(channel, MemoryMappedFileRights.Read);
        using var counter = mapping.CreateViewAccessor(0, SystemPluginConfig.BatchChannelSize, MemoryMappedFileAccess.Read);
        var options = JsonConvert.DeserializeObject<BatchProgressOptions>(json);
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var language = CultureInfo.GetCultureInfo(options.Language);
        CultureInfo.CurrentUICulture = language;
        CultureInfo.CurrentCulture = language;
        var dispatcher = Dispatcher.CurrentDispatcher;
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
        using var kernel = new StandardKernel();
        RevitLintelsManagerCommand.ConfigureBatchWindowDependencies(kernel, new BatchTheme(options.Theme));
        using var service = kernel.Get<IProgressDialogService>();
        service.MaxValue = options.Count;
        service.StepValue = 1;
        service.DisplayTitleFormat = options.ProgressFormat;
        var progress = service.CreateAsyncProgress();
        using var cancel = service.CreateCancellationToken().Register(() => Signal(SystemPluginConfig.BatchCancel));
        var popup = app.Windows.Cast<Window>().Single();
        popup.Title = string.Format(language, options.ProgressFormat, 0, options.Count);
        popup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        popup.ShowInTaskbar = true;
        bool released = false;
        popup.Closing += (_, e) => {
            if(!released) {
                e.Cancel = true;
                Signal(SystemPluginConfig.BatchCancel);
            }
        };
        if(popup.FindName(SystemPluginConfig.BatchCancelButton) is ContentControl button) {
            button.Content = options.CancelText;
        }
        service.Show();
        int shown = -1;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SystemPluginConfig.BatchRefreshMilliseconds) };
        timer.Tick += (_, _) => {
            int completed = counter.ReadInt32(0);
            if(completed != shown) {
                // Dosymep принимает индекс с нуля и показывает индекс + 1.
                popup.Title = string.Format(language, options.ProgressFormat, completed, options.Count);
                progress.Report(completed - 1);
                shown = completed;
            }
            if(counter.ReadBoolean(SystemPluginConfig.BatchReleaseOffset) || parent.HasExited) {
                timer.Stop();
                released = true;
                service.Close();
                app.Shutdown();
            }
        };
        timer.Start();
        Signal(SystemPluginConfig.BatchReady);
        app.Run();
    }

    // Канал сигналов не использует диспетчер или контекст API родителя.
    private static void Signal(string message) {
        Console.Out.WriteLine(message);
        Console.Out.Flush();
    }

    /// <summary>Тема текущей сессии для окна Dosymep.</summary>
    private sealed class BatchTheme(UIThemes theme) : IUIThemeService {
        public UIThemes HostTheme { get; } = theme;
        public event Action<UIThemes> UIThemeChanged { add { } remove { } }
    }
}
