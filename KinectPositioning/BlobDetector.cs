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
    /// <summary>
    /// Has functions to detect and highlight blobs.
    /// </summary>
    public class BlobTracker
    {
        private Bitmap image = null;
        private int imageWidth, imageHeight;

        private BlobCounter blobCounter = new BlobCounter();

        // The current blob being processed and displayed
        private Blob currentBlob;
        public int BlobCount;

        public BlobTracker()
        {
        }

        private int GetBlobOfInterest(Bitmap image)
        {
            Blob[] blobs;
            int blobCount;

            // Set blob filters
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 15;
            blobCounter.MinWidth = 15;
            blobCounter.MaxHeight = 100;
            blobCounter.MaxWidth = 100;
            blobCounter.ObjectsOrder = ObjectsOrder.Area;

            // Find the blobs
            blobCounter.ProcessImage(this.image);
            blobs = blobCounter.GetObjectsInformation();

            blobCount = blobs.Count();

            if(blobCount > 0)
            {
                currentBlob = blobs[0];
            }

            return blobCount;
        }

        public Bitmap ProcessFrame(Bitmap depthImage)
        {
            // Prepare the output image
            Bitmap outImage = new Bitmap(depthImage.Width, depthImage.Height);

            // Create an image for AForge to process
            this.image = AForge.Imaging.Image.Clone(depthImage, PixelFormat.Format24bppRgb);
            imageWidth = this.image.Width;
            imageHeight = this.image.Height;

            // Find the blob of interest
            BlobCount = GetBlobOfInterest(this.image);

            if (BlobCount > 0)
            {
                blobCounter.ExtractBlobsImage(this.image, currentBlob, true);
                outImage = currentBlob.Image.ToManagedImage();
            }
            else
            {
                //Clear the output bitmap
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(outImage))
                {
                    g.Clear(System.Drawing.Color.Black);
                }
            }

            return outImage;
        }
    }
}
