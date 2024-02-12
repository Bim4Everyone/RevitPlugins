using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Extensions {
    /// <summary>
    /// Расширения для менеджера семейства FamilyManager.
    /// </summary>
    internal static class FamilyManagerExtensions {

        /// <summary>
        /// Проверяет на наличие параметера в семействе.
        /// </summary>
        /// <param name="familyManager">Менеджер семейства FamilyManager.</param>
        /// <param name="paramName">Наименование параметра.</param>
        /// <returns>Возвращает true если параметр был добавлен в семейство Revit, иначе false.</returns>
        public static bool IsExistsParam(this FamilyManager familyManager, string paramName) {
            if(familyManager is null) {
                throw new ArgumentNullException(nameof(familyManager));
            }

            if(string.IsNullOrEmpty(paramName)) {
                throw new ArgumentException($"'{nameof(paramName)}' cannot be null or empty.", nameof(paramName));
            }
            return familyManager.GetParameters().Any(item => item.Definition.Name.Equals(paramName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Проверяет на наличие общего параметера в семействе.
        /// </summary>
        /// <param name="familyManager">Менеджер семейства FamilyManager.</param>
        /// <param name="paramName">Наименование параметра.</param>
        /// <returns>Возвращает true если общий параметр был добавлен в семейство Revit, иначе false.</returns>
        public static bool IsExistsSharedParam(this FamilyManager familyManager, string paramName) {
            if(familyManager is null) {
                throw new ArgumentNullException(nameof(familyManager));
            }

            if(string.IsNullOrEmpty(paramName)) {
                throw new ArgumentException($"'{nameof(paramName)}' cannot be null or empty.", nameof(paramName));
            }
            return familyManager.GetSharedParams().Any(item => item.Definition.Name.Equals(paramName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Проверяет на наличие параметера проекта в семействе.
        /// </summary>
        /// <param name="familyManager">Менеджер семейства FamilyManager.</param>
        /// <param name="paramName">Наименование параметра.</param>
        /// <returns>Возвращает true если параметр проекта был добавлен в семейство Revit, иначе false.</returns>
        public static bool IsExistProjectParam(this FamilyManager familyManager, string paramName) {
            if(familyManager is null) {
                throw new ArgumentNullException(nameof(familyManager));
            }

            if(string.IsNullOrEmpty(paramName)) {
                throw new ArgumentException($"'{nameof(paramName)}' cannot be null or empty.", nameof(paramName));
            }
            return familyManager.GetProjectParams().Any(item => item.Definition.Name.Equals(paramName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Возвращает список общих параметров семейства.
        /// </summary>
        /// <param name="familyManager">Менеджер семейства FamilyManager.</param>
        /// <returns>Возвращает список общих параметров семейства.</returns>
        public static IEnumerable<FamilyParameter> GetSharedParams(this FamilyManager familyManager) {
            if(familyManager is null) {
                throw new ArgumentNullException(nameof(familyManager));
            }
            return familyManager.GetParameters().Where(e => e.IsShared);
        }

        /// <summary>
        /// Возвращает список параметров семейства.
        /// </summary>
        /// <param name="familyManager">Менеджер семейства FamilyManager.</param>
        /// <returns>Возвращает список параметров проекта семейства.</returns>
        public static IEnumerable<FamilyParameter> GetProjectParams(this FamilyManager familyManager) {
            if(familyManager is null) {
                throw new ArgumentNullException(nameof(familyManager));
            }
            return familyManager.GetParameters().Where(e => !e.IsShared);
        }
    }
}
