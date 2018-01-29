using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Eyeshot;

namespace AGPlayground
{
    public partial class Main : XtraForm
    {
        public Main()
        {
            InitializeComponent();
            myViewportLayout1.Unlock("");
        }

        private void myViewportLayout1_Click(object sender, EventArgs e)
        {

        }

        private void Main_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            Text = "Genetic Algorithm Playground";

            myViewportLayout1.Viewports[0].Rotate.Enabled = false;
            myViewportLayout1.CoordinateSystemIcon.Visible = false;
            myViewportLayout1.OriginSymbol.Visible = false;
            myViewportLayout1.ToolBar.Visible = false;
            myViewportLayout1.ViewportBorder.CornerRadius = 0;
            myViewportLayout1.Grid.Visible = false;
            myViewportLayout1.ViewCubeIcon.Visible = false;

            myViewportLayout1.Background.TopColor = Color.Black;
            myViewportLayout1.Background.IntermediateColor = Color.Black;
            myViewportLayout1.Background.BottomColor = Color.Black;

            myViewportLayout1.Viewports[0].SetView(viewType.Top);
            myViewportLayout1.Viewports[0].Camera.Location = new Point3D(0, 0, 10);
            myViewportLayout1.Viewports[0].Camera.ProjectionMode = devDept.Graphics.projectionType.Orthographic;
            myViewportLayout1.Viewports[0].AdjustNearAndFarPlanes();

            Line xLine = new Line(0, 0, 0, 100, 0, 0);
            xLine.ColorMethod = colorMethodType.byEntity;
            xLine.Color = Color.Red;

            Line yLine = new Line(0, 0, 0, 0, 100, 0);
            yLine.ColorMethod = colorMethodType.byEntity;
            yLine.Color = Color.Green;

            myViewportLayout1.Entities.AddRange(new List<Entity> { xLine, yLine });
        }

        private void myViewportLayout1_Click_1(object sender, EventArgs e)
        {

        }
    }
}