using Autodesk.Revit.ApplicationServices;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    /// <summary>
    /// Класс для получения значения логина пользователя, запустившего плагин
    /// </summary>
    internal class UsernameGetter : IValueGetter<StringParamValue> {
        private readonly Application _app;


        /// <summary>
        /// Конструктор класса, предоставляющего имя пользователя, запустившего плагин
        /// </summary>
        /// <param name="app"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public UsernameGetter(Application app) {
            _app = app ?? throw new System.ArgumentNullException(nameof(app));
        }

        public StringParamValue GetValue() {
            return new StringParamValue(_app.Username);
        }
    }
}
