using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Interfaces;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models {
    internal class Renamer {
        public static string GetName(INamedEntity entity, IEnumerable<INamedEntity> oldEntities) {
            if(!oldEntities.Any(item => item.Name.Equals(entity.Name, StringComparison.CurrentCulture))) {
                return entity.Name;
            }
            var number = oldEntities.Where(item => item.Name.StartsWith(entity.Name))
                   .Select(item => GetNameNumber(item.Name, entity.Name))
                   .Max();
            return entity.Name + (number / 10 > 0 ? $"_{number + 1}" : $"_0{number + 1}");
        }

        private static int GetNameNumber(string name, string addedElementName) {
            if(name.Length == addedElementName.Length) {
                return 0;
            }
            if(int.TryParse(name.Substring(addedElementName.Length + 1), out int res)) {
                return res;
            }
            return 0;
        }
    }
}
