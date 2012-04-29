using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Imaging.Filters;
using AForge;
using AForge.Imaging;
using AForge.Math.Geometry;
using System.Drawing;
using System.Drawing.Imaging;

namespace KinectPositioning
{

    // Blobs' highlight types enumeration
    public enum HighlightType
    {
        ConvexHull = 0,
        LeftAndRightEdges = 1,
        TopAndBottomEdges = 2,
        Quadrilateral = 3
    }

    /// <summary>
    /// Has functions to detect and highlight blobs.
    /// </summary>
    public class BlobsDetector
    {
        private Bitmap image = null;
        private int imageWidth, imageHeight;


        private BlobCounter blobCounter = new BlobCounter();
        private Blob[] blobs;
        private int selectedBlobID;
        

        Dictionary<int, List<IntPoint>> leftEdges = new Dictionary<int, List<IntPoint>>();
        Dictionary<int, List<IntPoint>> rightEdges = new Dictionary<int, List<IntPoint>>();
        Dictionary<int, List<IntPoint>> topEdges = new Dictionary<int, List<IntPoint>>();
        Dictionary<int, List<IntPoint>> bottomEdges = new Dictionary<int, List<IntPoint>>();

        Dictionary<int, List<IntPoint>> hulls = new Dictionary<int, List<IntPoint>>();
        Dictionary<int, List<IntPoint>> quadrilaterals = new Dictionary<int, List<IntPoint>>();

        private HighlightType highlighting = HighlightType.ConvexHull;
        private bool showRectangleAroundSelection = false;

        // Blobs' highlight type
        public HighlightType Highlighting
        {
            get { return highlighting; }
            set
            {
                highlighting = value;
            }
        }

        // Show rectangle around selection or not
        public bool ShowRectangleAroundSelection
        {
            get { return showRectangleAroundSelection; }
            set
            {
                showRectangleAroundSelection = value;
            }
        }

        public BlobsDetector()
        {
        }

        public void CountBlobs(Bitmap depthImage)
        {
            selectedBlobID = 0;

            this.image = AForge.Imaging.Image.Clone(depthImage, PixelFormat.Format24bppRgb);
            imageWidth = this.image.Width;
            imageHeight = this.image.Height;

            blobCounter.ProcessImage(this.image);
            blobs = blobCounter.GetObjectsInformation();

            BlobCount = blobs.Count();
        }

        // Set image to display by the control
        public Bitmap ProcessImage(Bitmap depthImage, Bitmap colorImage)
        {
            leftEdges.Clear();
            rightEdges.Clear();
            topEdges.Clear();
            bottomEdges.Clear();
            hulls.Clear();
            quadrilaterals.Clear();


            selectedBlobID = 0;

            this.image = AForge.Imaging.Image.Clone(depthImage, PixelFormat.Format24bppRgb);
            imageWidth = this.image.Width;
            imageHeight = this.image.Height;

            blobCounter.ProcessImage(this.image);
            blobs = blobCounter.GetObjectsInformation();

            ResizeNearestNeighbor filter = new ResizeNearestNeighbor(depthImage.Width, depthImage.Height);
            var outImage = filter.Apply(colorImage);
            outImage.RotateFlip(RotateFlipType.RotateNoneFlipX);


            BlobCount = blobs.Count();

            GrahamConvexHull grahamScan = new GrahamConvexHull();

            foreach (Blob blob in blobs)
            {
                List<IntPoint> leftEdge = new List<IntPoint>();
                List<IntPoint> rightEdge = new List<IntPoint>();
                List<IntPoint> topEdge = new List<IntPoint>();
                List<IntPoint> bottomEdge = new List<IntPoint>();

                // collect edge points
                blobCounter.GetBlobsLeftAndRightEdges(blob, out leftEdge, out rightEdge);
                blobCounter.GetBlobsTopAndBottomEdges(blob, out topEdge, out bottomEdge);

                leftEdges.Add(blob.ID, leftEdge);
                rightEdges.Add(blob.ID, rightEdge);
                topEdges.Add(blob.ID, topEdge);
                bottomEdges.Add(blob.ID, bottomEdge);

                // find convex hull
                List<IntPoint> edgePoints = new List<IntPoint>();
                edgePoints.AddRange(leftEdge);
                edgePoints.AddRange(rightEdge);

                List<IntPoint> hull = grahamScan.FindHull(edgePoints);
                hulls.Add(blob.ID, hull);

                List<IntPoint> quadrilateral = null;

                // find quadrilateral
                if (hull.Count < 4)
                {
                    quadrilateral = new List<IntPoint>(hull);
                }
                else
                {
                    quadrilateral = PointsCloud.FindQuadrilateralCorners(hull);
                }
                quadrilaterals.Add(blob.ID, quadrilateral);

                // shift all points for vizualization
                IntPoint shift = new IntPoint(1, 1);

                PointsCloud.Shift(leftEdge, shift);
                PointsCloud.Shift(rightEdge, shift);
                PointsCloud.Shift(topEdge, shift);
                PointsCloud.Shift(bottomEdge, shift);
                PointsCloud.Shift(hull, shift);
                PointsCloud.Shift(quadrilateral, shift);
            }

            DrawHighLights(outImage);

            return outImage;


        }

        private void DrawHighLights(Bitmap outImage)
        {
            Graphics g = Graphics.FromImage(outImage);
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            Pen borderPen = new Pen(Color.FromArgb(64, 64, 64), 4);
            Pen highlightPen = new Pen(Color.Red);
            Pen highlightPenBold = new Pen(Color.FromArgb(0, 255, 0), 3);
            Pen rectPen = new Pen(Color.Blue);


            // draw rectangle
            g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

            if (image != null)
            {
                g.DrawImage(outImage, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);

                foreach (Blob blob in blobs)
                {
                    Pen pen = (blob.ID == selectedBlobID) ? highlightPenBold : highlightPen;

                    if ((showRectangleAroundSelection) && (blob.ID == selectedBlobID))
                    {
                        g.DrawRectangle(rectPen, blob.Rectangle);
                    }

                    switch (highlighting)
                    {
                        case HighlightType.ConvexHull:
                            g.DrawPolygon(pen, PointsListToArray(hulls[blob.ID]));
                            break;
                        case HighlightType.LeftAndRightEdges:
                            DrawEdge(g, pen, leftEdges[blob.ID]);
                            DrawEdge(g, pen, rightEdges[blob.ID]);
                            break;
                        case HighlightType.TopAndBottomEdges:
                            DrawEdge(g, pen, topEdges[blob.ID]);
                            DrawEdge(g, pen, bottomEdges[blob.ID]);
                            break;
                        case HighlightType.Quadrilateral:
                            g.DrawPolygon(pen, PointsListToArray(quadrilaterals[blob.ID]));
                            break;
                    }
                }
            }
            else
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, 128, 128)),
                    rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
            }
        }

        public int BlobCount { get; set; }

        // Convert list of AForge.NET's IntPoint to array of .NET's Point
        private static System.Drawing.Point[] PointsListToArray(List<IntPoint> list)
        {
            System.Drawing.Point[] array = new System.Drawing.Point[list.Count];

            for (int i = 0, n = list.Count; i < n; i++)
            {
                array[i] = new System.Drawing.Point(list[i].X, list[i].Y);
            }

            return array;
        }

        // Draw object's edge
        private static void DrawEdge(Graphics g, Pen pen, List<IntPoint> edge)
        {
            System.Drawing.Point[] points = PointsListToArray(edge);

            if (points.Length > 1)
            {
                g.DrawLines(pen, points);
            }
            else
            {
                g.DrawLine(pen, points[0], points[0]);
            }
        }
    }


}
