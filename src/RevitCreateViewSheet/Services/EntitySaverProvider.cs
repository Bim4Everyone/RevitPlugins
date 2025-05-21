using Ninject;
using Ninject.Syntax;

namespace RevitCreateViewSheet.Services {
    internal class EntitySaverProvider {
        private readonly IResolutionRoot _resolutionRoot;

        public EntitySaverProvider(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot ?? throw new System.ArgumentNullException(nameof(resolutionRoot));
        }


        public NewEntitySaver GetNewEntitySaver() {
            return _resolutionRoot.Get<NewEntitySaver>();
        }

        public ExistsEntitySaver GetExistsEntitySaver() {
            return _resolutionRoot.Get<ExistsEntitySaver>();
        }
    }
}
