using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models
{
    internal class PublicArea : RoomGroup {
        public PublicArea(IEnumerable<RoomElement> rooms, DeclarationSettings settings)
            : base(rooms, settings) {
        }
    }
}
