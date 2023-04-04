using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.Factories.LevelProviders {
    internal abstract class LevelProviderFactory : ILevelProviderFactory {
        private readonly Dictionary<ElementId, ILevelProvider> _providersCache =
            new Dictionary<ElementId, ILevelProvider>();

        public bool CanCreate(Element element) {
            return _providersCache.ContainsKey(element.Category.Id) || CanCreateImpl(element);
        }

        public ILevelProvider Create(Element element) {
            if(_providersCache.TryGetValue(element.Category.Id, out ILevelProvider levelProvider)) {
                return levelProvider;
            }

            levelProvider = CreateImpl(element);
            _providersCache.Add(element.Category.Id, levelProvider);

            return levelProvider;
        }

        protected abstract bool CanCreateImpl(Element element);
        protected abstract ILevelProvider CreateImpl(Element element);
    }
}