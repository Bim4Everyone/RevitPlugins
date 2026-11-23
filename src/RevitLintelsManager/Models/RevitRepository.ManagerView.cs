using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

internal partial class RevitRepository {
    // Ошибки создания или настройки вида откатываются без диалога исправления модели.
    private sealed class ManagerViewFailures : IFailuresPreprocessor {
        /// <inheritdoc />
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failures) {
            return failures.GetSeverity() >= FailureSeverity.Error
                ? FailureProcessingResult.ProceedWithRollBack : FailureProcessingResult.Continue;
        }
    }

    internal const string ManagerViewTransactionName = "Подготовка 3D-вида менеджера перемычек";
    private const string _managerViewOwnerField = "RevitUsername";
    private const string _managerViewIdField = "ViewUniqueId";
    private static readonly Guid _managerViewSchemaId = new("2ad1c331-0d63-452b-9f38-df4082f11b49");

    /// <summary>Подготавливает собственный изометрический вид и возвращает его UniqueId в исходном проекте.</summary>
    public string EnsureManagerView(UIApplication app, ManagerConfigSnapshot config) {
        var activeDocument = app.ActiveUIDocument?.Document;
        if(!Document.IsValidObject || activeDocument?.IsValidObject != true || !Document.Equals(activeDocument)) {
            return null;
        }

        string owner = app.Application.Username;
        if(string.IsNullOrWhiteSpace(owner)) {
            throw new InvalidOperationException("Имя пользователя Revit недоступно.");
        }

        var phaseId = GetReadPhaseId(config);
        var schema = Schema.Lookup(_managerViewSchemaId);
        var existingView = schema is null ? null : FindManagerView(schema, owner);
        if(existingView is not null
            && existingView.GetParamValueOrDefault<ElementId>(BuiltInParameter.VIEW_PHASE) == phaseId) {
            return existingView.UniqueId;
        }

        if(Document.IsReadOnly || Document.IsReadOnlyFile || Document.IsModifiable || Document.IsFamilyDocument) {
            throw new InvalidOperationException("Исходный документ недоступен для подготовки 3D-вида.");
        }

        if(existingView is not null) {
            return SaveManagerView(() => existingView, phaseId);
        }

        var typeId = GetThreeDimensionalViewTypeId();
        string name = GetAvailableManagerViewName(owner);
        return SaveManagerView(() => CreateManagerView(typeId, name, schema ?? CreateManagerViewSchema(), owner),
            phaseId);
    }

    // Проверяет сохранённого владельца и исходный UniqueId, чтобы отличить пользовательскую копию вида.
    private View3D FindManagerView(Schema schema, string owner) {
        using var filter = new ExtensibleStorageFilter(schema.GUID);
        using var collector = new FilteredElementCollector(Document);
        return collector.OfClass(typeof(View3D)).WherePasses(filter).Cast<View3D>()
            .FirstOrDefault(view => IsManagerView(view, schema, owner));
    }

    // Имя вида может поменяться независимо от метки принадлежности.
    private static bool IsManagerView(View3D view, Schema schema, string owner) {
        if(!view.IsValidObject || view.IsTemplate || view.IsPerspective) {
            return false;
        }
        using var entity = view.GetEntity(schema);
        return entity.IsValid()
            && string.Equals(entity.Get<string>(_managerViewOwnerField), owner, StringComparison.Ordinal)
            && string.Equals(entity.Get<string>(_managerViewIdField), view.UniqueId, StringComparison.Ordinal);
    }

    // Использует тип 3D-вида по умолчанию проекта или доступный тип того же семейства.
    private ElementId GetThreeDimensionalViewTypeId() {
        var defaultId = Document.GetDefaultElementTypeId(ElementTypeGroup.ViewType3D);
        if(defaultId != ElementId.InvalidElementId
            && Document.GetElement(defaultId) is ViewFamilyType { ViewFamily: ViewFamily.ThreeDimensional }) {
            return defaultId;
        }

        using var collector = new FilteredElementCollector(Document);
        var type = collector.OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
            .FirstOrDefault(item => item.ViewFamily == ViewFamily.ThreeDimensional);
        return type?.Id ?? throw new InvalidOperationException("В исходном документе нет типа 3D-вида.");
    }

    // Подбирает имя среди всех видов исходного документа, включая шаблоны.
    private string GetAvailableManagerViewName(string owner) {
        using var collector = new FilteredElementCollector(Document);
        var names = new HashSet<string>(collector.OfClass(typeof(View)).Cast<View>().Select(view => view.Name),
            StringComparer.OrdinalIgnoreCase);
        string displayOwner = string.Concat(owner.Trim().Select(character =>
            !char.IsControl(character) && NamingUtils.IsValidName(character.ToString()) ? character : '_'));
        string baseName = "Менеджер перемычек_" + displayOwner;
        string name = baseName;
        for(int suffix = 2; names.Contains(name); suffix++) {
            name = baseName + "_" + suffix.ToString(CultureInfo.InvariantCulture);
        }
        return name;
    }

    // Создаёт схему с неизменным GUID для поиска вида после повторного открытия RVT.
    private static Schema CreateManagerViewSchema() {
        using var builder = new SchemaBuilder(_managerViewSchemaId);
        builder.SetSchemaName("RevitLintelsManagerView");
        builder.AddSimpleField(_managerViewOwnerField, typeof(string));
        builder.AddSimpleField(_managerViewIdField, typeof(string));
        return builder.Finish();
    }

    // Метит только новый изометрический вид внутри транзакции.
    private View3D CreateManagerView(ElementId typeId, string name, Schema schema, string owner) {
        var view = View3D.CreateIsometric(Document, typeId);
        view.Name = name;
        using var entity = new Entity(schema);
        entity.Set(_managerViewOwnerField, owner);
        entity.Set(_managerViewIdField, view.UniqueId);
        view.SetEntity(entity);
        return view;
    }

    // Сохраняет создание или изменение стадии собственного вида одной короткой транзакцией.
    private string SaveManagerView(Func<View3D> getView, ElementId phaseId) {
        using var transaction = new Transaction(Document, ManagerViewTransactionName);
        if(transaction.Start() != TransactionStatus.Started) {
            throw new InvalidOperationException("Не удалось начать подготовку 3D-вида.");
        }

        try {
            using var options = transaction.GetFailureHandlingOptions()
                .SetFailuresPreprocessor(new ManagerViewFailures())
                .SetClearAfterRollback(true)
                .SetForcedModalHandling(true);
            transaction.SetFailureHandlingOptions(options);

            var view = getView();
            SetManagerViewPhase(view, phaseId);
            string viewUniqueId = view.UniqueId;
            if(transaction.Commit() != TransactionStatus.Committed) {
                throw new InvalidOperationException("Revit не сохранил подготовленный 3D-вид.");
            }
            return viewUniqueId;
        } finally {
            if(transaction.GetStatus() == TransactionStatus.Started) {
                transaction.RollBack();
            }
        }
    }

    // Учитывает заданную типом стадию и проверяет фактическое значение после записи.
    private static void SetManagerViewPhase(View3D view, ElementId phaseId) {
        const BuiltInParameter phaseParameter = BuiltInParameter.VIEW_PHASE;
        if(view.GetParamValueOrDefault<ElementId>(phaseParameter) == phaseId) {
            return;
        }
        if(!view.IsExistsParam(phaseParameter) || view.IsReadOnlyParam(phaseParameter)) {
            throw new InvalidOperationException("Стадия служебного 3D-вида недоступна для изменения.");
        }
        view.SetParamValue(phaseParameter, phaseId);
        if(view.GetParamValueOrDefault<ElementId>(phaseParameter) != phaseId) {
            throw new InvalidOperationException("Revit не назначил выбранную стадию на служебном 3D-виде.");
        }
    }
}
