using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Interop;
using IPA.Bcfier.Models.Bcf;
using IPA.Bcfier.Models.Clashes;
using IPA.Bcfier.Navisworks.OpenProject;
using IPA.Bcfier.Navisworks.Utilities;

namespace IPA.Bcfier.Navisworks.Services
{
    public class NavisworksViewpointCreationService
    {
        private readonly Document _doc;

        public NavisworksViewpointCreationService(Document doc)
        {
            _doc = doc;
        }
        private class ClashTestWrapper
        {
            public string? TestDisplayName { get; set; }

            public SavedItem? SavedItem { get; set; }

            public ClashTest ClashTest { get; set; }
        }

        ///<summary>
        ///  Generate a VisualizationInfo of the current view
        ///</summary>
        ///<returns></returns>
        public BcfViewpoint? GenerateViewpoint()
        {
            var viewpoint = _doc.CurrentViewpoint.Value;
            NavisUtils.GetGunits(_doc);
            var v = GetViewpointFromNavisworksViewpoint(viewpoint, generateLargeViewpoints: true, centerForBoundingBox: null);
            return v;
        }

        private BcfViewpoint GetViewpointFromNavisworksViewpoint(Viewpoint viewpoint,
            bool generateLargeViewpoints,
            Point3D centerForBoundingBox,
            List<ModelItem> selectedItems = null)
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
            //if (viewpoint.Projection == ViewpointProjection.Orthographic)
            if (true) // TODO always creating orthographic views for now from Navisworks
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

            var elementBoundingBoxes = new List<BoundingBox3D>();
            var selectedIfcGuids = selectedItems == null
                ? _doc.CurrentSelection.SelectedItems.Select(selectedItem =>
                {
                    elementBoundingBoxes.Add(selectedItem.BoundingBox());
                    return selectedItem.InstanceGuid.ToIfcGuid();
                }).ToList()
                : selectedItems.Select(selectedItem =>
                {
                    elementBoundingBoxes.Add(selectedItem.BoundingBox());
                    return selectedItem.InstanceGuid.ToIfcGuid();
                }).ToList();
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

            if (elementBoundingBoxes.Any())
            {
                // We need to construct a common bounding box for all selected elements
                var minX = elementBoundingBoxes.Min(b => b.Min.X);
                var minY = elementBoundingBoxes.Min(b => b.Min.Y);
                var minZ = elementBoundingBoxes.Min(b => b.Min.Z);
                var maxX = elementBoundingBoxes.Max(b => b.Max.X);
                var maxY = elementBoundingBoxes.Max(b => b.Max.Y);
                var maxZ = elementBoundingBoxes.Max(b => b.Max.Z);

                var commonBoundingBox = new BoundingBox3D(new Point3D(minX, minY, minZ),
                    new Point3D(maxX, maxY, maxZ));

                if (centerForBoundingBox != null)
                {
                    // This means we want to move the bounding box to the center of the actual clash,
                    // since there were issues where the camera coordinates were correct but the element
                    // bounding boxes did not work properly
                    // We're using the center of the clash as the center of the bounding box
                    var elementBoundingBoxDeltaX = maxX - minX;
                    var elementBoundingBoxDeltaY = maxY - minY;
                    var elementBoundingBoxDeltaZ = maxZ - minZ;
                    var clashBoundingBoxMinPoint = new Point3D(centerForBoundingBox.X - 1d.ToInternal(),
                        centerForBoundingBox.Y - 1d.ToInternal(),
                        centerForBoundingBox.Z - 1d.ToInternal());
                    var clashBoundingBoxMaxPoint = new Point3D(centerForBoundingBox.X + 1d.ToInternal(),
                        centerForBoundingBox.Y + 1d.ToInternal(),
                        centerForBoundingBox.Z + 1d.ToInternal());

                    commonBoundingBox = new BoundingBox3D(clashBoundingBoxMinPoint, clashBoundingBoxMaxPoint);
                }

                var clippingPlanes = TransformBoundingBoxToClippingPlanes(commonBoundingBox);

                v.ClippingPlanes ??= new List<BcfViewpointClippingPlane>();
                v.ClippingPlanes.AddRange(clippingPlanes);
            }

            try
            {
#if NAVISWORKS_2023 || NAVISWORKS_2022 || NAVISWORKS_2021
                var navisworksSnapshot = generateLargeViewpoints
                    ? _doc.GenerateImage(ImageGenerationStyle.Scene, 1920, 1080)
                    : _doc.GenerateImage(ImageGenerationStyle.Scene, 600, 400);

#else
                var navisworksSnapshot = generateLargeViewpoints
                    ? _doc.GenerateImage(ImageGenerationStyle.Scene, 1920, 1080, true)
                    : _doc.GenerateImage(ImageGenerationStyle.Scene, 300, 200, true);
#endif

                using var imageStream = new MemoryStream();
                navisworksSnapshot.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
                v.SnapshotBase64 = Convert.ToBase64String(imageStream.ToArray());
            }
            catch
            {
                // We're ignoring errors during snapshot generation
            }

            return v;
        }

        public List<NavisworksClashSelection> GetAvailableClashesForExport()
        {
            var doc = _doc;
            var tests = doc.GetClash().TestsData.Tests;
            return tests.Select(test => new NavisworksClashSelection
            {
                Id = test.Guid,
                DisplayName = test.DisplayName,
                IsGroup = test.IsGroup
            }).ToList();
        }

        // Mostly taken from here: https://forums.autodesk.com/t5/navisworks-api/here-s-how-to-export-clash-result-images-using-navisworks-api/td-p/9089222
        public List<BcfTopic> CreateClashIssues(NavisworksClashCreationData clashCreationData,
            Action<int> reportTotalCount,
            Action<int> reportCurrentCount,
            Action checkForNavisworksClashCancellation,
            CancellationToken cancellationToken)
        {
            var shouldMoveBoundingBox = clashCreationData.ShouldMoveBoundingBoxToCenterOfClash;
            var bcfTopics = new List<BcfTopic>();
            NavisUtils.GetGunits(_doc);

            var doc = _doc;
            var tests = doc.GetClash().TestsData.Tests;
            // Assuming you've already run all Clash Tests and have the results

            var testItems = tests
                .OfType<ClashTest>()
                .Where(t => t.Children.Count > 0
                    && t.Guid == clashCreationData.ClashId)
                .SelectMany(t => t.Children.Select(tt => new ClashTestWrapper
                {
                    TestDisplayName = t.DisplayName,
                    SavedItem = tt,
                    ClashTest = t
                }))
                .Where(t =>
                {
                    if (t.SavedItem is ClashResult result)
                    {
                        return (clashCreationData.ExcludedClashIds == null
                            || !clashCreationData.ExcludedClashIds.Contains(result.Guid))
                            && (clashCreationData.Status == null
                             || clashCreationData.Status == result.Status.ToString());
                    }

                    if (t.SavedItem is ClashResultGroup resultGroup)
                    {
                        return (clashCreationData.ExcludedClashIds == null
                            || !clashCreationData.ExcludedClashIds.Contains(resultGroup.Guid))
                            && (clashCreationData.Status == null
                             || clashCreationData.Status == resultGroup.Status.ToString());
                    }

                    return false;
                })
                .ToList();

            // If there are less than 50 clashes to generate, we can generate large viewpoints.
            // Otherwise, there are too many clashes and the generation would take too long
            var generateLargeViewpoints = testItems.Count <= 50;
            reportTotalCount(testItems.Count);
            var instance = LcClCurrentIssue.GetInstance((LcOpState)Autodesk.Navisworks.Api.Application.MainDocument.State);

            var currentCount = 0;
            foreach (var testItem in testItems)
            {
                if (ClashCurrentIssue.CurrentTest != testItem.ClashTest)
                {
                    ClashCurrentIssue.ClearCurrentIssue();
                    ClashCurrentIssue.ClearCurrentTest();
                    ClashCurrentIssue.CurrentTest = testItem.ClashTest;
                }

                if (testItem.SavedItem is ClashResult result)
                {
                    if (clashCreationData.ExcludedClashIds != null && clashCreationData.ExcludedClashIds.Contains(result.Guid))
                    {
                        continue;
                    }

                    instance.SetCurrentIssueFromSavedItem(result, 0, false);
                    instance.OnGotFocus();

                    ClashCurrentIssue.CurrentIssue = result;
                    var viewpoint = doc.CurrentViewpoint.Value;
                    // Create a collection of the 2 clashing items from the ClashResult
                    var items = new ModelItemCollection();
                    items.Add(result.CompositeItem1);
                    items.Add(result.CompositeItem2);
                    var selectedItems = items.ToList();

                    // Prevent redraw for every test and item
                    doc.ActiveView.RequestDelayedRedraw(ViewRedrawRequests.All);


                    var bcfViewpoint = GetViewpointFromNavisworksViewpoint(viewpoint, generateLargeViewpoints, shouldMoveBoundingBox ? result.Center : null, selectedItems);
                    bcfViewpoint.Id = result.Guid;
                    bcfTopics.Add(new BcfTopic
                    {
                        ServerAssignedId = result.Guid.ToString(),
                        Viewpoints = new List<BcfViewpoint>
                            {
                                bcfViewpoint
                            },
                        TopicStatus = result.Status.ToString(),
                        Title = $"{testItem.TestDisplayName} - {result.DisplayName}"
                    });
                }
                else if (testItem.SavedItem is ClashResultGroup resultGroup)
                {
                    if (clashCreationData.ExcludedClashIds != null && clashCreationData.ExcludedClashIds.Contains(resultGroup.Guid))
                    {
                        continue;
                    }

                    instance.SetCurrentIssueFromSavedItem(resultGroup, 0, false);
                    instance.OnGotFocus();

                    ClashCurrentIssue.CurrentIssue = resultGroup;
                    var viewpoint = doc.CurrentViewpoint.Value;
                    // Create a collection of the 2 clashing items from the ClashResult
                    var items = new ModelItemCollection();
                    resultGroup.CompositeItemSelection1.CopyTo(items);
                    resultGroup.CompositeItemSelection2.CopyTo(items);
                    var selectedItems = items.ToList();

                    // Adjust the camera and lighting
                    var copy = viewpoint.CreateCopy();
                    copy.Lighting = ViewpointLighting.None;
                    doc.Models.ResetAllPermanentMaterials();
                    doc.CurrentViewpoint.CopyFrom(copy);

                    var bcfViewpoint = GetViewpointFromNavisworksViewpoint(viewpoint, generateLargeViewpoints,
                        null, selectedItems);
                    bcfViewpoint.Id = resultGroup.Guid;
                    bcfTopics.Add(new BcfTopic
                    {
                        ServerAssignedId = resultGroup.Guid.ToString(),
                        CreationDate = DateTime.Now,
                        Viewpoints = new List<BcfViewpoint>
                            {
                                bcfViewpoint
                            },
                        TopicStatus = resultGroup.Status.ToString(),
                        Title = $"{testItem.TestDisplayName} - {resultGroup.DisplayName} (Group)"
                    });
                }
            
                reportCurrentCount(++currentCount);
                checkForNavisworksClashCancellation();
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }

            ClashCurrentIssue.ClearCurrentIssue();

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

        private List<BcfViewpointClippingPlane> TransformBoundingBoxToClippingPlanes(BoundingBox3D clippingBox)
        {
            Vector3 center = new Vector3(clippingBox.Center.X.ToDecimal(), clippingBox.Center.Y.ToDecimal(), clippingBox.Center.Z.ToDecimal());

            var planes = new List<BcfViewpointClippingPlane>();

            planes.Add(new BcfViewpointClippingPlane
            {
                Location = new BcfViewpointPoint
                {
                    X = Convert.ToSingle(clippingBox.Min.X.FromInternal()),
                    Y = Convert.ToSingle(center.Y.FromInternal()),
                    Z = Convert.ToSingle(center.Z.FromInternal())
                },
                Direction = new BcfViewpointVector { X = -1, Y = 0, Z = 0 }
            });

            planes.Add(new BcfViewpointClippingPlane
            {
                Location = new BcfViewpointPoint
                {
                    X = Convert.ToSingle(center.X.FromInternal()),
                    Y = Convert.ToSingle(clippingBox.Min.Y.FromInternal()),
                    Z = Convert.ToSingle(center.Z.FromInternal())
                },
                Direction = new BcfViewpointVector { X = 0, Y = -1, Z = 0 }
            });

            planes.Add(new BcfViewpointClippingPlane
            {
                Location = new BcfViewpointPoint
                {
                    X = Convert.ToSingle(center.X.FromInternal()),
                    Y = Convert.ToSingle(center.Y.FromInternal()),
                    Z = Convert.ToSingle(clippingBox.Min.Z.FromInternal())
                },
                Direction = new BcfViewpointVector { X = 0, Y = 0, Z = -1 }
            });

            planes.Add(new BcfViewpointClippingPlane
            {
                Location = new BcfViewpointPoint
                {
                    X = Convert.ToSingle(clippingBox.Max.X.FromInternal()),
                    Y = Convert.ToSingle(center.Y.FromInternal()),
                    Z = Convert.ToSingle(center.Z.FromInternal())
                },
                Direction = new BcfViewpointVector { X = 1, Y = 0, Z = 0 }
            });

            planes.Add(new BcfViewpointClippingPlane
            {
                Location = new BcfViewpointPoint
                {
                    X = Convert.ToSingle(center.X.FromInternal()),
                    Y = Convert.ToSingle(clippingBox.Max.Y.FromInternal()),
                    Z = Convert.ToSingle(center.Z.FromInternal())
                },
                Direction = new BcfViewpointVector { X = 0, Y = 1, Z = 0 }
            });

            planes.Add(new BcfViewpointClippingPlane
            {
                Location = new BcfViewpointPoint
                {
                    X = Convert.ToSingle(center.X.FromInternal()),
                    Y = Convert.ToSingle(center.Y.FromInternal()),
                    Z = Convert.ToSingle(clippingBox.Max.Z.FromInternal())
                },
                Direction = new BcfViewpointVector { X = 0, Y = 0, Z = 1 }
            });

            return planes;
        }
    }
}
