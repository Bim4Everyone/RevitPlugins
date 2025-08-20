using System;

using Autodesk.Revit.DB;

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
        // Формируем данные для объекта Transform
        var originPoint = SheetInfo.ElemsInfo.HostOrigin;
        var hostDir = SheetInfo.ElemsInfo.VectorByLength;
        var upDir = XYZ.BasisZ;
        var viewDir = hostDir.CrossProduct(upDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = hostDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        double hostLength = SheetInfo.ElemsInfo.ElemsBoundingBoxLength;
        double hostWidth = SheetInfo.ElemsInfo.ElemsBoundingBoxWidth;
        double minZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMinZ;
        double maxZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMaxZ;

        double generalViewXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                        int.Parse(ViewModel.ViewSectionSettings.GeneralViewXOffset));
        double generalViewYTopOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.GeneralViewYTopOffset));
        double generalViewYBottomOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.GeneralViewYBottomOffset));

        double coordinateX = hostLength * 0.5 + generalViewXOffset;
        double coordinateYTop = maxZ - originPoint.Z + generalViewYTopOffset;
        double coordinateYBottom = minZ - originPoint.Z - generalViewYBottomOffset;

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
        // Формируем данные для объекта Transform
        var originPoint = SheetInfo.ElemsInfo.HostOrigin;
        var hostDir = SheetInfo.ElemsInfo.VectorByLength;
        var upDir = XYZ.BasisZ;
        var viewDir = hostDir.CrossProduct(upDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = hostDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        double hostLength = SheetInfo.ElemsInfo.ElemsBoundingBoxLength;
        double hostWidth = SheetInfo.ElemsInfo.ElemsBoundingBoxWidth;
        double minZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMinZ;
        double maxZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMaxZ;

        double generalViewXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                        int.Parse(ViewModel.ViewSectionSettings.GeneralViewXOffset));
        double generalViewYTopOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.GeneralViewYTopOffset));
        double generalViewYBottomOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.GeneralViewYBottomOffset));

        double coordinateX = hostLength * 0.5 + generalViewXOffset;
        double coordinateYTop = maxZ - originPoint.Z + generalViewYTopOffset;
        double coordinateYBottom = minZ - originPoint.Z - generalViewYBottomOffset;

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
        // Формируем данные для объекта Transform
        var originPoint = SheetInfo.ElemsInfo.HostOrigin;
        var upDir = XYZ.BasisZ;
        var viewDir = SheetInfo.ElemsInfo.VectorByLength.Negate();
        var rightDir = upDir.CrossProduct(viewDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = rightDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        double hostLength = SheetInfo.ElemsInfo.ElemsBoundingBoxLength;
        double hostWidth = SheetInfo.ElemsInfo.ElemsBoundingBoxWidth;
        double minZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMinZ;
        double maxZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMaxZ;

        double generalViewPerpXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpXOffset));
        double generalViewPerpYTopOffset = UnitUtilsHelper.ConvertToInternalValue(
                                                int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpYTopOffset));
        double generalViewPerpYBottomOffset = UnitUtilsHelper.ConvertToInternalValue(
                                                int.Parse(ViewModel.ViewSectionSettings.GeneralViewPerpYBottomOffset));

        double coordinateX = hostWidth * 0.5 + generalViewPerpXOffset;
        double coordinateYTop = maxZ - originPoint.Z + generalViewPerpYTopOffset;
        double coordinateYBottom = minZ - originPoint.Z - generalViewPerpYBottomOffset;

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
        // Формируем данные для объекта Transform
        var originPoint = SheetInfo.ElemsInfo.HostOrigin;
        var upDir = XYZ.BasisZ;
        var viewDir = SheetInfo.ElemsInfo.VectorByLength.Negate();
        var rightDir = upDir.CrossProduct(viewDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = rightDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        double hostLength = SheetInfo.ElemsInfo.ElemsBoundingBoxLength;
        double hostWidth = SheetInfo.ElemsInfo.ElemsBoundingBoxWidth;
        double minZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMinZ;
        double maxZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMaxZ;

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
        // Формируем данные для объекта Transform
        var originPoint = SheetInfo.ElemsInfo.HostOrigin;
        var hostDir = SheetInfo.ElemsInfo.VectorByLength.Negate();
        var viewDir = XYZ.BasisZ.Negate();
        var upDir = viewDir.CrossProduct(hostDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = hostDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        double hostLength = SheetInfo.ElemsInfo.ElemsBoundingBoxLength;
        double hostWidthToMax = SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMax;
        double hostWidthToMin = SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMin;

        double minZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMinZ;
        double maxZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMaxZ;

        double transverseViewXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.TransverseViewXOffset));
        double transverseViewYOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.TransverseViewYOffset));

        double coordinateX = hostLength * 0.5 + transverseViewXOffset;
        double coordinateYToMax = hostWidthToMax + transverseViewYOffset;
        double coordinateYToMin = hostWidthToMin + transverseViewYOffset;

        // Определяем глубину дальней плоскости горизонтального вида
        double viewDepth = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(ViewModel.ViewSectionSettings.TransverseViewDepth));
        XYZ sectionBoxMin;
        XYZ sectionBoxMax;
        if(transverseViewNum == 1) {
            // Располагаем сечение на высоте 1000 мм от низа BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(ViewModel.ViewSectionSettings.TransverseViewFirstElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(minZ + elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(minZ + elevationOffset - viewDepth - originPoint.Z));
        } else if(transverseViewNum == 2) {
            // Располагаем сечение на высоте 2000 мм от низа BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
            double.Parse(ViewModel.ViewSectionSettings.TransverseViewSecondElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(minZ + elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(minZ + elevationOffset - viewDepth - originPoint.Z));
        } else if(transverseViewNum == 3) {
            // Располагаем сечение на высоте -300 мм от верха BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(ViewModel.ViewSectionSettings.TransverseViewThirdElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(maxZ - elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(maxZ - elevationOffset - viewDepth - originPoint.Z));
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
        // Формируем данные для объекта Transform
        var originPoint = SheetInfo.ElemsInfo.HostOrigin;
        var hostDir = SheetInfo.ElemsInfo.VectorByLength.Negate();
        var viewDir = XYZ.BasisZ.Negate();
        var upDir = viewDir.CrossProduct(hostDir);

        // Передаем данные для объекта Transform
        var t = Transform.Identity;
        t.Origin = originPoint;
        t.BasisX = hostDir;
        t.BasisY = upDir;
        t.BasisZ = viewDir;

        double minZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMinZ;
        double maxZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMaxZ;

        double transverseViewXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.TransverseViewXOffset));
        double transverseViewYOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(ViewModel.ViewSectionSettings.TransverseViewYOffset));

        double hostLength = SheetInfo.ElemsInfo.ElemsBoundingBoxLength;
        double hostWidthToMax = SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMax;
        double hostWidthToMin = SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMin;

        double coordinateX = hostLength * 0.5 + transverseViewXOffset;
        double coordinateYToMax = hostWidthToMax + transverseViewYOffset;
        double coordinateYToMin = hostWidthToMin + transverseViewYOffset;

        // Определяем глубину дальней плоскости горизонтального вида
        double viewDepth = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(ViewModel.ViewSectionSettings.TransverseRebarViewDepth));
        XYZ sectionBoxMin;
        XYZ sectionBoxMax;
        if(transverseRebarViewNum == 1) {
            // Располагаем сечение на высоте 1000 мм от низа BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                        double.Parse(ViewModel.ViewSectionSettings.TransverseRebarViewFirstElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(minZ + elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(minZ + elevationOffset - viewDepth - originPoint.Z));
        } else if(transverseRebarViewNum == 2) {
            // Располагаем сечение на высоте -300 мм от верха BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(ViewModel.ViewSectionSettings.TransverseRebarViewSecondElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(maxZ - elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(maxZ - elevationOffset - viewDepth - originPoint.Z));
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
}
