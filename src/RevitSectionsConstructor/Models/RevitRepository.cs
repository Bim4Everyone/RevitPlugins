using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitSectionsConstructor.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }


        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public IReadOnlyCollection<LevelWrapper> GetLevelWrappers() {
            var levels = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Levels)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .Select(level => new LevelWrapper(level))
                .ToList();
            return new ReadOnlyCollection<LevelWrapper>(levels);
        }

        public ICollection<Group> GetParentGroups() {
            return new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .OfClass(typeof(Group))
                .Where(group => group.GroupId == ElementId.InvalidElementId)
                .Cast<Group>()
                .ToArray();
        }

        public void RemoveElement(ElementId elementId) {
            if(elementId is null) { throw new ArgumentNullException(nameof(elementId)); }

            Document.Delete(elementId);
        }

        /// <summary>
        /// Копирует группу на уровни, которые в ней заданы.
        /// Если группа расположена на уровне, на который ее снова надо скопировать,
        /// то группа не удаляется и не копируется на этот уровень, а остается на этом текущем уровне
        /// </summary>
        /// <param name="group"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void CopyGroup(GroupWithAction group) {
            if(group is null) { throw new ArgumentNullException(nameof(group)); }
            if(!(group.Group.Location is LocationPoint)) {
                throw new ArgumentException($"Location группы с Id={group.Group.Id} не является точкой");
            }

            XYZ currentLocation = ((LocationPoint) group.Group.Location).Point;
            GroupType groupType = group.Group.GroupType;
            var levelsForPlacing = group.LevelsForPlacing
                .Where(level => !level.Equals(group.CurrentLevel))
                .ToArray();
            foreach(var level in levelsForPlacing) {
                XYZ levelsDiff = new XYZ(0, 0, level.Elevation - group.CurrentLevel.Elevation);
                XYZ newLocation = currentLocation + levelsDiff;

                Document.Create.PlaceGroup(newLocation, groupType);
            }
        }

        /// <summary>
        /// Проверяет, открыт ли в активном документе только 1 пустой лист
        /// </summary>
        /// <returns></returns>
        public bool ActiveDocOnEmptySheet() {
            return ActiveUIDocument.GetOpenUIViews().Count == 1
                && Document.ActiveView is ViewSheet sheet
                && sheet.GetAllPlacedViews().Count == 0;
        }
    }
}
