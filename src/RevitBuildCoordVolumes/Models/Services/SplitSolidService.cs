using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SplitSolidService : ISplitSolidService {
    
    public IList<SolidObject> SplitSolidObjects(IList<SolidObject> solids, ProgressService progressService) {
        var parts = solids
            .Where(x => x != null && SolidUtility.IsValid(x.Solid))
            .Select((x, i) => new SplitPart(i, x))
            .ToList();

        progressService?.BeginStage(ProgressType.ProcessingVolumes);

        if (parts.Count == 0)
        {
            progressService?.ProgressCount?.Report(100);
            return new List<SolidObject>();
        }

        int nextId = parts.Count;

        // Пары, для которых Boolean-операция завершилась ошибкой.
        // Повторно друг с другом эти части не проверяем.
        var failedPairs = new HashSet<(int, int)>();

        long processedPairs = 0;
        long initialPairs = (long)parts.Count * (parts.Count - 1) / 2;
        int lastProgress = 0;

        progressService?.ProgressCount?.Report(0);

        while (true)
        {
            progressService?.CancellationToken.ThrowIfCancellationRequested();

            bool splitCompleted = false;

            for (int i = 0; i < parts.Count && !splitCompleted; i++)
            {
                for (int j = i + 1; j < parts.Count; j++)
                {
                    progressService?.CancellationToken.ThrowIfCancellationRequested();

                    var first = parts[i];
                    var second = parts[j];

                    var pair = (
                        Math.Min(first.Id, second.Id),
                        Math.Max(first.Id, second.Id));

                    processedPairs++;

                    // Обновляем прогресс, оставляя 5% на завершение.
                    long remainingPairs =
                        (long)parts.Count * (parts.Count - 1) / 2;

                    long estimatedTotal = Math.Max(
                        initialPairs,
                        processedPairs + remainingPairs);

                    int progress = estimatedTotal > 0
                        ? (int)Math.Min(
                            95,
                            processedPairs * 95 / estimatedTotal)
                        : 0;

                    lastProgress = Math.Max(lastProgress, progress);
                    progressService?.ProgressCount?.Report(lastProgress);

                    if (failedPairs.Contains(pair))
                        continue;

                    // Находим пересечение.
                    if (!SolidUtility.TryGetBooleanSolid(
                            first.Value.Solid,
                            second.Value.Solid,
                            BooleanOperationsType.Intersect,
                            out var intersection))
                    {
                        failedPairs.Add(pair);
                        continue;
                    }

                    // Нет значимого пересечения — разбивать нечего.
                    if (intersection == null)
                        continue;

                    // Вычисляем остатки обеих частей.
                    if (!SolidUtility.TryGetBooleanSolid(
                            first.Value.Solid,
                            intersection,
                            BooleanOperationsType.Difference,
                            out var firstRest))
                    {
                        failedPairs.Add(pair);
                        continue;
                    }

                    if (!SolidUtility.TryGetBooleanSolid(
                            second.Value.Solid,
                            intersection,
                            BooleanOperationsType.Difference,
                            out var secondRest))
                    {
                        failedPairs.Add(pair);
                        continue;
                    }

                    // Все операции прошли успешно.
                    // Удаляем исходные части и добавляем результат разбиения.
                    parts.RemoveAt(j);
                    parts.RemoveAt(i);

                    if (SolidUtility.IsValid(firstRest))
                    {
                        parts.Add(new SplitPart(
                            nextId++,
                            new SolidObject
                            {
                                Solid = firstRest,
                                LevelName = first.Value.LevelName
                            }));
                    }

                    parts.Add(new SplitPart(
                        nextId++,
                        new SolidObject
                        {
                            Solid = intersection,
                            LevelName = GetSolidLevelName(
                                intersection,
                                first.Value,
                                second.Value)
                        }));

                    if (SolidUtility.IsValid(secondRest))
                    {
                        parts.Add(new SplitPart(
                            nextId++,
                            new SolidObject
                            {
                                Solid = secondRest,
                                LevelName = second.Value.LevelName
                            }));
                    }

                    // После разбиения начинаем поиск заново:
                    // новые части тоже должны проверяться с остальными.
                    splitCompleted = true;
                    break;
                }
            }

            // Полный проход не привёл к новому разбиению.
            if (!splitCompleted)
                break;
        }

        progressService?.CancellationToken.ThrowIfCancellationRequested();
        progressService?.ProgressCount?.Report(100);

        return parts
            .Select(x => x.Value)
            .ToList();
    }
    
    // Метод определяет уровень у солидов
    private static string GetSolidLevelName(Solid solid, SolidObject first, SolidObject second) {
        double minZ = solid.GetBoundingBox().Min.Z;
        double firstMinZ = first.Solid.GetBoundingBox().Min.Z;
        double secondMinZ = second.Solid.GetBoundingBox().Min.Z;
    
        if (Math.Abs(minZ - firstMinZ) < GeometryTolerance.Model) {
            return first.LevelName;
        }
    
        return Math.Abs(minZ - secondMinZ) < GeometryTolerance.Model 
            ? second.LevelName 
            : null;
    }    
    
    private sealed class SplitPart(int id, SolidObject value) {
        public int Id { get; } = id;
        public SolidObject Value { get; } = value;
    }
    
}
