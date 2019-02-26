﻿using DancingTraficLight.Properties;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using System.Linq;

namespace DancingTraficLight
{
    public partial class TraficLight : Form
    {
        // World To Matrix parameters
        const int MATRIX_WIDTH = 64;
        const float matrixUnitInMeter = 0.033f; //0.038f;
        const int verticalOffset = -7;
        const int horizontalOffset = 0;

        // Kinect
        private KinectSensor kinectSensor;
        private BodyFrameReader bodyFrameReader;
        private IList<Body> bodies;

        // Fields
        private bool[,] outputMatrix = new bool[MATRIX_WIDTH , MATRIX_WIDTH];
        private List<Point> line1 = new List<Point>();
        private List<Point> line2 = new List<Point>();
        private List<Point> line3 = new List<Point>();
        private List<Point> line4 = new List<Point>();
        private List<int> borders = new List<int>();
        private Dictionary<JointType, Point> latestJointPositions;

        // FormApp Only
        private Bitmap matrixImage = new Bitmap(MATRIX_WIDTH, MATRIX_WIDTH);
        private Color positiveColor = Color.DarkRed;//Color.FromArgb(255, 225, 0, 0);
        private Color negativeColor = Color.DarkGray;//Color.FromArgb(255, 35, 35, 35);

        public TraficLight()
        {
            InitializeComponent();
            InitializeKinect();

            latestJointPositions = new Dictionary<JointType, Point>();
            foreach (var jointType in Enum.GetNames(typeof(JointType)))
            {
                latestJointPositions.Add((JointType)Enum.Parse(typeof(JointType), jointType), Point.Empty);
            }

            //DrawRectangle(new Point(14, 14), new Point(30, 30), 5);
            //DrawRectangle(new Point(50, 50), new Point(34, 34), 5);

            //DrawRectangle(new Point(27, 5), new Point(5, 27), 5);
            DrawRectangle(new Point(37, 49), new Point(49, 37), 2, 1);

            DrawRectangle(new Point(40, 10), new Point(20, 10), 1, 2);
            DrawRectangle(new Point(40, 20), new Point(20, 20), 2, 1);

            RefreshImage();
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

            // Head
            DrawCircle(joints[JointType.Head].Position, 8);

            // Torso 2.0
            DrawBone(joints, JointType.SpineShoulder, JointType.SpineBase, 12, 8);               // Hip

            //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderLeft, 1);          // Left Shoulder
            //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderRight, 1);         // Right Shoulder

            //Right Arm
            DrawBone(joints, JointType.ShoulderRight, JointType.ElbowRight, 4, 3);           // Right Upper Arm
            DrawBone(joints, JointType.ElbowRight, JointType.WristRight);              // Right Lower Arm
            //DrawBone(joints, JointType.WristRight, JointType.HandRight);                  // Right Hand tip

            // Left Arm
            DrawBone(joints, JointType.ShoulderLeft, JointType.ElbowLeft, 4, 3);             // Left Upper Arm
            DrawBone(joints, JointType.ElbowLeft, JointType.WristLeft);                // Left Lower Arm
            //DrawBone(joints, JointType.WristLeft, JointType.HandLeft);             // Left Hand tip

            // Right Leg
            DrawBone(joints, JointType.HipRight, JointType.KneeRight, 4, 3);                 // Right Upper leg
            DrawBone(joints, JointType.KneeRight, JointType.AnkleRight);               // Right Lower leg
            //DrawBone(joints, JointType.AnkleRight, JointType.FootRight);                  // Right Feet

            // Left Leg
            DrawBone(joints, JointType.HipLeft, JointType.KneeLeft, 4, 3);                   // Left Upper leg
            DrawBone(joints, JointType.KneeLeft, JointType.AnkleLeft);                 // Left Lower leg
            //DrawBone(joints, JointType.AnkleLeft, JointType.FootLeft);                    // Left Feet
        }
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, int joint0_Width = 2)
        {
            DrawBone(joints, jointType0, jointType1, joint0_Width, joint0_Width);
        }
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, int joint0_Width, int joint1_Width)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked || joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }
            
            DrawRectangle(GridPositionFromCameraPoint(joint0.Position), GridPositionFromCameraPoint(joint1.Position), joint0_Width, joint1_Width);
        }

        private void DrawCircle(CameraSpacePoint point, int diameter)
        {
            //ToDo - DrawCircle: rewrite
            Point point2 = GridPositionFromCameraPoint(point);

            int x = point2.X;
            int y = point2.Y;

            x -= diameter / 2;
            y -= 1 + diameter / 2;

            //Jelenleg csak diameter = 8 esetén működik megfelelően
            for ( var i = 0; i < diameter; i++ )
            {
                for ( var j = 0; j < diameter; j++ )
                {
                    if (0 <= x + i && Math.Abs(x + i) < MATRIX_WIDTH && 0 <= y + j && Math.Abs(y + j) < MATRIX_WIDTH)
                    {

                        if( 1 < i && i < 6 || 1 < j && j < 6)
                        {
                            outputMatrix[x + i, y + j] = true;
                        }
                        else if ((i == 1 || i == 6) && (j == 1 || j == 6))
                        {
                            outputMatrix[x + i, y + j] = true;
                        }
                    }
                }
            }
        }

        private void DrawRectangle(Point pointA, Point pointB, int widthA, int widthB)
        {
            Point[] points = CalculateRectangleCorners(pointA, pointB, widthA, widthB);

            CalculateLinePositions(points[0], points[1], ref line1);
            CalculateLinePositions(points[1], points[2], ref line2);
            CalculateLinePositions(points[2], points[3], ref line3);
            CalculateLinePositions(points[3], points[0], ref line4);

            int minY = (from point in points
                      select point.Y).Min();

            int maxY = (from point in points
                       select point.Y).Max();

            for (int y = minY ; y <= maxY; y++)
            {
                borders.Clear();

                AddToBorders(line1, borders, y);
                AddToBorders(line2, borders, y);
                AddToBorders(line3, borders, y);
                AddToBorders(line4, borders, y);

                if (borders.Count != 0)
                {
                    for (int x = borders.Min(); x <= borders.Max(); x++)
                    {
                        SetMatrixValue(x, y);
                    }
                }
            }

        }
        private void AddToBorders(List<Point> listToEvaulate, List<int> listToStore, int y)
        {
            var min = (from point in listToEvaulate where point.Y == y select point.X);
            var max = (from point in listToEvaulate where point.Y == y select point.X);

            if (min.Any())
            {
                
                listToStore.Add(min.Min());
            }
            if (max.Any())
            {
                listToStore.Add(max.Max());
            }
        }
        private void CalculateLinePositions(Point pointA, Point pointB, ref List<Point> positions)
        {
            int x = pointA.X;
            int y = pointA.Y;
            int x2 = pointB.X;
            int y2 = pointB.Y;

            positions.Clear();

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
                positions.Add(new Point(x, y));

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
        private Point[] CalculateRectangleCorners(Point pointA, Point pointB, int widthA, int widthB)
        {
            Vector2 direction = new Vector2(pointB.X - pointA.X, pointB.Y - pointA.Y);
            direction = Vector2.Normalize(direction);

            Vector2 perpendicular = new Vector2(direction.Y, -direction.X);

            
            int widthA_1 = widthA / 2;
            int widthA_2 = widthA - widthA_1;

            int widthB_1 = widthB / 2;
            int widthB_2 = widthB - widthB_1;

            //Console.WriteLine(string.Format("widthA_1: {0}, widthA_2: {1}, widthB_1: {2}, widthB_2: {3}", widthA_1, widthA_2, widthB_1, widthB_2));

            Point pointC = new Point
                (
                    pointA.X + (int)Math.Round(perpendicular.X * widthA_1),
                    pointA.Y + (int)Math.Round(perpendicular.Y * widthA_1)
                );
            Point pointD = new Point
                (
                    pointA.X - (int)Math.Round(perpendicular.X * widthA_2),
                    pointA.Y - (int)Math.Round(perpendicular.Y * widthA_2)
                );
            Point pointE = new Point
                (
                    pointB.X - (int)Math.Round(perpendicular.X * widthB_1),
                    pointB.Y - (int)Math.Round(perpendicular.Y * widthB_1)
                );
            Point pointF = new Point
                (
                    pointB.X + (int)Math.Round(perpendicular.X * widthB_2),
                    pointB.Y + (int)Math.Round(perpendicular.Y * widthB_2)
                );

            return new Point[]{ pointC, pointD, pointE, pointF};
        }

        private Point GridPositionFromCameraPoint(CameraSpacePoint point)
        {
            int x = (int)Math.Floor(point.X / matrixUnitInMeter);
            int y = (int)Math.Floor(point.Y / matrixUnitInMeter);

            x += (MATRIX_WIDTH / 2) + horizontalOffset;
            y += (MATRIX_WIDTH / 2) + verticalOffset;

            if (0 <= x && Math.Abs(x) < MATRIX_WIDTH && 0 <= y && Math.Abs(y) < MATRIX_WIDTH)
            {
                return new Point(x, y);
            }
            else
            {
                return new Point(0, 0);
            }
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

            outPutPicture.Image = matrixImage;
        }

    }
}
