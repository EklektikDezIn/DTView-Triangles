using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;   // For BitmMapData type
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using ShadowEngine;

namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {
        bool loaded = false;
        float[] rotAngle = new float[5] { 0.0f, 20.0f, 40.0f, 60.0f, 80.0f };
        int[][] rotAxis = new int[5][];
        int[] cubeSize = new int[5];
        float[] angleInc = new float[5];
        int[] texture = new int[6];
        int[] axis = new int[10] { -1, 1, -1, 1, -1, 1, -1, 1, -1, 1 };
        int[] shape = new int[6];

        Camera cam = new Camera();

        public Form1()
        {
            InitializeComponent();
        }


        //
        // OnLoad
        //  - Game window initialisation
        //

        private void glControl1_Load(object sender, EventArgs e)
        {
            loaded = true; // Setup OpenGL capabilities
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);

            // Grab textures
            texture[0] = UploadTexture("C:\\Users\\Mishko\\Desktop\\DTView-Triangles\\WindowsFormsApplication4\\Resources\\sol.jpg");
            texture[1] = UploadTexture("C:\\Users\\Mishko\\Desktop\\DTView-Triangles\\WindowsFormsApplication4\\Resources\\jupiter.jpg");
            texture[2] = UploadTexture("C:\\Users\\Mishko\\Desktop\\DTView-Triangles\\WindowsFormsApplication4\\Resources\\marte.jpg");
            texture[3] = UploadTexture("C:\\Users\\Mishko\\Desktop\\DTView-Triangles\\WindowsFormsApplication4\\Resources\\saturno.bmp");
            texture[4] = UploadTexture("C:\\Users\\Mishko\\Desktop\\DTView-Triangles\\WindowsFormsApplication4\\Resources\\venus.jpg");
            texture[5] = UploadTexture("C:\\Users\\Mishko\\Desktop\\DTView-Triangles\\WindowsFormsApplication4\\Resources\\urano.jpg");
            // Set cube sizes and rotations
            Random random = new Random();
            int i;

            for (i = 0; i < 5; i++)
            {
                cubeSize[i] = random.Next(0, 25);
                angleInc[i] = 4.1f * (float)(random.Next(0, 100) / 100.0f);

                rotAxis[i] = new int[3];
                rotAxis[i][0] = axis[random.Next(0, 9)];
                rotAxis[i][1] = axis[random.Next(0, 9)];
                rotAxis[i][2] = axis[random.Next(0, 9)];

                shape[i] = random.Next(0, 2);
            }

            // Setup background colour
            GL.ClearColor(Color.Black);
            UpdateImage();
            timer1.Start();
        }

        //
        // OnRenderFrame
        //  - Draw a single 3D frame
        //
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void Render()
        {
            // Clear the screen
            if (!loaded) { return; }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Initialise the model view matrix
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Draw the scene
            DrawScene();
            // Display the new frame
            glControl1.SwapBuffers();

        }

        //
        // DrawScene
        //  - Draws our game world
        //
        private void DrawScene()
        {           
            //DrawCube(index,x,y,z,size,rotInc)
            int j = 0;
            for (int i = -5; i < 5; i+=2)
            {
                switch(shape[j])
                {
                    case 0: DrawCube(j, 20 * i, 10 * i, 0, cubeSize[j], rotAxis[j], angleInc[j], texture);
                            break;

                    case 1: DrawPyramid(j, 20 * i, 10 * i, -200, cubeSize[j], rotAxis[j], angleInc[j], texture);
                            break;
                }
                j++;
            }
            
        }


        static public int UploadTexture(string pathname)
        {
            // Create a new OpenGL texture object
            int id = GL.GenTexture();

            // Select the new texture
            GL.BindTexture(TextureTarget.Texture2D, id);

            // Load the image
            Bitmap bmp = new Bitmap(pathname);

            // Lock image data to allow direct access
            BitmapData bmp_data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Import the image data into the OpenGL texture
            GL.TexImage2D(TextureTarget.Texture2D,
                          0,
                          PixelInternalFormat.Rgba,
                          bmp_data.Width,
                          bmp_data.Height,
                          0,
                          OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                          OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,
                          bmp_data.Scan0);

            // Unlock the image data
            bmp.UnlockBits(bmp_data);

            // Configure minification and magnification filters
            GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);

            // Return the OpenGL object ID for use
            return id;
        }

        private void DrawPyramid(int index, float x, float y, float z, float size, int[] rotAxis, float rotInc, int[] tRef)
        {
            rotAngle[index] += rotInc;

            GL.PushMatrix();
            GL.Translate(x, y, z);
            GL.Rotate(-rotAngle[index], rotAxis[0], rotAxis[1], rotAxis[2]);

            //Front
            GL.BindTexture(TextureTarget.Texture2D, tRef[0]);
            GL.Begin(PrimitiveType.Triangles);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(0, size / 2, 0);
            GL.End();

            //Right
            GL.BindTexture(TextureTarget.Texture2D, tRef[5]);
            GL.Begin(PrimitiveType.Triangles);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(size / 2, -size / 2, -size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(0, size / 2, 0);
            GL.End();

            //Left
            GL.BindTexture(TextureTarget.Texture2D, tRef[3]);
            GL.Begin(PrimitiveType.Triangles);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, -size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(0, size / 2, 0);
            GL.End();

            //Back
            GL.BindTexture(TextureTarget.Texture2D, tRef[2]);
            GL.Begin(PrimitiveType.Triangles);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(size / 2, -size / 2, -size / 2);     // back right botton
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, -size / 2);    // back left bottom      
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(0, size / 2, 0);                     // back left top
            GL.End();

            //Botttom
            GL.BindTexture(TextureTarget.Texture2D, tRef[1]);
            GL.Begin(PrimitiveType.Quads);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, -size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(size / 2, -size / 2, -size / 2);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(size / 2, -size / 2, size / 2);
            GL.End();

            GL.PopMatrix();
        }

        private void DrawCube(int index, float x, float y, float z, float size, int[] rotAxis, float rotInc, int[] tRef)
        {
            rotAngle[index] += rotInc;

            GL.PushMatrix();
            GL.Translate(x, y, z);
            GL.Rotate(-rotAngle[index], rotAxis[0], rotAxis[1], rotAxis[2]);


            //Front
            GL.BindTexture(TextureTarget.Texture2D, tRef[0]);
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(size / 2, size / 2, size / 2);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-size / 2, size / 2, size / 2);
            GL.End();


            //Right
            GL.BindTexture(TextureTarget.Texture2D, tRef[1]);
            GL.Begin(PrimitiveType.Quads);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(size / 2, -size / 2, -size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(size / 2, size / 2, -size / 2);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(size / 2, size / 2, size / 2);
            GL.End();

            //Botttom
            GL.BindTexture(TextureTarget.Texture2D, tRef[2]);
            GL.Begin(PrimitiveType.Quads);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, -size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(size / 2, -size / 2, -size / 2);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(size / 2, -size / 2, size / 2);
            GL.End();

            //Left
            GL.BindTexture(TextureTarget.Texture2D, tRef[3]);
            GL.Begin(PrimitiveType.Quads);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, -size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-size / 2, size / 2, size / 2);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-size / 2, size / 2, -size / 2);
            GL.End();

            //Top
            GL.BindTexture(TextureTarget.Texture2D, tRef[4]);
            GL.Begin(PrimitiveType.Quads);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-size / 2, size / 2, size / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(size / 2, size / 2, size / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(size / 2, size / 2, -size / 2);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-size / 2, size / 2, -size / 2);
            GL.End();

            //Back
            GL.BindTexture(TextureTarget.Texture2D, tRef[5]);
            GL.Begin(PrimitiveType.Quads);
            //GL.Color3(Color.Red);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(size / 2, -size / 2, -size / 2);     // back right botton
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-size / 2, -size / 2, -size / 2);    // back left bottom      
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-size / 2, size / 2, -size / 2);     // back left top
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(size / 2, size / 2, -size / 2);      // back right top
            GL.End();

            GL.PopMatrix();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            UpdateImage();
        }


        private void UpdateImage()
        {

            if (!loaded) { return; }
            int w = Width;
            int h = Height;
            float aspect = 1;


          

            // Calculate aspect ratio, checking for divide by zero
            if (h > 0)
            {
                aspect = (float)w / (float)h;
            }

            // Initialise the projection view matrix
            GL.MatrixMode(MatrixMode.Projection);

            //Manipulates stretching of x,y,
            //if (w <= h)
            //{
            //GL.Ortho(-aspect, aspect, -aspect, aspect, 1, 4000);
            ////    GL.Ortho(-1.5, 1.5, -1.5 * (double)h / (double)w, 1.5 * (double)h / (double)w, 1.0, 4000.0);
            //}
            //else
            //{

            //GL.Ortho(-aspect, aspect, -aspect, aspect, -1, 4000);
            ////    GL.Ortho(-1.5 * (double)w / (double)h, 1.5 * (double)w / (double)h, -1.5, 1.5, -1, 4000.0);
            //}
            // GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Setup a perspective view
            float FOVradians = MathHelper.DegreesToRadians(zoom);
            Matrix4 perspective = cam.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(FOVradians, aspect, 1.0f, 4000.0f);
            GL.MultMatrix(ref perspective);

            // Set the viewport to the whole window
            GL.Viewport(0, 0, w, h);


        }
        
        //int max = 4000;
        //int min = 1;


        private void timer1_Tick(object sender, EventArgs e)
        {
            Render();
            Rotate();
        }
        int zoom = 45;




        OpenTK.Vector2 lastMousePos = new OpenTK.Vector2();
        void ResetCursor()
        {
            OpenTK.Input.Mouse.SetPosition(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
            lastMousePos = new OpenTK.Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        private void Form1_Enter(object sender, EventArgs e)
        {
            ResetCursor();
        }

        private void glControl1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {

            base.OnKeyPress(e);

            if (e.KeyChar == 27)
            {
                this.Close();

            }

            switch (e.KeyChar)
            {
                case 'w':
                    cam.Move(0f, 10.5f, 0f);
                    UpdateImage();
                    break;
                case 'a':
                    cam.Move(-10.5f, 0f, 0f);
                    UpdateImage();
                    break;
                case 's':
                    cam.Move(0f, -10.5f, 0f);
                    UpdateImage();
                    break;
                case 'd':
                    cam.Move(10.5f, 0f, 0f);
                    UpdateImage();
                    break;
                case 'q':
                    cam.Move(0f, 0f, 10.5f);
                    UpdateImage();
                    break;
                case 'e':
                    cam.Move(0f, 0f, -10.5f);
                    UpdateImage();
                    break;
            }
        }


       public void Rotate(){
            OpenTK.Vector2 delta = lastMousePos - new OpenTK.Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);

            cam.AddRotation(delta.X, delta.Y);
            ResetCursor();
            UpdateImage();
       }

        class Camera
        {
            //public OpenTK.Vector3 Position = OpenTK.Vector3.Zero;
            public OpenTK.Vector3 Position = new OpenTK.Vector3(0.0f, 0.0f, 200.0f);
            public OpenTK.Vector3 Orientation = new OpenTK.Vector3((float)Math.PI, 0f, 0f);
            public float MoveSpeed = 1.0f;
            public float MouseSensitivity = 0.001f;

            public Matrix4 GetViewMatrix()
            {
                OpenTK.Vector3 lookat = new OpenTK.Vector3();

                lookat.X = (float)(Math.Sin((float)Orientation.X) * Math.Cos((float)Orientation.Y));
                lookat.Y = (float)Math.Sin((float)Orientation.Y);
                lookat.Z = (float)(Math.Cos((float)Orientation.X) * Math.Cos((float)Orientation.Y));

                return Matrix4.LookAt(Position, Position + lookat, OpenTK.Vector3.UnitY);
            }
            public void Move(float x, float y, float z)
            {
                OpenTK.Vector3 offset = new OpenTK.Vector3();

                OpenTK.Vector3 forward = new OpenTK.Vector3((float)Math.Sin((float)Orientation.X), 0, (float)Math.Cos((float)Orientation.X));
                OpenTK.Vector3 right = new OpenTK.Vector3(-forward.Z, 0, forward.X);

                offset += x * right;
                offset += y * forward;
                offset.Y += z;

                offset.NormalizeFast();
                offset = OpenTK.Vector3.Multiply(offset, MoveSpeed);

                Position += offset;
            }
            public void AddRotation(float x, float y)
            {
                x = x * MouseSensitivity;
                y = y * MouseSensitivity;

                Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
                Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
            }
        }
    }
}


