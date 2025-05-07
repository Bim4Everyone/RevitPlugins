using System.Collections.Generic;

namespace RevitRsnManager.Interfaces {
    public interface IRsnConfigService 
    {
        List<string> LoadServersFromIni();
        void SaveServersToIni(List<string> servers);
        string GetProjectPathFromRevitIni();
    }
}

