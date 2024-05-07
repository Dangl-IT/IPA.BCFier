using Autodesk.Navisworks.Api;
using IPA.Bcfier.Models.Bcf;
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
                var v = new BcfViewpoint();

                Vector3D vi = GetViewDirection(viewpoint);
                Vector3D up = GetViewUp(viewpoint);
                Point3D c = new Point3D(viewpoint.Position.X, viewpoint.Position.Y, viewpoint.Position.Z);
                double zoomValue = 1;

                //prepare view
                viewpoint = viewpoint.CreateCopy();
                if (!viewpoint.HasFocalDistance)
                    viewpoint.FocalDistance = 1;

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
            catch (System.Exception ex1)
            {
                // TODO
                //TaskDialog.Show("Error generating viewpoint", "exception: " + ex1);
            }
            return null;
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
    }
}
