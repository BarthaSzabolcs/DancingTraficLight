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
        public const int MATRIX_SIZE_MULTIPLIER = 1;
        public const int MATRIX_WIDTH = 32 * MATRIX_SIZE_MULTIPLIER;
        public const float MATRIXUNIT_IN_METER = 0.033f * 1.7f / MATRIX_SIZE_MULTIPLIER; //0.038f;
        public const int VERTICAL_OFFSET = -2;
        public const int HORIZONTAL_OFFSET = 0;

        //Drawing
        private Graphics gfx;
        private Pen pen;
        private Bitmap output;

        private byte[,] headMatrix = 
        {
            {0, 0, 1, 1, 0, 0},
            {0, 1, 1, 1, 1, 0},
            {1, 1, 1, 1, 1, 1},
            {1, 1, 1, 1, 1, 1},
            {0, 1, 1, 1, 1, 0},
            {0, 0, 1, 1, 0, 0},
        };

        // Kinect
        private KinectSensor kinectSensor;
        private BodyFrameReader bodyFrameReader;
        private IList<Body> bodies;

        // FormApp Only
        private Bitmap matrixImage = new Bitmap(MATRIX_WIDTH, MATRIX_WIDTH);
        private Color positiveColor = Color.DarkRed;    //Color.FromArgb(255, 225, 0, 0);
        private Color negativeColor = Color.Black;      //Color.FromArgb(255, 35, 35, 35);

        public TraficLight()
        {
            InitializeComponent();
            InitializeKinect();

            output = new Bitmap(MATRIX_WIDTH, MATRIX_WIDTH);
            outPutPicture.Image = output;

            gfx = Graphics.FromImage(output);
            pen = new Pen(positiveColor, 1);
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
            gfx.Clear(negativeColor);
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
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
            outPutPicture.Refresh();
        }

        private void DrawBody(Body body)
        {
            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

            if (MATRIX_WIDTH == 32)
            {
                DrawBody32(joints);
            }
            else if (MATRIX_WIDTH == 64)
            {
                DrawBody64(joints);
            }

        }
        private void DrawBody32(IReadOnlyDictionary<JointType, Joint> joints)
        {
            // Head
            //DrawCircle(joints[JointType.Head].Position.ToGridPosition(), 6);
            DrawFixHead(joints[JointType.Head].Position.ToGridPosition(), headMatrix);

            // Left Shoulder
            DrawBoneWithCircles(joints, JointType.SpineShoulder, JointType.ShoulderLeft, 2, 2);
            // Right Shoulder
            DrawBoneWithCircles(joints, JointType.SpineShoulder, JointType.ShoulderRight, 2, 2);

            // Upper Torso
           DrawBone(joints, JointType.SpineShoulder, JointType.SpineMid, 6);
           DrawBoneWithCircles(joints, JointType.SpineMid, JointType.ShoulderLeft, 2, 2);
           DrawBoneWithCircles(joints, JointType.SpineMid, JointType.ShoulderRight, 2, 2);
            
            // Lower Torso
            DrawBoneWithCircles(joints, JointType.SpineBase, JointType.SpineMid, 6,6);

            //Right Upper Arm
            DrawBoneWithCircles(joints, JointType.ShoulderRight, JointType.ElbowRight, 2, 2);
            //Right Lower Arm
            DrawBoneWithCircles(joints, JointType.ElbowRight, JointType.WristRight, 2, 2);
            //Right Hand
            //DrawBone(joints, JointType.WristRight, JointType.HandRight, 3);

            //Left Upper Arm
            DrawBoneWithCircles(joints, JointType.ShoulderLeft, JointType.ElbowLeft, 2, 2);
            //Left Lower Arm
            DrawBoneWithCircles(joints, JointType.ElbowLeft, JointType.WristLeft, 2, 2);
            //Left Hand
            //DrawBone(joints, JointType.WristLeft, JointType.HandLeft, 3);

            // Hip
            DrawBone(joints, JointType.HipLeft, JointType.HipRight, 4, 4);

            // Right Upper leg
            DrawBoneWithCircles(joints, JointType.HipRight, JointType.KneeRight, 2, 2);
            // Right Lower leg
            DrawBoneWithCircles(joints, JointType.KneeRight, JointType.AnkleRight, 2, 2);
            // Right Feet
            //DrawBone(joints, JointType.AnkleRight, JointType.FootRight, 3);

            // Left Upper leg
            DrawBoneWithCircles(joints, JointType.HipLeft, JointType.KneeLeft, 2, 2);
            // Left Lower leg
            DrawBoneWithCircles(joints, JointType.KneeLeft, JointType.AnkleLeft, 2, 2);
            // Left Feet
            //DrawBone(joints, JointType.AnkleLeft, JointType.FootLeft, 3);
        }
        private void DrawBody64(IReadOnlyDictionary<JointType, Joint> joints)
        {
            // Head
            DrawCircle(joints[JointType.Head].Position.ToGridPosition(), 10);

            // Left Shoulder
            //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderLeft, 2);
            // Right Shoulder
            //DrawBone(joints, JointType.SpineShoulder, JointType.ShoulderRight, 2);         

            // Upper Torso
            DrawBone(joints, JointType.SpineShoulder, JointType.SpineMid, 6, 10);
            // Lower Torso
            DrawBone(joints, JointType.SpineBase, JointType.SpineMid, 6, 10);

            //Right Upper Arm
            DrawBoneWithCircles(joints, JointType.ShoulderRight, JointType.ElbowRight, 3, 3);
            //Right Lower Arm
            DrawBoneWithCircles(joints, JointType.ElbowRight, JointType.WristRight, 3, 2);
            //Right Hand
            //DrawBone(joints, JointType.WristRight, JointType.HandRight, 3);

            //Left Upper Arm
            DrawBoneWithCircles(joints, JointType.ShoulderLeft, JointType.ElbowLeft, 3, 3);
            //Left Lower Arm
            DrawBoneWithCircles(joints, JointType.ElbowLeft, JointType.WristLeft, 3, 2);
            //Left Hand
            //DrawBone(joints, JointType.WristLeft, JointType.HandLeft, 3);

            // Hip
            DrawBone(joints, JointType.HipLeft, JointType.HipRight, 6);

            // Right Upper leg
            DrawBoneWithCircles(joints, JointType.HipRight, JointType.KneeRight, 3, 3);
            // Right Lower leg
            DrawBoneWithCircles(joints, JointType.KneeRight, JointType.AnkleRight, 3, 2);
            // Right Feet
            //DrawBone(joints, JointType.AnkleRight, JointType.FootRight, 3);

            // Left Upper leg
            DrawBoneWithCircles(joints, JointType.HipLeft, JointType.KneeLeft, 3, 3);
            // Left Lower leg
            DrawBoneWithCircles(joints, JointType.KneeLeft, JointType.AnkleLeft, 3, 2);
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

            pen.Width = joint0_width;
            gfx.DrawLine(pen, joint0.Position.ToGridPosition(), joint1.Position.ToGridPosition());
        }
        private void DrawBoneWithCircles(IReadOnlyDictionary<JointType, Joint> joints, JointType jointType0, JointType jointType1, int joint0_width, int joint1_width)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked || joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            DrawBone(joints, jointType0, jointType1, joint0_width, joint1_width);
            DrawCircle(joint0.Position.ToGridPosition(), joint0_width);
            DrawCircle(joint1.Position.ToGridPosition(), joint1_width);
        }

        private void DrawCircle(Point middlePoint, int diameter)
        {
            gfx.FillEllipse(pen.Brush, middlePoint.X - diameter / 2, middlePoint.Y - diameter / 2, diameter, diameter);
        }
        private void DrawFixHead(Point middlePoint, byte[,] headMatrix)
        {
            var corner = new Point(middlePoint.X - headMatrix.GetLength(1) / 2, middlePoint.Y - headMatrix.GetLength(0) / 2);

            for (int y = 0; y < headMatrix.GetLength(0); y++)
            {
                for (int x = 0; x < headMatrix.GetLength(1); x++)
                {
                    if (headMatrix[y, x] == 1)
                    {
                        gfx.FillRectangle(pen.Brush, corner.X + x, corner.Y + y, 1, 1);
                    }
                }
            }
        }
    }
}