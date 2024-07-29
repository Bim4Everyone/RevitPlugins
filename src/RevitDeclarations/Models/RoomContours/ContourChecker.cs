using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models {
    internal static class ContourChecker {
        public static ContourCheckResult CheckRoomSize(Room room,
                                                       int accuracy,
                                                       double lengthCheck,
                                                       double widthCheck) {
            lengthCheck = ParamConverter.ConvertLengthToInternal(lengthCheck, accuracy);
            widthCheck = ParamConverter.ConvertLengthToInternal(widthCheck, accuracy);

            RoomContour contour = new RoomContour(room);
            IReadOnlyList<Curve> curves = contour
                .ContourCurves
                .OrderByDescending(x => x.Length)
                .ToList();
            int contourLength = curves.Count;

            switch(contourLength) {
                case 0:
                    return new ContourCheckResult() { HasError = true };
                case 1:
                    return new ContourCheckResult() { HasError = true };
                case 2:
                    return new ContourCheckResult() { HasError = true };
                case 3:
                    return new ContourCheckResult() { 
                        MainResult = false, 
                        NeedToCheck = true
                    };
                case 4:
                    if(curves[0].Length >= lengthCheck && curves[3].Length >= widthCheck) {
                        return new ContourCheckResult() {
                            MainResult = true,
                            NeedToCheck = contour.NeedToCheck
                        };
                    }
                    return new ContourCheckResult() {
                        MainResult = false,
                        NeedToCheck = contour.NeedToCheck
                    };
                default:
                    if(curves[0].Length >= lengthCheck && curves[3].Length >= widthCheck) {
                        return new ContourCheckResult() {
                            MainResult = true,
                            NeedToCheck = true
                        };
                    }
                    return new ContourCheckResult() {
                        MainResult = false,
                        NeedToCheck = true
                    };
            }
        }

        public static ContourCheckEnum CheckAnyRoomSizes(IEnumerable<Room> rooms,
                                                         int accuracy,
                                                         double lengthCheck,
                                                         double widthCheck = 0) {
            var checkedRooms = rooms
                .Select(x => CheckRoomSize(x, accuracy, lengthCheck, widthCheck))
                .Select(x => x.GetFullResult())
                .OrderBy(x => x);

            return checkedRooms.FirstOrDefault();
        }

        public static bool CheckArea(Room room, int accuracy, double areaCheck) {
            if(ParamConverter.ConvertArea(room.Area, accuracy) >= areaCheck) {
                return true;
            }
            return false;
        }
    }
}
