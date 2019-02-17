using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

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
        const int MATRIX_WIDTH = 64;
        bool[,] outputMatrix = new bool[MATRIX_WIDTH, MATRIX_WIDTH];
        const float matrixUnitInMeter = /*0.033f;//*/0.038f;
        const int verticalOffset = -7;
        const int horizontalOffset = 0;
        
        // FormApp needs it
        Bitmap matrixImage = new Bitmap(MATRIX_WIDTH, MATRIX_WIDTH);
        Color positiveColor = Color.FromArgb(255, 200, 0, 0);
        Color negativeColor = Color.FromArgb(255, 0, 0, 0);

        public TraficLight()
        {
            InitializeComponent();
            InitializeKinect();

            latestJointPositions = new Dictionary<JointType, Point>();
            foreach (var jointType in Enum.GetNames(typeof(JointType)))
            {
                latestJointPositions.Add((JointType)Enum.Parse(typeof(JointType), jointType), Point.Empty);
            }
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

           // Torso
            //DrawBone(joints, JointType.Neck, JointType.SpineShoulder);                    // Neck
            DrawBone(joints, JointType.SpineShoulder, JointType.SpineMid, 4);           // Chest

            DrawBone(joints, JointType.SpineMid, JointType.SpineBase, 6);               // Hip
            
            DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderLeft);          // Left Shoulder
            DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderRight);         // Right Shoulder
            DrawBone(joints, JointType.SpineMid, JointType.ShoulderRight, 2);
            DrawBone(joints, JointType.SpineMid, JointType.ShoulderLeft, 2);
            DrawBone(joints, JointType.ShoulderLeft, JointType.ShoulderRight);

            //DrawBone(joints, JointType.HipRight, JointType.ShoulderRight);
            //DrawBone(joints, JointType.HipLeft, JointType.ShoulderLeft);

            // Right Arm
            DrawBone(joints, JointType.ShoulderRight, JointType.ElbowRight);           // Right Upper Arm
            DrawBone(joints, JointType.ElbowRight, JointType.WristRight);              // Right Lower Arm
            DrawBone(joints, JointType.WristRight, JointType.HandRight);                  // Right Hand tip

            // Left Arm
            DrawBone(joints, JointType.ShoulderLeft, JointType.ElbowLeft);             // Left Upper Arm
            DrawBone(joints, JointType.ElbowLeft, JointType.WristLeft);                // Left Lower Arm
            DrawBone(joints, JointType.WristLeft, JointType.HandLeft);                    // Left Hand tip

            // Right Leg
            DrawBone(joints, JointType.HipRight, JointType.KneeRight);                 // Right Upper leg
            DrawBone(joints, JointType.KneeRight, JointType.AnkleRight);               // Right Lower leg
            DrawBone(joints, JointType.AnkleRight, JointType.FootRight);                  // Right Feet

            // Left Leg
            DrawBone(joints, JointType.HipLeft, JointType.KneeLeft);                   // Left Upper leg
            DrawBone(joints, JointType.KneeLeft, JointType.AnkleLeft);                 // Left Lower leg
            DrawBone(joints, JointType.AnkleLeft, JointType.FootLeft);                    // Left Feet
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

            if (joint0.TrackingState == TrackingState.Tracked)
            {
                latestJointPositions[jointType0] = GridPositionCameraPoint(joint0.Position);
            }
            if (joint1.TrackingState == TrackingState.Tracked)
            {
                latestJointPositions[jointType1] = GridPositionCameraPoint(joint1.Position);
            }

            Point pointA = latestJointPositions[jointType0];
            Point pointB = latestJointPositions[jointType1];

            DrawLine(pointA.X, pointA.Y, pointB.X, pointB.Y, lineWidth);
        }

        public void DrawLine(int x, int y, int x2, int y2, int lineWidth)
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
                DrawBox(x, y, lineWidth);
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
        public void DrawBox(int x, int y, int width)
        {
            for (int i = x - width / 2; i < x + width / 2; i++)
            {
                for (int j = y - width / 2; j < y + width / 2; j++)
                {
                    if (0 <= i && i < MATRIX_WIDTH && 0 <= j && j < MATRIX_WIDTH)
                    {
                        outputMatrix[i, j] = true;
                    }
                }
            }
        }
        private void DrawCircle(CameraSpacePoint point, int diameter)
        {
            Point point2 = GridPositionCameraPoint(point);

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
        private Point GridPositionCameraPoint(CameraSpacePoint point)
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
            outPutPicture.Image = matrixImage;
        }
        
    }

}
