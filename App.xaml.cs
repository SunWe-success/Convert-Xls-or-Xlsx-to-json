using ModelsToJson.Views;
using Prism.Ioc;
using Prism.Navigation.Regions;
using System.Text;
using System.Windows;

namespace ModelsToJson
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CsvToJsonAsConfigView>();
            containerRegistry.RegisterForNavigation<ModelToJsonView>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            IRegionManager regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("AnyModelToJsonView", typeof(ModelToJsonView));
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
