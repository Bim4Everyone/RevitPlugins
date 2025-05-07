using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.FileInfo;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;
using dosymep.WpfUI.Core.SimpleServices;

using RevitRsnManager.Interfaces;
using RevitRsnManager.Models;

namespace RevitRsnManager.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly WpfUIMessageBoxService _messageBoxService;
    private readonly ILocalizationService _localizationService;
    private readonly IRsnConfigService _rsnConfigService;

    private string _errorText;
    private string _selectedServer;
    private string _newServerName;
    private ObservableCollection<string> _servers;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        WpfUIMessageBoxService messageBoxService,
        ILocalizationService localizationService, 
        IRsnConfigService rsnConfigService) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _rsnConfigService = rsnConfigService;
        _messageBoxService = messageBoxService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        AutoConfigureCommand = RelayCommand.Create(AutoConfigure);
        RemoveServerCommand = RelayCommand.Create<string>(RemoveServer);
        AddServerCommand = RelayCommand.Create(AddServer, () => !string.IsNullOrWhiteSpace(NewServerName));
        MoveUpCommand = RelayCommand.Create<object>(MoveUp, server => CanMove(server, -1));
        MoveDownCommand = RelayCommand.Create<object>(MoveDown, server => CanMove(server, 1));
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }
    public ICommand AddServerCommand { get; }
    public ICommand RemoveServerCommand { get; }
    public ICommand AcceptViewCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand AutoConfigureCommand { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string NewServerName {
        get => _newServerName;
        set => RaiseAndSetIfChanged(ref _newServerName, value);
    }

    public ObservableCollection<string> Servers {
        get => _servers;
        set => RaiseAndSetIfChanged(ref _servers, value);
    }

    public string SelectedServer {
        get => _selectedServer;
        set => RaiseAndSetIfChanged(ref _selectedServer, value);
    }

    private void AutoConfigure() {
        string modelsPath = _rsnConfigService.GetProjectPathFromRevitIni();
        DateTime now = DateTime.Now;

        List<FileInfo> allFiles = Directory.GetFiles(modelsPath, "*.rvt", SearchOption.TopDirectoryOnly)
                                           .Select(path => new FileInfo(path))
                                           .ToList();

        List<FileInfo> recentFiles = allFiles
            .Where(file => (now - file.LastWriteTime).TotalDays <= 5)
            .OrderByDescending(file => file.LastWriteTime)
            .ToList();

        List<string> serversFromRecentFiles = recentFiles
            .Select(GetServerIfValid)
            .Where(server => !string.IsNullOrWhiteSpace(server))
            .Distinct()
            .ToList();

        List<string> existingServers = Servers.Except(serversFromRecentFiles).ToList();
        Servers = new ObservableCollection<string>(serversFromRecentFiles.Concat(existingServers));
    }

    private string GetServerIfValid(FileInfo file) {
        try {
            var info = new RevitFileInfo(file.FullName);

            if(info.BasicFileInfo.AppInfo.Format != ModuleEnvironment.RevitVersion) {
                return null;
            }

            string centralPath = info.BasicFileInfo.CentralPath;

            if(!centralPath.StartsWith("RSN://", StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            return new Uri(centralPath).Host;
        } catch {
            return null;
        }
    }

    private void RemoveServer(string server) {
        if(string.IsNullOrWhiteSpace(server)) {
            return;
        }

        string title = _localizationService.GetLocalizedString("MainWindow.ConfirmTitle");
        string message = _localizationService.GetLocalizedString("MainWindow.ConfirmDeleteServer");

        if(_messageBoxService.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
            Servers.Remove(server);

            if(SelectedServer == server) {
                SelectedServer = null;
            }
        }
    }

    private void AddServer() {
        string trimmed = NewServerName?.Trim();
       
        if(string.IsNullOrWhiteSpace(trimmed)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.Empty");
            return;
        }

        if(Servers.Contains(trimmed)) {
            ErrorText = string.Format(
                _localizationService.GetLocalizedString("MainWindow.AlreadyExists"),
                trimmed);
            return;
        }

        Servers.Add(trimmed);
        SelectedServer = trimmed;
        NewServerName = string.Empty;
        ErrorText = null; 
    }


    private void Move(object serverObj, int delta) {
        if(serverObj is not string server)
            return;

        int index = Servers.IndexOf(server);
        int newIndex = index + delta;

        if(index >= 0 && newIndex >= 0 && newIndex < Servers.Count) {
            Servers.Move(index, newIndex);
        }
    }

    private bool CanMove(object serverObj, int delta) {
        if(serverObj is not string server)
            return false;

        int index = Servers.IndexOf(server);
        return index >= 0 && index + delta >= 0 && index + delta < Servers.Count;
    }

    private void MoveUp(object serverObj) => Move(serverObj, -1);

    private void MoveDown(object serverObj) => Move(serverObj, 1);

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        var servers = _rsnConfigService.LoadServersFromIni();
        Servers = new ObservableCollection<string>(servers);
        LoadConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        _rsnConfigService.SaveServersToIni(Servers.ToList());
        var updateSuccess = _localizationService.GetLocalizedString("MainWindow.UpdateSuccess");
        var updateSuccessTitle = _localizationService.GetLocalizedString("MainWindow.UpdateSuccessTitle");
        SaveConfig();
        _messageBoxService.Show(
               updateSuccess,
               updateSuccessTitle,
               MessageBoxButton.OK,
               MessageBoxImage.Information
           );
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        ErrorText = null;
        if(Servers == null || Servers.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorEmptyServerList");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
        if(setting?.Servers?.Any() == true) {
            Servers = new ObservableCollection<string>(setting.Servers);
        }
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.Servers = Servers.ToList();
        _pluginConfig.SaveProjectConfig();
    }
}
