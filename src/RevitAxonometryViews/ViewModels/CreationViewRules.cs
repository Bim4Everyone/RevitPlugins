using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using RevitAxonometryViews.Models;

namespace RevitAxonometryViews.ViewModels
{
    internal class CreationViewRules
    {
        public CreationViewRules(bool isCombined, bool useSharedSystemName) {
            IsCombined = isCombined;
            IsSingle = !isCombined;

            UseSharedSystemName = useSharedSystemName;
            UseSystemName = !useSharedSystemName;

            GetCategories();
        }
        public bool IsSingle { get; set; }
        public bool IsCombined {  get; set; }
        public bool UseSharedSystemName { get; set; }
        public bool UseSystemName { get; set; }
        public List<ElementId> Categories { get; set; }


        private void GetCategories() {
            Categories = AxonometryConfig.SystemCategories
                .Select(category => new ElementId(category))
                .ToList();
            // Если фильтруем по ФОП_ВИС_Имя системы - оставляем обобщенки, они есть в оборудовании.
            // Системного имени системы у них нет, будет ошибка.
            if(UseSharedSystemName != true) {
                Categories.Remove(new ElementId(BuiltInCategory.OST_GenericModel));
            }
        }
    }
}


//List<ElementId> categories = AxonometryConfig.SystemCategories
//    .Select(category => new ElementId(category))
//    .ToList();
//// Если фильтруем по ФОП_ВИС_Имя системы - оставляем обобщенки, они есть в оборудовании.
//// Системного имени системы у них нет, будет ошибка.
//if(_useFopNames != true) {
//    categories.Remove(new ElementId(BuiltInCategory.OST_GenericModel));
//}
