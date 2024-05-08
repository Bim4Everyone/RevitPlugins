using System;
using System.Collections.Generic;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamValuesByEvents.Models;

namespace RevitParamValuesByEvents.ViewModels {
    internal class SettingsPageViewModel : BaseViewModel {

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _saveProperty;
        private string _someText;

        public SettingsPageViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            SomeText = "1";

            UIApplication uiApp = new UIApplication(_revitRepository.UIApplication.Application);

            uiApp.Idling += new EventHandler<IdlingEventArgs>(IdleUpdateForElems);

            _revitRepository.Application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string SomeText {
            get => _someText;
            set => this.RaiseAndSetIfChanged(ref _someText, value);
        }
        public static List<ElementId> ElemIdsForWrite { get; set; } = new List<ElementId>();

        private static void OnDocumentChanged(object sender, DocumentChangedEventArgs e) {

            if(e.Operation == UndoOperation.TransactionCommitted && e.GetAddedElementIds().Count > 0) {

                ElemIdsForWrite.AddRange(e.GetAddedElementIds());
            }
        }


        private readonly List<BuiltInCategory> _neededTypes = new List<BuiltInCategory>() {

            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_Columns,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFoundation,
            BuiltInCategory.OST_Rebar
        };

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        public void IdleUpdateForElems(object sender, IdlingEventArgs e) {

            if(ElemIdsForWrite.Count > 0) {

                Document doc = _revitRepository.Document;

                foreach(ElementId id in ElemIdsForWrite) {

                    Element elem = doc.GetElement(id);

                    if(elem is null
                        || elem.Category is null
                        || !_neededTypes.Contains(elem.Category.GetBuiltInCategory())) {

                        continue;
                    }

                    using(Transaction transaction = doc.StartTransaction($"Element {id} change")) {

                        try {

                            Parameter parameter = elem.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                            if(parameter is null) {

                                transaction.RollBack();
                            } else {

                                parameter.Set(SomeText);
                                transaction.Commit();
                            }

                        } catch(Exception) {

                            transaction.RollBack();
                        }
                    }
                }

                ElemIdsForWrite.Clear();
            }
        }


        private void AcceptView() {

            SaveConfig();

            TaskDialog.Show("I'm okey!", SomeText);
        }


        private bool CanAcceptView() {

            return true;
        }


        private void LoadView() {
            LoadConfig();
        }


        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SaveProperty = setting?.SaveProperty ?? "Привет Revit!";
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
