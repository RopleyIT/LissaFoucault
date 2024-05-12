
using System;
using SvgPlotter;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace PlotShape
{
    class Program
    {

        private static readonly string usage =
@"Usage: plotshape.exe -l XCycles YCycles Diameter Phase
    XCycles    Number of cycles on X axis
    YCycles    Number of cycles on Y axis
    Diameter   Diameter of the circle enclosing the Lissajous figure at its corners
    Phase      The rotational phase of the shape in degrees
plotshape.exe -f XTwelfth YTwelfth Diameter
    XTwelfth   The angle in multiples of 30 degrees for the outermost point of
               the shape at the diameter of the shape. Range -6 to +6
    YTwelfth   The angle on multiples of 30 degrees of the second edge of the shape
    Diameter   The outer diameter of the circle surrouding the shape
plotshape.exe -F filePath Diameter
    filePath   The path to the Foucault segment description file
    Diameter   The outer diameter of the circle surrouding the shape";

        private static void Usage()
        {
            Console.WriteLine(usage);
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 2 || args.Length < 1 || args.Length > 5)
                {
                    Usage();
                    return;
                }

                bool lissaJous = args[0] == "-l";
                bool foucault = args[0] == "-f";
                bool foucaultFile = args[0] == "-F";
                bool graph = args[0] == "-g";

                if (!lissaJous && !foucault && !foucaultFile && !graph)
                {
                    Usage();
                    return;
                }

                int xVal = 0;
                int yVal = 0;
                int diameter = 1080;
                int phase = 0;

                if (lissaJous && (args.Length != 5
                    || !int.TryParse(args[1], out xVal)
                    || !int.TryParse(args[2], out yVal)
                    || !int.TryParse(args[3], out diameter)
                    || !int.TryParse(args[4], out phase)))
                {
                    Usage();
                    return;
                }

                if (foucault && (args.Length != 4
                    || !int.TryParse(args[1], out xVal)
                    || !int.TryParse(args[2], out yVal)
                    || !int.TryParse(args[3], out diameter)))
                {
                    Usage();
                    return;
                }

                if (foucaultFile && (args.Length != 3
                    || !int.TryParse(args[2], out diameter)))
                {
                    Usage();
                    return;
                }

                if(graph && (args.Length != 1))
                {
                    Usage();
                    return;
                }

                SVGCreator svgFactory = new ()
                {
                    DocumentDimensionUnits = "mm",
                    DocumentDimensions = new SizeF(diameter, diameter),
                    ViewBoxDimensions = new RectangleF
                    (-diameter / 2, -diameter / 2, diameter, diameter),
                    //ViewBoxDimensionUnits = "mm"
                };

                if (foucaultFile)
                {
                    List<Segment> segments = new();
                    using StreamReader sw = new(args[1]);
                    for (Segment segment = Segment.FromText(sw); segment != null; segment = Segment.FromText(sw))
                        segments.Add(segment);
                    foreach (Segment s in segments)
                        svgFactory.AddPath
                            (FoucaultSegment(diameter, s.Start, s.End), true, s.Stroke, s.LineWidth, s.Fill);
                }
                else if(graph)
                {
                    List<List<PointF>> plots = new();
                    for(int i = -6; i <= 6; i++)
                        plots.Add(CartesianFoucault(180, i).ToList());
                    plots.Add(ArcAt(56.4566).ToList());
                    Image image = Plot.PlotPolarGraphs(plots, 1080, 1080);
                    image.Save("polar.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else if (foucault)
                    svgFactory.AddPath(FoucaultSegment(diameter, xVal, yVal), true, null, 00, $"#{xVal + 6:x}00000");
                else
                    svgFactory.AddPath(CalcLissa(xVal, yVal, diameter, phase), true, null, 00, "gray");
                if (!graph)
                {
                    //using var ostream = new StreamWriter($"{xVal}-{yVal}-{phase}-{diameter}.svg");
                    using var ostream = new StreamWriter($"{DateTime.Now:HHmmss}.svg");
                    svgFactory.CalculateViewBox(new SizeF(10, 10));
                    ostream.Write(svgFactory.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static IEnumerable<PointF> CalcLissa(int xCycles, int yCycles, int diameter, int phase)
        {
            double pointCount = 256.0;
            double xLimit = 2 * Math.PI * xCycles;
            double yLimit = 2 * Math.PI * yCycles;
            double amplitude = diameter * 0.4 * 0.7071;
            double phi = phase * Math.PI / 180.0;
            for (int i = 0; i < pointCount; i++)
            {
                double x = xLimit * i / pointCount;
                double y = yLimit * i / pointCount;
                yield return new PointF((float)(amplitude * Math.Cos(x + phi)),
                    (float)(amplitude * Math.Cos(y)));
            }
        }

        //static IEnumerable<PointF> FoucaultSegment(int diameter, int hours)
        //    => FoucaultSegment(diameter, hours, hours + 1);

        static IEnumerable<PointF> FoucaultSegment(int diameter, int startHours, int endHours)
        {
            return CalcFoucault(diameter, startHours)
                .Concat(CircumSegment(diameter, startHours, endHours))
                .Concat(CalcFoucault(diameter, endHours).Reverse());
        }

        static IEnumerable<PointF> CircumSegment(int diameter, int startHours, int endHours)
        {
            for (double angle = startHours / 6.0 * Math.PI;
                angle < endHours / 6.0 * Math.PI;
                angle += Math.PI / 1080)
                //yield return new PointF((float)(diameter / 2.0 * Math.Cos(angle)),
                //    (float)(diameter / 2.0 * Math.Sin(angle)));
                yield return new PointF((float)(diameter / 2.0 * Math.Cos(angle)),
                    (float)(diameter / 2.0 * Math.Sin(angle)));
        }

        static IEnumerable<PointF> CalcFoucault(int diameter, int hours)
        {

            double latCount = 270;
            //double xLimit = 2 * Math.PI * xCycles;
            //double yLimit = 2 * Math.PI * yCycles;
            //double amplitude = diameter * 0.4 * 0.7071;
            //double phi = phase * Math.PI / 180.0;
            for (int i = 0; i <= latCount; i++)
            {
                double sinLat = Math.Sin(Math.PI / 180.0 * i / 3.0);
                double theta = Math.PI / 6 * hours * sinLat;
                double r = i / 270.0 * diameter / 2.0;
                //double r = (270-i) / 270.0 * diameter / 2.0;
                double x = r * Math.Cos(theta);
                double y = r * Math.Sin(theta);
                yield return new PointF((float)x,
                    (float)y);
            }
        }

        static IEnumerable<PointF> CartesianFoucault(int diameter, int hours)
        {

            double latCount = 270;
            for (int i = 0; i <= latCount; i++)
            {
                double sinLat = Math.Sin(Math.PI / 180.0 * i / 3.0);
                double theta = Math.PI / 6 * hours * sinLat;
                double r = i / 270.0 * diameter / 2.0;
                yield return new PointF((float)theta,
                    (float)r);
            }
        }

        static IEnumerable<PointF> ArcAt(double latitude)
        {
            double sinLat = Math.Sin(latitude*Math.PI/180.0);
            for(int i = -180; i <= 180; i++)
            {
                float angle = (float)(Math.PI / 180.0 * i * sinLat);
                yield return new PointF((float)angle, (float)latitude);
            }
        }
    }
}
