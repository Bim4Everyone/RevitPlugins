using System;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitScheduleImport.Models {
    public class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public ViewSchedule CreateSchedule(string name) {
            if(string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException(nameof(name));
            }

            var schedule = ViewSchedule.CreateSchedule(Document, new ElementId(BuiltInCategory.OST_Parking));
            var definition = schedule.Definition;
            definition.ShowHeaders = false;
            definition.IncludeLinkedFiles = false;
            definition.IsItemized = false;
            try {
                schedule.Name = name;
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                try {
                    // пытаемся добавить суффикс к названию
                    schedule.Name = $"{name}_{DateTime.Now:yyyy_MM_dd_HH-mm-ss}";
                } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                    // оставляем название по умолчанию
                }
            }
            SchedulableField schedulableField = schedule.Definition.GetSchedulableFields().First();
            schedule.Definition.AddField(schedulableField); // добавляем 1 параметр в спецификацию
            return schedule;
        }
    }
}
