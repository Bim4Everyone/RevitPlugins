using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitRooms.Models;

public class PluginSettings {
    public string PhaseRoomsPartition => "Межквартирные перегородки";
    public string PhaseBuildingContour => "Контур здания";
    public string LevelRoofPrefix => "К";
    public string LevelRoof => "Кровля";
    public string LevelUndergroundPrefix => "П";
    public string LevelUnderground => "Подземный";
    public string LevelTechPrefix => "Т";
    public string LevelTech => "Технический";
    public string RoomsName => "квартира";
    public string ApartmentName => "апартаменты";
    public string HotelRoom => "гостиничный номер";
    public string Penthouse => "пентхаус";
    public string BalconyDoorFamilyName => "Окн_ББлок_";
}
