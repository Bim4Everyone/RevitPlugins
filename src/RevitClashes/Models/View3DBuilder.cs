using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitViews {
    internal class View3DBuilder : IView3DBuilder {
        private string _name;
        private View3D _template;
        private IView3DSetting[] _settings;

        public IView3DBuilder SetName(string name) {
            if(name is null) {
                throw new ArgumentNullException(nameof(name));
            }

            _name = name;
            return this;
        }

        public IView3DBuilder SetTemplate(View3D template) {
            if(template is null) {
                throw new ArgumentNullException(nameof(template));
            }

            _template = template;
            return this;
        }

        public IView3DBuilder SetViewSettings(params IView3DSetting[] settings) {
            _settings = settings;
            return this;
        }

        public View3D Build(Document doc) {
            if(doc is null) {
                throw new ArgumentNullException(nameof(doc));
            }

            if(string.IsNullOrEmpty(_name)) {
                throw new ArgumentNullException(nameof(_name), "Необходимо задать наименование для вида.");
            }

            View3D view = null;
            using(Transaction t = doc.StartTransaction("Создание 3D вида")) {
                var type = Get3DViewType(doc);
                ApplyTemplate(type);
                view = View3D.CreateIsometric(doc, type.Id);
                view.Name = _name;
                ApplySettings(view);
                t.Commit();
            }

            return view;
        }

        private ViewFamilyType Get3DViewType(Document doc) {
            var type = new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewFamilyType))
                        .Cast<ViewFamilyType>()
                        .FirstOrDefault(v => v.ViewFamily == ViewFamily.ThreeDimensional);

            if(type == null) {
                throw new ArgumentNullException(nameof(doc), "В документе отсутствует тип 3D вида.");
            }

            return type;
        }

        private void ApplyTemplate(ViewFamilyType type) {
            if(_template == null) {
                type.DefaultTemplateId = ElementId.InvalidElementId;
            } else {
                if(!_template.IsTemplate) {
                    throw new ArgumentException($"Вид \"{_template.Name}\" c Id = {_template.Id} не является шаблоном.", nameof(_template));
                }
                type.DefaultTemplateId = _template.Id;
            }
        }

        private void ApplySettings(View3D view) {
            if(_settings != null) {
                foreach(var setting in _settings) {
                    setting.Apply(view);
                }
            }
        }
    }
}
