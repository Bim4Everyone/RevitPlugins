using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.UI;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models {
    internal class NameResolver<T> where T : INamedEntity {
        private IEnumerable<T> _oldCollection;
        private IEnumerable<T> _addedCollection;
        private List<T> _intersection;

        public NameResolver(IEnumerable<T> oldCollection, IEnumerable<T> addedCollection) {
            _oldCollection = oldCollection;
            _addedCollection = addedCollection;
        }

        public IEnumerable<T> GetCollection() {
            _intersection = _addedCollection
                .Where(item => _oldCollection.Any(i => i.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();
            if(_intersection.Count > 0) {
                switch(GetResult(string.Join(", ", _intersection.Select(item => item.Name).ToList()))) {
                    case TaskDialogResult.CommandLink1: {
                        _oldCollection = Replace();
                        break;
                    }
                    case TaskDialogResult.CommandLink2: {
                        _oldCollection = LeftOnlyNew();
                        break;
                    }
                    case TaskDialogResult.CommandLink3: {
                        _oldCollection = CopyWithRename();
                        break;
                    }
                    default: {
                        return _oldCollection;
                    }
                }
            }
            return _oldCollection.Union(_addedCollection.Where(item => !_intersection.Any(i => i.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))));
        }

        private TaskDialogResult GetResult(string names) {
            var dialog = new TaskDialog("BIM");
            dialog.MainContent = $"В загружаемом файле содержатся элементы с совпадающими именами: {names}.";
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Заменить");
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Оставить только новые");
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Копировать с переименованием");
            dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
            return dialog.Show();
        }

        private IEnumerable<T> Replace() {
            return _oldCollection
                .Where(item => !_intersection.Any(i => i.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)))
                .Union(_intersection)
                .ToList();
        }

        private IEnumerable<T> LeftOnlyNew() {
            return _oldCollection;
        }

        private IEnumerable<T> CopyWithRename() {
            _intersection = GetRenamedViewModels().ToList();
            return _oldCollection.Union(_intersection).ToList();
        }

        private IEnumerable<T> GetRenamedViewModels() {
            foreach(var element in _intersection) {
                var number = _oldCollection.Where(item => item.Name.StartsWith(element.Name))
                    .Select(item => GetNameNumber(item.Name, element.Name))
                    .Max();
                element.Name += number / 10 > 0 ? $"_{number + 1}" : $"_0{number + 1}";
                yield return element;
            }
        }

        private int GetNameNumber(string name, string addedElementName) {
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
