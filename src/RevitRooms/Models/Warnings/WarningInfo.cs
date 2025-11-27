using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;

using RevitRooms.Views;

namespace RevitRooms.Models;

internal class WarningInfo {
    public static WarningInfo GetRedundantRooms(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Error,
            Message = localizationService.GetLocalizedString("WarningsWindow.RedundantRooms"),
            Description = localizationService.GetLocalizedString("WarningsWindow.RedundantRoomsDescription")
        };
    }

    public static WarningInfo GetRedundantAreas(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Error,
            Message = localizationService.GetLocalizedString("WarningsWindow.RedundantAreas"),
            Description = localizationService.GetLocalizedString("WarningsWindow.RedundantAreasDescription")
        };
    }

    public static WarningInfo GetRequiredParams(ILocalizationService localizationService, params string[] args) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Error,
            Message = localizationService.GetLocalizedString("WarningsWindow.RequiredParams", args),
            Description = localizationService.GetLocalizedString("WarningsWindow.RequiredParamsDescription")
        };
    }

    public static WarningInfo GetNotEqualGroupType(ILocalizationService localizationService, params string[] args) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Error,
            Message = localizationService.GetLocalizedString("WarningsWindow.NotEqualGroupType", args),
            Description = localizationService.GetLocalizedString("WarningsWindow.NotEqualGroupTypeDescription", 
                                                                 ProjectParamsConfig.Instance.RoomTypeGroupName.Name, 
                                                                 ProjectParamsConfig.Instance.RoomTypeGroupName.Name, 
                                                                 ProjectParamsConfig.Instance.RoomGroupName.Name)
        };
    }

    public static WarningInfo GetNotEqualMultiLevel(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Error,
            Message = localizationService.GetLocalizedString("WarningsWindow.NotEqualMultiLevel"),
            Description = localizationService.GetLocalizedString("WarningsWindow.NotEqualMultiLevelDescription", 
                                                                 SharedParamsConfig.Instance.RoomMultilevelGroup.Name, 
                                                                 ProjectParamsConfig.Instance.RoomGroupName.Name)
        };
    }
    public static WarningInfo GetErrorMultiLevelRoom(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Error,
            Message = localizationService.GetLocalizedString("WarningsWindow.ErrorMultiLevelRoom", ProjectParamsConfig.Instance.IsRoomMainLevel.Name),
            Description = localizationService.GetLocalizedString("WarningsWindow.ErrorMultiLevelRoomDescription", ProjectParamsConfig.Instance.IsRoomMainLevel.Name)
        };
    }

    public static WarningInfo GetEqualSectionDoors(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Warning,
            Message = localizationService.GetLocalizedString("WarningsWindow.EqualSectionDoors", ProjectParamsConfig.Instance.RoomSectionName.Name),
            Description = localizationService.GetLocalizedString("WarningsWindow.EqualSectionDoorsDescription", ProjectParamsConfig.Instance.RoomSectionName.Name)
        };
    }

    public static WarningInfo GetNotEqualGroup(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Warning,
            Message = localizationService.GetLocalizedString("WarningsWindow.NotEqualGroup", ProjectParamsConfig.Instance.RoomGroupName.Name),
            Description = localizationService.GetLocalizedString("WarningsWindow.NotEqualGroupDescription", ProjectParamsConfig.Instance.RoomGroupName.Name)
        };
    }

    public static WarningInfo GetContourIntersectRooms(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Warning,
            Message = localizationService.GetLocalizedString("WarningsWindow.ContourIntersectRooms"),
            Description = localizationService.GetLocalizedString("WarningsWindow.CoutourIntersectRoomsDescription")
        };
    }

    public static WarningInfo GetBigChangesRoomAreas(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Info,
            Message = localizationService.GetLocalizedString("WarningsWindow.BigChangesRoomAreas"),
            Description = localizationService.GetLocalizedString("WarningsWindow.BigChangesDescription")
        };
    }

    public static WarningInfo GetBigChangesFlatAreas(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Info,
            Message = localizationService.GetLocalizedString("WarningsWindow.BigChangesFlatAreas"),
            Description = localizationService.GetLocalizedString("WarningsWindow.BigChangesDescription")
        };
    }

    public static WarningInfo GetBigChangesAreas(ILocalizationService localizationService) {
        return new WarningInfo() {
            TypeInfo = WarningTypeInfo.Info,
            Message = localizationService.GetLocalizedString("WarningsWindow.BigChangesAreas"),
            Description = localizationService.GetLocalizedString("WarningsWindow.BigChangesDescription")
        };
    }

    public string Message { get; set; }
    public string Description { get; set; }

    public WarningTypeInfo TypeInfo { get; set; }
}
