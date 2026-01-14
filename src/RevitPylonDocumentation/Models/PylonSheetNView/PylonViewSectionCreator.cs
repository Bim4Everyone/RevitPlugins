using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewSectionCreator {
    internal PylonViewSectionCreator(CreationSettings settings, Document document, PylonSheetInfo pylonSheetInfo) {
        VerticalSectionSettings = settings.VerticalViewSettings;
        HorizontalSectionSettings = settings.HorizontalViewSettings;
        ProjectTypesSettings = settings.ProjectSettings;
        Doc = document;
        SheetInfo = pylonSheetInfo;
    }

    internal UserVerticalViewSettings VerticalSectionSettings { get; set; }
    internal UserHorizontalViewSettings HorizontalSectionSettings { get; set; }
    internal UserProjectSettings ProjectTypesSettings { get; set; }
    internal Document Doc { get; set; }
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
                                        int.Parse(VerticalSectionSettings.GeneralViewXOffset));
        double generalViewYTopOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(VerticalSectionSettings.GeneralViewYTopOffset));
        double generalViewYBottomOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(VerticalSectionSettings.GeneralViewYBottomOffset));

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
            viewSection = ViewSection.CreateSection(Doc, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                viewSection.Name = VerticalSectionSettings.GeneralViewPrefix + SheetInfo.PylonKeyName
                    + VerticalSectionSettings.GeneralViewSuffix;
                if(VerticalSectionSettings.SelectedGeneralViewTemplate != null) {
                    viewSection.ViewTemplateId = VerticalSectionSettings.SelectedGeneralViewTemplate.Id;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Doc.Delete(viewSection.Id);
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
                                        int.Parse(VerticalSectionSettings.GeneralViewXOffset));
        double generalViewYTopOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(VerticalSectionSettings.GeneralViewYTopOffset));
        double generalViewYBottomOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(VerticalSectionSettings.GeneralViewYBottomOffset));

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
            viewSection = ViewSection.CreateSection(Doc, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                viewSection.Name =
                    VerticalSectionSettings.GeneralRebarViewPrefix
                    + SheetInfo.PylonKeyName
                    + VerticalSectionSettings.GeneralRebarViewSuffix;
                if(VerticalSectionSettings.SelectedGeneralRebarViewTemplate != null) {
                    viewSection.ViewTemplateId = VerticalSectionSettings.SelectedGeneralRebarViewTemplate.Id;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Doc.Delete(viewSection.Id);
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
        double hostWidthToMax = SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMax;
        double hostWidthToMin = SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMin;
        double minZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMinZ;
        double maxZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMaxZ;

        double generalViewPerpXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(VerticalSectionSettings.GeneralViewPerpXOffset));
        double generalViewPerpYTopOffset = UnitUtilsHelper.ConvertToInternalValue(
                                                int.Parse(VerticalSectionSettings.GeneralViewPerpYTopOffset));
        double generalViewPerpYBottomOffset = UnitUtilsHelper.ConvertToInternalValue(
                                                int.Parse(VerticalSectionSettings.GeneralViewPerpYBottomOffset));

        double coordinateXToMax = hostWidthToMin + generalViewPerpXOffset;
        double coordinateXToMin = hostWidthToMax + generalViewPerpXOffset;
        double coordinateYTop = maxZ - originPoint.Z + generalViewPerpYTopOffset;
        double coordinateYBottom = minZ - originPoint.Z - generalViewPerpYBottomOffset;

        var sectionBoxMax = new XYZ(coordinateXToMax, coordinateYTop, hostLength * 0.49);
        var sectionBoxMin = new XYZ(-coordinateXToMin, coordinateYBottom, 0);

        var sectionBox = new BoundingBoxXYZ {
            Transform = t,
            Min = sectionBoxMin,
            Max = sectionBoxMax
        };

        ViewSection viewSection = null;
        try {
            viewSection = ViewSection.CreateSection(Doc, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                viewSection.Name = VerticalSectionSettings.GeneralViewPerpendicularPrefix + SheetInfo.PylonKeyName
                    + VerticalSectionSettings.GeneralViewPerpendicularSuffix;
                if(VerticalSectionSettings.SelectedGeneralViewTemplate != null) {
                    viewSection.ViewTemplateId = VerticalSectionSettings.SelectedGeneralViewTemplate.Id;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Doc.Delete(viewSection.Id);
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
        double hostWidthToMax = SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMax;
        double hostWidthToMin = SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMin;
        double minZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMinZ;
        double maxZ = SheetInfo.ElemsInfo.ElemsBoundingBoxMaxZ;

        double generalViewPerpXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    int.Parse(VerticalSectionSettings.GeneralViewPerpXOffset));
        double generalViewPerpYTopOffset = UnitUtilsHelper.ConvertToInternalValue(
                                                int.Parse(VerticalSectionSettings.GeneralViewPerpYTopOffset));
        double generalViewPerpYBottomOffset = UnitUtilsHelper.ConvertToInternalValue(
                                                int.Parse(VerticalSectionSettings.GeneralViewPerpYBottomOffset));

        double coordinateXToMax = hostWidthToMin + generalViewPerpXOffset;
        double coordinateXToMin = hostWidthToMax + generalViewPerpXOffset;
        double coordinateYTop = maxZ - originPoint.Z + generalViewPerpYTopOffset;
        double coordinateYBottom = minZ - originPoint.Z - generalViewPerpYBottomOffset;

        var sectionBoxMax = new XYZ(coordinateXToMax, coordinateYTop, hostLength * 0.49);
        var sectionBoxMin = new XYZ(-coordinateXToMin, coordinateYBottom, 0.2);

        var sectionBox = new BoundingBoxXYZ {
            Transform = t,
            Min = sectionBoxMin,
            Max = sectionBoxMax
        };

        ViewSection viewSection = null;
        try {
            viewSection = ViewSection.CreateSection(Doc, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                viewSection.Name =
                    VerticalSectionSettings.GeneralRebarViewPerpendicularPrefix
                    + SheetInfo.PylonKeyName
                    + VerticalSectionSettings.GeneralRebarViewPerpendicularSuffix;
                if(VerticalSectionSettings.SelectedGeneralRebarViewTemplate != null) {
                    viewSection.ViewTemplateId = VerticalSectionSettings.SelectedGeneralRebarViewTemplate.Id;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Doc.Delete(viewSection.Id);
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

        // Подрезка вида по всем элементам (с учетом выпусков армирования) нужна в случае:
        // 1. Когда это третий поперечный вид
        // 2. Когда это первый поперечный вид и используется армирование для паркинга
        bool needElemsBoundingBox = transverseViewNum switch {
            1 => SheetInfo.RebarInfo.SkeletonParentRebarForParking,
            2 => false,
            3 => true,
            _ => false
        };
        double hostWidthToMax = needElemsBoundingBox
            ? SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMax
            : SheetInfo.ElemsInfo.HostWidth / 2;

        double hostWidthToMin = needElemsBoundingBox
            ? SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMin
            : SheetInfo.ElemsInfo.HostWidth / 2;

        double minZ = SheetInfo.ElemsInfo.LastPylonMinZ;
        double maxZ = SheetInfo.ElemsInfo.LastPylonMaxZ;

        double transverseViewXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(HorizontalSectionSettings.TransverseViewXOffset));
        double transverseViewYOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(HorizontalSectionSettings.TransverseViewYOffset));

        double coordinateX = hostLength * 0.5 + transverseViewXOffset;
        double coordinateYToMax = hostWidthToMax + transverseViewYOffset;
        double coordinateYToMin = hostWidthToMin + transverseViewYOffset;

        // Определяем глубину дальней плоскости горизонтального вида
        double viewDepth = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(HorizontalSectionSettings.TransverseViewDepth));
        XYZ sectionBoxMin;
        XYZ sectionBoxMax;
        if(transverseViewNum == 1) {
            // Располагаем сечение на высоте +1000 мм от низа BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(HorizontalSectionSettings.TransverseViewFirstElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(minZ + elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(minZ + elevationOffset - viewDepth - originPoint.Z));
        } else if(transverseViewNum == 2) {
            // Располагаем сечение на высоте -3000 мм от верха BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(HorizontalSectionSettings.TransverseViewSecondElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(maxZ - elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(maxZ - elevationOffset - viewDepth - originPoint.Z));
        } else if(transverseViewNum == 3) {
            // Располагаем сечение на высоте +400 мм от верха BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(HorizontalSectionSettings.TransverseViewThirdElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(maxZ + elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(maxZ + elevationOffset - viewDepth - originPoint.Z));
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
            viewSection = ViewSection.CreateSection(Doc, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                if(transverseViewNum == 1) {
                    viewSection.Name = HorizontalSectionSettings.TransverseViewFirstPrefix + SheetInfo.PylonKeyName
                        + HorizontalSectionSettings.TransverseViewFirstSuffix;
                    // Если был выбран шаблон вида, то назначаем
                    if(HorizontalSectionSettings.SelectedTransverseViewTemplate != null) {
                        viewSection.ViewTemplateId = HorizontalSectionSettings.SelectedTransverseViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewFirst.ViewElement = viewSection;

                } else if(transverseViewNum == 2) {
                    viewSection.Name = HorizontalSectionSettings.TransverseViewSecondPrefix + SheetInfo.PylonKeyName
                        + HorizontalSectionSettings.TransverseViewSecondSuffix;
                    if(HorizontalSectionSettings.SelectedTransverseViewTemplate != null) {
                        viewSection.ViewTemplateId = HorizontalSectionSettings.SelectedTransverseViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewSecond.ViewElement = viewSection;
                } else if(transverseViewNum == 3) {
                    viewSection.Name = HorizontalSectionSettings.TransverseViewThirdPrefix + SheetInfo.PylonKeyName
                        + HorizontalSectionSettings.TransverseViewThirdSuffix;
                    if(HorizontalSectionSettings.SelectedTransverseViewTemplate != null) {
                        viewSection.ViewTemplateId = HorizontalSectionSettings.SelectedTransverseViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewThird.ViewElement = viewSection;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Doc.Delete(viewSection.Id);
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

        double minZ = SheetInfo.ElemsInfo.LastPylonMinZ;
        double maxZ = SheetInfo.ElemsInfo.LastPylonMaxZ;

        double transverseViewXOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(HorizontalSectionSettings.TransverseViewXOffset));
        double transverseViewYOffset = UnitUtilsHelper.ConvertToInternalValue(
                                            int.Parse(HorizontalSectionSettings.TransverseViewYOffset));

        double hostLength = SheetInfo.ElemsInfo.ElemsBoundingBoxLength;

        // Подрезка вида по всем элементам (с учетом выпусков армирования) нужна в случае:
        // 1. Когда это третий поперечный вид
        // 2. Когда это первый поперечный вид и используется армирование для паркинга
        bool needElemsBoundingBox = transverseRebarViewNum switch {
            1 => SheetInfo.RebarInfo.SkeletonParentRebarForParking,
            2 => false,
            3 => true,
            _ => false
        };
        double hostWidthToMax = needElemsBoundingBox
            ? SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMax
            : SheetInfo.ElemsInfo.HostWidth / 2;

        double hostWidthToMin = needElemsBoundingBox
            ? SheetInfo.ElemsInfo.ElemsBoundingBoxWidthToMin
            : SheetInfo.ElemsInfo.HostWidth / 2;

        double coordinateX = hostLength * 0.5 + transverseViewXOffset;
        double coordinateYToMax = hostWidthToMax + transverseViewYOffset;
        double coordinateYToMin = hostWidthToMin + transverseViewYOffset;

        // Определяем глубину дальней плоскости горизонтального вида
        double viewDepth = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(HorizontalSectionSettings.TransverseRebarViewDepth));
        XYZ sectionBoxMin;
        XYZ sectionBoxMax;
        if(transverseRebarViewNum == 1) {
            // Располагаем сечение на высоте 1000 мм от низа BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                        double.Parse(HorizontalSectionSettings.TransverseViewFirstElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(minZ + elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(minZ + elevationOffset - viewDepth - originPoint.Z));
        } else if(transverseRebarViewNum == 2) {
            // Располагаем сечение на высоте -1500 мм от верха BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(HorizontalSectionSettings.TransverseViewSecondElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(maxZ - elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(maxZ - elevationOffset - viewDepth - originPoint.Z));
        } else if(transverseRebarViewNum == 3) {
            // Располагаем сечение на высоте -300 мм от верха BoundingBox (или по значению указанному пользователем)
            double elevationOffset = UnitUtilsHelper.ConvertToInternalValue(
                                    double.Parse(HorizontalSectionSettings.TransverseViewThirdElevation));

            sectionBoxMin = new XYZ(-coordinateX, -coordinateYToMin, -(maxZ + elevationOffset - originPoint.Z));
            sectionBoxMax = new XYZ(coordinateX, coordinateYToMax, -(maxZ + elevationOffset - viewDepth - originPoint.Z));
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
            viewSection = ViewSection.CreateSection(Doc, selectedViewFamilyType.Id, sectionBox);
            if(viewSection != null) {
                if(transverseRebarViewNum == 1) {
                    viewSection.Name =
                        HorizontalSectionSettings.TransverseRebarViewFirstPrefix
                        + SheetInfo.PylonKeyName
                        + HorizontalSectionSettings.TransverseRebarViewFirstSuffix;
                    // Если был выбран шаблон вида, то назначаем
                    if(HorizontalSectionSettings.SelectedTransverseRebarViewTemplate != null) {
                        viewSection.ViewTemplateId = HorizontalSectionSettings.SelectedTransverseRebarViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewFirstRebar.ViewElement = viewSection;

                } else if(transverseRebarViewNum == 2) {
                    viewSection.Name =
                        HorizontalSectionSettings.TransverseRebarViewSecondPrefix
                        + SheetInfo.PylonKeyName
                        + HorizontalSectionSettings.TransverseRebarViewSecondSuffix;
                    if(HorizontalSectionSettings.SelectedTransverseRebarViewTemplate != null) {
                        viewSection.ViewTemplateId = HorizontalSectionSettings.SelectedTransverseRebarViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewSecondRebar.ViewElement = viewSection;

                } else if(transverseRebarViewNum == 3) {
                    viewSection.Name =
                        HorizontalSectionSettings.TransverseRebarViewThirdPrefix
                        + SheetInfo.PylonKeyName
                        + HorizontalSectionSettings.TransverseRebarViewThirdSuffix;
                    if(HorizontalSectionSettings.SelectedTransverseRebarViewTemplate != null) {
                        viewSection.ViewTemplateId = HorizontalSectionSettings.SelectedTransverseRebarViewTemplate.Id;
                    }
                    SheetInfo.TransverseViewThirdRebar.ViewElement = viewSection;
                }
            }
        } catch(Exception) {
            if(viewSection != null) {
                Doc.Delete(viewSection.Id);
            }
            return false;
        }

        viewSection.CropBoxVisible = false;
        return true;
    }
}
