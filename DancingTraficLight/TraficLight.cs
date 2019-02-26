using DancingTraficLight.Properties;
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
        // Kinect
        private KinectSensor kinectSensor;
        private BodyFrameReader bodyFrameReader;
        private IList<Body> bodies;
        private Dictionary<JointType, Point> latestJointPositions;

        // World To Matrix parameters
        const int SCALE_FACTOR = 16;
        const int MATRIX_WIDTH = 64;
        bool[,] outputMatrix = new bool[MATRIX_WIDTH , MATRIX_WIDTH];
        const float matrixUnitInMeter = /*0.033f;//*/0.038f;
        const int verticalOffset = -7;
        const int horizontalOffset = 0;

        // Chached
        List<Point> relativePositions = new List<Point>();

        // FormApp needs it
        Bitmap matrixImage = new Bitmap(MATRIX_WIDTH, MATRIX_WIDTH);
        Color positiveColor = Color.DarkGoldenrod;//Color.FromArgb(255, 225, 0, 0);
        Color negativeColor = Color.FromArgb(255, 35, 35, 35);

        public TraficLight()
        {
            InitializeComponent();
            InitializeKinect();

            //outPutPicture.SizeMode = PictureBoxSizeMode.CenterImage;

            latestJointPositions = new Dictionary<JointType, Point>();
            foreach (var jointType in Enum.GetNames(typeof(JointType)))
            {
                latestJointPositions.Add((JointType)Enum.Parse(typeof(JointType), jointType), Point.Empty);
            }

            //// ToDo - Remove test
            //int test1X = 20;
            //int test1Y = 10;
            //int test2X = 10;
            //int test2Y = 20;

            ////DrawLine(test1X, test1Y, test2X, test2Y, 2);
            ////DrawRectangle(new Point(test1X, test1Y), new Point(test2X, test2Y), 10);
            //DrawRectangle(new Point(14, 14), new Point(30, 30), 5);
            //DrawRectangle(new Point(50, 50), new Point(34, 34), 5);

            //DrawRectangle(new Point(27, 5), new Point(5, 27), 5);
            //DrawRectangle(new Point(37, 49), new Point(49, 37), 5);

            //DrawRectangle(new Point(40, 10), new Point(20, 10), 5);

            //DrawRectangle(new Point(16, 0), new Point(32, 16), new Point(16, 32), new Point(0, 16));
            DrawRectangle(new Point(60, 20), new Point(15, 12), new Point(50, 20), new Point(1, 8));

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
            //DrawBone(joints, JointType.SpineMid, JointType.ShoulderRight);
            //DrawBone(joints, JointType.SpineMid, JointType.ShoulderLeft);
            //DrawBone(joints, JointType.ShoulderLeft, JointType.ShoulderRight);
            //DrawBone(joints, JointType.HipRight, JointType.ShoulderRight, 3);
            //DrawBone(joints, JointType.HipLeft, JointType.ShoulderLeft, );

            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

            // Head
            DrawCircle(joints[JointType.Head].Position, 8);

            // Torso
            //DrawBone(joints, JointType.Neck, JointType.SpineShoulder, 4);                    // Neck
            DrawBone(joints, JointType.SpineShoulder, JointType.SpineMid, 8);           // Chest
            DrawBone(joints, JointType.SpineMid, JointType.SpineBase, 6);               // Hip

            DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderLeft, 2);          // Left Shoulder
            DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderRight, 2);         // Right Shoulder


            //Right Arm
            DrawBone(joints, JointType.ShoulderRight, JointType.ElbowRight);           // Right Upper Arm
            DrawBone(joints, JointType.ElbowRight, JointType.WristRight);              // Right Lower Arm
            DrawBone(joints, JointType.WristRight, JointType.HandRight);                  // Right Hand tip

            // Left Arm
            DrawBone(joints, JointType.ShoulderLeft, JointType.ElbowLeft);             // Left Upper Arm
            DrawBone(joints, JointType.ElbowLeft, JointType.WristLeft);                // Left Lower Arm
            DrawBone(joints, JointType.WristLeft, JointType.HandLeft);             // Left Hand tip

            // Right Leg
            DrawBone(joints, JointType.HipRight, JointType.KneeRight);                 // Right Upper leg
            DrawBone(joints, JointType.KneeRight, JointType.AnkleRight);               // Right Lower leg
            //DrawBone(joints, JointType.AnkleRight, JointType.FootRight);                  // Right Feet

            // Left Leg
            DrawBone(joints, JointType.HipLeft, JointType.KneeLeft);                   // Left Upper leg
            DrawBone(joints, JointType.KneeLeft, JointType.AnkleLeft);                 // Left Lower leg
            //DrawBone(joints, JointType.AnkleLeft, JointType.FootLeft);                    // Left Feet
        }
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, int lineWidth = 2)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked || joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            //if (joint0.TrackingState == TrackingState.Tracked)
            //{
                latestJointPositions[jointType0] = GridPositionFromCameraPoint(joint0.Position);
            //}
            //if (joint1.TrackingState == TrackingState.Tracked)
            //{
                latestJointPositions[jointType1] = GridPositionFromCameraPoint(joint1.Position);
            //}

            Point pointA = latestJointPositions[jointType0];
            Point pointB = latestJointPositions[jointType1];

            //DrawLine(pointA.X, pointA.Y, pointB.X, pointB.Y, lineWidth);

            if (pointA.X < pointB.X)
            {
                DrawRectangle(pointA, pointB, lineWidth);
            }
            else
            {
                DrawRectangle(pointB, pointA, lineWidth);
            }
        }
        private void DrawRectangle(Point pointA, Point pointB, int width)
        {
            Vector2 direction = new Vector2(pointB.X - pointA.X, pointB.Y - pointA.Y);
            direction = Vector2.Normalize(direction);

            Vector2 perpendicular = new Vector2(direction.Y, -direction.X);
            var angle = Math.Atan2(perpendicular.Y, perpendicular.X);
            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }
            angle = angle * 180 / Math.PI;

            if ((20d < angle && angle < 70d) || (115d < angle && angle < 160d) ||(200d < angle && angle < 250d) || (290d < angle && angle < 340d))
            {
                width = (int)Math.Ceiling(width * 0.708f);
            }
            int firstWidth = width / 2;
            int secondWidth = width - firstWidth;

            Point pointC = new Point
                (
                    pointA.X + (int)Math.Round(perpendicular.X * firstWidth), 
                    pointA.Y + (int)Math.Round(perpendicular.Y * secondWidth)
                );
            Point pointD = new Point
                (
                    pointA.X - (int)Math.Round(perpendicular.X * firstWidth),
                    pointA.Y - (int)Math.Round(perpendicular.Y * secondWidth)
                );

            CalculateRelativePositions(pointC.X, pointC.Y, pointD.X, pointD.Y);

            int yPlus = 0;

            if (22.5d < angle && angle < 67.5d)
            {
                // In Correct
                yPlus = -1;
                //DrawLine(pointA.X, pointA.Y, pointB.X, pointB.Y, yPlus);
            }
            else if (112.5 < angle && angle < 157.5d)
            {
                // Correct
                yPlus = -1;
                //DrawLine(pointA.X, pointA.Y, pointB.X, pointB.Y, yPlus);

            }
            else if (202.5d < angle && angle < 247.5d)
            {
                // InCorrect
                yPlus = 1;
                //DrawLine(pointA.X, pointA.Y, pointB.X, pointB.Y, yPlus);
            }
            else if (292.5 < angle && angle < 337.5d)
            {
                // Correct
                yPlus = 1;
                //DrawLine(pointA.X, pointA.Y, pointB.X, pointB.Y, yPlus);
            }

            DrawLine(pointA.X, pointA.Y, pointB.X, pointB.Y, yPlus);

        }
        private void CalculateRelativePositions(int x, int y, int x2, int y2)
        {
            relativePositions.Clear();

            int original_x = (x + x2) / 2;
            int original_y = (y + y2) / 2;

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
                //DrawBox(x, y, lineWidth);
                relativePositions.Add(new Point(x - original_x, y - original_y));

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
        public void DrawLine(int x, int y, int x2, int y2, int yPlus)
        {
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
                //foreach (Point point in relativePositions)
                //{
                //    SetMatrixValue(x + point.X, y + point.Y);
                //    if (yPlus != 0)
                //    {
                //        SetMatrixValue(x + point.X, y + point.Y + yPlus);
                //    }
                //}

                for (int j = 0; j < relativePositions.Count; j++)
                {
                    SetMatrixValue(x + relativePositions[j].X, y + relativePositions[j].Y);
                    if ((yPlus > 0 && j != relativePositions.Count - 1) || (yPlus < 0 && j != relativePositions.Count - 1))
                    {
                        SetMatrixValue(x + relativePositions[j].X, y + relativePositions[j].Y + yPlus);
                    }
                }

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
        private void SetMatrixValue(int x, int y, bool value = true)
        {
            if (0 <= x && x < MATRIX_WIDTH && 0 <= y && y < MATRIX_WIDTH)
            {
                outputMatrix[x, y] = value;
            }
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
            //Bitmap temp = new Bitmap(matrixImage, new Size(MATRIX_WIDTH * SCALE_FACTOR, MATRIX_WIDTH * SCALE_FACTOR));
            //outPutPicture.Image = Overlap(temp, Resources._64x64);
            outPutPicture.Image = matrixImage;
        }

        private void DrawCircle(CameraSpacePoint point, int diameter)
        {
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

        public static Image Overlap(Image source1, Image source2)
        {
            var target = new Bitmap(source1.Width, source1.Height);
            var graphics = Graphics.FromImage(target);

            graphics.DrawImage(source1, 0, 0);
            graphics.DrawImage(source2, 0, 0);

            return target;
        }

        // Fresh code
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

        List<Point> line1 = new List<Point>();
        List<Point> line2 = new List<Point>();
        List<Point> line3 = new List<Point>();
        List<Point> line4 = new List<Point>();
        List<int> borders = new List<int>();

        private void DrawRectangle(Point pointA, Point pointB, Point pointC, Point pointD)
        {
            CalculateLinePositions(pointA, pointB, ref line1);
            CalculateLinePositions(pointB, pointC, ref line2);
            CalculateLinePositions(pointC, pointD, ref line3);
            CalculateLinePositions(pointD, pointA, ref line4);

            for (int y = 0; y < 64; y++)
            {
                borders.Clear();

                AddToBorders(line1, borders, y);
                AddToBorders(line2, borders, y);
                AddToBorders(line3, borders, y);
                AddToBorders(line4, borders, y);

                if (borders.Count != 0)
                {
                    int min = borders.Min();
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
    }

}
