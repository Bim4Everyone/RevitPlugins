using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using dosymep.SimpleServices;

using pyRevitLabs.Json;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Views;

namespace RevitLintelsManager.Services;

/// <summary>Одно соединение с изолированным окном. В области обмена хранится только число обработанных строк.</summary>
internal sealed class BatchWindowConnection : IProgress<int>, IDisposable {
    private readonly string _channel = Guid.NewGuid().ToString("N");
    private readonly CancellationTokenSource _cancellation = new();
    private readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly object _sync = new();
    private readonly Process _process = new();
    private readonly MemoryMappedFile _mapping;
    private readonly MemoryMappedViewAccessor _counter;
    private readonly ILoggerService _logger;
    private bool _disposed;
    private string _executable;
    private bool _started;

    public BatchWindowConnection(ILoggerService logger) {
        _logger = logger;
        _mapping = MemoryMappedFile.CreateNew(_channel, SystemPluginConfig.BatchChannelSize);
        _counter = _mapping.CreateViewAccessor();
    }

    public CancellationToken Token => _cancellation.Token;

    /// <summary>Ожидает готовности окна прогресса перед первым изменением модели.</summary>
    public async Task StartAsync(BatchProgressOptions progress, ILocalizationService localization) {
        var assembly = typeof(BatchWindowHost).Assembly;
        string unavailable = localization.GetLocalizedString("Processing.ProgressUnavailable");
        string path = Path.Combine(Path.GetTempPath(), _channel + "." + SystemPluginConfig.BatchHostResource);
        using(var executable = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
            _executable = path;
            using var resource = assembly.GetManifestResourceStream(SystemPluginConfig.BatchHostResource)
                ?? throw new InvalidOperationException(unavailable);
            resource.CopyTo(executable);
        }
        _process.StartInfo = new ProcessStartInfo(path) {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        _process.OutputDataReceived += (_, e) => {
            if(e.Data == SystemPluginConfig.BatchReady) {
                _ready.TrySetResult(true);
            } else if(e.Data == SystemPluginConfig.BatchCancel) {
                Cancel();
            } else if(e.Data is null) {
                _ready.TrySetException(new InvalidOperationException(unavailable));
                Cancel();
            }
        };
        _process.ErrorDataReceived += (_, e) => {
            if(e.Data is not null) {
                _logger.Warning(e.Data);
            }
        };
        _process.Start();
        _started = true;
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        var directories = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
            .Select(a => a.Location).Where(File.Exists).Select(Path.GetDirectoryName).Distinct(StringComparer.OrdinalIgnoreCase);
        using(var input = new BinaryWriter(_process.StandardInput.BaseStream)) {
            input.Write(assembly.Location);
            input.Write(string.Join(Path.PathSeparator.ToString(), directories));
            input.Write(JsonConvert.SerializeObject(progress));
            input.Write(_channel);
            using var parent = Process.GetCurrentProcess();
            input.Write(parent.Id);
        }
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(SystemPluginConfig.BatchStartupTimeoutSeconds));
        using var deadline = timeout.Token.Register(() => _ready.TrySetException(
            new TimeoutException(unavailable)));
        await _ready.Task;
    }

    /// <summary>Обрабатывает как локальное закрытие сессии, так и сигнал из второго процесса.</summary>
    public void Cancel() {
        lock(_sync) {
            if(!_disposed) {
                _cancellation.Cancel();
                _ready.TrySetCanceled();
            }
        }
    }

    /// <inheritdoc />
    public void Report(int value) => _counter.Write(0, value);

    /// <summary>Разрешает закрытие окна прогресса после возврата API-обработчика.</summary>
    public void Dispose() {
        lock(_sync) {
            if(_disposed) {
                return;
            }
            _disposed = true;
            _counter.Write(SystemPluginConfig.BatchReleaseOffset, true);
            _counter.Dispose();
            _mapping.Dispose();
            _cancellation.Dispose();
        }
        // Windows освобождает исполняемый файл после выхода процесса. Ожидание не занимает поток Revit.
        _ = Task.Run(ReleaseExecutable);
    }

    private void ReleaseExecutable() {
        try {
            if(_started) {
                if(!_process.WaitForExit((int) TimeSpan.FromSeconds(SystemPluginConfig.BatchStartupTimeoutSeconds).TotalMilliseconds)) {
                    _process.Kill();
                }
                _process.WaitForExit();
            }
            if(_executable is not null) {
                File.Delete(_executable);
            }
        } catch(Exception error) {
            _logger.Warning(error, SystemPluginConfig.BatchHostResource);
        } finally {
            _process.Dispose();
        }
    }
}
