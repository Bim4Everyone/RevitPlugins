using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitToMongoDB.Model;

namespace RevitToMongoDB.ViewModels.Interfaces {
    internal interface IElementRepository {
        void Insert(ElementDto elementDto); 
    }
}
