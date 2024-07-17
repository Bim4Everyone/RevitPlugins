using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.Services {
    internal class ViewPlanCreationService : IViewPlanCreationService {
        private readonly RevitRepository _revitRepository;
        private readonly IBoundsCalculationService _boundsCalculateService;
        private readonly IRectangleLoopProvider _rectangleLoopProvider;

        public ViewPlanCreationService(
            RevitRepository revitRepository,
            IBoundsCalculationService boundsCalculateService,
            IRectangleLoopProvider rectangleLoopProvider) {

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _boundsCalculateService = boundsCalculateService
                ?? throw new ArgumentNullException(nameof(boundsCalculateService));
            _rectangleLoopProvider = rectangleLoopProvider
                ?? throw new ArgumentNullException(nameof(rectangleLoopProvider));
        }


        public ICollection<ViewPlan> CreateViews(
            ICollection<Apartment> apartments,
            ICollection<ViewPlan> templates,
            double feetOffset,
            IProgress<int> progress = null,
            CancellationToken ct = default) {

            List<ViewPlan> views = new List<ViewPlan>();
            using(Transaction t = _revitRepository.Document.StartTransaction("Создание планов квартир")) {
                var i = 0;
                foreach(var apartment in apartments) {
                    ct.ThrowIfCancellationRequested();
                    var createdViews = CreateViews(apartment, templates, feetOffset);
                    if(createdViews.Count > 0) {
                        views.AddRange(createdViews);
                    }
                    progress?.Report(++i);
                }
                t.Commit();
            }
            return views;
        }


        private ViewPlan CreateView(Apartment apartment, ViewPlan template, CurveLoop cropShape) {
            var viewPlan = ViewPlan.Create(
                _revitRepository.Document,
                _revitRepository.GetViewFamilyTypeId(template),
                apartment.LevelId);
            viewPlan.CropBoxActive = true;
            viewPlan.CropBoxVisible = true;

            string planName = CreatePlanName(apartment, template);
            try {
                viewPlan.Name = planName;
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                viewPlan.Name = planName + Guid.NewGuid();
            }

            try {
                viewPlan.ViewTemplateId = template.Id;
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                // pass
            }

            try {
                viewPlan.GetCropRegionShapeManager().SetCropShape(cropShape);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                try {
                    // Нельзя чтобы в контуре были дуги окружностей и т.п. Только прямые отрезки
                    viewPlan.GetCropRegionShapeManager()
                        .SetCropShape(_rectangleLoopProvider.CreateRectCounterClockwise(cropShape));
                } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                    // pass
                }
            }
            return viewPlan;
        }

        private ICollection<ViewPlan> CreateViews(
            Apartment apartment,
            ICollection<ViewPlan> templates,
            double feetOffset) {

            CurveLoop loop = _boundsCalculateService.CreateBounds(apartment, feetOffset);

            return templates
                .Select(t => CreateView(apartment, t, loop))
                .ToArray();
        }

        private string CreatePlanName(Apartment apartment, ViewPlan template) {
            return $"{template.Name}_{apartment.Name}_{apartment.LevelName}";
        }
    }
}
