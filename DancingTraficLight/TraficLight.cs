using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DancingTraficLight
{
    public partial class TraficLight : Form
    {
        #region Kinect

        private KinectSensor kinectSensor;
        private BodyFrameReader bodyFrameReader;
        private IList<Body> bodies;
        
        #endregion
        #region Output parameters

        const int MATRIX_WIDTH = 64;
        bool[,] outputMatrix = new bool[MATRIX_WIDTH, MATRIX_WIDTH];
        const float matrixUnitInMeter = /*0.033f;//*/0.038f;
        const int verticalOffset = -7;
        const int horizontalOffset = 0;
        
        #endregion
        #region FormApplication output parameters

        Bitmap matrixImage = new Bitmap(MATRIX_WIDTH, MATRIX_WIDTH);
        Color positiveColor = Color.FromArgb(255, 200, 0, 0);
        Color negativeColor = Color.FromArgb(255, 0, 0, 0);
        
        #endregion
        #region Body patameters
        //Leg
        const float FEET_WIDTH          = 0.1250f;
        const float LOWER_LEG_WIDTH     = 0.1300f;
        const float UPPER_LEG_WIDTH     = 0.1300f;

        //Arm
        const float HAND_TIP_WIDTH      = 0.0750f;
        const float HAND_WIDTH          = 0.1250f;
        const float LOWER_ARM_WIDTH     = 0.1000f;
        const float UPPER_ARM_WIDTH     = 0.1000f;

        //Torso
        const int   HEAD_DIAMETER       = 8;        //jelenleg az egyetlen helyes érték
        const float NECK_WIDTH          = 0.1250f;
        const float SHOULDER_WITDH      = 0.1000f;
        const float PLUS_BONE_WIDTH     = 0.1250f;
        const float UPPER_TORSO_WIDTH   = 0.1500f;
        const float LOWER_TORSO_WIDTH   = 0.250f;
        const float HIP_WIDTH           = 0.1250f;

        #endregion
        #region Helper variables

        CameraSpacePoint point_1 = new CameraSpacePoint();
        CameraSpacePoint point_2 = new CameraSpacePoint();
        
        #endregion

        public TraficLight()
        {
            InitializeComponent();
            InitializeKinect();
        }

        #region Kinect Handling
        private void InitializeKinect()
        {
            kinectSensor = KinectSensor.GetDefault();
            kinectSensor.Open();

            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += HandleFrameArrived;
        }

        /// <summary>
        /// Az új képkocka adatait fogadó függvény.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #endregion
        #region Advanced draw functions

        /// <summary>
        /// Az egész test kirajzolásáért felelős függvény.
        /// </summary>
        /// <param name="body"></param>
        private void DrawBody(Body body)
        {
            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

            // Head
            DrawCircle(joints[JointType.Head].Position, HEAD_DIAMETER);

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

        /// <summary>Két izület között rajzol vonalat, adott távolságonként téglalapot rajzolva (a = szélesség, b = magasság).</summary>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, int lineWidth = 2)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }


            Point pointA = GridPositionCameraPoint(joint0.Position);
            Point pointB = GridPositionCameraPoint(joint1.Position);

            DrawLineWithWidth(pointA.X, pointA.Y, pointB.X, pointB.Y, lineWidth);
        }
        
        /// <summary>
        /// Kört rajzol az adott pont köré, jelenleg csak 8-as diameter esetén működik megfelelően.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="diameter"></param>
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

        #endregion
        #region Basic draw functions
        
        // New Shit
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

        public void DrawLineWithWidth(int x, int y, int x2, int y2, int lineWidth)
        {
            DrawLine(x, y, x2, y2, lineWidth);
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
            for (int i = x - width /2; i < x + width /2; i++)
            {
                for (int j = y - width /2; j < y + width /2; j++)
                {
                    if (0 <= i && i < MATRIX_WIDTH && 0 <= j && j < MATRIX_WIDTH)
                    {
                        outputMatrix[i, j] = true;
                    }
                }
            }
        }

        //private float CalculateAngle(int x, int y, int x2, int y2)
        //{
        //    return Math.Abs((float)(Math.Atan2(y2 - y, x2 - x) * (180.0 / Math.PI)));
        //}

        #endregion
        #region FormApp functions

        /// <summary>
        /// Lenullázza a mátrix értékeit.
        /// </summary>
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
        
        /// <summary>
        /// Frissíti a Form-on megjelenített képet.
        /// </summary>
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
        
        #endregion
    }

}
