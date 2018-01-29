using DevExpress.XtraEditors;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.Drawing;

namespace Playground
{
    public partial class XtraForm1 : XtraForm
    {
        private Controller Controller { get; set; }

        public XtraForm1()
        {
            InitializeComponent();

            eyeshotViewport.Initialize();

            Controller = new Controller();
            Controller.UnitCreated += OnUnitCreated;
            Controller.UnitDestroyed += OnUnitDestroyed;
            Controller.UpdateDone += OnUpdateDone;

            //unitListBox.DataSource = Controller.ReadOnlyPlayerUnits;
        }

        private void OnUpdateDone(Controller controller)
        {
            //eyeshotViewport.Entities.Regen();
            eyeshotViewport.Invalidate();
        }

        private void OnUnitDestroyed(Controller a_controller, Controller.UnitEventArgs a_args)
        {
            eyeshotViewport.DestroyUnit(a_args.Unit, a_args.IsPlayer);
        }

        private void OnUnitCreated(Controller a_controller, Controller.UnitEventArgs a_args)
        {
            eyeshotViewport.CreateUnit(a_args.Unit, a_args.IsPlayer);
        }

        private void XtraForm1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;

            eyeshotViewport.InitializeScene();
            Controller.Initialize();

            MaterialLib.InitMaterials(eyeshotViewport);

            Quad floor = new Quad(Plane.XY,
                new Point2D(-100, -100),
                new Point2D(100, -100),
                new Point2D(100, 100),
                new Point2D(-100, 100))
            {
                Color = Color.Green,
                ColorMethod = colorMethodType.byEntity,
                MaterialName = "floor"
            };

            eyeshotViewport.Entities.Add(floor);
        }

        private void AddUnitButton_Click(object sender, EventArgs e)
        {
            Controller.CreatePlayerUnit();
            unitListBox.Refresh();
        }

        private void infoBarButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            XtraMessageBox.Show(
                "Renderer Name : " + eyeshotViewport.RendererName + "\n"
                + "Renderer Version : " + eyeshotViewport.RendererVersion + "\n"
                + "Renderer Vendor : " + eyeshotViewport.RendererVendor);
        }
    }
}