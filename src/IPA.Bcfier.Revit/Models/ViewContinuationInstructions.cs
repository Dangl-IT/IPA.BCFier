using Autodesk.Revit.DB;

namespace IPA.Bcfier.Revit.Models
{
    public class ViewContinuationInstructions
    {
        public Action? ViewContinuation { get; set; }

        public ElementId? ViewId { get; set; }

        public Func<Task>? Callback { get; set; }
    }
}
