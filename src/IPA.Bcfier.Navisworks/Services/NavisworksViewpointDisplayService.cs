using Autodesk.Navisworks.Api;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Navisworks.Utilities;
using System.Windows;

namespace IPA.Bcfier.Navisworks.Services
{
    public class NavisworksViewpointDisplayService
    {
        private readonly Document _doc;

        public NavisworksViewpointDisplayService(Document doc)
        {
            _doc = doc;
        }

        public void DisplayViewpoint(BcfViewpoint v)
        {
            try
            {

                NavisUtils.GetGunits(_doc);
                Viewpoint viewpoint = new Viewpoint();

                //orthogonal
                if (v.OrthogonalCamera != null)
                {
                    if (v.OrthogonalCamera.ViewPoint == null || v.OrthogonalCamera.UpVector == null ||
                        v.OrthogonalCamera.Direction == null)
                        return;

                    var zoom = v.OrthogonalCamera.ViewToWorldScale.ToInternal();
                    var cameraDirection = NavisUtils.GetNavisVector(v.OrthogonalCamera.Direction);
                    var cameraUpVector = NavisUtils.GetNavisVector(v.OrthogonalCamera.UpVector);
                    var cameraViewPoint = NavisUtils.GetNavisXYZ(v.OrthogonalCamera.ViewPoint);

                    viewpoint.Position = cameraViewPoint;
                    viewpoint.AlignUp(cameraUpVector);
                    viewpoint.AlignDirection(cameraDirection);
                    viewpoint.Projection = ViewpointProjection.Orthographic;
                    viewpoint.FocalDistance = 1;

                    //TODO
                    //for better zooming from revit should use > zoom * 1.25
                    //for better zooming from tekla should use > zoom / 1.25
                    //still not sure why
                    Point3D xyzTL = cameraViewPoint.Add(cameraUpVector.Multiply(zoom));
                    var dist = xyzTL.DistanceTo(cameraViewPoint);
                    viewpoint.SetExtentsAtFocalDistance(1, dist);
                }
                //perspective
                else if (v.PerspectiveCamera != null)
                {
                    if (v.PerspectiveCamera.ViewPoint == null || v.PerspectiveCamera.UpVector == null ||
                        v.PerspectiveCamera.Direction == null)
                        return;

                    var zoom = v.PerspectiveCamera.FieldOfView;
                    var cameraDirection = NavisUtils.GetNavisVector(v.PerspectiveCamera.Direction);
                    var cameraUpVector = NavisUtils.GetNavisVector(v.PerspectiveCamera.UpVector);
                    var cameraViewPoint = NavisUtils.GetNavisXYZ(v.PerspectiveCamera.ViewPoint);

                    viewpoint.Position = cameraViewPoint;
                    viewpoint.AlignUp(cameraUpVector);
                    viewpoint.AlignDirection(cameraDirection);
                    viewpoint.Projection = ViewpointProjection.Perspective;
                    viewpoint.FocalDistance = zoom;
                }

                _doc.CurrentViewpoint.CopyFrom(viewpoint);


                //show/hide elements
                //todo: needs improvement
                //todo: add settings

                if (v.ViewpointComponents?.SelectedComponents != null && v.ViewpointComponents.SelectedComponents.Any())
                {
                    List<ModelItem> attachedElems = new List<ModelItem>();
                    List<ModelItem> elems = _doc.Models.First.RootItem.DescendantsAndSelf.ToList<ModelItem>();

                    var selectedElements = v.ViewpointComponents.SelectedComponents;

                    foreach (var item in elems.Where(o => o.InstanceGuid != Guid.Empty))
                    {
                        string ifcguid = item.InstanceGuid.ToIfcGuid();
                        if (selectedElements.Any(o => o.IfcGuid == ifcguid))
                        {
                            attachedElems.Add(item);
                        }
                    }

                    if (attachedElems.Any()) //avoid to hide everything if no elements matches
                    {
                        _doc.CurrentSelection.Clear();
                        _doc.CurrentSelection.AddRange(attachedElems);
                    }

                    if (v.ViewpointComponents?.Visibility != null
                        && v.ViewpointComponents.Visibility.DefaultVisibility == true
                        && v.ViewpointComponents.Visibility.Exceptions.Any())
                    {
                        var hiddenComponents = v.ViewpointComponents.Visibility.Exceptions;
                        var hiddenElements = elems.Where(e => hiddenComponents.Any(h => h.IfcGuid == e.InstanceGuid.ToIfcGuid()));

                        if (hiddenElements.Any())
                        {
                        _doc.Models.ResetAllHidden();
                        _doc.Models.SetHidden(hiddenElements, true);
                        }
                    }
                }
            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error opening view");
            }
        }

    }
}

