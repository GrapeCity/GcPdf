//
// This code is part of http://localhost:20395.
// Copyright (c) GrapeCity, Inc. All rights reserved.
//
using System;
using System.IO;
using System.Drawing;
using GrapeCity.Documents.Drawing;
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Pdf.AcroForms;
using GrapeCity.Documents.Pdf.Graphics;
using GrapeCity.Documents.Text;
using System.Security.Cryptography.X509Certificates;

namespace GcPdfWeb.Samples
{
    // This sample demonstrates how to create and sign a PDF with a .pfx file,
    // specifying a custom AppearanceStream to represent the signature.
    // This sample is similar to SignDoc, except that it adds the AppearanceStream.
    public class SignatureAppearance
    {
        public int CreatePDF(Stream stream)
        {
            GcPdfDocument doc = new GcPdfDocument();
            Page page = doc.NewPage();
            TextFormat tf = new TextFormat() { Font = StandardFonts.Times, FontSize = 14 };
            page.Graphics.DrawString(
                "Hello, World!\r\nSigned below by GcPdfWeb SignatureAppearance sample.",
                tf, new PointF(72, 72));

            // Init a test certificate:
            var pfxPath = Path.Combine("Resources", "Misc", "GcPdfTest.pfx");
            X509Certificate2 cert = new X509Certificate2(File.ReadAllBytes(pfxPath), "qq",
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            SignatureProperties sp = new SignatureProperties();
            sp.Certificate = cert;
            sp.Location = "GcPdfWeb Sample Browser";
            sp.SignerName = "GcPdfWeb";

            // Create a signature field to hold the signature:
            SignatureField sf = new SignatureField();
            // Add the signature field to the document:
            doc.AcroForm.Fields.Add(sf);
            // Connect the signature field and signature props:
            sp.SignatureField = sf;

            // Set up the signature field:
            sf.Widget.Rect = new RectangleF(page.Size.Width - 72 * 4, 72 * 2, 72 * 3, 72);
            sf.Widget.Page = page;
            // Widget visual props will be overridden by sf.Widget.AppearanceStreams.Normal.Default set below:
            sf.Widget.BackColor = Color.PeachPuff;
            sf.Widget.Border = new GrapeCity.Documents.Pdf.Annotations.Border()
            {
                Color = Color.SaddleBrown,
                Width = 3,
            };
            sf.Widget.ButtonAppearance.Caption = $"Signer: {sp.SignerName}\r\nLocation: {sp.Location}";
            // Create custom signature appearance stream:
            var rc = new RectangleF(PointF.Empty, sf.Widget.Rect.Size);
            var fxo = new FormXObject(doc, rc);
            rc.Inflate(-4, -4);
            fxo.Graphics.FillEllipse(rc, Color.CornflowerBlue);
            fxo.Graphics.DrawEllipse(rc, new Pen(Color.RoyalBlue, 3));
            rc.Inflate(-5, -5);
            fxo.Graphics.DrawEllipse(rc, new Pen(Color.LightSteelBlue, 1));
            fxo.Graphics.DrawString($"Signed by {sp.SignerName}\non {DateTime.Now.ToShortDateString()}.",
                new TextFormat()
                {
                    FontName = "Times New Roman",
                    FontSize = 14,
                    FontItalic = true,
                    ForeColor = Color.Navy
                },
                fxo.Bounds,
                TextAlignment.Center, ParagraphAlignment.Center, false);
            sf.Widget.AppearanceStreams.Normal.Default = fxo;

            // Reset signature appearance so that the widget appearance stream is used:
            sp.SignatureAppearance = null;

            // Sign and save the document:
            // NOTES:
            // - Signing and saving is an atomic operation, the two cannot be separated.
            // - The stream passed to the Sign() method must be readable.
            doc.Sign(sp, stream);

            // Done (the generated and signed docment has already been saved to 'stream').
            return doc.Pages.Count;
        }
    }
}
