﻿using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace FlightStreamDeck.Logics
{
    public interface IImageLogic
    {
        string GetImage(string text, bool active, bool legacyButtonStyle, string value = null);
        string GetNumberImage(int number);
        string GetNavComImage(string type, bool dependant, string value1 = null, string value2 = null, bool showMainOnly = false, bool valid = false);
        string GetNavComActionLabel(string label, bool error = false);
        public string GetHorizonImage(float pitchInDegrees, float rollInDegrees, float headingInDegrees);
        public string GetGaugeImage(string text, float value, float min, float max);
    }

    public class ImageLogic : IImageLogic
    {
        readonly Image backGround = Image.Load("Images/button.png");
        readonly Image activeBackground = Image.Load("Images/button_active.png");
        readonly Image toggleOff = Image.Load("Images/off.png");
        readonly Image toggleOn = Image.Load("Images/on.png");
        readonly Image horizon = Image.Load("Images/horizon.png");
        readonly Image gaugeImage = Image.Load("Images/gauge.png");

        private const int WIDTH = 72;
        private const int HALF_WIDTH = 36;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Base64 image data</returns>
        public string GetImage(string text, bool active, bool legacyButtonStyle, string value = null)
        {
            var font = SystemFonts.CreateFont("Arial", 17, FontStyle.Regular);
            var valueFont = SystemFonts.CreateFont("Arial", 15, FontStyle.Regular);
            bool hasValue = value != null && value.Length > 0;
            Image activeImg = legacyButtonStyle ? activeBackground : toggleOn;
            Image inactiveImg = legacyButtonStyle || hasValue ? backGround : toggleOff;

            Image img = active && !hasValue ? activeImg : inactiveImg;
            using var img2 = img.Clone(ctx =>
            {
                var imgSize = ctx.GetCurrentSize();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var size = TextMeasurer.Measure(text, new RendererOptions(font));
                    ctx.DrawText(text, font, Color.White, new PointF(imgSize.Width / 2 - size.Width / 2, imgSize.Height / 4));

                    if (hasValue)
                    {
                        size = TextMeasurer.Measure(value, new RendererOptions(valueFont));
                        ctx.DrawText(value, valueFont, active ? Color.Yellow : Color.White, new PointF(imgSize.Width / 2 - size.Width / 2, 46));
                    }
                }
            });
            using var memoryStream = new MemoryStream();
            img2.Save(memoryStream, new PngEncoder());
            var base64 = Convert.ToBase64String(memoryStream.ToArray());

            return "data:image/png;base64, " + base64;
        }

        /// <returns>Base64 image data</returns>
        public string GetNumberImage(int number)
        {
            var font = SystemFonts.CreateFont("Arial", 36, FontStyle.Bold);

            var text = number.ToString();
            Image img = backGround;
            using var img2 = img.Clone(ctx =>
            {
                var imgSize = ctx.GetCurrentSize();
                var size = TextMeasurer.Measure(text, new RendererOptions(font));
                ctx.DrawText(text, font, Color.White, new PointF(imgSize.Width / 2 - size.Width / 2, imgSize.Height / 2 - size.Height / 2));
            });
            using var memoryStream = new MemoryStream();
            img2.Save(memoryStream, new PngEncoder());
            var base64 = Convert.ToBase64String(memoryStream.ToArray());

            return "data:image/png;base64, " + base64;
        }

        public string GetNavComImage(string type, bool dependant, string value1 = null, string value2 = null, bool showMainOnly = false, bool valid = false)
        {
            var font = SystemFonts.CreateFont("Arial", 17, FontStyle.Regular);
            var valueFont = SystemFonts.CreateFont("Arial", showMainOnly ? 26 : 13, FontStyle.Regular);

            Image img = backGround;
            using var img2 = img.Clone(ctx =>
            {
                var imgSize = ctx.GetCurrentSize();

                if (!string.IsNullOrWhiteSpace(type))
                {
                    var size = TextMeasurer.Measure(type, new RendererOptions(font));
                    Color displayColor = dependant ? Color.White : Color.LightGray;
                    ctx.DrawText(type, font, displayColor, new PointF(imgSize.Width / 2 - size.Width / 2, imgSize.Height / 4));
                }

                if (!string.IsNullOrWhiteSpace(value1))
                {
                    var size1 = TextMeasurer.Measure(value1, new RendererOptions(valueFont));
                    Color displayColor = dependant ? Color.Yellow : Color.LightGray;
                    ctx.DrawText(value1, valueFont, displayColor, new PointF(imgSize.Width / 2 - size1.Width / 2, imgSize.Height / 2));
                }
                if (!string.IsNullOrWhiteSpace(value2) && !showMainOnly)
                {
                    var size2 = TextMeasurer.Measure(value2, new RendererOptions(valueFont));
                    Color displayColor = dependant ? Color.White : Color.LightGray;
                    if (string.IsNullOrWhiteSpace(value1))
                    {
                        displayColor = valid ? Color.Green : Color.Red;
                    }
                    ctx.DrawText(value2, valueFont, displayColor, new PointF(imgSize.Width / 2 - size2.Width / 2, imgSize.Height / 2 + size2.Height + 2));
                }
            });
            using var memoryStream = new MemoryStream();
            img2.Save(memoryStream, new PngEncoder());
            var base64 = Convert.ToBase64String(memoryStream.ToArray());

            return "data:image/png;base64, " + base64;
        }

        public string GetNavComActionLabel(string label, bool error = false)
        {
            var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);

            Image img = backGround;
            using var img2 = img.Clone(ctx =>
            {
                var imgSize = ctx.GetCurrentSize();

                if (label != null)
                {
                    var size = TextMeasurer.Measure(label, new RendererOptions(font));
                    ctx.DrawText(label, font, error ? Color.Red : Color.White, new PointF(imgSize.Width / 2 - size.Width / 2, imgSize.Height / 2));
                }
            });
            using var memoryStream = new MemoryStream();
            img2.Save(memoryStream, new PngEncoder());
            var base64 = Convert.ToBase64String(memoryStream.ToArray());

            return "data:image/png;base64, " + base64;
        }

        public string GetHorizonImage(float pitchInDegrees, float rollInDegrees, float headingInDegrees)
        {
            //var font = SystemFonts.CreateFont("Arial", 10, FontStyle.Regular);
            //var valueFont = SystemFonts.CreateFont("Arial", 12, FontStyle.Regular);
            var pen = new Pen(Color.Yellow, 3);

            var shiftedRolledHorizon = new Image<Rgba32>(105, 105);
            shiftedRolledHorizon.Mutate(ctx =>
            {
                var size = horizon.Size();
                ctx.DrawImage(horizon, new Point(
                    (int)Math.Round((float)-size.Width / 2 + 52),
                    (int)Math.Round((float)-size.Height / 2 + 52 - (pitchInDegrees * 2))
                    ), new GraphicsOptions());
                ctx.Rotate(rollInDegrees);
            });

            using (var img = new Image<Rgba32>(WIDTH, WIDTH))
            {
                img.Mutate(ctx =>
                {
                    var size = shiftedRolledHorizon.Size();
                    ctx.DrawImage(shiftedRolledHorizon, new Point(
                        (int)Math.Round((float)-size.Width / 2 + HALF_WIDTH),
                        (int)Math.Round((float)-size.Height / 2 + HALF_WIDTH)
                        ), new GraphicsOptions());

                    // Draw bug
                    PointF[] leftLine = { new PointF(6, 36), new PointF(26, 36) };
                    PointF[] rightLine = { new PointF(46, 36), new PointF(66, 36) };
                    PointF[] bottomLine = { new PointF(36, 41), new PointF(36, 51) };
                    ctx.DrawLines(pen, leftLine);
                    ctx.DrawLines(pen, rightLine);
                    ctx.DrawLines(pen, bottomLine);
                });

                using var memoryStream = new MemoryStream();
                img.Save(memoryStream, new PngEncoder());
                var base64 = Convert.ToBase64String(memoryStream.ToArray());

                return "data:image/png;base64, " + base64;
            }
        }

        public string GetGaugeImage(string text, float value, float min, float max)
        {
            var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Regular);
            var titleFont = SystemFonts.CreateFont("Arial", 15, FontStyle.Regular);
            var pen = new Pen(Color.DarkRed, 5);
            var range = max - min;

            value = value < min ? min : value;
            value = value > max ? max : value;

            if (range <= 0)
            {
                range = 1;
            }

            using var img = gaugeImage.Clone(ctx =>
            {
                double angleOffset = Math.PI * -1.25;
                var ratio = (value - min) / range;
                if (ratio < 0) ratio = 0;
                if (ratio > 1) ratio = 1;
                double angle = Math.PI * ratio + angleOffset;

                var startPoint = new PointF(HALF_WIDTH, HALF_WIDTH);
                var middlePoint = new PointF(
                    (float)((HALF_WIDTH - 16) * Math.Cos(angle)),
                    (float)((HALF_WIDTH - 16) * Math.Sin(angle))
                    );

                var endPoint = new PointF(
                    (float)(HALF_WIDTH * Math.Cos(angle)),
                    (float)(HALF_WIDTH * Math.Sin(angle))
                    );

                PointF[] needle = { startPoint + middlePoint, startPoint + endPoint };

                ctx.DrawLines(pen, needle);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var size = TextMeasurer.Measure(text, new RendererOptions(titleFont));
                    ctx.DrawText(text, titleFont, Color.White, new PointF(HALF_WIDTH - size.Width / 3, 57));
                }

                var valueText = value.ToString();
                Color textColor = value > max ? Color.Red : Color.White;
                ctx.DrawText(valueText, font, textColor, new PointF(25, 30));
            });

            using var memoryStream = new MemoryStream();
            img.Save(memoryStream, new PngEncoder());
            var base64 = Convert.ToBase64String(memoryStream.ToArray());

            return "data:image/png;base64, " + base64;
        }
    }
}
