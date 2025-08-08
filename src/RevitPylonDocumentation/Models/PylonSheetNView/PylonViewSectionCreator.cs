using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewSectionCreator {
    internal PylonViewSectionCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }


    public bool TryCreateGeneralView(ViewFamilyType selectedViewFamilyType) {
        // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
        var elemForWork = SheetInfo.HostElems.First();

        if(elemForWork is null) { return false; }

        double hostLength = 0;
        double hostWidth = 0;
        XYZ midlePoint = null;
        XYZ hostVector = null;

        // Заполняем нужные поля для объекта Transform
        if(!PrepareInfoForTransform(elemForWork, ref midlePoint, ref hostVector, ref hostLength, ref hostWidth)) { return false; }

        // Формируем данные для объекта Transform
        var originPoint = midlePoint;
        var hostDir = GetHostDirByProjectTransform(hostVector);
        var upDir = XYZ.BasisZ;
        var viewDir = hostDir.CrossProduct(upDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = hostDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        var bb = elemForWork.get_BoundingBox(null);
        double minZ = bb.Min.Z;
        double maxZ = bb.Max.Z;

        double coordinateX = hostLength * 0.5 
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewXOffset));
        double coordinateYTop = maxZ - originPoint.Z 
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewYTopOffset));
        double coordinateYBottom = minZ - originPoint.Z 
            - UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewYBottomOffset));

        var sectionBoxMax = new XYZ(coordinateX, coordinateYTop, hostWidth);
        var sectionBoxMin = new XYZ(-coordinateX, coordinateYBottom, -hostWidth);

        var sectionBox = new BoundingBoxXYZ {
            Transform = t,
            Min = sectionBoxMin,
            Max = sectionBoxMax
        };

        ViewSection viewSection = null;
        try {
            viewSection = ViewSection.CreateSection(Repository.Document, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                viewSection.Name = ViewModel.ViewSectionSettings.GeneralViewPrefix + SheetInfo.PylonKeyName 
                    + ViewModel.ViewSectionSettings.GeneralViewSuffix;
                if(ViewModel.SelectedGeneralViewTemplate != null) {
                    viewSection.ViewTemplateId = ViewModel.SelectedGeneralViewTemplate.Id;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Repository.Document.Delete(viewSection.Id);
            }
            return false;
        }

        viewSection.CropBoxVisible = false;
        SheetInfo.GeneralView.ViewElement = viewSection;
        return true;
    }


    public bool TryCreateGeneralRebarView(ViewFamilyType selectedViewFamilyType) {
        // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
        var elemForWork = SheetInfo.HostElems.First();

        if(elemForWork is null) { return false; }

        double hostLength = 0;
        double hostWidth = 0;
        XYZ midlePoint = null;
        XYZ hostVector = null;

        // Заполняем нужные поля для объекта Transform
        if(!PrepareInfoForTransform(elemForWork, ref midlePoint, ref hostVector, ref hostLength, ref hostWidth)) { return false; }

        // Формируем данные для объекта Transform
        var originPoint = midlePoint;
        var hostDir = GetHostDirByProjectTransform(hostVector);
        var upDir = XYZ.BasisZ;
        var viewDir = hostDir.CrossProduct(upDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = hostDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        var bb = elemForWork.get_BoundingBox(null);
        double minZ = bb.Min.Z;
        double maxZ = bb.Max.Z;

        double coordinateX = hostLength * 0.5 
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewXOffset));
        double coordinateYTop = maxZ - originPoint.Z 
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewYTopOffset));
        double coordinateYBottom = minZ - originPoint.Z 
            - UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewYBottomOffset));

        var sectionBoxMax = new XYZ(coordinateX, coordinateYTop, hostWidth);
        var sectionBoxMin = new XYZ(-coordinateX, coordinateYBottom, -hostWidth);

        var sectionBox = new BoundingBoxXYZ {
            Transform = t,
            Min = sectionBoxMin,
            Max = sectionBoxMax
        };

        ViewSection viewSection = null;
        try {
            viewSection = ViewSection.CreateSection(Repository.Document, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                viewSection.Name =
                    ViewModel.ViewSectionSettings.GeneralRebarViewPrefix
                    + SheetInfo.PylonKeyName
                    + ViewModel.ViewSectionSettings.GeneralRebarViewSuffix;
                if(ViewModel.SelectedGeneralRebarViewTemplate != null) {
                    viewSection.ViewTemplateId = ViewModel.SelectedGeneralRebarViewTemplate.Id;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Repository.Document.Delete(viewSection.Id);
            }
            return false;
        }

        viewSection.CropBoxVisible = false;
        SheetInfo.GeneralViewRebar.ViewElement = viewSection;
        return true;
    }


    public bool TryCreateGeneralPerpendicularView(ViewFamilyType selectedViewFamilyType) {
        // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
        var elemForWork = SheetInfo.HostElems.First();

        if(elemForWork is null) { return false; }

        double hostLength = 0;
        double hostWidth = 0;
        XYZ midlePoint = null;
        XYZ hostVector = null;

        // Заполняем нужные для объекта Transform поля
        if(!PrepareInfoForTransform(elemForWork, ref midlePoint, ref hostVector, ref hostLength, ref hostWidth)) { return false; }

        // Формируем данные для объекта Transform
        var originPoint = midlePoint;
        var upDir = XYZ.BasisZ;
        var viewDir = GetHostDirByProjectTransform(hostVector).Negate();
        var rightDir = upDir.CrossProduct(viewDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = rightDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        var bb = elemForWork.get_BoundingBox(null);
        double minZ = bb.Min.Z;
        double maxZ = bb.Max.Z; 

        double coordinateX = hostWidth * 0.5 
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpXOffset));
        double coordinateYTop = maxZ - originPoint.Z 
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpYTopOffset));
        double coordinateYBottom = minZ - originPoint.Z 
            - UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpYBottomOffset));

        var sectionBoxMax = new XYZ(coordinateX, coordinateYTop, hostLength * 0.49);
        var sectionBoxMin = new XYZ(-coordinateX, coordinateYBottom, 0);

        var sectionBox = new BoundingBoxXYZ {
            Transform = t,
            Min = sectionBoxMin,
            Max = sectionBoxMax
        };

        ViewSection viewSection = null;
        try {
            viewSection = ViewSection.CreateSection(Repository.Document, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                viewSection.Name = ViewModel.ViewSectionSettings.GeneralViewPerpendicularPrefix + SheetInfo.PylonKeyName 
                    + ViewModel.ViewSectionSettings.GeneralViewPerpendicularSuffix;
                if(ViewModel.SelectedGeneralViewTemplate != null) {
                    viewSection.ViewTemplateId = ViewModel.SelectedGeneralViewTemplate.Id;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Repository.Document.Delete(viewSection.Id);
            }
            return false;
        }

        viewSection.CropBoxVisible = false;
        SheetInfo.GeneralViewPerpendicular.ViewElement = viewSection;
        return true;
    }


    public bool TryCreateGeneralRebarPerpendicularView(ViewFamilyType selectedViewFamilyType) {
        // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
        var elemForWork = SheetInfo.HostElems.First();

        if(elemForWork is null) { return false; }

        double hostLength = 0;
        double hostWidth = 0;
        XYZ midlePoint = null;
        XYZ hostVector = null;

        // Заполняем нужные для объекта Transform поля
        if(!PrepareInfoForTransform(elemForWork, ref midlePoint, ref hostVector, ref hostLength, ref hostWidth)) { return false; }

        // Формируем данные для объекта Transform
        var originPoint = midlePoint;
        var upDir = XYZ.BasisZ;
        var viewDir = GetHostDirByProjectTransform(hostVector).Negate();
        var rightDir = upDir.CrossProduct(viewDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = rightDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        var bb = elemForWork.get_BoundingBox(null);
        double minZ = bb.Min.Z;
        double maxZ = bb.Max.Z;

        double coordinateX = hostWidth * 0.5
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpXOffset));
        double coordinateYTop = maxZ - originPoint.Z
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpYTopOffset));
        double coordinateYBottom = minZ - originPoint.Z
            - UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpYBottomOffset));

        var sectionBoxMax = new XYZ(coordinateX, coordinateYTop, hostLength * 0.49);
        var sectionBoxMin = new XYZ(-coordinateX, coordinateYBottom, 0.2);

        var sectionBox = new BoundingBoxXYZ {
            Transform = t,
            Min = sectionBoxMin,
            Max = sectionBoxMax
        };

        ViewSection viewSection = null;
        try {
            viewSection = ViewSection.CreateSection(Repository.Document, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                viewSection.Name =
                    ViewModel.ViewSectionSettings.GeneralRebarViewPerpendicularPrefix
                    + SheetInfo.PylonKeyName
                    + ViewModel.ViewSectionSettings.GeneralRebarViewPerpendicularSuffix;
                if(ViewModel.SelectedGeneralRebarViewTemplate != null) {
                    viewSection.ViewTemplateId = ViewModel.SelectedGeneralRebarViewTemplate.Id;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Repository.Document.Delete(viewSection.Id);
            }
            return false;
        }

        viewSection.CropBoxVisible = false;
        SheetInfo.GeneralViewPerpendicularRebar.ViewElement = viewSection;
        return true;
    }


    public bool TryCreateTransverseView(ViewFamilyType selectedViewFamilyType, int transverseViewNum) {
        // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
        var elemForWork = SheetInfo.HostElems.First();

        if(elemForWork is null) { return false; }

        double hostLength = 0;
        double hostWidth = 0;
        XYZ midlePoint = null;
        XYZ hostVector = null;

        // Заполняем нужные для объекта Transform поля
        if(!PrepareInfoForTransform(elemForWork, ref midlePoint, ref hostVector, ref hostLength, ref hostWidth)) { return false; }

        // Формируем данные для объекта Transform
        var originPoint = midlePoint;
        var hostDir = GetHostDirByProjectTransform(hostVector).Negate();
        var viewDir = XYZ.BasisZ.Negate();
        var upDir = viewDir.CrossProduct(hostDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = hostDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        var bb = elemForWork.get_BoundingBox(null);
        double minZ = bb.Min.Z;
        double maxZ = bb.Max.Z;

        XYZ sectionBoxMin;
        XYZ sectionBoxMax;
        double elevation;
        double coordinateX = hostLength * 0.5 
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.TransverseViewXOffset));
        double coordinateY = hostWidth * 0.5 
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.TransverseViewYOffset));

        if(transverseViewNum == 1) {
            // Располагаем сечение на высоте 1/4 высоты пилона (или по пропорции, указанной пользователем)
            elevation = double.Parse(ViewModel.ViewSectionSettings.TransverseViewFirstElevation);

            sectionBoxMin = new XYZ(-coordinateX, -coordinateY, -(minZ + (maxZ - minZ) * elevation - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateY, -(minZ + (maxZ - minZ) * (elevation - 0.125) - originPoint.Z));
        } else if(transverseViewNum == 2) {
            // Располагаем сечение на высоте 1/2 высоты пилона (или по пропорции, указанной пользователем)
            elevation = double.Parse(ViewModel.ViewSectionSettings.TransverseViewSecondElevation);

            sectionBoxMin = new XYZ(-coordinateX, -coordinateY, -(minZ + (maxZ - minZ) * elevation - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateY, -(minZ + (maxZ - minZ) * (elevation - 0.125) - originPoint.Z));
        } else if(transverseViewNum == 3) {
            // Располагаем сечение на высоте 5/4 высоты пилона (или по пропорции, указанной пользователем)
            elevation = double.Parse(ViewModel.ViewSectionSettings.TransverseViewThirdElevation);

            double coordinateZBottom;
            if(elevation >= 1) {
                coordinateZBottom = -(minZ + ((maxZ - minZ) * 0.999) - originPoint.Z);
            } else {
                coordinateZBottom = -(minZ + ((maxZ - minZ) * (elevation - 0.125)) - originPoint.Z);
            }
            sectionBoxMin = new XYZ(-coordinateX, -coordinateY, -(minZ + ((maxZ - minZ) * elevation) - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateY, coordinateZBottom);
        } else {
            return false;
        }

        var sectionBox = new BoundingBoxXYZ {
            Transform = t,
            Min = sectionBoxMin,
            Max = sectionBoxMax
        };

        ViewSection viewSection = null;
        try {
            viewSection = ViewSection.CreateSection(Repository.Document, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                if(transverseViewNum == 1) {
                    viewSection.Name = ViewModel.ViewSectionSettings.TransverseViewFirstPrefix + SheetInfo.PylonKeyName 
                        + ViewModel.ViewSectionSettings.TransverseViewFirstSuffix;
                    // Если был выбран шаблон вида, то назначаем
                    if(ViewModel.SelectedTransverseViewTemplate != null) {
                        viewSection.ViewTemplateId = ViewModel.SelectedTransverseViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewFirst.ViewElement = viewSection;

                } else if(transverseViewNum == 2) {
                    viewSection.Name = ViewModel.ViewSectionSettings.TransverseViewSecondPrefix + SheetInfo.PylonKeyName 
                        + ViewModel.ViewSectionSettings.TransverseViewSecondSuffix;
                    if(ViewModel.SelectedTransverseViewTemplate != null) {
                        viewSection.ViewTemplateId = ViewModel.SelectedTransverseViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewSecond.ViewElement = viewSection;
                } else if(transverseViewNum == 3) {
                    viewSection.Name = ViewModel.ViewSectionSettings.TransverseViewThirdPrefix + SheetInfo.PylonKeyName 
                        + ViewModel.ViewSectionSettings.TransverseViewThirdSuffix;
                    if(ViewModel.SelectedTransverseViewTemplate != null) {
                        viewSection.ViewTemplateId = ViewModel.SelectedTransverseViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewThird.ViewElement = viewSection;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Repository.Document.Delete(viewSection.Id);
            }
            return false;
        }

        viewSection.CropBoxVisible = false;
        return true;
    }


    public bool TryCreateTransverseRebarView(ViewFamilyType selectedViewFamilyType, int transverseRebarViewNum) {
        // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
        var elemForWork = SheetInfo.HostElems.First();

        if(elemForWork is null) { return false; }

        double hostLength = 0;
        double hostWidth = 0;
        XYZ midlePoint = null;
        XYZ hostVector = null;

        // Заполняем нужные для объекта Transform поля
        if(!PrepareInfoForTransform(elemForWork, ref midlePoint, ref hostVector, ref hostLength, ref hostWidth)) { return false; }

        // Формируем данные для объекта Transform
        var originPoint = midlePoint;
        var hostDir = GetHostDirByProjectTransform(hostVector).Negate();
        var viewDir = XYZ.BasisZ.Negate();
        var upDir = viewDir.CrossProduct(hostDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = hostDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        var bb = elemForWork.get_BoundingBox(null);
        double minZ = bb.Min.Z;
        double maxZ = bb.Max.Z;

        XYZ sectionBoxMin;
        XYZ sectionBoxMax;
        //double elevation;
        double coordinateX = hostLength * 0.5
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.TransverseViewXOffset));
        double coordinateY = hostWidth * 0.5
            + UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ViewSectionSettings.TransverseViewYOffset));

        if(transverseRebarViewNum == 1) {
            // Располагаем сечение на высоте 1/4 высоты пилона (или по пропорции, указанной пользователем)
            double viewFirstElevation = double.Parse(ViewModel.ViewSectionSettings.TransverseRebarViewFirstElevation);

            sectionBoxMin = new XYZ(-coordinateX, -coordinateY, -(minZ + (maxZ - minZ) * viewFirstElevation - originPoint.Z));
            // Дальняя секущая плоскость разреза будет немного выше низа опалубки пилона
            sectionBoxMax = new XYZ(coordinateX, coordinateY, -(minZ + 0.1 - originPoint.Z));
        } else if(transverseRebarViewNum == 2) {
            // Располагаем сечение на высоте 1/2 высоты пилона (или по пропорции, указанной пользователем)
            double viewFirstElevation = double.Parse(ViewModel.ViewSectionSettings.TransverseRebarViewFirstElevation);
            double viewSecondElevation = double.Parse(ViewModel.ViewSectionSettings.TransverseRebarViewSecondElevation);

            sectionBoxMin = new XYZ(-coordinateX, -coordinateY, -(minZ + (maxZ - minZ) * viewSecondElevation - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateY, -(minZ + ((maxZ - minZ) * viewFirstElevation + 0.1) - originPoint.Z));
        } else {
            return false;
        }

        var sectionBox = new BoundingBoxXYZ {
            Transform = t,
            Min = sectionBoxMin,
            Max = sectionBoxMax
        };

        ViewSection viewSection = null;
        try {
            viewSection = ViewSection.CreateSection(Repository.Document, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                if(transverseRebarViewNum == 1) {
                    viewSection.Name =
                        ViewModel.ViewSectionSettings.TransverseRebarViewFirstPrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.ViewSectionSettings.TransverseRebarViewFirstSuffix;
                    // Если был выбран шаблон вида, то назначаем
                    if(ViewModel.SelectedTransverseRebarViewTemplate != null) {
                        viewSection.ViewTemplateId = ViewModel.SelectedTransverseRebarViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewFirstRebar.ViewElement = viewSection;

                } else if(transverseRebarViewNum == 2) {
                    viewSection.Name =
                        ViewModel.ViewSectionSettings.TransverseRebarViewSecondPrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.ViewSectionSettings.TransverseRebarViewSecondSuffix;
                    if(ViewModel.SelectedTransverseRebarViewTemplate != null) {
                        viewSection.ViewTemplateId = ViewModel.SelectedTransverseRebarViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewSecondRebar.ViewElement = viewSection;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Repository.Document.Delete(viewSection.Id);
            }
            return false;
        }

        viewSection.CropBoxVisible = false;
        return true;
    }


    public bool PrepareInfoForTransform(Element elemForWork, ref XYZ middlePoint, ref XYZ hostVector, 
                                        ref double hostLength, ref double hostWidth) {
        if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_StructuralColumns) {
            var column = elemForWork as FamilyInstance;

            var locationPoint = column.Location as LocationPoint;
            middlePoint = locationPoint.Point;
            double rotation = locationPoint.Rotation + 90 * Math.PI / 180;
            hostVector = Transform.CreateRotation(XYZ.BasisZ, rotation).OfVector(XYZ.BasisX);

            var hostSymbol = column.Symbol;
            hostLength = hostSymbol.LookupParameter(ViewModel.ProjectSettings.PylonLengthParamName).AsDouble();
            hostWidth = hostSymbol.LookupParameter(ViewModel.ProjectSettings.PylonWidthParamName).AsDouble();

        } else if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls) {
            if(elemForWork is not Wall wall) { return false; }
            var locationCurve = wall.Location as LocationCurve;

            if(locationCurve.Curve is not Line line) { return false; }

            var wallLineStart = line.GetEndPoint(0);
            var wallLineEnd = line.GetEndPoint(1);
            hostVector = wallLineEnd - wallLineStart;
            hostLength = hostVector.GetLength();

            hostWidth = wall.WallType.Width;
            middlePoint = wallLineStart + 0.5 * hostVector;
        } else { return false; }
        return true;
    }


    /// <summary>
    /// Метод проверяет вектор основы на направленность и редактирует при необходимости
    /// Виды должны располагаться на плане снизу-вверх и справа-налево
    /// Некоторые основы локально повернуты не по данным направлениям, поэтому вектор нужно исправить
    /// </summary>
    private XYZ GetHostDirByProjectTransform(XYZ hostVector) {
        var hostDir = hostVector.Normalize();

        // Получаем углы между вектором основы и базисами
        var angleToX = Math.Round(hostDir.AngleTo(XYZ.BasisX) * (180.0 / Math.PI));
        var angleToY = Math.Round(hostDir.AngleTo(XYZ.BasisY) * (180.0 / Math.PI));

        // Определяем нужно ли инвертировать вектор в зависимости от его положения в системе координат проекта
        bool shouldInvert = (angleToX <= 45 || angleToX >= 135) ? (angleToX <= 45) : (angleToY <= 45);
        return shouldInvert ? hostDir.Negate() : hostDir;
    }
}
