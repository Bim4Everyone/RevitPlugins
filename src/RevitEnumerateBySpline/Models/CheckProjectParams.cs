using System;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.Templates;

namespace RevitEnumerateBySpline.Models;

/// <summary>Загружает и проверяет обязательный параметр помещений.</summary>
internal class CheckProjectParams(
    SystemPluginConfig systemPluginConfig,
    Application application,
    Document document) {
    private bool _isChecked;

    /// <summary>Возвращает результат проверки загрузки параметра.</summary>
    public bool GetIsChecked() {
        return _isChecked;
    }

    /// <summary>Загружает параметр и проверяет его привязку к экземплярам помещений.</summary>
    public CheckProjectParams CopyProjectParams() {
        _isChecked = false;
        try {
            var parameter = systemPluginConfig.DefaultDependentParam;
            ProjectParameters.Create(application).SetupRevitParams(document, [parameter]);

            var roomsCategory = Category.GetCategory(document, BuiltInCategory.OST_Rooms);
            var iterator = document.ParameterBindings.ForwardIterator();
            while(iterator.MoveNext()) {
                if(iterator.Key.Name == parameter.Name
                   && iterator.Current is InstanceBinding binding
                   && binding.Categories.Contains(roomsCategory)) {
                    _isChecked = true;
                    break;
                }
            }
        } catch(Exception) {
            // Ошибка загрузки будет показана командой через сервис сообщений.
            _isChecked = false;
        }

        return this;
    }
}
