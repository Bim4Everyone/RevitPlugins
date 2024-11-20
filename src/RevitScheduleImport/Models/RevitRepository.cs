using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitScheduleImport.Models {
    public class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public ViewSchedule CreateSchedule(string name, BuiltInCategory category) {
            if(string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException(nameof(name));
            }

            var schedule = ViewSchedule.CreateSchedule(Document, new ElementId(category));
            var definition = schedule.Definition;
            definition.ShowHeaders = false;
            definition.IncludeLinkedFiles = false;
            definition.IsItemized = false;
            try {
                schedule.Name = name;
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                try {
                    // пытаемся добавить суффикс к названию
                    schedule.Name = $"{name}_{Application.Username}_{DateTime.Now:yyyy_MM_dd_HH-mm-ss}";
                } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                    // оставляем название по умолчанию
                }
            }
            SchedulableField schedulableField = schedule.Definition.GetSchedulableFields().First();
            schedule.Definition.AddField(schedulableField); // добавляем 1 параметр в спецификацию
            return schedule;
        }

        public ICollection<Category> GetScheduledCategories() {
            return Document.Settings.Categories
                .OfType<Category>()
                .Where(c => c.CategoryType == CategoryType.Model && c.IsVisibleInUI == true)
                .ToArray();
        }

        public void DeleteSchedule(ViewSchedule schedule) {
            if(schedule != null && schedule.Id.IsNotNull()) {
                Document.Delete(schedule.Id);
            }
        }
    }
}
