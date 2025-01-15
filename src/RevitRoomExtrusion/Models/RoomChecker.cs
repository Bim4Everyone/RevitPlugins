using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;


namespace RevitRoomExtrusion.Models {
    internal class RoomChecker {
        private readonly RevitRepository _revitRepository;
        public RoomChecker(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public bool IsSelected() {
            bool result = true;
            if(_revitRepository.GetRooms().Count <= 0) {
                MessageBox.Show("Помещения не выбраны",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                result = false;
            }
            return result;
        }
        public bool IsCheked() {
            bool resultCheck = true;
            foreach(Room room in _revitRepository.GetRooms()) {
                if(IsInValidRoom(room)) {
                    resultCheck = false;
                    break;
                }
            }
            if(!resultCheck) {
                MessageBox.Show("В выборке содержатся помещения с ошибками",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            return resultCheck;
        }
        public bool IsInValidRoom(Room room) {
            bool resultCheck = false;
            if (room.IsNotEnclosed() || room.IsRedundant() || IsIntersectBoundary(room)) {
                resultCheck = true;
            }            
            return resultCheck;
        }        
        public bool IsIntersectBoundary(Room room) {
            bool resultCheck = false;
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> listListSegments = room.GetBoundarySegments(options);
           
            foreach(IList<BoundarySegment> listSegments in listListSegments) {
                if(IsIntersectCurve(listSegments)) {
                    resultCheck = true;
                }                
            }
            return resultCheck;
        }         
        private bool IsIntersectCurve(IList<BoundarySegment> segments) {           
            bool resultCheck = false;            
            List<Curve> curves = new List<Curve>();
            foreach(BoundarySegment segment in segments) {
                curves.Add(segment.GetCurve());
            }
            for(int i = 0; i < curves.Count; i++) {                
                Curve curve1 = curves[i];
                for(int j = i + 1; j < curves.Count; j++) {
                    Curve curve2 = curves[j];
                    SetComparisonResult result = curve1.Intersect(curve2, out IntersectionResultArray results);
                    if(result == SetComparisonResult.Equal) {
                        resultCheck = true;                        
                    }
                }
            }
            return resultCheck;
        }    
    }    
}
