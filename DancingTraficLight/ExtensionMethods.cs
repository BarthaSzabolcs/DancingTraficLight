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
                return new Point(-1, -1);
            }
        }
        public static CameraSpacePoint ToCameraSpacePoint(this Point point)
        {
            float x = point.X - TraficLight.HORIZONTAL_OFFSET - TraficLight.MATRIX_WIDTH / 2;
            float y = point.Y - TraficLight.VERTICAL_OFFSET - TraficLight.MATRIX_WIDTH / 2;

            x = x * TraficLight.MATRIXUNIT_IN_METER;
            y = y * TraficLight.MATRIXUNIT_IN_METER;

            return new CameraSpacePoint()
            {
                X = x,
                Y = y
            };
        }
        public static Point CalculateCorrection(this Point pointA, Point pointB, int desiredDistance)
        {
            Point correction = new Point();

            Point difference = new Point(pointB.X - pointA.X, pointB.Y - pointA.Y);
            double differenceMagnitude = Math.Sqrt(difference.X * difference.X + difference.Y * difference.Y);

            if ((int)differenceMagnitude > desiredDistance)
            {
                correction.X = Math.Abs(difference.X) > 0 ? 
                    Math.Sign(difference.X) * 1 : 
                    0;

                correction.Y = Math.Abs(difference.Y) > 0 ? 
                    Math.Sign(difference.Y) * 1 : 
                    0;
            }
            else if((int)differenceMagnitude < desiredDistance)
            {
                correction.X = Math.Abs(difference.X) > 0 ?
                    Math.Sign(difference.X) * -1 :
                    0;

                correction.Y = Math.Abs(difference.Y) > 0 ?
                    Math.Sign(difference.Y) * -1 :
                    0;
            }

            return new Point
            {
                X = pointA.X + correction.X,
                Y = pointA.Y + correction.Y
            };
        } 
        public static int SquaredPixelDistance(this Point pointA, Point pointB)
        {
            Point distance = new Point
            {
                X = pointB.X - pointA.X,
                Y = pointB.Y - pointA.Y
            };

            return Math.Abs(distance.X * distance.X) + Math.Abs(distance.Y * distance.Y);
        }

        //original code: https://rosettacode.org/wiki/Bitmap/Midpoint_circle_algorithm
        public static void CalculateCircle(this Point middlePoint, int radius, List<Point> edgePoints)
        {
            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;

            edgePoints.Clear();

            do
            {
                edgePoints.Add(new Point(middlePoint.X + x, middlePoint.Y + y));
                edgePoints.Add(new Point(middlePoint.X + x, middlePoint.Y - y));
                edgePoints.Add(new Point(middlePoint.X - x, middlePoint.Y + y));
                edgePoints.Add(new Point(middlePoint.X - x, middlePoint.Y - y));
                edgePoints.Add(new Point(middlePoint.X + y, middlePoint.Y + x));
                edgePoints.Add(new Point(middlePoint.X + y, middlePoint.Y - x));
                edgePoints.Add(new Point(middlePoint.X - y, middlePoint.Y + x));
                edgePoints.Add(new Point(middlePoint.X - y, middlePoint.Y - x));

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;

            } while (x <= y);
        }

        public static Point NearestPoint(this Point point, List<Point> points)
        {
            int idx = 0;

            for (int i = 1; i < points.Count; i++)
            {
                if (points[idx].SquaredPixelDistance(point) > points[i].SquaredPixelDistance(point))
                {
                    idx = i;
                }
            }

            return points[idx];
        } 
    }
}
