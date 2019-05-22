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
            int y = -(int)Math.Round(point.Y / TraficLight.MATRIXUNIT_IN_METER);

            // Align the Kinect coordiante system with ours.
            x += (TraficLight.MATRIX_WIDTH / 2) + TraficLight.HORIZONTAL_OFFSET;
            y += (TraficLight.MATRIX_WIDTH / 2) - TraficLight.VERTICAL_OFFSET;

            // Clamp x to the edge of the matrix, if needed.
            if (x < 0)
            {
                x = 0;
            }
            else if (x > TraficLight.MATRIX_WIDTH)
            {
                x = TraficLight.MATRIX_WIDTH;
            }

            // Clamp y to the edge of the matrix, if needed.
            if (y < 0)
            {
                y = 0;
            }
            else if (y > TraficLight.MATRIX_WIDTH)
            {
                y = TraficLight.MATRIX_WIDTH;
            }

            return new Point(x, y);
        }
    }
}
