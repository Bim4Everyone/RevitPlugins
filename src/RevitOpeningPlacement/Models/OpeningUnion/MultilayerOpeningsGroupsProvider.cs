using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.OpeningUnion {
    /// <summary>
    /// Класс, предоставляющий коллекцию групп исходящих заданий на отверстия из активного документа,
    /// которые находятся в многослойных конструкциях и соприкасаются между собой.
    /// </summary>
    internal class MultilayerOpeningsGroupsProvider : IOpeningsGroupsProvider {
        private readonly RevitRepository _revitRepository;

        public MultilayerOpeningsGroupsProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        public ICollection<OpeningsGroup> GetOpeningsGroups(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            // создать итоговый список, в котором находятся группы заданий на отверстия, при этом в каждую группу должны добавляться только касающиеся друг друга задания на отверстия
            List<OpeningsGroup> resultOpeningsGroups = new List<OpeningsGroup>();

            foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
                // для каждого типа отверстия получить коллекцию заданий на отверстия
                ICollection<OpeningMepTaskOutcoming> openingsOfOpeningType = GetOpeningsOfOpeningType(openingTasks, openingType);
                // создать словарь Id и заданий на отверстия, чтобы не пересоздавать объекты
                IDictionary<ElementId, OpeningMepTaskOutcoming> openingsDictionary = GetElementIdAndOpeningTaskPairs(openingsOfOpeningType);
                // создать хэш-таблицу для хранения заданий на отверстия, для которых уже были найдены все касающиеся задания на отверстия
                HashSet<ElementId> excludingOpenings = new HashSet<ElementId>();

                foreach(KeyValuePair<ElementId, OpeningMepTaskOutcoming> idAndOpeningPair in openingsDictionary) {
                    excludingOpenings.Add(idAndOpeningPair.Key);
                    OpeningMepTaskOutcoming currentOpening = idAndOpeningPair.Value;
                    ICollection<ElementId> openingsToFilter = openingsDictionary.Keys;
                    ICollection<OpeningMepTaskOutcoming> touchingOpenings = GetTouchingOpenings(currentOpening, openingsDictionary, openingsToFilter, excludingOpenings);

                    if(touchingOpenings.Count > 0) {
                        // если касающиеся задания на отверстия найдены, то нужно добавить в итоговый список с группами отверстий текущее задание на отверстие и задания, которые касаются с текущим

                        // добавить текущее задание на отверстие к тем, которые пересекаются с ним
                        touchingOpenings.Add(currentOpening);
                        AddTouchingOpenings(ref resultOpeningsGroups, touchingOpenings);
                    }
                }
            }

            return resultOpeningsGroups;
        }

        private void AddTouchingOpenings(ref List<OpeningsGroup> openingsGroups, ICollection<OpeningMepTaskOutcoming> touchingOpenings) {
            // найти все группы отверстий, в которых уже есть заданные касающиеся отверстия
            OpeningsGroup[] groupsForMerge = openingsGroups.Where(group => group.ContainsAny(touchingOpenings)).ToArray();
            if(groupsForMerge.Length > 1) {
                // если таких групп больше 1, то нужно объединить уже существующие группы отверстий между собой в одну,
                // и в эту объединенную группу добавить текущее задание на отверстие и задания, которые пересекаются с текущим

                HashSet<OpeningMepTaskOutcoming> openingsFamInstances = groupsForMerge.SelectMany(group => group.Elements).ToHashSet();
                openingsFamInstances.UnionWith(touchingOpenings);

                foreach(OpeningsGroup groupForMerge in groupsForMerge) {
                    openingsGroups.Remove(groupForMerge);
                }
                openingsGroups.Add(new OpeningsGroup(openingsFamInstances));

            } else if(groupsForMerge.Length == 1) {
                // если такая группа одна, то добавить в нее текущее задание на отверстие и задания, которые пересекаются с текущим
                groupsForMerge.First().AddOpenings(touchingOpenings);
            } else if(groupsForMerge.Length == 0) {
                // если такой группы нет, то создать новую группу с текущим заданием на отверстие и заданиями, которые пересекаются с текущим
                OpeningsGroup newGroup = new OpeningsGroup(touchingOpenings);
                openingsGroups.Add(newGroup);
            }
        }


        private ICollection<OpeningMepTaskOutcoming> GetOpeningsOfOpeningType(ICollection<OpeningMepTaskOutcoming> openingsTasks, OpeningType openingType) {
            return openingsTasks.Where(task => task.OpeningType == openingType).ToHashSet();
        }

        private IDictionary<ElementId, OpeningMepTaskOutcoming> GetElementIdAndOpeningTaskPairs(ICollection<OpeningMepTaskOutcoming> openingMepTasks) {
            return openingMepTasks.ToDictionary(task => task.Id);
        }

        private ICollection<ElementId> GetNearestOpeningTasks(
            OpeningMepTaskOutcoming baseOpening,
            ICollection<ElementId> openingsToFilter,
            ICollection<ElementId> excludingOpenings) {

            if((openingsToFilter is null) || !openingsToFilter.Any()) {
                return Array.Empty<ElementId>();
            }
            return new FilteredElementCollector(_revitRepository.Doc, openingsToFilter)
                .Excluding(excludingOpenings)
                .WherePasses(new BoundingBoxIntersectsFilter(GetOutline(baseOpening)))
                .ToElementIds();
        }

        private Outline GetOutline(OpeningMepTaskOutcoming openingTask) {
            var box = openingTask.GetExtendedBoxXYZ();
            return new Outline(box.Min, box.Max);
        }

        private ICollection<OpeningMepTaskOutcoming> GetOpeningTasks(IDictionary<ElementId, OpeningMepTaskOutcoming> dictionary, ICollection<ElementId> openingIds) {
            HashSet<OpeningMepTaskOutcoming> result = new HashSet<OpeningMepTaskOutcoming>();
            foreach(ElementId openingId in openingIds) {
                result.Add(dictionary[openingId]);
            }
            return result;
        }

        private ICollection<OpeningMepTaskOutcoming> GetTouchingOpenings(OpeningMepTaskOutcoming baseOpening, ICollection<OpeningMepTaskOutcoming> otherOpenings) {
            HashSet<OpeningMepTaskOutcoming> touchingOpenings = new HashSet<OpeningMepTaskOutcoming>();
            foreach(OpeningMepTaskOutcoming otherOpening in otherOpenings) {
                if(baseOpening.HasCommonFace(otherOpening)) {
                    touchingOpenings.Add(otherOpening);
                }
            }
            return touchingOpenings;
        }

        private ICollection<OpeningMepTaskOutcoming> GetTouchingOpenings(
            OpeningMepTaskOutcoming baseOpening,
            IDictionary<ElementId, OpeningMepTaskOutcoming> openingsDictionary,
            ICollection<ElementId> openingsToFilter,
            ICollection<ElementId> excludingOpenings) {

            // получение заданий на отверстия, которые находятся близко к текущему заданию на отверстие
            ICollection<ElementId> nearestOpeningTasksIds = GetNearestOpeningTasks(baseOpening, openingsToFilter, excludingOpenings);
            ICollection<OpeningMepTaskOutcoming> nearestOpeningTasks = GetOpeningTasks(openingsDictionary, nearestOpeningTasksIds);
            // из этих ближайших заданий на отверстия получить те, которые касаются с текущим
            return GetTouchingOpenings(baseOpening, nearestOpeningTasks);
        }
    }
}
