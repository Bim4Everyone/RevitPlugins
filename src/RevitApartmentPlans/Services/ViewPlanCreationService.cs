using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.Services {
    internal class ViewPlanCreationService : IViewPlanCreationService {
        private readonly RevitRepository _revitRepository;
        private readonly IBoundsCalculationService _boundsCalculateService;

        public ViewPlanCreationService(
            RevitRepository revitRepository,
            IBoundsCalculationService boundsCalculateService) {

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _boundsCalculateService = boundsCalculateService
                ?? throw new ArgumentNullException(nameof(boundsCalculateService));
        }


        public ICollection<ViewPlan> CreateViews(
            ICollection<Apartment> apartments,
            ICollection<ViewPlan> templates,
            double feetOffset) {

            List<ViewPlan> views = new List<ViewPlan>();
            using(Transaction t = _revitRepository.Document.StartTransaction("Создание планов квартир")) {
                foreach(var apartment in apartments) {
                    views.AddRange(CreateViews(apartment, templates, feetOffset));
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
                //pass
            }

            try {
                viewPlan.GetCropRegionShapeManager().SetCropShape(cropShape);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                //pass
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
