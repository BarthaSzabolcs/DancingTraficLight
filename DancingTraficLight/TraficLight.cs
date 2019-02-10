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
        const float matrixUnitWidthInMeter = 0.033f;//0.038f;
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
           // DrawBone(joints, JointType.Neck, JointType.SpineShoulder, NECK_WIDTH);                    // Neck
            DrawBone(joints, JointType.SpineShoulder, JointType.SpineMid, UPPER_TORSO_WIDTH);           // Chest

            DrawBone(joints, JointType.SpineMid, JointType.SpineBase, LOWER_TORSO_WIDTH);               // Hip
            DrawBone(joints, JointType.SpineMid, JointType.ShoulderLeft, PLUS_BONE_WIDTH);
            DrawBone(joints, JointType.SpineMid, JointType.ShoulderRight, PLUS_BONE_WIDTH);
            DrawBone(joints, JointType.ShoulderLeft, JointType.ShoulderRight, PLUS_BONE_WIDTH);

            DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderLeft, SHOULDER_WITDH);          // Left Shoulder
            DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderRight, SHOULDER_WITDH);         // Right Shoulder

            // Right Arm
            DrawBone(joints, JointType.ShoulderRight, JointType.ElbowRight, UPPER_ARM_WIDTH);           // Right Upper Arm
            DrawBone(joints, JointType.ElbowRight, JointType.WristRight, LOWER_ARM_WIDTH);              // Right Lower Arm
            //DrawBone(joints, JointType.WristRight, JointType.HandRight, HAND_WIDTH);                  // Right Hand tip

            // Left Arm
            DrawBone(joints, JointType.ShoulderLeft, JointType.ElbowLeft, UPPER_ARM_WIDTH);             // Left Upper Arm
            DrawBone(joints, JointType.ElbowLeft, JointType.WristLeft, LOWER_ARM_WIDTH);                // Left Lower Arm
            //DrawBone(joints, JointType.WristLeft, JointType.HandLeft, HAND_WIDTH);                    // Left Hand tip

            // Right Leg
            DrawBone(joints, JointType.HipRight, JointType.KneeRight, UPPER_LEG_WIDTH);                 // Right Upper leg
            DrawBone(joints, JointType.KneeRight, JointType.AnkleRight, LOWER_LEG_WIDTH);               // Right Lower leg
            //DrawBone(joints, JointType.AnkleRight, JointType.FootRight, FEET_WIDTH);                  // Right Feet

            // Left Leg
            DrawBone(joints, JointType.HipLeft, JointType.KneeLeft, UPPER_LEG_WIDTH);                   // Left Upper leg
            DrawBone(joints, JointType.KneeLeft, JointType.AnkleLeft, LOWER_LEG_WIDTH);                 // Left Lower leg
            //DrawBone(joints, JointType.AnkleLeft, JointType.FootLeft, FEET_WIDTH);                    // Left Feet
        }

        /// <summary>
        /// Két izület között rajzol vonalat, adott távolságonként téglalapot rajzolva (a = szélesség, b = magasság).
        /// </summary>
        /// <param name="joints"></param>
        /// <param name="jointType0"></param>
        /// <param name="jointType1"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, float a, float b = 0)
        {
            if( b == 0)
            {
                b = a;
            }
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                DrawLine(joint0.Position, joint1.Position, a, b);
            }
        }
        
        /// <summary>
        /// Két pont között rajzol vonalat, adott távolságonként téglalapot rajzolva (a = szélesség, b = magasság).
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void DrawLine(CameraSpacePoint first, CameraSpacePoint second, float a, float b = 0)
        {
            if (b == 0)
            {
                b = a;
            }
            float distanceX = second.X - first.X;
            float distanceY = second.Y - first.Y;

            float distance = (float)Math.Sqrt((first.X - second.X) * (first.X - second.X) + (first.Y - second.Y) * (first.Y - second.Y));
            int count = (int)Math.Round(distance / matrixUnitWidthInMeter);

            for (int i = 0; i < count; i++)
            {
                point_1.X = first.X + (float)i / count * distanceX;
                point_1.Y = first.Y + (float)i / count * distanceY;

                Matrix_DrawRectangle(point_1, a, b);
            }
        }
        
        /// <summary>
        /// Kört rajzol az adott pont köré, jelenleg csak 8-as diameter esetén működik megfelelően.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="diameter"></param>
        private void DrawCircle(CameraSpacePoint point, int diameter)
        {
            //int x = (int)Math.Round(point.X / matrixUnitInMeter);
            //int y = (int)Math.Round(point.Y / matrixUnitInMeter);
            int x = (int)Math.Floor(point.X / matrixUnitWidthInMeter);
            int y = (int)Math.Floor(point.Y / matrixUnitWidthInMeter);

            x += (MATRIX_WIDTH / 2) + horizontalOffset - diameter / 2;
            y += (MATRIX_WIDTH / 2) + verticalOffset - diameter / 2;

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
        /// <summary>
        /// Téglalapot rajzol egy adott pont köré a Matrix_DrawPoint fg-t használva, ahol "a" a téglalap hossza és "b" a magassága.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void Matrix_DrawRectangle(CameraSpacePoint point, float a, float b = 0.0f)
        {
            if(b == 0)
            {
                b = a;
            }
            point_1.X = point.X - a / 2;
            point_1.Y = point.Y - b / 2;

            point_2.X = point.X + a / 2;
            point_2.Y = point.Y + b / 2;

            float smallerX = point_1.X < point_2.X ? point_1.X : point_2.X;
            float smallerY = point_1.Y < point_2.Y ? point_1.Y : point_2.Y;

            int distanceX = (int)(Math.Abs(point_1.X - point_2.X) / matrixUnitWidthInMeter);
            int distanceY = (int)(Math.Abs(point_1.Y - point_2.Y) / matrixUnitWidthInMeter);

            for (int x = 0; x < distanceX; x++)
            {
                for (int y = 0; y < distanceY; y++)
                {
                    point_1.X = smallerX + x * matrixUnitWidthInMeter;
                    point_1.Y = smallerY + y * matrixUnitWidthInMeter;
                    Matrix_DrawPoint(point_1);
                }
            }

        }
        
        /// <summary>
        /// A kapott pont alapján a megfelelő mátrix értéket igazra állítja, ha a pont kilóg a matrixból nem tesz semmit.
        /// </summary>
        /// <param name="point"></param>
        private void Matrix_DrawPoint(CameraSpacePoint point)
        {

            //int x = (int)Math.Round(point.X / matrixUnitInMeter);
            //int y = (int)Math.Round(point.Y / matrixUnitInMeter);
            int x = (int)Math.Floor(point.X / matrixUnitWidthInMeter);
            int y = (int)Math.Floor(point.Y / matrixUnitWidthInMeter);

            x += (MATRIX_WIDTH / 2) + horizontalOffset;
            y += (MATRIX_WIDTH / 2) + verticalOffset;

            if (0 <= x && Math.Abs(x) < MATRIX_WIDTH && 0 <= y && Math.Abs(y) < MATRIX_WIDTH)
            {
                outputMatrix[x, y] = true;
            }
        }
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
