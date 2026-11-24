using System;
using System.IO.Packaging;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.UI;

namespace RevitLintelsManager.Services;

/// <summary>Удерживает единственную сессию и её WPF-ресурсы при загрузке копий плагина.</summary>
internal sealed class ManagerSessionRegistration : IDisposable {
    private const string _sessionKey = "BIM4Everyone.RevitLintelsManager.Session";
    private static readonly Assembly _assembly = typeof(ManagerSessionRegistration).Assembly;
    private static readonly string _assemblyName = _assembly.GetName().Name;
    private readonly Tuple<Action, Action<UIApplication>> _commands;
    private AssemblyLoadEventHandler _updateResources;
    private Action _activate;

    private ManagerSessionRegistration() {
        _commands = Tuple.Create<Action, Action<UIApplication>>(Activate, RevitLintelsSettingsCommand.ShowSettings);
    }

    /// <summary>Резервирует сессию на UI-потоке Revit или активирует уже открытый менеджер.</summary>
    public static ManagerSessionRegistration RegisterOrActivate() {
        var domain = AppDomain.CurrentDomain;
        if(domain.GetData(_sessionKey) is Tuple<Action, Action<UIApplication>> commands) {
            commands.Item1();
            return null;
        }
        var registration = new ManagerSessionRegistration();
        domain.SetData(_sessionKey, registration._commands);
        return registration;
    }

    /// <summary>Открывает настройки из сборки, которой принадлежат ресурсы открытого менеджера.</summary>
    public static bool TryShowSettings(UIApplication application) {
        if(AppDomain.CurrentDomain.GetData(_sessionKey) is not Tuple<Action, Action<UIApplication>> commands) {
            return false;
        }
        commands.Item2(application);
        return true;
    }

    /// <summary>Закрепляет ресурсы сборки до создания окна и до освобождения сессии.</summary>
    public void KeepWindowResources() {
        // Первый запрос подключает обработчик AssemblyLoad самого WPF до нашей подписки.
        var viewUri = new Uri($"/{_assemblyName};component/views/managerwindow.xaml", UriKind.Relative);
        using var stream = Application.GetResourceStream(viewUri)?.Stream;
        if(stream is null) {
            throw new InvalidOperationException("Ресурсы окна менеджера перемычек недоступны.");
        }

        _updateResources = GetResourceUpdater();
        var domain = AppDomain.CurrentDomain;
        _updateResources(domain, new AssemblyLoadEventArgs(_assembly));
        domain.AssemblyLoad += OnAssemblyLoaded;
    }

    /// <summary>Подключает активацию после успешного запуска менеджера.</summary>
    public void SetActivation(Action activate) {
        _activate = activate;
    }

    /// <summary>Освобождает подписку и резервирование, принадлежащие этой сессии.</summary>
    public void Dispose() {
        var domain = AppDomain.CurrentDomain;
        domain.AssemblyLoad -= OnAssemblyLoaded;
        _activate = null;
        if(ReferenceEquals(domain.GetData(_sessionKey), _commands)) {
            domain.SetData(_sessionKey, null);
        }
    }

    // Возвращает штатный обработчик .NET Framework WPF, меняющий сборку в кэше pack URI.
    private static AssemblyLoadEventHandler GetResourceUpdater() {
        var getPackage = typeof(Application).GetMethod("GetResourcePackage",
            BindingFlags.NonPublic | BindingFlags.Static);
        var packageUri = PackUriHelper.GetPackageUri(new Uri("pack://application:,,,/"));
        var package = getPackage?.Invoke(null, new object[] { packageUri });
        var update = package?.GetType().GetMethod("OnAssemblyLoadEventHandler",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if(update is null) {
            throw new InvalidOperationException("Не удалось закрепить ресурсы менеджера в кэше WPF.");
        }
        return (AssemblyLoadEventHandler) Delegate.CreateDelegate(typeof(AssemblyLoadEventHandler), package, update);
    }

    // После загрузки дубликата возвращает в кэш WPF сборку уже открытого окна.
    private void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args) {
        var loaded = args.LoadedAssembly;
        if(!loaded.ReflectionOnly && !ReferenceEquals(loaded, _assembly)
            && string.Equals(loaded.GetName().Name, _assemblyName, StringComparison.OrdinalIgnoreCase)) {
            _updateResources(sender, new AssemblyLoadEventArgs(_assembly));
        }
    }

    // Пропускает повторный вызов до передачи окна в запущенную сессию.
    private void Activate() {
        _activate?.Invoke();
    }
}
