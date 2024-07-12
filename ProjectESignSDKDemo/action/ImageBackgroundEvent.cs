using iTextSharp.text.pdf;
using iTextSharp.text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectESignSDKDemo.action
{
    public class ImageBackgroundEvent : IPdfPCellEvent
    {
        protected Image image;

        public ImageBackgroundEvent(Image image)
        {
            this.image = image;
        }

        public void CellLayout(PdfPCell cell, Rectangle position, PdfContentByte[] canvases)
        {
            //IL_00ca: Expected O, but got Unknown
            try
            {
                PdfContentByte val = canvases[1];
                if (position.Width < 20f || position.Height < 20f)
                {
                    image.ScaleToFit(position.Width, position.Height);
                }
                else
                {
                    image.ScaleToFit(position.Width - 20f, position.Height - 20f);
                }

                image.SetAbsolutePosition(position.GetLeft(0f) + (position.Width - image.ScaledWidth) / 2f, position.GetBottom(0f) + (position.Height - image.ScaledHeight) / 2f);
                val.AddImage(image);
            }
            catch (DocumentException val2)
            {
                DocumentException value = val2;
                Console.WriteLine("ImageBackgroundEvent CellLayout:" + JsonConvert.SerializeObject(value) + " ");
            }
        }
    }
}
