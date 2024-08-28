using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace IPA.Bcfier.Revit.Models
{
    public class ViewContinuationInstructions
    {
        public Action<UIDocument>? ViewContinuation { get; set; }

        public ElementId? ViewId { get; set; }

        public Func<Task>? Callback { get; set; }
    }
}
