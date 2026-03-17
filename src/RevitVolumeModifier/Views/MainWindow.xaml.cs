using System;
using System.ComponentModel;

using System.Windows.Media;
using System.Windows.Media.Animation;

using dosymep.SimpleServices;

using Ninject;

using RevitVolumeModifier.Enums;
using RevitVolumeModifier.Models;
using RevitVolumeModifier.ViewModels;

namespace RevitVolumeModifier.Views;

public partial class MainWindow : IDisposable {
    private readonly IKernel _kernel;
    private readonly SelectionMonitor _selectionMonitor;
    private CommandStateViewModel _currentCommandState;
    public MainWindow(
        IKernel kernel,
        SelectionMonitor selectionMonitor,
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _selectionMonitor = selectionMonitor ?? throw new ArgumentNullException(nameof(selectionMonitor));
    }

    public override string PluginName => nameof(RevitVolumeModifier);

    public override string ProjectConfigName => nameof(MainWindow);

    public void Dispose() {
        _kernel?.Dispose();
    }

    protected override void OnContentRendered(EventArgs e) {
        base.OnContentRendered(e);

        if(DataContext is MainViewModel vm) {
            vm.PropertyChanged += OnViewModelPropertyChanged;

            // Подписка на внутренние свойства CommandState
            if(vm.CommandState != null) {
                vm.CommandState.PropertyChanged += OnCommandStatePropertyChanged;
            }
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not MainViewModel vm) {
            return;
        }

        if(e.PropertyName == nameof(MainViewModel.CommandState)) {

            if(_currentCommandState != null) {
                _currentCommandState.PropertyChanged -= OnCommandStatePropertyChanged;
            }

            _currentCommandState = vm.CommandState;

            if(_currentCommandState != null) {
                _currentCommandState.PropertyChanged += OnCommandStatePropertyChanged;
                AnimateBorder(_currentCommandState.CommandType, _currentCommandState.CommandStatus);
            }
        }
    }

    private void OnCommandStatePropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not CommandStateViewModel state) {
            return;
        }

        // Анимация при изменении CommandType или CommandStatus
        if(e.PropertyName is (nameof(CommandStateViewModel.CommandType)) or
            (nameof(CommandStateViewModel.CommandStatus))) {
            AnimateBorder(state.CommandType, state.CommandStatus);
        }
    }


    private void AnimateBorder(CommandType activeCommand, CommandStatus status) {
        object resource = TryFindResource(activeCommand.ToString());
        if(resource is not SolidColorBrush brush) {
            return;
        }

        switch(status) {
            case CommandStatus.Success:
                PlayChangeColorEffect(brush, Colors.MediumSeaGreen, 2000);
                break;
            case CommandStatus.Failed:
                PlaySparkEffect(brush, Colors.Orange, Colors.DimGray, 400, 3);
                break;
            case CommandStatus.None:
                PlayChangeColorEffect(brush, Colors.DimGray, 1000);
                break;
            default:
                PlayChangeColorEffect(brush, Colors.DimGray, 300);
                break;
        }
    }

    private void PlayChangeColorEffect(SolidColorBrush brush, Color to, int time) {
        var animation = new ColorAnimation {
            To = to,
            Duration = TimeSpan.FromMilliseconds(time),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
    }

    private void PlaySparkEffect(SolidColorBrush brush, Color from, Color to, int time, int repeat) {
        var blinkAnimation = new ColorAnimation {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(time),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(repeat),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        brush.BeginAnimation(SolidColorBrush.ColorProperty, blinkAnimation);
    }

    private void MainWindow_OnClosed(object sender, EventArgs e) {
        _selectionMonitor.Dispose();
        Dispose();
    }
}
