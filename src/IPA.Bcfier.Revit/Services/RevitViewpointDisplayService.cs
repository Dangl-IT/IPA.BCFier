using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Revit.Models;
using IPA.Bcfier.Revit.OpenProject;

namespace IPA.Bcfier.Revit.Services
{
    public class RevitViewpointDisplayService
    {
        private readonly UIDocument _uiDocument;

        public RevitViewpointDisplayService(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
        }

        public ViewContinuationInstructions? DisplayViewpoint(BcfViewpoint bcfViewpoint)
        {
            try
            {
                Document doc = _uiDocument.Document;
                var uniqueView = false;
                double zoom;
                var adjustZoomForOrthoView = false;

                ElementId? viewId = null;
                // IS ORTHOGONAL
                if (bcfViewpoint.OrthogonalCamera != null)
                {
                    if (bcfViewpoint.OrthogonalCamera.ViewPoint == null || bcfViewpoint.OrthogonalCamera.UpVector == null || bcfViewpoint.OrthogonalCamera.Direction == null)
                    {
                        return null;
                    }

                    //type = "OrthogonalCamera";
                    zoom = bcfViewpoint.OrthogonalCamera.ViewToWorldScale.ToFeet();
                    var cameraDirection = RevitUtilities.GetRevitXYZ(bcfViewpoint.OrthogonalCamera.Direction);
                    var cameraUpVector = RevitUtilities.GetRevitXYZ(bcfViewpoint.OrthogonalCamera.UpVector);
                    var cameraViewPoint = RevitUtilities.GetRevitXYZ(bcfViewpoint.OrthogonalCamera.ViewPoint);
                    var orient3D = RevitUtilities.ConvertBasePoint(doc, cameraViewPoint, cameraDirection, cameraUpVector, true);

                    View3D? orthoView = null;
                    //try to use an existing 3D view
                    IEnumerable<View3D> viewcollector3D = Get3DViews(doc);
                    if (viewcollector3D.Any(o => o.Name == "BCF Coordination View"))
                    {
                        orthoView = viewcollector3D.First(o => o.Name == "BCF Coordination View");
                    }

                    using (var trans = new Transaction(_uiDocument.Document))
                    {
                        if (trans.Start("Open orthogonal view") == TransactionStatus.Started)
                        {
                            //create a new 3d ortho view

                            if (orthoView == null || uniqueView)
                            {
                                orthoView = View3D.CreateIsometric(doc, GetFamilyViews(doc).First().Id);
                                orthoView.Name = "BCF Coordination View";
                            }
                            else
                            {
                                //reusing an existing view, I net to reset the visibility
                                //placed this here because if set afterwards it doesn't work
                                orthoView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                            }

                            orthoView.SetOrientation(orient3D);

                            orthoView.CropBoxActive = false;
                            orthoView.CropBoxVisible = false;

                            if (ShouldEnableSectionBox(bcfViewpoint))
                            {
                                orthoView.IsSectionBoxActive = true;
                            }
                            else
                            {
                                orthoView.IsSectionBoxActive = false;
                            }

                            trans.Commit();
                        }
                    }

                    viewId = orthoView!.Id;
                    _uiDocument.RequestViewChange(orthoView);
                    adjustZoomForOrthoView = true;
                }
                //perspective
                else if (bcfViewpoint.PerspectiveCamera != null)
                {
                    if (bcfViewpoint.PerspectiveCamera.ViewPoint == null || bcfViewpoint.PerspectiveCamera.UpVector == null || bcfViewpoint.PerspectiveCamera.Direction == null)
                    {
                        return null;
                    }

                    //not used since the fov cannot be changed in Revit
                    zoom = bcfViewpoint.PerspectiveCamera.FieldOfView;
                    //FOV - not used

                    var cameraDirection = RevitUtilities.GetRevitXYZ(bcfViewpoint.PerspectiveCamera.Direction);
                    var cameraUpVector = RevitUtilities.GetRevitXYZ(bcfViewpoint.PerspectiveCamera.UpVector);
                    var cameraViewPoint = RevitUtilities.GetRevitXYZ(bcfViewpoint.PerspectiveCamera.ViewPoint);
                    var orient3D = RevitUtilities.ConvertBasePoint(doc, cameraViewPoint, cameraDirection, cameraUpVector, true);

                    View3D? perspView = null;
                    //try to use an existing 3D view
                    IEnumerable<View3D> viewcollector3D = Get3DViews(doc);
                    if (viewcollector3D.Any(o => o.Name == "BCF Coordination View Perspective"))
                        perspView = viewcollector3D.First(o => o.Name == "BCF Coordination View Perspective");

                    using (var trans = new Transaction(_uiDocument.Document))
                    {
                        if (trans.Start("Open perspective view") == TransactionStatus.Started)
                        {
                            if (null == perspView || uniqueView)
                            {
                                perspView = View3D.CreatePerspective(doc, GetFamilyViews(doc).First().Id);
                                perspView.Name = "BCF Coordination View Perspective";
                            }
                            else
                            {
                                //reusing an existing view, I net to reset the visibility
                                //placed this here because if set afterwards it doesn't work
                                perspView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                            }

                            perspView.SetOrientation(orient3D);

                            // turn off the far clip plane
                            if (perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR).HasValue)
                            {
                                Parameter m_farClip = perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR);
                                m_farClip.Set(0);
                            }

                            perspView.CropBoxActive = false;
                            perspView.CropBoxVisible = false;

                            if (ShouldEnableSectionBox(bcfViewpoint))
                            {
                                perspView.IsSectionBoxActive = true;
                            }
                            else
                            {
                                perspView.IsSectionBoxActive = false;
                            }

                            trans.Commit();
                        }
                    }

                    _uiDocument.RequestViewChange(perspView);
                    viewId = perspView!.Id;
                }
                //no view included
                else
                {
                    return null;
                }

                Action<UIDocument> viewContinuation = (uiDocument) =>
                {
                    if (bcfViewpoint.ViewpointComponents == null)
                    {
                        return;
                    }

                    if (adjustZoomForOrthoView)
                    {
                        //adjust view rectangle
                        double x = zoom;
                        //set UI view position and zoom
                        XYZ m_xyzTl = _uiDocument.ActiveView.Origin.Add(_uiDocument.ActiveView.UpDirection.Multiply(x)).Subtract(_uiDocument.ActiveView.RightDirection.Multiply(x));
                        XYZ m_xyzBr = _uiDocument.ActiveView.Origin.Subtract(_uiDocument.ActiveView.UpDirection.Multiply(x)).Add(_uiDocument.ActiveView.RightDirection.Multiply(x));
                        _uiDocument.GetOpenUIViews().First().ZoomAndCenterRectangle(m_xyzTl, m_xyzBr);
                    }

                    var baseViewTemplate = new FilteredElementCollector(uiDocument.Document)
                        .OfClass(typeof(View))
                        .Cast<View>()
                        .Where(view => view.IsTemplate && view.Name.Contains("3D") && view.Name.Contains("IPA BCF"))
                        .FirstOrDefault();
                    if (baseViewTemplate != null)
                    {
                        using (var trans = new Transaction(uiDocument.Document))
                        {
                            if (trans.Start("Apply IPA BCF View Template") == TransactionStatus.Started)
                            {
                                uiDocument.Document.ActiveView.ApplyViewTemplateParameters(baseViewTemplate);
                            }

                            trans.Commit();
                        }
                    }

                    var elementsToSelect = new List<ElementId>();
                    var elementsToHide = new List<ElementId>();
                    var elementsToShow = new List<ElementId>();

                    var visibleElems = new FilteredElementCollector(uiDocument.Document, uiDocument.Document.ActiveView.Id)
                        .WhereElementIsNotElementType()
                        .WhereElementIsViewIndependent()
                        .ToElementIds()
                        .Where(e => uiDocument.Document.GetElement(e).CanBeHidden(uiDocument.Document.ActiveView)); //might affect performance, but it's necessary

                    bool canSetVisibility = (bcfViewpoint.ViewpointComponents.Visibility != null &&
                      bcfViewpoint.ViewpointComponents.Visibility.DefaultVisibility &&
                      bcfViewpoint.ViewpointComponents.Visibility.Exceptions.Any());
                    bool canSetSelection = (bcfViewpoint.ViewpointComponents.SelectedComponents != null && bcfViewpoint.ViewpointComponents.SelectedComponents.Any());

                    //loop elements
                    foreach (var e in visibleElems)
                    {
                        var guid = ExportUtils.GetExportId(doc, e).ToIfcGuid();

                        if (canSetVisibility && bcfViewpoint.ViewpointComponents.Visibility != null)
                        {
                            if (bcfViewpoint.ViewpointComponents.Visibility.DefaultVisibility)
                            {
                                if (bcfViewpoint.ViewpointComponents.Visibility.Exceptions.Any(x => x.IfcGuid == guid))
                                {
                                    elementsToHide.Add(e);
                                }
                            }
                            else
                            {
                                if (bcfViewpoint.ViewpointComponents.Visibility.Exceptions.Any(x => x.IfcGuid == guid))
                                {
                                    elementsToShow.Add(e);
                                }
                            }
                        }

                        if (canSetSelection)
                        {
                            if (bcfViewpoint.ViewpointComponents.SelectedComponents.Any(x => x.IfcGuid == guid))
                            {
                                elementsToSelect.Add(e);
                            }
                        }
                    }

                    using (var trans = new Transaction(uiDocument.Document))
                    {
                        if (trans.Start("Apply BCF visibility and selection and section box") == TransactionStatus.Started)
                        {
                            if (elementsToHide.Any())
                                uiDocument.Document.ActiveView.HideElementsTemporary(elementsToHide);
                            //there are no items to hide, therefore hide everything and just show the visible ones
                            else if (elementsToShow.Any())
                                uiDocument.Document.ActiveView.IsolateElementsTemporary(elementsToShow);

                            if (elementsToSelect.Any())
                                uiDocument.Selection.SetElementIds(elementsToSelect);

                            if (uiDocument.ActiveView is View3D view3d)
                            {
                                ApplyClippingPlanes(uiDocument, view3d, bcfViewpoint);
                            }
                        }

                        trans.Commit();
                    }

                    uiDocument.RefreshActiveView();
                };

                return new ViewContinuationInstructions
                {
                    ViewContinuation = viewContinuation,
                    ViewId = viewId
                };
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error!", "exception: " + ex);
                return null;
            }
        }

        private IEnumerable<ViewFamilyType> GetFamilyViews(Document doc)
        {
            return from elem in new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                   let type = elem as ViewFamilyType
                   where type.ViewFamily == ViewFamily.ThreeDimensional
                   select type;
        }

        private IEnumerable<View3D> Get3DViews(Document doc)
        {
            return from elem in new FilteredElementCollector(doc).OfClass(typeof(View3D))
                   let view = elem as View3D
                   select view;
        }

        public string GetName()
        {
            return "3D View";
        }

        // Take from:
        // https://github.com/opf/openproject-revit-add-in/blob/93e117ad10176f4fffa741116733a3ee113e9335/src/OpenProject.Revit/Entry/OpenViewpointEventHandler.cs#L212
        private const decimal _viewpointAngleThresholdRad = 0.087266462599716m;

        private bool ShouldEnableSectionBox(BcfViewpoint bcfViewpoint)
        {
            if (bcfViewpoint.ClippingPlanes?.Count != 6)
            {
                return false;
            }

            AxisAlignedBoundingBox boundingBox = GetViewpointClippingBox(bcfViewpoint);
            if (boundingBox.Equals(AxisAlignedBoundingBox.Infinite))
            {
                return false;
            }

            return true;
        }

        private void ApplyClippingPlanes(UIDocument uiDocument, View3D view, BcfViewpoint bcfViewpoint)
        {
            if (bcfViewpoint.ClippingPlanes?.Count != 6)
            {
                // Don't apply section box if it's not a full box
                view.IsSectionBoxActive = false;
                return;
            }

            AxisAlignedBoundingBox boundingBox = GetViewpointClippingBox(bcfViewpoint);

            if (!boundingBox.Equals(AxisAlignedBoundingBox.Infinite))
            {
                var revitSectionBox = ToRevitSectionBox(boundingBox);
                var transform = _uiDocument.Document.ActiveProjectLocation.GetTransform();
                revitSectionBox.Transform = transform;

                view.SetSectionBox(revitSectionBox);
                view.IsSectionBoxActive = true;

                // We want to zoom to the section box, but then also zoom a bit out of it
                _uiDocument.GetOpenUIViews().First().ZoomAndCenterRectangle(transform.OfPoint(revitSectionBox.Min), transform.OfPoint(revitSectionBox.Max));
                _uiDocument.GetOpenUIViews().First().Zoom(0.7);
            }
            else
            {
                view.IsSectionBoxActive = false;
            }
        }

        private AxisAlignedBoundingBox GetViewpointClippingBox(BcfViewpoint bcfViewpoint)
        {
            return bcfViewpoint.ClippingPlanes
                .Select(p => p.ToAxisAlignedBoundingBox(_viewpointAngleThresholdRad))
                .Aggregate(AxisAlignedBoundingBox.Infinite, (current, nextBox) => current.MergeReduce(nextBox));
        }

        private static BoundingBoxXYZ ToRevitSectionBox(AxisAlignedBoundingBox box)
        {
            var min = new XYZ(
              box.Min.X == decimal.MinValue ? double.MinValue : ((double)box.Min.X).ToInternalRevitUnit(),
              box.Min.Y == decimal.MinValue ? double.MinValue : ((double)box.Min.Y).ToInternalRevitUnit(),
              box.Min.Z == decimal.MinValue ? double.MinValue : ((double)box.Min.Z).ToInternalRevitUnit());
            var max = new XYZ(
              box.Max.X == decimal.MaxValue ? double.MaxValue : ((double)box.Max.X).ToInternalRevitUnit(),
              box.Max.Y == decimal.MaxValue ? double.MaxValue : ((double)box.Max.Y).ToInternalRevitUnit(),
              box.Max.Z == decimal.MaxValue ? double.MaxValue : ((double)box.Max.Z).ToInternalRevitUnit());

            return new BoundingBoxXYZ { Min = min, Max = max };
        }
    }
}
