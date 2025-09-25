using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.Win32;

using RevitGenLookupTables.Extensions;
using RevitGenLookupTables.Models;
using RevitGenLookupTables.Services;
using RevitGenLookupTables.Views.Dialogs;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace RevitGenLookupTables.ViewModels;

/// <summary>
///     Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ISelectFamilyParamsService _selectFamilyParamsService;

    private string _errorText;
    private FamilyParamViewModel _chosenFamilyParam;
    private ObservableCollection<FamilyParamViewModel> _mainFamilyParams = [];
    private ObservableCollection<FamilyParamViewModel> _chosenFamilyParams = [];
    private ObservableCollection<FamilyParamViewModel> _selectedFamilyParams = [];

    /// <summary>
    ///     Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        ISaveFileDialogService saveFileDialogService,
        ISelectFamilyParamsService selectFamilyParamsService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _selectFamilyParamsService = selectFamilyParamsService;

        SaveFileDialogService = saveFileDialogService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        AddFamilyParamCommand = RelayCommand.CreateAsync(AddFamilyParamAsync, CanAddFamilyParam);
        RemoveFamilyParamCommand = RelayCommand.Create(RemoveFamilyParam, CanRemoveFamilyParam);

        UpFamilyParamCommand = RelayCommand.Create(UpFamilyParam, CanUpFamilyParam);
        DownFamilyParamCommand = RelayCommand.Create(DownFamilyParam, CanDownFamilyParam);
    }

    /// <summary>
    ///     Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    /// <summary>
    ///     Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    public ICommand AddFamilyParamCommand { get; set; }
    public ICommand RemoveFamilyParamCommand { get; set; }
    public ICommand UpFamilyParamCommand { get; set; }
    public ICommand DownFamilyParamCommand { get; set; }

    public ISaveFileDialogService SaveFileDialogService { get; }

    /// <summary>
    ///     Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public FamilyParamViewModel ChosenFamilyParam {
        get => _chosenFamilyParam;
        set => RaiseAndSetIfChanged(ref _chosenFamilyParam, value);
    }

    public ObservableCollection<FamilyParamViewModel> MainFamilyParams {
        get => _mainFamilyParams;
        set => this.RaiseAndSetIfChanged(ref _mainFamilyParams, value);
    }

    public ObservableCollection<FamilyParamViewModel> ChosenFamilyParams {
        get => _chosenFamilyParams;
        set => this.RaiseAndSetIfChanged(ref _chosenFamilyParams, value);
    }

    public ObservableCollection<FamilyParamViewModel> SelectedFamilyParams {
        get => _selectedFamilyParams;
        set => this.RaiseAndSetIfChanged(ref _selectedFamilyParams, value);
    }

    /// <summary>
    ///     Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        IEnumerable<FamilyParamViewModel> familyParams = _revitRepository.GetFamilyParams()
            .Select(item => new FamilyParamViewModel(item) {
                ColumnMetadata = _revitRepository.GetColumnMetadata(item),
                FamilyParamValues = new FamilyParamValuesViewModel(item.StorageType, _localizationService)
            });

        MainFamilyParams = new ObservableCollection<FamilyParamViewModel>(familyParams);
    }

    /// <summary>
    ///     Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    ///     В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveFileDialogService.Filter = _localizationService.GetLocalizedString("MainViewModel.ExcelFilter");
        SaveFileDialogService.CheckFileExists = false;
        SaveFileDialogService.RestoreDirectory = true;
        SaveFileDialogService.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        bool result = SaveFileDialogService.ShowDialog(
            SaveFileDialogService.InitialDirectory,
            _revitRepository.DocumentTitle + ".csv");

        if(!result) {
            throw new OperationCanceledException();
        }

        var familyParams = ChosenFamilyParams
            .Where(item => !string.IsNullOrEmpty(item.FamilyParamValues.ParamValues))
            .ToList();

        var builder = new StringBuilder();
        builder.Append(";");
        builder.AppendLine(string.Join(";", familyParams.Select(item => item.Name + item.ColumnMetadata)));

        var combinations = familyParams
            .Select(item => item.FamilyParamValues.GetParamValues())
            .Combination();

        foreach(var combination in combinations) {
            builder.Append(";");
            builder.AppendLine(string.Join(";", combination));
        }

        string filePath = SaveFileDialogService.File.FullName;

        File.WriteAllText(
            filePath,
            builder.ToString(),
            Encoding.GetEncoding(CultureInfo.CurrentUICulture.TextInfo.ANSICodePage));

        Process.Start(filePath);
    }

    /// <summary>
    ///     Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    ///     В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    ///     В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        if(ChosenFamilyParams.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.EmptyChosenList");
            return false;
        }

        var familyParam = ChosenFamilyParams
            .FirstOrDefault(item => !string.IsNullOrEmpty(item.FamilyParamValues.GetValueErrors()));

        if(familyParam != null) {
            ErrorText = familyParam.Name + ": " + familyParam.FamilyParamValues.GetValueErrors();
            return false;
        }

        ErrorText = null;
        return true;
    }

    private async Task AddFamilyParamAsync() {
        bool? result = await _selectFamilyParamsService.ShowDialogAsync(MainFamilyParams.ToArray());
        if(result == true) {
            var viewModels = _selectFamilyParamsService.SelectedFamilyParams;
            foreach(var viewModel in viewModels) {
                MainFamilyParams.Remove(viewModel);
                ChosenFamilyParams.Add(viewModel);
            }
        }
    }

    private bool CanAddFamilyParam() {
        return true;
    }

    private void RemoveFamilyParam() {
        foreach(var selectedParam in SelectedFamilyParams.ToList()) {
            MainFamilyParams.Add(selectedParam);
            ChosenFamilyParams.Remove(selectedParam);
        }
    }

    private bool CanRemoveFamilyParam() {
        return true;
    }

    private void UpFamilyParam() {
        var indexes = SelectedFamilyParams
            .Select(item => ChosenFamilyParams.IndexOf(item))
            .OrderBy(item => item);

        int count = 0;
        foreach(int index in indexes) {
            if(index == count++) {
                continue;
            }

            ChosenFamilyParams.Move(index, index - 1);
        }
    }

    private bool CanUpFamilyParam() {
        return true;
    }

    private void DownFamilyParam() {
        var indexes = SelectedFamilyParams
            .Select(item => ChosenFamilyParams.IndexOf(item))
            .OrderByDescending(item => item);

        int count = 0;
        foreach(int index in indexes) {
            if(index == ChosenFamilyParams.Count - ++count) {
                continue;
            }
        
            ChosenFamilyParams.Move(index, index + 1);
        }
    }

    private bool CanDownFamilyParam() {
        return true;
    }
}
