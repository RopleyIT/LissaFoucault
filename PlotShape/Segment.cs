using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotShape
{
    /// <summary>
    /// Describes one coloured segment of the shape. Each input line of text
    /// is of the form in the following example:
    /// -2 3 red 2 green
    /// Where the -2 states the starting angle is a -2*30 degrees, the ending
    /// angle at +3*30 degrees, the line colour red with thickness 2, and the
    /// fill colour green. Pretty garish, in fact!
    /// </summary>
    public class Segment
    {
        public int Start { get; set; }
        public int End { get; set; }
        public string Stroke { get; set; }
        public int LineWidth { get; set; }
        public string Fill { get; set; }

        public static Segment FromText(TextReader tr)
        {
            string line = tr.ReadLine();
            if (line == null)
                return null;
            string[] elements = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Segment segment = new();
            try
            {
                segment.Start = int.Parse(elements[0]);
                segment.End = int.Parse(elements[1]);
                segment.Stroke = elements[2];
                segment.LineWidth = int.Parse(elements[3]);
                segment.Fill = elements[4];
                if (elements[2] == "-" || elements[2] == "none")
                    elements[2] = null;
                if (elements[4] == "-" || elements[4] == "none")
                    elements[4] = null;
            }
            catch
            {
                return null;
            }
            return segment;
        }
    }
}
