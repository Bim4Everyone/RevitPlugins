﻿
using System;

using dosymep.Bim4Everyone;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillParamViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _isEnabled;
        private DesingOptionsViewModel _designOptions;

        public FillParamViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
        }

        public RevitParam RevitParam { get; set; }
        public string Name => $"Заполнить \"{RevitParam.Name}\"";

        public bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public DesingOptionsViewModel DesignOptions {
            get => _designOptions;
            set => this.RaiseAndSetIfChanged(ref _designOptions, value);
        }
    }
}