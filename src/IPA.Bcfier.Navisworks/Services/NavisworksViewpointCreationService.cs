using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Navisworks.Utilities;
using System.Drawing.Imaging;

namespace IPA.Bcfier.Navisworks.Services
{
    public class NavisworksViewpointCreationService
    {
        private readonly Document _doc;

        public NavisworksViewpointCreationService(Document doc)
        {
            _doc = doc;
        }

        ///<summary>
        ///  Generate a VisualizationInfo of the current view
        ///</summary>
        ///<returns></returns>
        public BcfViewpoint? GenerateViewpoint()
        {
            try
            {
                var viewpoint = _doc.CurrentViewpoint.Value;
                NavisUtils.GetGunits(_doc);
                var v = GetViewpointFromNavisworksViewpoint(viewpoint);
                return v;
            }
            catch (System.Exception ex1)
            {
                // TODO
                //TaskDialog.Show("Error generating viewpoint", "exception: " + ex1);
            }
            return null;
        }

        private BcfViewpoint GetViewpointFromNavisworksViewpoint(Viewpoint viewpoint)
        {
            var v = new BcfViewpoint();
            Vector3D vi = GetViewDirection(viewpoint);
            Vector3D up = GetViewUp(viewpoint);
            Point3D c = new Point3D(viewpoint.Position.X, viewpoint.Position.Y, viewpoint.Position.Z);
            double zoomValue = 1;

            //prepare view
            viewpoint = viewpoint.CreateCopy();
            if (!viewpoint.HasFocalDistance)
            {
                viewpoint.FocalDistance = 1;
            }

            // it is a orthogonal view
            if (viewpoint.Projection == ViewpointProjection.Orthographic)
            {
                //TODO: needs checking!!!
                double dist = viewpoint.VerticalExtentAtFocalDistance / 2;
                zoomValue = 3.125 * dist / (up.Length * 1.25);

                v.OrthogonalCamera = new BcfViewpointOrthogonalCamera
                {
                    ViewPoint =
                          {
                            X = c.X.FromInternal(),
                            Y = c.Y.FromInternal(),
                            Z = c.Z.FromInternal()
                          },
                    UpVector =
                          {
                            X = up.X.FromInternal(),
                            Y = up.Y.FromInternal(),
                            Z = up.Z.FromInternal()
                          },
                    Direction =
                          {
                            X = vi.X.FromInternal(),
                            Y = vi.Y.FromInternal(),
                            Z = vi.Z.FromInternal()
                          },
                    ViewToWorldScale = zoomValue.FromInternal()
                };
            }
            else
            {
                zoomValue = viewpoint.FocalDistance;

                v.PerspectiveCamera = new BcfViewpointPerspectiveCamera
                {
                    ViewPoint =
                          {
                            X = c.X.FromInternal(),
                            Y = c.Y.FromInternal(),
                            Z = c.Z.FromInternal()
                          },
                    UpVector =
                          {
                            X = up.X.FromInternal(),
                            Y = up.Y.FromInternal(),
                            Z = up.Z.FromInternal()
                          },
                    Direction =
                          {
                            X = vi.X.FromInternal(),
                            Y = vi.Y.FromInternal(),
                            Z = vi.Z.FromInternal()
                          },
                    FieldOfView = zoomValue
                };
            }

            var selectedIfcGuids = _doc.CurrentSelection.SelectedItems.Select(selectedItem => selectedItem.InstanceGuid.ToIfcGuid()).ToList();
            if (selectedIfcGuids.Any())
            {
                v.ViewpointComponents = new BcfViewpointComponents
                {
                    SelectedComponents = selectedIfcGuids.Select(ifcGuid => new BcfViewpointComponent
                    {
                        IfcGuid = ifcGuid,
                        OriginatingSystem = "IPA.BCFier.Navisworks",
                    }).ToList()
                };
            }

#if NAVISWORKS_2023 || NAVISWORKS_2022 || NAVISWORKS_2021
                var navisworksSnapshot = _doc.GenerateImage(ImageGenerationStyle.Scene, 1920, 1080);

#else
            var navisworksSnapshot = _doc.GenerateImage(ImageGenerationStyle.Scene, 1920, 1080, true);
#endif

            using var imageStream = new MemoryStream();
            navisworksSnapshot.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
            v.SnapshotBase64 = Convert.ToBase64String(imageStream.ToArray());

            return v;
        }

        // Mostly taken from here:
        // https://forums.autodesk.com/t5/navisworks-api/here-s-how-to-export-clash-result-images-using-navisworks-api/td-p/9089222
        public List<BcfTopic> CreateClashIssues()
        {
            var bcfTopics = new List<BcfTopic>();
            NavisUtils.GetGunits(_doc);

            var doc = _doc;
            var tests = doc.GetClash().TestsData.Tests;

            // Assuming you've already run all Clash Tests and have the results
            foreach (ClashTest test in tests)
            {
                if (test.Children.Count <= 0) continue;

                // test.IsGroup // TODO?
                if (test.IsGroup)
                {
                    var wow = test.Children.OfType<ClashResultGroup>().ToList();
                    // TODO
                }

                foreach (var testItem in test.Children)
                {
                    if (testItem is ClashResult result)
                    {
                        var viewpoint = doc.CurrentViewpoint.Value;
                        // Create a collection of the 2 clashing items from the ClashResult
                        var items = new ModelItemCollection();
                        items.Add(result.Item1);
                        items.Add(result.Item2);
                        // Select the 2 clashing items
                        doc.CurrentSelection.Clear();
                        doc.CurrentSelection.CopyFrom(items);
                        // Focus on the clashing items
                        doc.ActiveView.FocusOnCurrentSelection();
                        // Make all items visible
                        doc.Models.ResetAllHidden();

                        // Hide everything except for the 2 clashing items
                        var to_hide = new ModelItemCollection();
                        var to_show = new ModelItemCollection();
                        foreach (var item in doc.CurrentSelection.SelectedItems)
                        {
                            // Collect all items upstream to the root
                            if (item.AncestorsAndSelf != null)
                                to_show.AddRange(item.AncestorsAndSelf);
                            // Collect all subtrees of the item
                            if (item.Descendants != null)
                                to_show.AddRange(item.Descendants);
                        }

                        foreach (var item in to_show)
                        {
                            // If an item has no parent (root item) save the subtrees 
                            if (!NativeHandle.ReferenceEquals(item.Parent, null))
                                to_hide.AddRange(item.Parent.Children);
                        }
                        // Remove the to be shown items from list of the to be hidden items
                        foreach (var item in to_show)
                            to_hide.Remove(item);
                        // Hide all explicitly to be hidden items
                        doc.Models.SetHidden(to_hide, true);
                        // Hide all other items except those already hidden and those to be shown
                        doc.Models.SetHidden(doc.Models
                                                .SelectMany<Model, ModelItem>(
                                                    (Func<Model, ModelItemEnumerableCollection>)(c => c.RootItem.Children)
                                                  )
                                                .Except<ModelItem>(to_hide)
                                                .Except<ModelItem>(to_show)
                                              , true);
                        // Remove selction color from the clashing items
                        doc.CurrentSelection.Clear();

                        // Adjust the camera and lighting
                        var copy = viewpoint.CreateCopy();
                        copy.Lighting = ViewpointLighting.None;
                        doc.Models.ResetAllPermanentMaterials();
                        doc.CurrentViewpoint.CopyFrom(copy);

                        // Paint the clashing items in Red and Green respectively
                        doc.Models.OverridePermanentColor(new ModelItem[1] { items.ElementAtOrDefault<ModelItem>(0) }, Color.Red);
                        doc.Models.OverridePermanentColor(new ModelItem[1] { items.ElementAtOrDefault<ModelItem>(1) }, Color.Green);
                        // Adjust the camera angle
                        doc.ActiveView.LookFromFrontRightTop();
                        // Prevent redraw for every test and item
                        doc.ActiveView.RequestDelayedRedraw(ViewRedrawRequests.All);


                        var bcfViewpoint = GetViewpointFromNavisworksViewpoint(viewpoint);
                        bcfTopics.Add(new BcfTopic
                        {
                            Viewpoints = new List<BcfViewpoint>
                            {
                                bcfViewpoint
                            },
                            Title = $"{test.DisplayName} - {result.DisplayName}"
                        });
                    }
                    else if (testItem is ClashResultGroup resultGroup)
                    {
                        var viewpoint = doc.CurrentViewpoint.Value;
                        // Create a collection of the 2 clashing items from the ClashResult
                        var items = new ModelItemCollection();
                        resultGroup.CompositeItemSelection1.CopyTo(items);
                        resultGroup.CompositeItemSelection2.CopyTo(items);
                        // Select the clashing items
                        doc.CurrentSelection.Clear();
                        doc.CurrentSelection.CopyFrom(items);
                        // Focus on the clashing items
                        doc.ActiveView.FocusOnCurrentSelection();
                        // Make all items visible
                        doc.Models.ResetAllHidden();

                        // Hide everything except for the 2 clashing items
                        var to_hide = new ModelItemCollection();
                        var to_show = new ModelItemCollection();
                        foreach (var item in doc.CurrentSelection.SelectedItems)
                        {
                            // Collect all items upstream to the root
                            if (item.AncestorsAndSelf != null)
                                to_show.AddRange(item.AncestorsAndSelf);
                            // Collect all subtrees of the item
                            if (item.Descendants != null)
                                to_show.AddRange(item.Descendants);
                        }

                        foreach (var item in to_show)
                        {
                            // If an item has no parent (root item) save the subtrees 
                            if (!NativeHandle.ReferenceEquals(item.Parent, null))
                                to_hide.AddRange(item.Parent.Children);
                        }
                        // Remove the to be shown items from list of the to be hidden items
                        foreach (var item in to_show)
                            to_hide.Remove(item);
                        // Hide all explicitly to be hidden items
                        doc.Models.SetHidden(to_hide, true);
                        // Hide all other items except those already hidden and those to be shown
                        doc.Models.SetHidden(doc.Models
                                                .SelectMany<Model, ModelItem>(
                                                    (Func<Model, ModelItemEnumerableCollection>)(c => c.RootItem.Children)
                                                  )
                                                .Except<ModelItem>(to_hide)
                                                .Except<ModelItem>(to_show)
                                              , true);
                        // Remove selction color from the clashing items
                        doc.CurrentSelection.Clear();

                        // Adjust the camera and lighting
                        var copy = viewpoint.CreateCopy();
                        copy.Lighting = ViewpointLighting.None;
                        doc.Models.ResetAllPermanentMaterials();
                        doc.CurrentViewpoint.CopyFrom(copy);

                        // Paint the clashing items in Red and Green respectively
                        doc.Models.OverridePermanentColor(new ModelItem[1] { items.ElementAtOrDefault<ModelItem>(0) }, Color.Red);
                        doc.Models.OverridePermanentColor(new ModelItem[1] { items.ElementAtOrDefault<ModelItem>(1) }, Color.Green);
                        // Adjust the camera angle
                        doc.ActiveView.LookFromFrontRightTop();
                        // Prevent redraw for every test and item
                        doc.ActiveView.RequestDelayedRedraw(ViewRedrawRequests.All);


                        var bcfViewpoint = GetViewpointFromNavisworksViewpoint(viewpoint);
                        bcfTopics.Add(new BcfTopic
                        {
                            CreationDate = DateTime.Now,
                            Viewpoints = new List<BcfViewpoint>
                            {
                                bcfViewpoint
                            },
                            Title = $"{test.DisplayName} - {resultGroup.DisplayName} (Group)"
                        });
                    }
                }
            }

            return bcfTopics;
        }

        private static Vector3D GetViewDirection(Viewpoint v)
        {
            Rotation3D oRot = v.Rotation;
            // calculate view direction
            Rotation3D oNegtiveZ = new Rotation3D(0, 0, -1, 0);
            Rotation3D otempRot = MultiplyRotation3D(oNegtiveZ, oRot.Invert());
            Rotation3D oViewDirRot = MultiplyRotation3D(oRot, otempRot);
            // get view direction
            Vector3D oViewDir = new Vector3D(oViewDirRot.A, oViewDirRot.B, oViewDirRot.C);

            return oViewDir.Normalize();
        }

        private static Vector3D GetViewUp(Viewpoint v)
        {
            Rotation3D oRot = v.Rotation;
            // calculate view direction
            Rotation3D oNegtiveZ = new Rotation3D(0, 1, 0, 0);
            Rotation3D otempRot = MultiplyRotation3D(oNegtiveZ, oRot.Invert());
            Rotation3D oViewDirRot = MultiplyRotation3D(oRot, otempRot);
            // get view direction
            Vector3D oViewDir = new Vector3D(oViewDirRot.A, oViewDirRot.B, oViewDirRot.C);

            return oViewDir.Normalize();
        }

        //multiply two Rotation3D
        private static Rotation3D MultiplyRotation3D(Rotation3D r2, Rotation3D r1)
        {
            Rotation3D rot = new Rotation3D(
              r2.D * r1.A + r2.A * r1.D +
              r2.B * r1.C - r2.C * r1.B,
              r2.D * r1.B + r2.B * r1.D +
              r2.C * r1.A - r2.A * r1.C,
              r2.D * r1.C + r2.C * r1.D +
              r2.A * r1.B - r2.B * r1.A,
              r2.D * r1.D - r2.A * r1.A -
              r2.B * r1.B - r2.C * r1.C);
            rot.Normalize();
            return rot;
        }

        private void SaveAllClashPictures(string image_dir)
        {
            var doc = _doc;
            var tests = doc.GetClash().TestsData.Tests;

            // Assuming you've already run all Clash Tests and have the results
            foreach (ClashTest test in tests)
            {
                if (test.Children.Count <= 0) continue;

                // test.IsGroup // TODO?

                foreach (ClashResult result in test.Children)
                {
                    var viewpoint = doc.CurrentViewpoint.Value;
                    // Create a collection of the 2 clashing items from the ClashResult
                    var items = new ModelItemCollection();
                    items.Add(result.Item1);
                    items.Add(result.Item2);
                    // Select the 2 clashing items
                    doc.CurrentSelection.Clear();
                    doc.CurrentSelection.CopyFrom(items);
                    // Focus on the clashing items
                    doc.ActiveView.FocusOnCurrentSelection();
                    // Make all items visible
                    doc.Models.ResetAllHidden();

                    // Hide everything except for the 2 clashing items
                    var to_hide = new ModelItemCollection();
                    var to_show = new ModelItemCollection();
                    foreach (var item in doc.CurrentSelection.SelectedItems)
                    {
                        // Collect all items upstream to the root
                        if (item.AncestorsAndSelf != null)
                            to_show.AddRange(item.AncestorsAndSelf);
                        // Collect all subtrees of the item
                        if (item.Descendants != null)
                            to_show.AddRange(item.Descendants);
                    }

                    foreach (var item in to_show)
                    {
                        // If an item has no parent (root item) save the subtrees 
                        if (!NativeHandle.ReferenceEquals(item.Parent, null))
                            to_hide.AddRange(item.Parent.Children);
                    }
                    // Remove the to be shown items from list of the to be hidden items
                    foreach (var item in to_show)
                        to_hide.Remove(item);
                    // Hide all explicitly to be hidden items
                    doc.Models.SetHidden(to_hide, true);
                    // Hide all other items except those already hidden and those to be shown
                    doc.Models.SetHidden(doc.Models
                                            .SelectMany<Model, ModelItem>(
                                                (Func<Model, ModelItemEnumerableCollection>)(c => c.RootItem.Children)
                                              )
                                            .Except<ModelItem>(to_hide)
                                            .Except<ModelItem>(to_show)
                                          , true);
                    // Remove selction color from the clashing items
                    doc.CurrentSelection.Clear();

                    // Adjust the camera and lighting
                    var copy = viewpoint.CreateCopy();
                    copy.Lighting = ViewpointLighting.None;
                    doc.Models.ResetAllPermanentMaterials();
                    doc.CurrentViewpoint.CopyFrom(copy);

                    // Paint the clashing items in Red and Green respectively
                    doc.Models.OverridePermanentColor(new ModelItem[1] { items.ElementAtOrDefault<ModelItem>(0) }, Color.Red);
                    doc.Models.OverridePermanentColor(new ModelItem[1] { items.ElementAtOrDefault<ModelItem>(1) }, Color.Green);
                    // Adjust the camera angle
                    doc.ActiveView.LookFromFrontRightTop();
                    // Prevent redraw for every test and item
                    doc.ActiveView.RequestDelayedRedraw(ViewRedrawRequests.All);

                    // Save the Clash image
                    var clashImage = doc.GenerateImage(ImageGenerationStyle.Scene, 500, 500, true);
                    //var clashImage = doc.ActiveView.GenerateThumbnail(500, 500);

                    // Save the view image as PNG file
                    var clashResultImageName = System.IO.Path.Combine(image_dir, test.DisplayName, result.DisplayName + ".png");
                    clashImage.Save(clashResultImageName, ImageFormat.Png);
                }
            }
        }
    }
}
