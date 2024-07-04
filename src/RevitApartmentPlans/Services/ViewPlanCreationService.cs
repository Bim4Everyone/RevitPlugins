using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.Services {
    internal class ViewPlanCreationService : IViewPlanCreationService {
        private readonly RevitRepository _revitRepository;
        private readonly IBoundsCalculateService _boundsCalculateService;

        public ViewPlanCreationService(
            RevitRepository revitRepository,
            IBoundsCalculateService boundsCalculateService) {

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


        private ViewPlan CreateView(Apartment apartment, ViewPlan template, double feetOffset) {
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

            var loop = _boundsCalculateService.CreateBounds(apartment, feetOffset);
            try {
                viewPlan.GetCropRegionShapeManager().SetCropShape(loop);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                //pass
            }
            return viewPlan;
        }

        private ICollection<ViewPlan> CreateViews(
            Apartment apartment,
            ICollection<ViewPlan> templates,
            double feetOffset) {

            return templates
                .Select(t => CreateView(apartment, t, feetOffset))
                .ToArray();
        }

        private string CreatePlanName(Apartment apartment, ViewPlan template) {
            return $"{template.Name}_{apartment.Name}_{apartment.LevelName}";
        }
    }
}
