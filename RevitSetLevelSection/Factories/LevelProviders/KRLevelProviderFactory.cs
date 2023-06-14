using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.Mvvm.Native;

using dosymep.Revit;

using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.ElementPositions;
using RevitSetLevelSection.Models.LevelProviders;

namespace RevitSetLevelSection.Factories.LevelProviders {
    internal class KRLevelProviderFactory : LevelProviderFactory {
        private View3D _view3D;

        public KRLevelProviderFactory(IResolutionRoot resolutionRoot, IElementPositionFactory positionFactory)
            : base(resolutionRoot, positionFactory) {
        }

        protected override bool CanCreateImpl(Element element) {
            return base.CanCreateImpl(element)
                   || element.InAnyCategory(BuiltInCategory.OST_Floors);
        }

        protected override ILevelProvider CreateImpl(Element element) {
            if(element.InAnyCategory(BuiltInCategory.OST_Floors)) {
                return _resolutionRoot.Get<LevelMagicBottomProvider>(GetConstructorArgument(element));
            }

            if(element is Wall wall) {
                if(IsStructFarmCode(wall) || IsStructFarm(wall)) {
                    var constructorArgument =
                        new ConstructorArgument("elementPosition", _resolutionRoot.Get<ElementTopPosition>());
                    return _resolutionRoot.Get<LevelMagicBottomProvider>(constructorArgument);
                }
            }

            return base.CreateImpl(element);
        }

        /// <summary>
        /// Проверяем стену, является ли она Балкой по коду классификатора
        /// </summary>
        /// <param name="wall">Проверяемая стена.</param>
        /// <returns>Возвращает true - если она является балкой, иначе false.</returns>
        private bool IsStructFarmCode(Wall wall) {
            return wall.GetElementType()
                ?.GetParamValue<string>(BuiltInParameter.UNIFORMAT_CODE)
                ?.StartsWith("ОС.КЭ.3.2") == true;
        }

        /// <summary>
        /// Проверяем стену, является ли она Балкой по коду классификатора
        /// </summary>
        /// <param name="wall">Проверяемая стена.</param>
        /// <returns>Возвращает true - если она является балкой, иначе false.</returns>
        private bool IsStructFarm(Wall wall) {
            // 1300 mm
            if(wall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM) > 4.26509186351706) {
                return false;
            }

            BuiltInCategory[] categories = new[] {
                BuiltInCategory.OST_Floors, BuiltInCategory.OST_Walls, BuiltInCategory.OST_StructuralFraming
            };

            var filter = new LogicalAndFilter(
                new ElementMulticategoryFilter(categories),
                new ExclusionFilter(new[] {wall.Id}));

            // Надеюсь что нужный вид существует
            var intersector = new ReferenceIntersector(filter, FindReferenceTarget.Face, GetView3D(wall));

            // пока считаю что начальной точки достаточно
            var point = ((LocationCurve) wall.Location).Curve.GetEndPoint(0);
            point = new XYZ(point.X, point.Y,
                point.Z
                - 0.00003 // magic number (нижний фейс, при выравнивании)
                + wall.GetParamValue<double>(BuiltInParameter.WALL_BASE_OFFSET)
                + wall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM)); 
            
            var result = intersector.FindNearest(point, XYZ.BasisZ);
            
            // 140 mm
            return result?.Proximity <= (0.459317585301837 + 0.00003);
        }

        private View3D GetView3D(Element element) {
            if(_view3D is null) {
                _view3D = new FilteredElementCollector(element.Document)
                              .OfClass(typeof(View3D))
                              .OfType<View3D>()
                              .Where(item => !item.IsTemplate)
                              .Where(item => item.Name.Equals("Navisworks"))
                              .FirstOrDefault()
                          ?? new FilteredElementCollector(element.Document)
                              .OfClass(typeof(View3D))
                              .OfType<View3D>()
                              .Where(item => !item.IsTemplate)
                              .FirstOrDefault();
            }

            return _view3D;
        }
    }
}