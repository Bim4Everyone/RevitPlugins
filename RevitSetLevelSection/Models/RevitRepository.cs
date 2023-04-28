using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DevExpress.Utils.Extensions;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.Revit.Geometry;
using dosymep.SimpleServices;

using RevitSetLevelSection.Factories;
using RevitSetLevelSection.Models.ElementPositions;
using RevitSetLevelSection.Models.LevelProviders;
using RevitSetLevelSection.Models.Repositories;

namespace RevitSetLevelSection.Models {
    internal class RevitRepository : ILevelRepository {
        private readonly IBimModelPartsService _bimModelPartsService;

        public RevitRepository(UIApplication uiApplication, IBimModelPartsService bimModelPartsService) {
            UIApplication = uiApplication;
            _bimModelPartsService = bimModelPartsService;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;
        
        public List<Level> GetElements() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();
        }

        public IList<Element> GetElementInstances(IEnumerable<RevitParam> revitParams) {
            List<ElementId> categories = revitParams
                .SelectMany(item => item.GetParamBinding(Document).Binding.GetCategories())
                .Select(item => item.Id)
                .Distinct()
                .ToList();

            var filter = new ElementMulticategoryFilter(categories);
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .WherePasses(filter)
                .Where(item => item.Category != null)
                .OrderBy(item => item.Category.Name)
                .ToList();
        }

        public BasePoint GetBasePoint() {
            return (BasePoint) new FilteredElementCollector(Document)
                .OfClass(typeof(BasePoint))
                .FirstElement();
        }

        public Transaction StartTransaction(string transactionName) {
            return Document.StartTransaction(transactionName);
        }

        public bool IsEomFile() {
            return _bimModelPartsService.InAnyBimModelParts(Document,
                BimModelPart.EOMPart,
                BimModelPart.EOPart,
                BimModelPart.EMPart);
        }

        public bool IsKoordFile() {
            return _bimModelPartsService.InAnyBimModelParts(Document, BimModelPart.KOORDPart);
        }

        public IEnumerable<RevitLinkType> GetKoordLinkTypes() {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfClass(typeof(RevitLinkType))
                .OfType<RevitLinkType>()
                .Where(item => _bimModelPartsService.InAnyBimModelParts(item, BimModelPart.KOORDPart))
                .ToList();
        }

        public IEnumerable<RevitLinkInstance> GetLinkInstances() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .ToList();
        }

        public Workset GetWorkset(RevitLinkType revitLinkType) {
            return Document.GetWorksetTable().GetWorkset(revitLinkType.WorksetId);
        }

        public IEnumerable<MainBimBuildPart> GetBuildParts() {
            yield return MainBimBuildPart.ARPart;
            yield return MainBimBuildPart.KRPart;
            yield return MainBimBuildPart.VisPart;
        }

        public MainBimBuildPart GetBuildPart() {
            if(_bimModelPartsService.GetBimModelPart(Document) == null) {
                return null;
            }

            if(_bimModelPartsService.InAnyBimModelParts(Document, BimModelPart.ARPart, BimModelPart.GPPart)) {
                return MainBimBuildPart.ARPart;
            }

            if(_bimModelPartsService.InAnyBimModelParts(Document, BimModelPart.KRPart, BimModelPart.KMPart)) {
                return MainBimBuildPart.KRPart;
            }

            if(_bimModelPartsService.InAnyBimModelParts(Document, BimModelPart.KOORDPart)) {
                return MainBimBuildPart.KOORDPart;
            }

            // Будем считать что все остальные это ВИС
            // ошибки будущего - ошибки будущего :D
            return MainBimBuildPart.VisPart;
        }

        public bool IsExistsParam(string paramName) {
            return Document.IsExistsParam(paramName);
        }
        
        public RevitParam CreateRevitParam(string paramName) {
            return SharedParamsConfig.Instance.CreateRevitParam(Document, paramName);
        }
    }
}
