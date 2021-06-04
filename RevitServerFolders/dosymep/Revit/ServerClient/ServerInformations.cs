using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dosymep.Revit.ServerClient {
    public enum ServerRoles {
        /// <summary>
        /// Host.
        /// </summary>
        Host,

        /// <summary>
        /// Accelerator.
        /// </summary>
        Accelerator,

        /// <summary>
        /// Администратор.
        /// </summary>
        Admin
    }

    /// <summary>
    /// Информация о Revit сервере.
    /// </summary>
    public class ServerInformations {
        /// <summary>
        /// Максимальная длина имени модели.
        /// </summary>
        public int MaximumModelNameLength { get; set; }

        /// <summary>
        /// Максимальная длина имени папки.
        /// </summary>
        public int MaximumFolderPathLength { get; set; }

        /// <summary>
        /// Список имен серверов сети Revit.
        /// </summary>
        public List<string> Servers { get; set; }

        /// <summary>
        /// Список ролей сервера.
        /// </summary>
        public List<ServerRoles> ServerRoles { get; set; }
    }
}
