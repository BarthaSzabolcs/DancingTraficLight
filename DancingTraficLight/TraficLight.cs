using DancingTraficLight.Properties;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using System.Linq;
using DancingTraficLight.ExtensionMethods;

namespace DancingTraficLight
{
    public partial class TraficLight : Form
    {
        // World To Matrix parameters
        public const int MATRIX_WIDTH = 32;
        public const float MATRIXUNIT_IN_METER = 0.033f; //0.038f;
        public const int VERTICAL_OFFSET = -3;
        public const int HORIZONTAL_OFFSET = 0;

        // Kinect
        private KinectSensor kinectSensor;
        private BodyFrameReader bodyFrameReader;
        private IList<Body> bodies;

        // Fields
        private bool[,] outputMatrix = new bool[MATRIX_WIDTH , MATRIX_WIDTH];
        private List<Point> linePositions = new List<Point>();
        private List<Point> circlePositions = new List<Point>();
        private int[] xValues = new int[2];

        // FormApp Only
        private Bitmap matrixImage = new Bitmap(MATRIX_WIDTH, MATRIX_WIDTH);
        private Color positiveColor = Color.DarkRed;    //Color.FromArgb(255, 225, 0, 0);
        private Color negativeColor = Color.Black;      //Color.FromArgb(255, 35, 35, 35);

        public TraficLight()
        {
            InitializeComponent();
            InitializeKinect();
        }
        private void InitializeKinect()
        {
            kinectSensor = KinectSensor.GetDefault();
            kinectSensor.Open();

            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += HandleFrameArrived;
        }

        private void HandleFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    ResetMatrix();

                    if (bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(bodies);

                    foreach (var body in bodies)
                    {
                        if (body != null && body.IsTracked)
                        {
                            DrawBody(body);
                        }
                    }
                }
            }
            RefreshImage();
        }

        private void DrawBody(Body body)
        {
            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

            if (MATRIX_WIDTH == 32)
            {
                DrawBody32(joints);
            }
            else if(MATRIX_WIDTH == 64)
            {
                DrawBody64(joints);
            }
            else
            {
                // Head
                DrawCircle(joints[JointType.Head].Position, 6);

                // Left Shoulder
                //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderLeft, 2, 2);
                // Right Shoulder
                //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderRight, 2, 2);         

                // Upper Torso
                DrawBone(joints, JointType.SpineMid, JointType.SpineShoulder, 10, 6);
                // Lower Torso
                DrawBone(joints, JointType.SpineBase, JointType.SpineMid, 6, 10);

                // Right Upper Arm
                DrawBone(joints, JointType.ShoulderRight, JointType.ElbowRight, 3);
                // Right Lower Arm
                DrawBone(joints, JointType.ElbowRight, JointType.WristRight, 3, 2);
                // Right Hand
                //DrawBone(joints, JointType.WristRight, JointType.HandRight, 3);

                // Left Upper Arm
                DrawBone(joints, JointType.ShoulderLeft, JointType.ElbowLeft, 3);
                // Left Lower Arm
                DrawBone(joints, JointType.ElbowLeft, JointType.WristLeft, 3, 2);
                // RIght Hand
                //DrawBone(joints, JointType.WristLeft, JointType.HandLeft, 3);

                // Hip
                //DrawBone(joints, JointType.HipLeft, JointType.HipRight, 6);

                // Right Upper leg
                DrawBone(joints, JointType.HipRight, JointType.KneeRight, 3);
                // Right Lower leg
                DrawBone(joints, JointType.KneeRight, JointType.AnkleRight, 3, 2);
                // Right Feet
                //DrawBone(joints, JointType.AnkleRight, JointType.FootRight, 3);

                // Left Upper leg
                DrawBone(joints, JointType.HipLeft, JointType.KneeLeft, 3);
                // Left Lower leg
                DrawBone(joints, JointType.KneeLeft, JointType.AnkleLeft, 3, 2);
                // Left Feet
                //DrawBone(joints, JointType.AnkleLeft, JointType.FootLeft, 3);
            }

        }
        private void DrawBody32(IReadOnlyDictionary<JointType, Joint> joints)
        {
            // Head
            DrawCircle(joints[JointType.Head].Position, 3);

            // Left Shoulder
            //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderLeft, 2, 2);
            // Right Shoulder
            //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderRight, 2, 2);         

            // Upper Torso
            DrawBone(joints, JointType.SpineMid, JointType.SpineShoulder, 6, 3);
            // Lower Torso
            DrawBone(joints, JointType.SpineBase, JointType.SpineMid, 3, 6);

            // Right Upper Arm
            DrawBone(joints, JointType.ShoulderRight, JointType.ElbowRight, 2);
            // Right Lower Arm
            DrawBone(joints, JointType.ElbowRight, JointType.WristRight, 2, 1);
            // Right Hand
            //DrawBone(joints, JointType.WristRight, JointType.HandRight, 1);

            // Left Upper Arm
            DrawBone(joints, JointType.ShoulderLeft, JointType.ElbowLeft, 2);
            // Left Lower Arm
            DrawBone(joints, JointType.ElbowLeft, JointType.WristLeft, 2, 1);
            // RIght Hand
            //DrawBone(joints, JointType.WristLeft, JointType.HandLeft, 1);

            // Hip
            //DrawBone(joints, JointType.HipLeft, JointType.HipRight, 3);

            // Right Upper leg
            DrawBone(joints, JointType.HipRight, JointType.KneeRight, 2);
            // Right Lower leg
            DrawBone(joints, JointType.KneeRight, JointType.AnkleRight, 2, 1);
            // Right Feet
            //DrawBone(joints, JointType.AnkleRight, JointType.FootRight, 1);

            // Left Upper leg
            DrawBone(joints, JointType.HipLeft, JointType.KneeLeft, 2);
            // Left Lower leg
            DrawBone(joints, JointType.KneeLeft, JointType.AnkleLeft, 2, 1);
            // Left Feet
            //DrawBone(joints, JointType.AnkleLeft, JointType.FootLeft, 1);
        }
        private void DrawBody64(IReadOnlyDictionary<JointType, Joint> joints)
        {
            // Head
            DrawCircle(joints[JointType.Head].Position, 6);

            // Left Shoulder
            //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderLeft, 2, 2);
            // Right Shoulder
            //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderRight, 2, 2);         

            // Upper Torso
            DrawBone(joints, JointType.SpineMid, JointType.SpineShoulder, 10, 6);
            // Lower Torso
            DrawBone(joints, JointType.SpineBase, JointType.SpineMid, 6, 10);

            // Right Upper Arm
            DrawBone(joints, JointType.ShoulderRight, JointType.ElbowRight, 3);
            // Right Lower Arm
            DrawBone(joints, JointType.ElbowRight, JointType.WristRight, 3, 2);
            // Right Hand
            //DrawBone(joints, JointType.WristRight, JointType.HandRight, 3);

            // Left Upper Arm
            DrawBone(joints, JointType.ShoulderLeft, JointType.ElbowLeft, 3);
            // Left Lower Arm
            DrawBone(joints, JointType.ElbowLeft, JointType.WristLeft, 3, 2);
            // RIght Hand
            //DrawBone(joints, JointType.WristLeft, JointType.HandLeft, 3);

            // Hip
            //DrawBone(joints, JointType.HipLeft, JointType.HipRight, 6);

            // Right Upper leg
            DrawBone(joints, JointType.HipRight, JointType.KneeRight, 3);
            // Right Lower leg
            DrawBone(joints, JointType.KneeRight, JointType.AnkleRight, 3, 2);
            // Right Feet
            //DrawBone(joints, JointType.AnkleRight, JointType.FootRight, 3);

            // Left Upper leg
            DrawBone(joints, JointType.HipLeft, JointType.KneeLeft, 3);
            // Left Lower leg
            DrawBone(joints, JointType.KneeLeft, JointType.AnkleLeft, 3, 2);
            // Left Feet
            //DrawBone(joints, JointType.AnkleLeft, JointType.FootLeft, 3);
        }
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, int joint0_width)
        {
            DrawBone(joints, jointType0, jointType1, joint0_width, joint0_width);
        }
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, int joint0_width, int joint1_width)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked || joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }
            
            DrawRectangle(joint0.Position, joint1.Position, joint0_width, joint1_width);
        }

        private void DrawRectangle(CameraSpacePoint pointA, CameraSpacePoint pointB, int widthA, int widthB)
        {
            Point[] points = CalculateRectangleCorners(pointA, pointB, widthA, widthB);

            linePositions.Clear();

            CalculateLinePositions(points[0], points[1], linePositions);
            CalculateLinePositions(points[1], points[2], linePositions);
            CalculateLinePositions(points[2], points[3], linePositions);
            CalculateLinePositions(points[3], points[0], linePositions);

            int minY = (from point in points select point.Y).Min();
            int maxY = (from point in points select point.Y).Max();

            for (int y = minY ; y <= maxY; y++)
            {
                FindXValues(linePositions, xValues, y);

                for (int x = xValues[0]; x <= xValues[1]; x++)
                {
                    SetMatrixValue(x, y);
                }
            }
        }
        private void DrawCircle(CameraSpacePoint pointA, int radius)
        {
            linePositions.Clear();

            Point centerPoint = pointA.ToGridPosition();
            centerPoint.CalculateCircle(radius, linePositions);

            for (int y = centerPoint.Y - radius; y <= centerPoint.Y + radius; y++)
            {
                FindXValues(linePositions, xValues, y);

                for (int x = xValues[0]; x <= xValues[1]; x++)
                {
                    SetMatrixValue(x, y);
                }
            }
        }

        private void FindXValues(List<Point> points, int[] xValues, int y)
        {
            var minX = (from point in points where point.Y == y select point.X);
            var maxX = (from point in points where point.Y == y select point.X);

            if (minX.Any())
            {
                xValues[0] = minX.Min();
            }
            if (maxX.Any())
            {
                xValues[1] = maxX.Max();
            }
        }
        private void CalculateLinePositions(Point pointA, Point pointB, List<Point> linePositions)
        {
            int x = pointA.X;
            int y = pointA.Y;
            int x2 = pointB.X;
            int y2 = pointB.Y;

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                linePositions.Add(new Point(x, y));

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }
        private Point[] CalculateRectangleCorners(CameraSpacePoint pointA, CameraSpacePoint pointB, int widthA, int widthB)
        {
            // Calculate the vector that points from PointA to PointB.
            Vector2 direction = new Vector2(pointB.X - pointA.X, pointB.Y - pointA.Y);
            direction = Vector2.Normalize(direction);

            // Calculate a perpendicular Vector to the direction.
            Vector2 perpendicular = new Vector2(direction.Y, -direction.X);

            float correctedWidthA = widthA * MATRIXUNIT_IN_METER;
            float correctedWidthB = widthB * MATRIXUNIT_IN_METER;

            CameraSpacePoint pointC = new CameraSpacePoint
            {
                X = pointA.X + perpendicular.X * (correctedWidthA / 2),
                Y = pointA.Y + perpendicular.Y * (correctedWidthA / 2)
            };
            CameraSpacePoint pointD = new CameraSpacePoint
            {
                X = pointA.X - perpendicular.X * (correctedWidthA / 2),
                Y = pointA.Y - perpendicular.Y * (correctedWidthA / 2)
            };
            CameraSpacePoint pointE = new CameraSpacePoint
            {
                X = pointB.X - perpendicular.X * (correctedWidthB / 2),
                Y = pointB.Y - perpendicular.Y * (correctedWidthB / 2)
            };
            CameraSpacePoint pointF = new CameraSpacePoint
            {
                X = pointB.X + perpendicular.X * (correctedWidthB / 2),
                Y = pointB.Y + perpendicular.Y * (correctedWidthB / 2)
            };

            Point[] points = new Point[]
            {
                pointC.ToGridPosition(),
                pointD.ToGridPosition(),
                pointE.ToGridPosition(),
                pointF.ToGridPosition()
            };

            //points[0].CalculateCircle(widthA, circlePositions);
            //points[1] = points[1].NearestPoint(circlePositions);

            //points[2].CalculateCircle(widthB, circlePositions);
            //points[3] = points[3].NearestPoint(circlePositions);

            return points;
        }

        private void SetMatrixValue(int x, int y, bool value = true)
        {
            if (0 <= x && x < MATRIX_WIDTH && 0 <= y && y < MATRIX_WIDTH)
            {
                outputMatrix[x, y] = value;
            }
        }
        
        // Display output
        private void ResetMatrix()
        {
            for (int y = 0; y < MATRIX_WIDTH; y++)
            {
                for (int x = 0; x < MATRIX_WIDTH; x++)
                {
                    outputMatrix[x, y] = false;
                }
            }
        }
        private void RefreshImage()
        {
            for (int y = 0; y < MATRIX_WIDTH; y++)
            {
                for (int x = 0; x < MATRIX_WIDTH; x++)
                {
                    if (outputMatrix[x, y])
                    {
                        matrixImage.SetPixel(x, MATRIX_WIDTH -1 - y, positiveColor);
                    }
                    else
                    {
                        matrixImage.SetPixel(x, MATRIX_WIDTH - 1 - y, negativeColor);
                    }
                }
            }
            
            //ToDo: Faster SetPixel (it's clearly the bottleneck now) - Use the code bellow

            //BitmapData BtmpDt = a.LockBits(new Rectangle(0, 0, btm.Width, btm.Height), ImageLockMode.ReadWrite, btm.PixelFormat);
            //IntPtr pointer = BtmDt.Scan0;
            //int size = Math.Abs(BtmDt.Stride) * btm.Height;
            //byte[] pixels = new byte[size];
            //Marshal.Copy(pointer, pixels, 0, size);
            //for (int b = 0; b < pixels.Length; b++)
            //{
            //    pixels[b] = 255;// do something here 
            //}
            //Marshal.Copy(pixels, 0, pointer, size);
            //btm.UnlockBits(BtmDt);

            outPutPicture.Image = matrixImage;
        }
    }
}