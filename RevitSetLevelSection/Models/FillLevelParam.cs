using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using DevExpress.Utils.Extensions;

using dosymep.Bim4Everyone;
using dosymep.Revit;

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

            List<Level> zoneLevels = _zoneInfos
                .Where(item => _intersectImpl.IsIntersect(item, element))
                .Select(item => _sourceLevels.GetValueOrDefault(item.Level.Name, null))
                .Where(item => item != null)
                .ToList();

            var level = _levelProviderFactory.Create(element).GetLevel(element, zoneLevels);
            element.SetParamValue(_revitParam, level?.Name.Split('_').FirstOrDefault());
        }
    }
}