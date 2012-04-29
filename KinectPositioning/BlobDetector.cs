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
    public class BlobsDetector
    {
        private Bitmap image = null;
        private int imageWidth, imageHeight;

        private BlobCounter blobCounter = new BlobCounter();
        private Blob[] blobs;
        
        public BlobsDetector()
        {
        }

        public Bitmap CountBlobs(Bitmap depthImage)
        {
            // Prepare the output image
            Bitmap outImage = new Bitmap(depthImage.Width, depthImage.Height);

            // Create an image for AForge to process
            this.image = AForge.Imaging.Image.Clone(depthImage, PixelFormat.Format24bppRgb);
            imageWidth = this.image.Width;
            imageHeight = this.image.Height;

            // Set blob filters
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 15;
            blobCounter.MinWidth = 15;
            blobCounter.MaxHeight =100;
            blobCounter.MaxWidth = 100;
            blobCounter.ObjectsOrder = ObjectsOrder.Area;

            // Find the blobs
            blobCounter.ProcessImage(this.image);
            blobs = blobCounter.GetObjectsInformation();

            if (blobs.Length > 0)
            {
                blobCounter.ExtractBlobsImage(this.image, blobs[0], true);
                outImage = blobs[0].Image.ToManagedImage();
            }
            else
            {
                
            }

            BlobCount = blobs.Count();

            return outImage;

        }

        public int BlobCount { get; set; }
 
    }
}
