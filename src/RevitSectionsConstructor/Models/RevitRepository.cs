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
                .Where(group => group.GroupId == ElementId.InvalidElementId && group.Location is LocationPoint)
                .Cast<Group>()
                .ToArray();
        }

        public void RemoveElement(ElementId elementId) {
            if(elementId is null) { throw new ArgumentNullException(nameof(elementId)); }

            Document.Delete(elementId);
        }

        public void CopyGroup(GroupWithAction group) {
            if(group is null) { throw new ArgumentNullException(nameof(group)); }

            XYZ currentLocation = (group.Group.Location as LocationPoint).Point;
            GroupType groupType = group.Group.GroupType;
            foreach(var level in group.LevelsForPlacing) {
                XYZ levelsDiff = new XYZ(0, 0, level.Elevation - group.CurrentLevel.Elevation);
                XYZ newLocation = currentLocation + levelsDiff;

                Document.Create.PlaceGroup(newLocation, groupType);
            }
        }
    }
}
