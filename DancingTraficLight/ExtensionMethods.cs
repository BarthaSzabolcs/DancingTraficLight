using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DancingTraficLight.ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static Point ToGridPosition(this CameraSpacePoint point)
        {
            // Scale up/down the x and y values to match the scale of our coordinate system.
            int x = (int)Math.Round(point.X / TraficLight.MATRIXUNIT_IN_METER);
            int y = (int)Math.Round(point.Y / TraficLight.MATRIXUNIT_IN_METER);

            // Align the Kinect coordiante system with ours.
            x += (TraficLight.MATRIX_WIDTH / 2) + TraficLight.HORIZONTAL_OFFSET;
            y += (TraficLight.MATRIX_WIDTH / 2) + TraficLight.VERTICAL_OFFSET;

            // If the coordinate is valid return it, else return (0, 0).
            if (0 <= x && Math.Abs(x) < TraficLight.MATRIX_WIDTH && 0 <= y && Math.Abs(y) < TraficLight.MATRIX_WIDTH)
            {
                return new Point(x, y);
            }
            else
            {
                return new Point(0, 0);
            }
        }
    }
}
