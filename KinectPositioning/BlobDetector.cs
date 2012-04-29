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

        public void CountBlobs(Bitmap depthImage)
        {
            
            // Create an image for AForge to process
            this.image = AForge.Imaging.Image.Clone(depthImage, PixelFormat.Format24bppRgb);
            imageWidth = this.image.Width;
            imageHeight = this.image.Height;

            // Set blob filters
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 15;
            blobCounter.MinWidth = 15;

            // Find the blobs
            blobCounter.ProcessImage(this.image);
            blobs = blobCounter.GetObjectsInformation();

            BlobCount = blobs.Count();
        }

        public int BlobCount { get; set; }
 
    }
}
