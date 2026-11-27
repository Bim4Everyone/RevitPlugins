using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Краткоживущие данные проверенной пары только для синхронного вызова в API-контексте.</summary>
internal sealed class ProcessingTarget(Document document, FamilyInstance opening, FamilyInstance lintel,
    OpeningLintelMatch match, LintelCalculation calculation) {
    public Document Document { get; } = document;
    public FamilyInstance Opening { get; } = opening;
    public FamilyInstance Lintel { get; } = lintel;
    public OpeningLintelMatch Match { get; } = match;
    public OpeningLintelMatch InstalledPair { get; internal set; }
    internal ManagerLintelSnapshot CorrectedLintel { get; set; }
    public FamilyInstance InstalledLintel { get; internal set; }
    public LintelCalculation Calculation { get; } = calculation;
    public LintelComparison Comparison { get; internal set; }
    public CorrectionSnapshot BeforeCorrection { get; internal set; }
    public IGeometryReadContext LockGeometry { get; internal set; }
    public string CreatedLockId { get; internal set; }
    public IReadOnlyList<(long Owner, long Parameter, bool HasValue, object Value)> OriginalLockParameters { get; internal set; }
}

/// <summary>Изменение одной строки и проверка результата после регенерации.</summary>
internal sealed class ProcessingAction(ManagerAction action, Action<ProcessingTarget, CancellationToken> apply,
    Action<ProcessingTarget> verify) {
    public ManagerAction Action { get; } = action;
    public bool ChangesFixation => Action is ManagerAction.SetFixation or ManagerAction.ClearFixation;
    public Action<ProcessingTarget, CancellationToken> Apply { get; } = apply ?? throw new ArgumentNullException(nameof(apply));
    public Action<ProcessingTarget> Verify { get; } = verify ?? throw new ArgumentNullException(nameof(verify));
}

internal interface IManagerProcessingService {
    /// <summary>В одном Execute обрабатывает подтверждённые назначения и объединяет сохранённые изменения.</summary>
    ProcessingBatchResult ExecuteBatch(ApprovedProcessingPlan approved, CancellationToken token, IProgress<int> progress);

    /// <summary>Исполняет зарегистрированное действие строки подтверждённого плана.</summary>
    /// <param name="approved">Разрешение текущей сессии.</param>
    /// <param name="index">Индекс строки плана.</param>
    /// <param name="token">Отмена текущей операции.</param>
    /// <returns>Фактический исход операции.</returns>
    ProcessingResult ExecuteAssigned(ApprovedProcessingPlan approved, int index, CancellationToken token);

    /// <summary>Подтверждает неизменяемый план до входа в изменяющий API-вызов.</summary>
    /// <param name="plan">Назначения текущего поколения менеджера.</param>
    /// <returns>Разрешение на этот план либо null при отказе, смене поколения или занятом сервисе.</returns>
    ApprovedProcessingPlan Confirm(ProcessingPlan plan);

    /// <summary>Перепроверяет и выполняет одно назначение в синхронном API-контексте исходного документа.</summary>
    /// <param name="approved">Разрешение этой сессии.</param>
    /// <param name="index">Индекс строки в подтверждённом плане.</param>
    /// <param name="action">Обработчик назначенного действия с проверкой результата.</param>
    /// <param name="token">Запрос отмены текущего запуска.</param>
    /// <returns>Исход и диагностика проверенной строки.</returns>
    ProcessingResult Execute(ApprovedProcessingPlan approved, int index, ProcessingAction action, CancellationToken token);

    /// <summary>В разрешённом API-контексте освобождает транзакцию после её нативного finalizer.</summary>
    /// <returns>Текущий исход ожидающей операции либо null, если такой операции нет.</returns>
    ProcessingResult FinishPending();
}
