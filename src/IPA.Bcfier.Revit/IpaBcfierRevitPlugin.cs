using Autodesk.Revit.UI;
using System.Reflection;
#if !REVIT_2025 && !REVIT_2026
using System.Windows.Media.Imaging;
#endif

namespace IPA.Bcfier.Revit
{
    public class IpaBcfierRevitPlugin : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var buttonData = new PushButtonData("openPluginButton", "IPA.BCFier", Assembly.GetExecutingAssembly().Location, "IPA.Bcfier.Revit.ListenToIpaBcfierAppPipeCommand");
            var pushButton = application.CreateRibbonPanel("IPA.BCFier").AddItem(buttonData) as PushButton;
            pushButton!.ToolTip = "Launch IPA.Bcfier Revit Plugin";
#if !REVIT_2025 && !REVIT_2026
            pushButton.Image = GetBitmapImage("button16.png");
            pushButton.LargeImage = GetBitmapImage("button32.png");
#endif
            ListenToIpaBcfierAppPipeCommand.ControlledApplication = application.ControlledApplication;
            return Result.Succeeded;
        }

#if !REVIT_2025 && !REVIT_2026
        private BitmapImage GetBitmapImage(string resourceName)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream($"IPA.Bcfier.Revit.Resources.{resourceName}");
            bitmapImage.EndInit();
            return bitmapImage;
        }
#endif

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
