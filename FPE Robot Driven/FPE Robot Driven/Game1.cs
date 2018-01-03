using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System;
using FarseerPhysics.Samples;
using FarseerPhysics.Controllers;
//using System.Windows.Forms;
using WindowsFormsApplication1;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace FarseerPhysics.Samples 
{   

    
    class Camera
    {
        private Matrix transform;
        public Matrix Transform
        {
            get { return transform; }
        }

        private Vector2 centre;
        private Viewport viewport;

        private float zoom = 1;
        private float rotation = 0;

        public float X
        {
            get { return centre.X; }
            set { centre.X = value; }
        }
        public float Y
        {
            get { return centre.Y; }
            set { centre.Y = value; }
        }

        public float Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                if (zoom < 0.1f)
                    zoom = 0.1f;
            }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }


        public Camera(Viewport newViewport)
        {
            viewport = newViewport;
        }

        public void Update(Vector2 position)
        {
            centre = new Vector2(position.X, position.Y);
            transform = Matrix.CreateTranslation(new Vector3(-centre.X, -centre.Y, 0)) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateScale(new Vector3(Zoom, Zoom, 0)) * Matrix.CreateTranslation(new Vector3(viewport.Width / 2, viewport.Height / 2, 0));
        }
    }
    public class Game1 : Game
    {

        Kontrol1 kontrol11 = new Kontrol1();

        private System.IO.Ports.SerialPort serialPort1;


        private int not_send_uart;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;
        private SpriteFont _font;

        private World _world;

        private Body _circleBody;
        private Body _groundBody;

        private Body _bigWall1;
        private Body _bigWall2;
        private Body _bigWall3;
        private Body _bigWall4;

        private Body _smallWall1;
        private Body _smallWall2;
        private Body _smallWall3;
        private Body _smallWall4;
        private Body _smallWall5;
        private Body _smallWall6;
        private Body _smallWall7;
        private Body _smallWall8;
        private Body _smallWall9;
        private Body _smallWall10;
        private Body _smallWall11;

        private Body Fuel;
        private Body Water;
        private Body Food;

        private Texture2D Fuel_texture;
        private Texture2D Water_texture;
        private Texture2D Food_texture;

        private Texture2D big_wall; //big wall texture
        private Texture2D small_wall;

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;
        private Texture2D TactSensorTexturef;
        private Texture2D TactSensorTextureb;

        private Texture2D RangeSensorTexturel;
        private Texture2D RangeSensorTexturec;
        private Texture2D RangeSensorTexturer;
        private Texture2D RangeSensorTextureb;

        private Texture2D RangeSensorTexturec_i;

        private Texture2D _bigWallSprite; //reserved



        // Simple camera controls
        private Matrix _view;
        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;
        private Vector2 _groundOrigin;
        private Vector2 _circleOrigin;

        private Vector2 bigWall1Position; 
        private Vector2 bigWall2Position; 
        private Vector2 bigWall3Position; 
        private Vector2 bigWall4Position;

        private Vector2 smallWall1Position;
        private Vector2 smallWall2Position;
        private Vector2 smallWall3Position;
        private Vector2 smallWall4Position;
        private Vector2 smallWall5Position;
        private Vector2 smallWall6Position;
        private Vector2 smallWall7Position;
        private Vector2 smallWall8Position;
        private Vector2 smallWall9Position;
        private Vector2 smallWall10Position;
        private Vector2 smallWall11Position;

        private Vector2 smallWall1DrawPos;
        private Vector2 smallWall2DrawPos;
        private Vector2 smallWall3DrawPos;
        private Vector2 smallWall4DrawPos;
        private Vector2 smallWall5DrawPos;
        private Vector2 smallWall6DrawPos;
        private Vector2 smallWall7DrawPos;
        private Vector2 smallWall8DrawPos;
        private Vector2 smallWall9DrawPos;
        private Vector2 smallWall10DrawPos;
        private Vector2 smallWall11DrawPos;

        private Vector2 FuelOrigin;
        private Vector2 WaterOrigin;
        private Vector2 FoodOrigin;

        //отладочный модуль отображения объектов
        private DebugView.DebugViewXNA physicsDebug;
        private Matrix proj;

        private List<Fixture> compaund; //тактильный сенсор

        private List<Fixture> compaund2; //сенсор
        private List<Fixture> compaund3; //сенсор
        private List<Fixture> compaund4; //сенсор
        private List<Fixture> compaund5; //сенсор задний
        private List<Fixture> compaund6; //сенсор задний тактильный

        Camera camera; //zoom and roptate camera

        public int[] SensorData; //наши сенсорные данные

        private List<Dynamics.Contacts.Contact> contacts; //списоок контактов CCD

        //прием параметров для движения роботов
        public byte sb;
        public float s;
        public byte wb;
        public float w;
        public byte ws;
        public byte timeb;
        public float time;
        //параметры для движения робота
        public int dt;
        public int index;
        public float linear;
        public float angular;

        public float direction_to_recource;

        public double distance_to_recource;

#if !XBOX360
        const string Text = "Press A or D to rotate the ball\n" +
                            "Press Space to jump\n" +
                            "Use arrow keys to move the camera";
#else
                const string Text = "Use left stick to move\n" +
                                    "Use right stick to move camera\n" +
                                    "Press A to jump\n";
#endif

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 864;

            Content.RootDirectory = "Content";

            //Create a world with gravity.
            _world = new World(new Vector2(0, 0.0f));

        }

        protected override void LoadContent()
        {
            //serial init 
            bool COM8_exist = false;
            not_send_uart = -1;
            serialPort1 = new System.IO.Ports.SerialPort("COM8", 256000);
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived); //function reference
            string[] ports = SerialPort.GetPortNames();
            
            foreach(string port in ports)
            {
                COM8_exist = false;
                if(port == "COM8")
                {
                    COM8_exist = true;
                    break;
                }
            }

            if (!serialPort1.IsOpen && (COM8_exist == true))
           {
               serialPort1.Open();

           }
            //serial init end


            // Initialize camera controls
            _view = Matrix.Identity;
            _cameraPosition = Vector2.Zero;
            _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f, _graphics.GraphicsDevice.Viewport.Height / 2f);
            _batch = new SpriteBatch(_graphics.GraphicsDevice);

            camera = new Camera(GraphicsDevice.Viewport);

            _font = Content.Load<SpriteFont>("font");

            // Load sprites
            //_circleSprite = Content.Load<Texture2D>("CircleSprite1"); //  96px x 96px => 1.5m x 1.5m
            _circleSprite = Content.Load<Texture2D>("body_robot"); //  96px x 96px => 1.5m x 1.5m
            _groundSprite = Content.Load<Texture2D>("GroundSprite"); // 512px x 64px =>   8m x 1m

            /* We need XNA to draw the ground and circle at the center of the shapes */
            _groundOrigin = new Vector2(_groundSprite.Width / 2f, _groundSprite.Height / 2f);
            _circleOrigin = new Vector2(_circleSprite.Width / 2f, _circleSprite.Height / 2f);

            // Farseer expects objects to be scaled to MKS (meters, kilos, seconds)
            // 1 meters equals 64 pixels here
            ConvertUnits.SetDisplayUnitToSimUnitRatio(64f);

            /* Circle */
            // Convert screen center from pixels to meters
            Random rand = new Random();
            float randX = (float)rand.NextDouble();
            float randY = (float)rand.NextDouble();

            Vector2 circlePosition = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2((randX*41.0f - 5.5f), (randY * 33.0f - 2.5f));

            // Create the circle fixture
            _circleBody = BodyFactory.CreateCircle(_world, ConvertUnits.ToSimUnits(96 / 2f), 1f, circlePosition);
            _circleBody.BodyType = BodyType.Dynamic;

            // Give it some bounce and friction
            _circleBody.Restitution = 0.3f;
            _circleBody.Friction = 0.5f;

            //создание сенсора
            CreateSensor();

            //создание карты
            CreateMAP();

            //отладочное отображение
            debugView();

            CreateResource();

         //   Change_clear_Map();
        }

        void CreateSensor() //создание сенсора (улититарная функция)
        {

            //инициализация сенсорных данных
            SensorData = new int[6];

            RangeSensorTexturec_i = Content.Load<Texture2D>("sensor_c_robot_i");

            // попытка создать по базиилиату
            //TactSensorTexture = Content.Load<Texture2D>("TactSensor");
            TactSensorTexturef = Content.Load<Texture2D>("sensor_bumper_robot");
            //_originTactSensor = new Vector2(TactSensorTexture.Width / 2f, TactSensorTexture.Height / 2f);
            //_origin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(8.0f, 3.5f); //вкоординыты внутренней стены
            uint[] data1 = new uint[TactSensorTexturef.Width * TactSensorTexturef.Height];
            TactSensorTexturef.GetData(data1);
            Vertices verts1 = PolygonTools.CreatePolygon(data1, TactSensorTexturef.Width);
            Vector2 scale = new Vector2(0.018f, 0.018f);
            Vector2 TranslateVectorTact = new Vector2(-77.777f, -71.44f);

            //Matrix TactTraslate = Matrix.CreateTranslation(new Vector3(300.0f, 300.0f, 0.0f));

            List<Vertices> _list = FarseerPhysics.Common.Decomposition.Triangulate.ConvexPartition(verts1, TriangulationAlgorithm.Bayazit);
            foreach (Vertices vertices in _list)
            {
                //vertices.Scale(ref scale);
                vertices.Translate(ref TranslateVectorTact);
                vertices.Scale(ref scale);
            }

            compaund = FixtureFactory.AttachCompoundPolygon(_list, 0.00f, _circleBody);

            foreach (Fixture fix in compaund)
            {
                fix.IsSensor = true;
            }
            // сенсор дистанционный первый

            // попытка создать по базиилиату
            //RangeSensorTexture = Content.Load<Texture2D>("RangeSensor");
            RangeSensorTexturel = Content.Load<Texture2D>("sensor_l_robot");
            //_originTactSensor = new Vector2(TactSensorTexture.Width / 2f, TactSensorTexture.Height / 2f);
            //_origin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(8.0f, 3.5f); //вкоординыты внутренней стены
            uint[] data2 = new uint[RangeSensorTexturel.Width * RangeSensorTexturel.Height];
            RangeSensorTexturel.GetData(data2);
            Vertices verts2 = PolygonTools.CreatePolygon(data2, RangeSensorTexturel.Width);
            Vector2 scale2 = new Vector2(0.018f, 0.018f);
            Vector2 TranslateVectorTact2 = new Vector2(-158.333f, -147.2222f);

            //Matrix TactTraslate = Matrix.CreateTranslation(new Vector3(300.0f, 300.0f, 0.0f));

            List<Vertices> _list2 = FarseerPhysics.Common.Decomposition.Triangulate.ConvexPartition(verts2, TriangulationAlgorithm.Bayazit);
            foreach (Vertices vertices in _list2)
            {
                vertices.Translate(ref TranslateVectorTact2);
                vertices.Rotate(0.00f);
                vertices.Scale(ref scale2);
            }

            compaund2 = FixtureFactory.AttachCompoundPolygon(_list2, 0.00f, _circleBody);

            foreach (Fixture fix2 in compaund2)
            {
                fix2.IsSensor = true;
            }
            // second range sensor //right

            // попытка создать по базиилиату
            //RangeSensorTexturec = Content.Load<Texture2D>("RangeSensor");
            RangeSensorTexturec = Content.Load<Texture2D>("sensor_r_robot");
            //_originTactSensor = new Vector2(TactSensorTexture.Width / 2f, TactSensorTexture.Height / 2f);
            //_origin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(8.0f, 3.5f); //вкоординыты внутренней стены
            uint[] data3 = new uint[RangeSensorTexturec.Width * RangeSensorTexturec.Height];
            RangeSensorTexturec.GetData(data3);
            Vertices verts3 = PolygonTools.CreatePolygon(data3, RangeSensorTexturec.Width);
            Vector2 scale3 = new Vector2(0.018f, 0.018f);
            Vector2 TranslateVectorTact3 = new Vector2(-158.333f, -149.999f);

            //Matrix TactTraslate = Matrix.CreateTranslation(new Vector3(300.0f, 300.0f, 0.0f));

            List<Vertices> _list3 = FarseerPhysics.Common.Decomposition.Triangulate.ConvexPartition(verts3, TriangulationAlgorithm.Bayazit);
            foreach (Vertices vertices in _list3)
            {
                vertices.Translate(ref TranslateVectorTact3);
                vertices.Rotate(0.0f);
                vertices.Scale(ref scale3);
            }

            compaund3 = FixtureFactory.AttachCompoundPolygon(_list3, 0.00f, _circleBody);

            foreach (Fixture fix3 in compaund3)
            {
                fix3.IsSensor = true;
            }

            // third range sensor (center)
            //RangeSensorTexturer = Content.Load<Texture2D>("RangeSensor");
            RangeSensorTexturer = Content.Load<Texture2D>("sensor_c_robot");
            //_originTactSensor = new Vector2(TactSensorTexture.Width / 2f, TactSensorTexture.Height / 2f);
            //_origin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(8.0f, 3.5f); //вкоординыты внутренней стены
            uint[] data4 = new uint[RangeSensorTexturer.Width * RangeSensorTexturer.Height];
            RangeSensorTexturer.GetData(data4);
            Vertices verts4 = PolygonTools.CreatePolygon(data4, RangeSensorTexturer.Width);
            Vector2 scale4 = new Vector2(0.018f, 0.018f);
            Vector2 TranslateVectorTact4 = new Vector2(-159.444f, -148.888f);

            //Matrix TactTraslate = Matrix.CreateTranslation(new Vector3(300.0f, 300.0f, 0.0f));

            List<Vertices> _list4 = FarseerPhysics.Common.Decomposition.Triangulate.ConvexPartition(verts4, TriangulationAlgorithm.Bayazit);
            foreach (Vertices vertices in _list4)
            {
                vertices.Translate(ref TranslateVectorTact4);
                //vertices.Rotate(0.00f);
                vertices.Scale(ref scale4);
            }

            compaund4 = FixtureFactory.AttachCompoundPolygon(_list4, 0.00f, _circleBody);

            foreach (Fixture fix4 in compaund4)
            {
                fix4.IsSensor = true;
            }



            // back range sensor
            //RangeSensorTextureb = Content.Load<Texture2D>("RangeSensor");
            RangeSensorTextureb = Content.Load<Texture2D>("sensor_c_robot");
            //_originTactSensor = new Vector2(TactSensorTexture.Width / 2f, TactSensorTexture.Height / 2f);
            //_origin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(8.0f, 3.5f); //вкоординыты внутренней стены
            uint[] data5 = new uint[RangeSensorTextureb.Width * RangeSensorTextureb.Height];
            RangeSensorTextureb.GetData(data5);
            Vertices verts5 = PolygonTools.CreatePolygon(data5, RangeSensorTextureb.Width);
            Vector2 scale5 = new Vector2(0.018f, 0.018f);
            Vector2 TranslateVectorTact5 = new Vector2(-152.777f, -149.9985f);

            //Matrix TactTraslate = Matrix.CreateTranslation(new Vector3(300.0f, 300.0f, 0.0f));

            List<Vertices> _list5 = FarseerPhysics.Common.Decomposition.Triangulate.ConvexPartition(verts5, TriangulationAlgorithm.Bayazit);
            foreach (Vertices vertices in _list5)
            {
                vertices.Translate(ref TranslateVectorTact5);
                vertices.Rotate(3.14f);
                vertices.Scale(ref scale5);
            }

            compaund5 = FixtureFactory.AttachCompoundPolygon(_list5, 0.00f, _circleBody);

            foreach (Fixture fix5 in compaund5)
            {
                fix5.IsSensor = true;
            }

            //тактильный задний сенсор
            //TactSensorTexture = Content.Load<Texture2D>("TactSensor");
            TactSensorTextureb = Content.Load<Texture2D>("sensor_bumper_robot");
            //_originTactSensor = new Vector2(TactSensorTexture.Width / 2f, TactSensorTexture.Height / 2f);
            //_origin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(8.0f, 3.5f); //вкоординыты внутренней стены
            uint[] data6 = new uint[TactSensorTextureb.Width * TactSensorTextureb.Height];
            TactSensorTextureb.GetData(data6);
            Vertices verts6 = PolygonTools.CreatePolygon(data1, TactSensorTextureb.Width);
            Vector2 scale6 = new Vector2(0.018f, 0.018f);
            Vector2 TranslateVectorTact6 = new Vector2(-74.999f, -69.444f);

            //Matrix TactTraslate = Matrix.CreateTranslation(new Vector3(300.0f, 300.0f, 0.0f));

            List<Vertices> _list6 = FarseerPhysics.Common.Decomposition.Triangulate.ConvexPartition(verts6, TriangulationAlgorithm.Bayazit);
            foreach (Vertices vertices in _list6)
            {

                vertices.Translate(ref TranslateVectorTact6);
                vertices.Rotate(3.14f);
                vertices.Scale(ref scale6);
            }

            compaund6 = FixtureFactory.AttachCompoundPolygon(_list6, 0.00f, _circleBody);

            foreach (Fixture fix6 in compaund6)
            {
                fix6.IsSensor = true;
            }




        }
        void CreateMAP() //Карта стены и прочее (улититарная функция)
        {   
            //загрузить текстуру для стены
            big_wall = Content.Load<Texture2D>("bigWall");

            bigWall1Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(20f, -6.5f);
            _bigWall1 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(4096f), ConvertUnits.ToSimUnits(32f), 1f, bigWall1Position);
            _bigWall1.IsStatic = true;

            bigWall2Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(20f, 33.8f);
            _bigWall2 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(4096f), ConvertUnits.ToSimUnits(32f), 1f, bigWall2Position);
            _bigWall2.IsStatic = true;

            bigWall3Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(-9.8f, 12.8f);
            _bigWall3 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(4096f), ConvertUnits.ToSimUnits(32f), 1f, bigWall3Position);
            _bigWall3.Rotation = 1.57079f;
            _bigWall3.IsStatic = true;

            bigWall4Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(50.3f, 12.8f);
            _bigWall4 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(4096f), ConvertUnits.ToSimUnits(32f), 1f, bigWall4Position);
            _bigWall4.Rotation = 1.57079f;
            _bigWall4.IsStatic = true;

            small_wall = Content.Load<Texture2D>("Small_wall");


            //------------------------------------------------------------------------------------
            smallWall1Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(2.3f, 2.8f); //1
            _smallWall1 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall1Position);
            _smallWall1.Rotation = 2.27079f;
            _smallWall1.IsStatic = true;

            smallWall2Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(0.3f, 15.8f); //2
            _smallWall2 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall2Position);
            _smallWall2.Rotation = 1.57079f;
            _smallWall2.IsStatic = true;

            smallWall3Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(4.3f, 23.8f); //3
            _smallWall3 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall3Position);
            _smallWall3.Rotation = 2.57079f;
            _smallWall3.IsStatic = true;

            smallWall4Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.3f, 8.8f); //4
            _smallWall4 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall4Position);
            _smallWall4.Rotation = 0.37079f;
            _smallWall4.IsStatic = true;

            smallWall5Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.3f, 20.8f); //5
            _smallWall5 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall5Position);
            _smallWall5.Rotation = 0.97079f;
            _smallWall5.IsStatic = true;

            smallWall6Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(30.3f, 2.8f); //6
            _smallWall6 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall6Position);
            _smallWall6.Rotation = 2.37079f;
            _smallWall6.IsStatic = true;

            smallWall7Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(33.3f, 15.8f); //7
            _smallWall7 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall7Position);
            _smallWall7.Rotation = 2.07079f;
            _smallWall7.IsStatic = true;

            smallWall8Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(36.3f, 25.8f); //8
            _smallWall8 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall8Position);
            _smallWall8.Rotation = 0f;
            _smallWall8.IsStatic = true;

            smallWall9Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(45.3f, 0.8f); //9
            _smallWall9 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall9Position);
            _smallWall9.Rotation = 0f;
            _smallWall9.IsStatic = true;

            smallWall10Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(38.3f, 14.8f); //10
            _smallWall10 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall10Position);
            _smallWall10.Rotation = 0.57f;
            _smallWall10.IsStatic = true;

            smallWall11Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(14.3f, 26.5f); //11
            _smallWall11 = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(32f), 1f, smallWall11Position);
            _smallWall11.Rotation = 2.37f;
            _smallWall11.IsStatic = true;

            smallWall1DrawPos = new Vector2(2.5f, -2.8f);
            smallWall2DrawPos = new Vector2(-0.1f, -3.8f);
            smallWall3DrawPos = new Vector2(3.0f, -2.2f);
            smallWall4DrawPos = new Vector2(-3.35f, -1.3f);
            smallWall5DrawPos = new Vector2(-2.1f, -2.8f);
            smallWall6DrawPos = new Vector2(2.5f, -2.3f);
            smallWall7DrawPos = new Vector2(1.5f, -3.0f);
            smallWall8DrawPos = new Vector2(-3.25f, 0.3f);
            smallWall9DrawPos = new Vector2(-3.1f, -0.1f);
            smallWall10DrawPos = new Vector2(-2.6f, -1.6f);
            smallWall11DrawPos = new Vector2(2.4f, -2.6f);


        }


        void ChangeFristMAP() //change to default map
        {
            _smallWall1.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(2.3f, 2.8f), 2.27079f); //1
            _smallWall2.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(0.3f, 15.8f), 1.57079f); //2
            _smallWall3.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(4.3f, 23.8f), 2.57079f); //3
            _smallWall4.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.3f, 8.8f), 0.37079f); //4
            _smallWall5.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.3f, 20.8f), 0.97079f); //5
            _smallWall6.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(30.3f, 2.8f), 2.37079f); //6
            _smallWall7.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(33.3f, 15.8f), 2.07079f); //7
            _smallWall8.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(36.3f, 25.8f), 0.0f); //8
            _smallWall9.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(45.3f, 0.8f), 0.0f); //9
            _smallWall10.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(38.3f, 14.8f), 0.57f); //10
            _smallWall11.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(14.3f, 26.5f), 2.37f); //11

            smallWall1DrawPos = new Vector2(2.5f, -2.8f);
            smallWall2DrawPos = new Vector2(-0.1f, -3.8f);
            smallWall3DrawPos = new Vector2(3.0f, -2.2f);
            smallWall4DrawPos = new Vector2(-3.35f, -1.3f);
            smallWall5DrawPos = new Vector2(-2.1f, -2.8f);
            smallWall6DrawPos = new Vector2(2.5f, -2.3f);
            smallWall7DrawPos = new Vector2(1.5f, -3.0f);
            smallWall8DrawPos = new Vector2(-3.25f, 0.3f);
            smallWall9DrawPos = new Vector2(-3.1f, -0.1f);
            smallWall10DrawPos = new Vector2(-2.6f, -1.6f);
            smallWall11DrawPos = new Vector2(2.4f, -2.6f);

            smallWall1Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(2.3f, 2.8f);
            smallWall2Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(0.3f, 15.8f);
            smallWall3Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(4.3f, 23.8f);
            smallWall4Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.3f, 8.8f);
            smallWall5Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.3f, 20.8f);
            smallWall6Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(30.3f, 2.8f);
            smallWall7Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(33.3f, 15.8f);
            smallWall8Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(36.3f, 25.8f);
            smallWall9Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(45.3f, 0.8f);
            smallWall10Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(38.3f, 14.8f);
            smallWall11Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(14.3f, 26.5f);
        }

        void ChangeSecondMAP()
        {
            _smallWall1.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(4.3f, 3.3f), 0.00f); //1
            _smallWall2.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(1.5f, 12.8f), 0.00f); //2
            _smallWall3.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(1.8f, 23.8f), -0.57079f); //3
            _smallWall4.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.7f, 3.3f), 1.57079f); //4
            _smallWall5.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.7f, 23.2f), 1.57079f); //5
            _smallWall6.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(27.3f, 5.2f), -0.50079f); //6     
            _smallWall7.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(27.0f, 17.8f), 0.50079f); //7
            _smallWall8.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(39.8f, 5.8f), 0.65079f); //8
            _smallWall9.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(35.5f, 12.8f), 0.65079f); //9
            _smallWall10.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(40.3f, 20.8f), -0.65079f); //10
            _smallWall11.SetTransform(ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(30.3f, 28.5f), 0.00f); //11

            smallWall1DrawPos = new Vector2(-1.7f, 0.5f);
            smallWall2DrawPos = new Vector2(-2.6f, -2.9f);
            smallWall3DrawPos = new Vector2(-5.65f, 2.4f); 
            smallWall4DrawPos = new Vector2(0.5f, -9.0f); 
            smallWall5DrawPos = new Vector2(0.2f, -1.1f); 
            smallWall6DrawPos = new Vector2(-6.0f, 4.1f); 
            smallWall7DrawPos = new Vector2(-9.5f, 0.5f); 
            smallWall8DrawPos = new Vector2(1.05f, -21.9f); 
            smallWall9DrawPos = new Vector2(-12.5f, 10.1f); 
            smallWall10DrawPos = new Vector2(-0.6f, 8.2f); 
            smallWall11DrawPos = new Vector2(12.6f, 2.3f); 

            smallWall1Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(2.3f, 2.8f);
            smallWall2Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(0.3f, 15.8f);
            smallWall3Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(4.3f, 23.8f);
            smallWall4Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.3f, 8.8f);
            smallWall5Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(15.3f, 20.8f);
            smallWall6Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(30.3f, 2.8f);
            smallWall7Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(33.3f, 15.8f);
            smallWall8Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(36.3f, 25.8f);
            smallWall9Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(45.3f, 0.8f);
            smallWall10Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(38.3f, 14.8f);
            smallWall11Position = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(14.3f, 26.5f);
        }
        protected override void Initialize()
        {
            //ElementHostumuz
            ElementHost elementHost1 = new ElementHost();
            elementHost1.Location = new System.Drawing.Point(88, 9);
            elementHost1.Name = "elementHost1";
            elementHost1.Size = new System.Drawing.Size(150, 35);
            elementHost1.TabIndex = 1;
            elementHost1.Text = "elementHost1";
            elementHost1.Child = this.kontrol11;
            Control.FromHandle(Window.Handle).Controls.Add(elementHost1);

            base.Initialize();
        }


        void Change_clear_Map()
        {
            _smallWall1.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall2.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall3.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall4.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall5.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall6.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall7.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall8.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall9.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall10.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);
            _smallWall11.SetTransform(new Vector2(999.3f, 999.5f), 0.0f);

            smallWall1Position = new Vector2(999.3f, 999.5f);
            smallWall2Position = new Vector2(999.3f, 999.5f);
            smallWall3Position = new Vector2(999.3f, 999.5f);
            smallWall4Position = new Vector2(999.3f, 999.5f);
            smallWall5Position = new Vector2(999.3f, 999.5f);
            smallWall6Position = new Vector2(999.3f, 999.5f);
            smallWall7Position = new Vector2(999.3f, 999.5f);
            smallWall8Position = new Vector2(999.3f, 999.5f);
            smallWall9Position = new Vector2(999.3f, 999.5f);
            smallWall10Position = new Vector2(999.3f, 999.5f);
            smallWall11Position = new Vector2(999.3f, 999.5f);

        }


        float distance(Vector2 origin, Vector2 propose)
        {
            float distance1;
            distance1 = (float)Math.Sqrt(Math.Abs(propose.X - origin.X) + Math.Abs(propose.Y - origin.Y));

            return distance1;
        }

        float rotation(Vector2 origin, Vector2 propose)
        {   
            float rot;
            Vector2 direction = new Vector2(0, 0);
            direction.X = propose.X - origin.X;
            direction.Y = propose.Y - origin.Y;
            rot = (float)Math.Atan2(direction.Y , direction.X);


            return rot;
        }
        void CreateResource()
        {
            Fuel_texture = Content.Load<Texture2D>("bakl");
            

            FuelOrigin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(30f, -3.5f);
            Fuel = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(32f), ConvertUnits.ToSimUnits(32f), 1f, FuelOrigin);
            //Fuel.IsStatic = true;
            Fuel.IsSensor = true;

            /*Water_texture = Content.Load<Texture2D>("water");

            WaterOrigin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(36f, 17.5f);
            Water = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(32f), ConvertUnits.ToSimUnits(32f), 1f, WaterOrigin);
            Water.IsStatic = true;
            Water.IsSensor = true;

            Food_texture = Content.Load<Texture2D>("bigmak");

            FoodOrigin = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(-5f, 16.5f);
            Food = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(32f), ConvertUnits.ToSimUnits(32f), 1f, FoodOrigin);
            Food.IsStatic = true;
            Food.IsSensor = true;*/
        }//создает ресурсы

        void ReMoveResource(Vector2 newPosition)
        {
            //FuelOrigin = newPosition;
            Fuel.SetTransform((_cameraPosition - _screenCenter) + newPosition, 0.0f);

        }

        void debugView()//отладочное отображение (улититарная функция)
        {
            //отладочное отображение
            physicsDebug = new DebugView.DebugViewXNA(_world);
            //physicsDebug.AppendFlags(DebugViewFlags.DebugPanel);
            physicsDebug.AppendFlags(DebugViewFlags.Shape);
            physicsDebug.AppendFlags(DebugViewFlags.ContactPoints);
            //physicsDebug.AppendFlags(DebugViewFlags.PolygonPoints);
            //physicsDebug.StaticShapeColor = Color.Black; // цвет стен
            physicsDebug.DefaultShapeColor = Color.Beige;
            physicsDebug.SleepingShapeColor = Color.LightGray;
            physicsDebug.LoadContent(this.GraphicsDevice, this.Content);

            Matrix proj = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
        }

        void CheckTouch() //проверка столкновения сенсора со стеной (улититарная функция)
        {
            for (int i = 0; i < 6; i++) //на каждом новом такте обнуляем показания датчиков, если есть стена они все равно обновяться до актуальных
            {
                SensorData[i] = 0;
            }


            contacts = _world.ContactList;

            foreach (Dynamics.Contacts.Contact contact in contacts)
            {
                foreach (Fixture comp in compaund) //проверка столкновения тактильного сенсора.
                {
                    if ((contact.FixtureA == comp && contact.IsTouching) || (contact.FixtureB == comp && contact.IsTouching))
                    {
                        SensorData[0] = 1;
                    }
                    else
                    {

                    }
                }

                foreach (Fixture comp in compaund2) //проверка столкновения  сенсора расстояния 1.
                {
                    if ((contact.FixtureA == comp && contact.IsTouching) || (contact.FixtureB == comp && contact.IsTouching))
                    {
                        SensorData[1] = 1;
                    }
                    else
                    {

                    }
                }

                foreach (Fixture comp in compaund4) //проверка столкновения  сенсора расстояния 2.
                {
                    if ((contact.FixtureA == comp && contact.IsTouching) || (contact.FixtureB == comp && contact.IsTouching))
                    {
                        SensorData[2] = 1;
                    }
                    else
                    {

                    }
                }

                foreach (Fixture comp in compaund3) //проверка столкновения  сенсора расстояния 3.
                {
                    if ((contact.FixtureA == comp && contact.IsTouching) || (contact.FixtureB == comp && contact.IsTouching))
                    {
                        SensorData[3] = 1;
                    }
                    else
                    {

                    }
                }
                foreach (Fixture comp in compaund5) //проверка столкновения  сенсора расстояния 4(заднего дистанционного).
                {
                    if ((contact.FixtureA == comp && contact.IsTouching) || (contact.FixtureB == comp && contact.IsTouching))
                    {
                        SensorData[4] = 1;
                    }
                    else
                    {

                    }
                }
                foreach (Fixture comp in compaund6) //проверка столкновения  сенсора тактильного 5(заднего тактильного).
                {
                    if ((contact.FixtureA == comp && contact.IsTouching) || (contact.FixtureB == comp && contact.IsTouching))
                    {
                        SensorData[5] = 1;
                    }
                    else
                    {

                    }
                }

            }
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            double xQ;
            double yQ;
            float a;
            HandleGamePad();
            HandleKeyboard();
            dt = UpdateMove(_circleBody, 1, s, w, gameTime, dt);
            camera.Update(_screenCenter);
            //We update the world
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            CheckTouch();

            //calculate direction to recource
            direction_to_recource = (_circleBody.Rotation - rotation(_circleBody.Position, Fuel.Position));
            a = (float)Math.Truncate(direction_to_recource / (2 * Math.PI));
            direction_to_recource = direction_to_recource - a * 2 * (float)Math.PI;

            //calculate distance to recource
            xQ = (double)(Math.Abs(_circleBody.Position.X - Fuel.Position.X));
            yQ = (double)(Math.Abs(_circleBody.Position.Y - Fuel.Position.Y));
            distance_to_recource = Math.Sqrt(xQ*xQ + yQ*yQ);


            switch(kontrol11.state_map)
            {
                case 0:
                    ChangeSecondMAP();
                    break;
                case 1:
                    Change_clear_Map();
                    break;
                case 2:
                    ChangeFristMAP();
                    break;
                default:
                    break;
                

            }
           

        

            base.Update(gameTime);
        }

        public int MoveBody(Body robot, ref int index, ref float linear, ref float angular) //индекс номер действия
        {

            //gt.TotalGameTime; 
            Vector2 dir = new Vector2(0, 0);

            if (index == 0)
            {
                linear = 2.0f;
                angular = -0.5f;
            }
            else if (index == 1)
            {
                linear = 2.0f;
                angular = 0.0f;
            }
            else if (index == 2)
            {
                linear = 2.0f;
                angular = 0.5f;
            }
            else if (index == 3)
            {
                linear = -2.0f;
                angular = -0.5f;
            }
            else if (index == 4)
            {
                linear = -2.0f;
                angular = 0;
            }
            else if (index == 5)
            {
                linear = -2.0f;
                angular = 0.5f;
            }
            else if (index == 6)
            {
                linear = 0.0f;
                angular = 0.0f;
            }
            else
            {

            }

            dir.X = (float)System.Math.Cos(robot.Rotation);
            dir.Y = (float)System.Math.Sin(robot.Rotation);
            _circleBody.LinearVelocity = dir * linear;
            _circleBody.AngularVelocity = angular;


            return 0;
        }

        public int UpdateMove(Body robot, int index, float linear, float angular, GameTime gameTime, int dt)
        {
            byte crc= 0;
            byte[] answer = new byte[] { 0, 0, 0, 0 , 0, 0, 0, 0 ,0}; //уже не однобайтный буффер 
            Vector2 dir = new Vector2(0, 0);

            dir.X = (float)System.Math.Cos(robot.Rotation);
            dir.Y = (float)System.Math.Sin(robot.Rotation);
            _circleBody.LinearVelocity = dir * linear;
            _circleBody.AngularVelocity = angular;

            if (dt < time)
            {
                dt = dt + gameTime.ElapsedGameTime.Milliseconds;
            }


            if (dt >= time)
            {
                _circleBody.LinearVelocity = new Vector2(0, 0);
                _circleBody.AngularVelocity = 0f;

                //send result here
               /* if (not_send_uart == 0)
                {
                    answer[0] = Convert.ToByte(Convert_to_int(SensorData));
                    //serialPort1.WriteLine(Convert.ToString(answer));
                    serialPort1.Write(answer,0,1);
                    not_send_uart = 1;
                }*/
            }

            if (serialPort1.IsOpen == true)
            {
                answer[0] = 0xC4;
                answer[1] = Convert.ToByte(Convert_to_int(SensorData));
                answer[2] = Convert_to_decimal_rot(direction_to_recource);
                answer[3] = Rot_sign(direction_to_recource);
                answer[4] = Convert_to_byte(distance_to_recource);
                answer[5] = 0;
                answer[6] = 0;
                crc = Convert.ToByte(answer[0] ^ answer[1] ^ answer[2] ^ answer[3] ^ answer[4] ^ answer[5] ^ answer[6]);
                answer[7] = crc;
                //serialPort1.WriteLine(Convert.ToString(answer));
                //serialPort1.Write(answer, 0, 6);

            }
            return dt;
        }
        //--------------------------------------------------------------------------------------------
        private byte Convert_to_int(int []SensorData)
        {
            int result= 0x00;

            if (SensorData[0] == 1)
            {
                result = result | 0x01;
            }
            if (SensorData[1] == 1)
            {
                result = result | 0x02;
            }
            if (SensorData[2] == 1)
            {
                result = result | 0x04;
            }
            if (SensorData[3] == 1)
            {
                result = result | 0x08;
            }
            if (SensorData[4] == 1)
            {
                result = result | 0x10;
            }
            if (SensorData[5] == 1)
            {
                result = result | 0x20;
            }

            return (byte)result;
        }

        private byte Convert_to_byte(double distance)
        {
            return (byte)Math.Truncate(distance);
        }
        private byte Convert_to_decimal_rot(float rot)
        {
            int result = 0;
            
            rot = (180 * rot / (float)Math.PI);
            if(rot < 0)
            {
                rot = 360 + rot;
            }
            
            result = (int)Math.Abs(rot-180);

            result = 180 - result;

            return (byte)result;
        }

        private byte Rot_sign(float rot)
        {
            int result = 0;
            int sign = 0;
            rot = (180 * rot / (float)Math.PI);

            if (rot < 0)
            {
                rot = 360 + rot;
            }
            if (rot > 180)
                sign = 1;

            

            return (byte)sign;
            
        }

        //--------------------------------------------------------------------------------------------
        private void HandleGamePad()
        {
            GamePadState padState = GamePad.GetState(0);

            if (padState.IsConnected)
            {
                if (padState.Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    Exit();

                if (padState.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed && _oldPadState.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Released)
                    _circleBody.ApplyLinearImpulse(new Vector2(0, -10));

                _circleBody.ApplyForce(padState.ThumbSticks.Left);
                _cameraPosition.X -= padState.ThumbSticks.Right.X;
                _cameraPosition.Y += padState.ThumbSticks.Right.Y;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

                _oldPadState = padState;
            }
        }


        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            byte []packet = new byte[] { 0, 0, 0, 0 , 0, 0}; 
            //byte[] request = new byte[] { 0, 0, 0, 0, 0, 0};
            /*byte sb;
            float s;
            byte wb;
            float w;
            byte ws;
            byte timeb;
            float time;*/
            serialPort1.Read(packet, 0, 4);

            sb = packet[0];
            wb = packet[1];
            ws = packet[2];
            timeb = packet[3];

            if (sb < 127)
            {
                s = -(4.0f / 127.0f) * sb;
            }
            else
            {
                s = (4.0f / 127.0f) * (sb - 127);
            }

            w = wb / 255.0f;
            if(ws > 0)
            {
                w = -w;
            }
            else
            {  
            }
            time = timeb * 50;

            index = 7;
            dt = MoveBody(_circleBody, ref index, ref s, ref w);
            //s[0] = ;
            /*if (str == "w") //recieved from serial port "w"
            {
                index = 1;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
                not_send_uart = 0;
            }
            else if (str == "q") //recieved from serial port "q"
            {
                index = 0;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
                not_send_uart = 0;
            }
            else if (str == "e") //recieved from serial port "e"
            {
                index = 2;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
                not_send_uart = 0;
            }
            else if (str == "a") //recieved from serial port "a"
            {
                index = 5;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
                not_send_uart = 0;
            }
            else if (str == "s") //recieved from serial port "s"
            {
                index = 4;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
                not_send_uart = 0;
            }
            else if (str == "d") //recieved from serial port "d"
            {
                index = 3;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
                not_send_uart = 0;
            }
            else if (str == "x") //recieved from serial port "x"
            {
                index = 6;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
                not_send_uart = 0;
            }*/
        }


        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();
            MouseState mousestate = Mouse.GetState();

            // Move camera
            // if (state.IsKeyDown(Keys.Left))
            //   _cameraPosition.X += 1.5f;

            // if (state.IsKeyDown(Keys.Right))
            //    _cameraPosition.X -= 1.5f;

            // if (state.IsKeyDown(Keys.Up))
            //     _cameraPosition.Y += 1.5f;

            // if (state.IsKeyDown(Keys.Down))
            //     _cameraPosition.Y -= 1.5f;

            //if (Keyboard.GetState().IsKeyDown(Keys.W))
            //    camera.Zoom += 1;
            //else if (Keyboard.GetState().IsKeyDown(Keys.S))
            //    camera.Zoom -= 1;

            if(mousestate.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                Vector2 mouse_pos;
                mouse_pos.X = mousestate.X;
                mouse_pos.Y = mousestate.Y;
                //ReMoveResource(mouse_pos);
            }

            _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f)) * Matrix.CreateScale(0.33f);



            //if (state.IsKeyDown(Keys.Space) && _oldKeyState.IsKeyUp(Keys.Space))
            //{
            //    index = 0;
            //    dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
            //}
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)) //move forward
            {
                index = 1;
                time = 2000;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
            }
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q)) //move left
            {
                index = 0;
                time = 2000;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
            }
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.E)) //move right
            {
                index = 2;
                time = 2000;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
            }
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))  //back left
            {
                index = 5;
                time = 2000;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
            }
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))  //back left
            {
                index = 3;
                time = 2000;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
            }
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))  //back left
            {
                index = 4;
                time = 2000;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
            }
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X))  //back left
            {
                index = 6;
                time = 2000;
                dt = MoveBody(_circleBody, ref index, ref linear, ref angular);
            }


            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();

            _oldKeyState = state;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);
            _batch.Draw(_circleSprite, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation, _circleOrigin, 1f, SpriteEffects.None, 0f);
            _batch.Draw(TactSensorTexturef, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation, _circleOrigin + new Vector2(19.777f, 20.44f), 1f, SpriteEffects.None, 0f);
            _batch.Draw(RangeSensorTexturel, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation, _circleOrigin + new Vector2(99.777f, 109.44f), 1f, SpriteEffects.None, 0f);
            _batch.Draw(RangeSensorTexturer, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation, _circleOrigin + new Vector2(99.777f, 104.44f), 1f, SpriteEffects.None, 0f);
            _batch.Draw(RangeSensorTexturec, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation, _circleOrigin + new Vector2(99.777f, 99.44f), 1f, SpriteEffects.None, 1f);
            _batch.Draw(RangeSensorTextureb, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation + 3.14f, _circleOrigin + new Vector2(94.777f, 99.44f), 1f, SpriteEffects.None, 0f);
            _batch.Draw(TactSensorTextureb, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation + 3.14f, _circleOrigin + new Vector2(19.777f, 19.44f), 1f, SpriteEffects.None, 0f);


            _batch.End();
            //Draw circle and ground
            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);

            //draw wall
            _batch.Draw(big_wall, ConvertUnits.ToDisplayUnits(bigWall1Position + new Vector2(-29.777f, -0.18f)), null, Color.White, _bigWall1.Rotation, _bigWall1.Position, 1f, SpriteEffects.None, 0f);
            _batch.Draw(big_wall, ConvertUnits.ToDisplayUnits(bigWall1Position + new Vector2(0.777f, -0.18f)), null, Color.White, _bigWall1.Rotation, _bigWall1.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(big_wall, ConvertUnits.ToDisplayUnits(bigWall2Position + new Vector2(-29.777f, 0.36f)), null, Color.White, _bigWall2.Rotation, _bigWall2.Position, 1f, SpriteEffects.None, 0f);
            _batch.Draw(big_wall, ConvertUnits.ToDisplayUnits(bigWall2Position + new Vector2(0.777f, 0.36f)), null, Color.White, _bigWall2.Rotation, _bigWall2.Position, 1f, SpriteEffects.None, 0f);


            _batch.Draw(big_wall, ConvertUnits.ToDisplayUnits(bigWall3Position + new Vector2(0.0f, -20.36f)), null, Color.White, _bigWall3.Rotation, _bigWall3.Position, 1f, SpriteEffects.None, 0f);
            _batch.Draw(big_wall, ConvertUnits.ToDisplayUnits(bigWall3Position + new Vector2(0.0f, 0.36f)), null, Color.White, _bigWall3.Rotation, _bigWall3.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(big_wall, ConvertUnits.ToDisplayUnits(bigWall4Position + new Vector2(0.0f, -20.36f)), null, Color.White, _bigWall4.Rotation, _bigWall4.Position, 1f, SpriteEffects.None, 0f);
            _batch.Draw(big_wall, ConvertUnits.ToDisplayUnits(bigWall4Position + new Vector2(0.0f, 0.36f)), null, Color.White, _bigWall4.Rotation, _bigWall4.Position, 1f, SpriteEffects.None, 0f);



            //draw small wall
            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall1Position + smallWall1DrawPos), null, Color.White, _smallWall1.Rotation, _smallWall1.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall2Position + smallWall2DrawPos), null, Color.White, _smallWall2.Rotation, _smallWall2.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall3Position + smallWall3DrawPos), null, Color.White, _smallWall3.Rotation, _smallWall3.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall4Position + smallWall4DrawPos), null, Color.White, _smallWall4.Rotation, _smallWall4.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall5Position + smallWall5DrawPos), null, Color.White, _smallWall5.Rotation, _smallWall5.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall6Position + smallWall6DrawPos), null, Color.White, _smallWall6.Rotation, _smallWall6.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall7Position + smallWall7DrawPos), null, Color.White, _smallWall7.Rotation, _smallWall7.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall8Position + smallWall8DrawPos), null, Color.White, _smallWall8.Rotation, _smallWall8.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall9Position + smallWall9DrawPos), null, Color.White, _smallWall9.Rotation, _smallWall9.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall10Position + smallWall10DrawPos), null, Color.White, _smallWall10.Rotation, _smallWall10.Position, 1f, SpriteEffects.None, 0f);

            _batch.Draw(small_wall, ConvertUnits.ToDisplayUnits(smallWall11Position + smallWall11DrawPos), null, Color.White, _smallWall11.Rotation, _smallWall11.Position, 1f, SpriteEffects.None, 0f);

            _batch.End();

            //resources
            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);

            _batch.Draw(Fuel_texture, ConvertUnits.ToDisplayUnits(FuelOrigin + new Vector2(-1.0f, -1.0f)), null, Color.White, Fuel.Rotation, Fuel.Position, 0.5f, SpriteEffects.None, 0f);
            //_batch.Draw(Water_texture, ConvertUnits.ToDisplayUnits(WaterOrigin + new Vector2(0.0f, 0.0f)), null, Color.White, Water.Rotation, Water.Position, 0.5f, SpriteEffects.None, 0f);
            //_batch.Draw(Food_texture, ConvertUnits.ToDisplayUnits(FoodOrigin + new Vector2(0.0f, 0.0f)), null, Color.White, Food.Rotation, Food.Position, 0.25f, SpriteEffects.None, 0f);
            
            _batch.End();

            // Display instructions
            _batch.Begin();

            _batch.DrawString(_font, System.Convert.ToString(SensorData[5]), new Vector2(14f, 10f), Color.Red); //рисование данных сенсора
            _batch.DrawString(_font, System.Convert.ToString(SensorData[4]), new Vector2(24f, 10f), Color.Red); //рисование данных сенсора
            _batch.DrawString(_font, System.Convert.ToString(SensorData[3]), new Vector2(34f, 10f), Color.Red); //рисование данных сенсора
            _batch.DrawString(_font, System.Convert.ToString(SensorData[2]), new Vector2(44f, 10f), Color.Red); //рисование данных сенсора
            _batch.DrawString(_font, System.Convert.ToString(SensorData[1]), new Vector2(54f, 10f), Color.Red); //рисование данных сенсора
            _batch.DrawString(_font, System.Convert.ToString(SensorData[0]), new Vector2(64f, 10f), Color.Red); //рисование данных сенсора

            //_batch.DrawString(_font, "Fuel distance:" + System.Convert.ToString(distance(_circleBody.Position, Fuel.Position)), new Vector2(20f, 25f), Color.Black); //рисование дистанции топливо
            //_batch.DrawString(_font, "Water distance:" + System.Convert.ToString(distance(_circleBody.Position, Water.Position)), new Vector2(20f, 40f), Color.Black); //рисование дистанции water
            //_batch.DrawString(_font, "Food distance:" + System.Convert.ToString(distance(_circleBody.Position, Food.Position)), new Vector2(20f, 55f), Color.Black); //рисование дистанции food
            //_batch.DrawString(_font, "Fuel direction:" + System.Convert.ToString(rotation(_circleBody.Position, Fuel.Position)), new Vector2(20f, 45f), Color.Black); //рисование дистанции топливо

           // _batch.DrawString(_font, "Fuel X :" + System.Convert.ToString(Fuel.Position.X) + " Fuel Y: " + System.Convert.ToString (Fuel.Position.Y), new Vector2(20f, 65f), Color.Black); //рисование дистанции топливо
           // _batch.DrawString(_font, "Robot X :" + System.Convert.ToString(_circleBody.Position.X) + " Robot Y: " + System.Convert.ToString(_circleBody.Position.Y), new Vector2(20f, 85f), Color.Black);
            //_batch.DrawString(_font, "Robot direction :" + System.Convert.ToString(_circleBody.Rotation), new Vector2(20f, 105f), Color.Black);

            _batch.DrawString(_font, "Robot direction to recource:" + System.Convert.ToString(direction_to_recource), new Vector2(20f, 45f), Color.Black);
            _batch.DrawString(_font, "Robot direction to byte:" + System.Convert.ToString(Convert_to_decimal_rot(direction_to_recource)), new Vector2(20f, 65f), Color.Black);
            _batch.DrawString(_font, "Robot direction sign :" + System.Convert.ToString(Rot_sign(direction_to_recource)), new Vector2(20f, 85f), Color.Black);

            _batch.DrawString(_font, "distance to recource :" + System.Convert.ToString(Convert_to_byte(distance_to_recource)), new Vector2(20f, 105f), Color.Black); 
            //_batch.DrawString(_font, System.Convert.ToString(dir), new Vector2(80f, 80f), Color.Red); //рисование данных сенсора
            //proj = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            proj = Matrix.CreateOrthographicOffCenter(0f, _graphics.GraphicsDevice.Viewport.Width / ConvertUnits.ToDisplayUnits(1f), _graphics.GraphicsDevice.Viewport.Height / ConvertUnits.ToDisplayUnits(1f), 0f, 0f, 1f) * camera.Transform;
           // (_circleBody.Rotation) - 
            //physicsDebug.RenderDebugData(ref proj, ref _view);
            

            _batch.End();

            base.Draw(gameTime);
        }
    }
}