using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitParamValuesByEvents.ViewModels;

namespace RevitParamValuesByEvents.Models {
    internal class EventsUtils {

        private readonly SettingsPageVM _settingsPageVM;
        private readonly RevitRepository _revitRepository;
        private readonly List<BuiltInCategory> _neededTypes = new List<BuiltInCategory>() {

            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_Columns,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFoundation,
            BuiltInCategory.OST_Rebar
        };



        public EventsUtils(SettingsPageVM settingsPageVM, RevitRepository revitRepository) {


            _settingsPageVM = settingsPageVM;
            _revitRepository = revitRepository;
        }



        public static List<ElementId> ElemIdsForWrite { get; set; } = new List<ElementId>();


        public void SubscribeToEvents() {

            UIApplication uiApp = new UIApplication(_revitRepository.UIApplication.Application);

            _revitRepository.Application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
            uiApp.Idling += new EventHandler<IdlingEventArgs>(IdleUpdateForElems);
        }


        /// <summary>
        /// Метод, отрабатывающий по событию изменения документа. Получает элементы, которые были созданы в последней транзакции
        /// </summary>
        private static void OnDocumentChanged(object sender, DocumentChangedEventArgs e) {

            if(e.Operation == UndoOperation.TransactionCommitted && e.GetAddedElementIds().Count > 0) {

                ElemIdsForWrite.AddRange(e.GetAddedElementIds());
            }
        }



        /// <summary>
        /// Метод, отрабатывающий по событию простоя Revit. Перебирает созданные в последней транзакции элементы и заполняет их параметры
        /// </summary>
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

                            foreach(TaskItemVM task in _settingsPageVM.Tasks) {

                                if(!task.IsCheck) {

                                    continue;
                                }

                                Parameter parameter = elem.GetParam(task.SelectedParamName);

                                if(parameter is null) {

                                    continue;
                                } else {

                                    parameter.Set(task.ParamValue);
                                }
                            }
                            transaction.Commit();

                        } catch(Exception) {

                            transaction.RollBack();
                        }
                    }
                }

                ElemIdsForWrite.Clear();
            }
        }
    }
}
