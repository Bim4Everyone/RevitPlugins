using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitSetLevelSection.Factories;

namespace RevitSetLevelSection.Models.LevelProviders {
    internal class LevelStairsProvider : ILevelProvider {
        private readonly ILevelProviderFactory _factory;

        public LevelStairsProvider(ILevelProviderFactory factory) {
            _factory = factory;
        }

        public Level GetLevel(Element element, ICollection<Level> levels) {
            Element hostObject = GetHostObject(element);
            return hostObject == null
                ? null
                : _factory.Create(hostObject).GetLevel(hostObject, levels);
        }

        private static Element GetHostObject(Element element) {
            if(element is StairsRun stairsRun) {
                return stairsRun.GetStairs();
            } else if(element is StairsLanding stairsLanding) {
                return stairsLanding.GetStairs();
            }

            return null;
        }

        public static bool IsValidElement(Element element) {
            return GetHostObject(element) != null;
        }
    }
}