using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DevExpress.Utils.Extensions;

using dosymep.Bim4Everyone;

using RevitSetLevelSection.Factories;
using RevitSetLevelSection.Models.Repositories;

namespace RevitSetLevelSection.Models {
    internal class FillLevelParam : IFillParam {
        private readonly RevitParam _revitParam;
        private readonly IZoneRepository _zoneRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly ILevelProviderFactory _levelProviderFactory;

        private readonly List<ZoneInfo> _zoneInfos;
        private readonly Dictionary<string, Level> _sourceLevels;
        private readonly IntersectImpl _intersectImpl;

        public FillLevelParam(RevitParam revitParam,
            IZoneRepository zoneRepository,
            ILevelRepository levelRepository,
            ILevelProviderFactory levelProviderFactory, Application application) {
            _revitParam = revitParam;
            _zoneRepository = zoneRepository;
            _levelRepository = levelRepository;
            _levelProviderFactory = levelProviderFactory;

            _intersectImpl =
                new IntersectImpl() {Application = application};

            // Кешируем нужные объекты
            _zoneInfos = zoneRepository.GetZones();
            _sourceLevels = levelRepository.GetElements().ToDictionary(item => item.Name);
        }

        public RevitParam RevitParam => _revitParam;

        public void UpdateValue(Element element) {
            if(!element.IsExistsParam(_revitParam)) {
                return;
            }

            if(!_levelProviderFactory.CanCreate(element)) {
                element.RemoveParamValue(_revitParam);
                return;
            }

            ICollection<Level> zoneLevels = _zoneInfos
                .Where(item => _intersectImpl.IsIntersect(item, element))
                .Select(item => _sourceLevels.GetValueOrDefault(item.Level.Name, null))
                .Where(item => item != null)
                .ToList();

            try {
                var level = GetLevel(element, zoneLevels);
                element.SetParamValue(_revitParam, level?.Name.Split('_').FirstOrDefault());
            } catch(InvalidOperationException) {
                // решили что существует много вариантов,
                // когда параметр не может заполнится из-за настроек в ревите
                // Например: элемент находится в более поздней стадии
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                // решили что существует много вариантов,
                // когда параметр не может заполнится из-за настроек в ревите
                // Например: элемент находится в более поздней стадии
            }
        }

        private Level GetLevel(Element element, ICollection<Level> levels) {
            return _levelProviderFactory.Create(element).GetLevel(element, levels)
                   ?? _levelProviderFactory.CreateDefault(element).GetLevel(element, levels);
        }
    }
}