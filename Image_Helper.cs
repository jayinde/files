using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using Microsoft.VisualBasic;

namespace JCMS.Library
{
    public class Image_Helper
    {
        static string m_source_file_url = string.Empty; //m_destination_file_url
        static string m_destination_file_url = string.Empty; //m_destination_file_url

        public static void Resize(string sourceFileURL, string destinationFileURL, int imageWidth, int imageHeight)
        {

            System.Drawing.Image originalImage = null;
            System.Drawing.Bitmap thumbnailImage = null;
            System.Drawing.Graphics thumbnailGraphics = null;
            decimal ShrinkPercentWidth = 0M;
            decimal ShrinkPercentHeight = 0M;
            int newImageWidth = 0;
            int newImageHeight = 0;
            string imgContentType = "image/jpeg";

            System.IO.FileInfo FI = new FileInfo(sourceFileURL);
            if (FI.Extension.Contains("gif"))
            {
                imgContentType = "image/gif";
            }
            else if (FI.Extension.Contains("png"))
            {
                imgContentType = "image/png";
            }

            originalImage = System.Drawing.Image.FromFile(sourceFileURL);

            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destinationFileURL));

            if ((imageWidth > 0 && originalImage.Width > imageWidth) || (imageHeight > 0 && originalImage.Height > imageHeight))
            {
                if (imageWidth > 0)
                {
                    ShrinkPercentWidth = (Convert.ToDecimal(imageWidth) / originalImage.Width * 100);
                }
                if (imageHeight > 0)
                {
                    ShrinkPercentHeight = (Convert.ToDecimal(imageHeight) / originalImage.Height * 100);
                }
                if (imageWidth <= 0)
                {
                    ShrinkPercentWidth = ShrinkPercentHeight;
                }
                if (imageHeight <= 0)
                {
                    ShrinkPercentHeight = ShrinkPercentWidth;
                }


                if (ShrinkPercentWidth == ShrinkPercentHeight)
                {
                    newImageWidth = Convert.ToInt32(Math.Round((Convert.ToDecimal(ShrinkPercentWidth) / 100) * originalImage.Width));
                    newImageHeight = Convert.ToInt32(Math.Round((Convert.ToDecimal(ShrinkPercentHeight) / 100) * originalImage.Height));
                }
                else if (ShrinkPercentWidth > ShrinkPercentHeight)
                {
                    newImageWidth = Convert.ToInt32(Math.Round((ShrinkPercentHeight / 100) * originalImage.Width));
                    newImageHeight = imageHeight;
                }
                else {
                    newImageWidth = imageWidth;
                    newImageHeight = Convert.ToInt32(Math.Round((ShrinkPercentWidth / 100) * originalImage.Height));
                }

                thumbnailImage = new System.Drawing.Bitmap(newImageWidth, newImageHeight);
                thumbnailGraphics = System.Drawing.Graphics.FromImage((System.Drawing.Image)thumbnailImage);
                //thumbnailGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic
                thumbnailGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                thumbnailGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                thumbnailGraphics.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraphics.SmoothingMode = SmoothingMode.HighQuality;

                //	Dim addictbrush As System.Drawing.Brush = New SolidBrush(System.Drawing.ColorTranslator.FromHtml("#F6F2F1"))
                //	thumbnailGraphics.FillRectangle(addictbrush, 0, 0, newImageWidth, newImageHeight)
                //	thumbnailGraphics.DrawImage(originalImage, -2, -2, newImageWidth + 4, newImageHeight + 4)
                thumbnailGraphics.DrawImage(originalImage, 0, 0, newImageWidth, newImageHeight);

                try
                {
                    ImageCodecInfo imgCodec = GetEncoderInfo(imgContentType);
                    EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100);
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;

                    thumbnailImage.Save(destinationFileURL, imgCodec, encoderParams);
                }
                catch(Exception ex)
                {
                    throw new Exception($"You do not have proper permissions set.  You must ensure that the ASPNET and NETWORK SERVICE users have write/modify permissions on the images directory and it's sub-directories. {ex}");
                }
                finally
                {
                    thumbnailGraphics.Dispose();
                    thumbnailImage.Dispose();
                }

            }
            else {
                originalImage.Save(destinationFileURL, originalImage.RawFormat);
            }

            originalImage.Dispose();

        }

        public static void ResizeNew(string sourceFileURL, string destinationFileURL, int imageWidth, int imageHeight, bool m_crop_me = false)
        {
            System.Drawing.Image origPhoto = null;
            try
            {
                origPhoto = System.Drawing.Image.FromFile(sourceFileURL);
            }
            catch
            {
                //Out of memory likely cause
                return;
            }

            bool resizeMe = true;

            if (resizeMe)
            {
                int resizedWidth = imageWidth, resizedHeight = imageHeight, resizedQuality = 100;
                string stretchMe = "false", cropV = "middle", cropH = "left", fillColor = "#FFFFFF";
                int sourceWidth = origPhoto.Width, sourceHeight = origPhoto.Height;
                int sourceX = 0, sourceY = 0, destX = -2, destY = -2;
                float nPercent = 0, nPercentW = 0, nPercentH = 0;
                int destWidth = 0, destHeight = 0;

                if (resizedWidth < 1 || resizedHeight < 1)
                {
                    resizedWidth = origPhoto.Width;
                    resizedHeight = origPhoto.Height;
                }

                if (m_crop_me)
                {
                    string AnchorUpDown = cropV;
                    string AnchorLeftRight = cropH;
                    nPercentW = (Convert.ToSingle(resizedWidth) / Convert.ToSingle(sourceWidth));
                    nPercentH = (Convert.ToSingle(resizedHeight) / Convert.ToSingle(sourceHeight));

                    if (nPercentH < nPercentW)
                    {
                        //HttpContext.Current.Trace.Write("setting Y");
                        nPercent = nPercentW;
                        switch (AnchorUpDown.ToLowerInvariant())
                        {
                            case "top": destY = -2; break;
                            case "bottom": destY = Convert.ToInt32(Conversion.Fix(resizedHeight - (sourceHeight * nPercent))); break;
                            default: destY = Convert.ToInt32(Conversion.Fix(((resizedHeight - (sourceHeight * nPercent)) / 2) - 2)); break;
                        }
                    }
                    else {
                        nPercent = nPercentH;
                        switch (AnchorLeftRight.ToLowerInvariant())
                        {
                            case "left": destX = 0; break;
                            case "right": destX = Convert.ToInt32(Conversion.Fix(resizedWidth - (sourceWidth * nPercent))); break;
                            default: destX = Convert.ToInt32(Conversion.Fix(((resizedWidth - (sourceWidth * nPercent)) / 2) - 2)); break;
                        }
                    }
                }
                else {
                    nPercentW = (Convert.ToSingle(resizedWidth) / Convert.ToSingle(sourceWidth));
                    nPercentH = (Convert.ToSingle(resizedHeight) / Convert.ToSingle(sourceHeight));

                    if (nPercentH < nPercentW)
                    {
                        nPercent = nPercentH;
                        destX = 0;
                        // CInt(Fix((resizedWidth - (sourceWidth * nPercent)) / 2))
                    }
                    else {
                        nPercent = nPercentW;
                        destY = Convert.ToInt32(Conversion.Fix(((resizedHeight - (sourceHeight * nPercent)) / 2) - 2));
                    }
                }
                // let's account for the extra pixels we left to avoid the borderbug here
                // some distortion will occur...but it should be unnoticeable
                if (stretchMe == "false" && (origPhoto.Width < resizedWidth && origPhoto.Height < resizedHeight))
                {
                    destWidth = origPhoto.Width;
                    destHeight = origPhoto.Height;
                    destX = Convert.ToInt32(Conversion.Fix((resizedWidth / 2) - (origPhoto.Width / 2)));
                    destY = Convert.ToInt32(Conversion.Fix((resizedHeight / 2) - (origPhoto.Height / 2)));
                }
                else {
                    destWidth = Convert.ToInt32(Conversion.Fix(Math.Ceiling(sourceWidth * nPercent))) + 4;
                    destHeight = Convert.ToInt32(Conversion.Fix(Math.Ceiling(sourceHeight * nPercent))) + 4;
                }

                SavePhoto(resizedWidth, resizedHeight, destHeight, destWidth, destX, destY, sourceHeight, sourceWidth, sourceX, sourceY,
                origPhoto, m_destination_file_url, fillColor, resizedQuality, "image/jpeg");
            }
            else {
                origPhoto.Save(destinationFileURL);
            }

        }

        private static void SavePhoto(int resizedWidth, int resizedHeight, int destHeight, int destWidth, int destX, int destY, int sourceHeight, int sourceWidth, int sourceX, int sourceY, System.Drawing.Image origPhoto, string img_fName, string fillColor, int resizedQuality, string img_ContentType)
        {
            Bitmap resizedPhoto = new Bitmap(resizedWidth, resizedHeight, PixelFormat.Format24bppRgb);
            resizedPhoto.SetResolution(origPhoto.HorizontalResolution, origPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(resizedPhoto);

            Color clearColor = new Color();
            try
            {
                clearColor = System.Drawing.ColorTranslator.FromHtml(fillColor);
            }
            catch
            {
                clearColor = Color.White;
            }
            grPhoto.Clear(clearColor);

            if (resizedQuality > 100 || resizedQuality < 1)
            {
                resizedQuality = 100;
            }

            // Encoder parameter for image quality 
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, resizedQuality);

            //If img_ContentType = "image/gif" Then
            //    img_ContentType = "image/jpeg"
            //End If
            // Image codec 
            ImageCodecInfo imgCodec = GetEncoderInfo(img_ContentType);

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;


            try
            {
                grPhoto.DrawImage(origPhoto, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
                try
                {
                    resizedPhoto.Save(img_fName, imgCodec, encoderParams);

                }
                catch
                {
                    throw new Exception("You do not have proper permissions set.  You must ensure that the ASPNET and NETWORK SERVICE users have write/modify permissions on the images directory and it's sub-directories.");

                }
                finally
                {
                    grPhoto.Dispose();
                    origPhoto.Dispose();
                }

            }
            catch
            {
            }

        }

        private static ImageCodecInfo GetEncoderInfo(string resizeMimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i <= codecs.Length - 1; i++)
            {
                if (codecs[i].MimeType == resizeMimeType)
                {
                    return codecs[i];
                }
            }
            return null;
        }

    }
}
