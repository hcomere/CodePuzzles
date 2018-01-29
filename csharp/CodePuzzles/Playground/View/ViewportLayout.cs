using devDept.Eyeshot;
using devDept.Geometry;
using devDept.Graphics;
using System.Drawing;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Playground
{
    class Viewport : ViewportLayout
    {
        private List<UnitDisplay> UnitDisplays { get; set; }

        public Viewport()
            : base()
        {
            UnitDisplays = new List<UnitDisplay>();
        }

        public void Initialize()
        {
            Unlock("");

            AllowDrop = false;

            CoordinateSystemIcon.Visible = false;
            OriginSymbol.Visible = false;
            ToolBar.Visible = false;
            ViewportBorder.CornerRadius = 0;
            //Grid.Visible = false;
            ViewCubeIcon.Visible = false;

            Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never;
            Rendered.ShadowMode = shadowType.None;
            Rendered.PlanarReflections = false;
            Renderer = rendererType.OpenGL;

            AntiAliasing = true;
            DisplayMode = displayType.Rendered;

            BackColor = Color.LightGray;
        }

        public void CreateUnit(Unit a_unit, bool a_isPlayer)
        {
            UnitDisplay display = new UnitDisplay(a_unit, a_isPlayer);
            UnitDisplays.Add(display);
            Blocks.Add(display.BlockName, display.Block);
            Entities.Add(display);
            Invalidate();
        }

        public void DestroyUnit(Unit a_unit, bool a_isPlayer)
        {
            UnitDisplay display = UnitDisplays.Find(ud => ud.SourceUnit == a_unit);
            UnitDisplays.Remove(display);
            Blocks.Remove("" + display.SourceUnit.UId);
        }

        public void InitializeScene()
        {
            Viewports[0].Camera.Rotation = new Quaternion(Math.PI / 4, 0, Math.PI / 2);
            Viewports[0].Camera.Location = new Point3D(0, -100, 100);
            AdjustNearAndFarPlanes();

            Viewports[0].Rotate.Enabled = false;
            Viewports[0].Navigation.Mode = Camera.navigationType.Walk;
            Viewports[0].Navigation.RotationSpeed = 1;
            Viewports[0].Navigation.Speed = 1;
            Viewports[0].Navigation.Acceleration = 1;
            
            Light1.Active = true;
            Light1.Type = lightType.Point;
            Light1.Color = Color.FromArgb(170, 170, 170);
            Light1.Position = new Point3D(0, 0, 2);
            Light1.Stationary = true;

            Light2.Active = false;
            Light3.Active = false;
            Light4.Active = false;
            Light5.Active = false;
            Light6.Active = false;
            Light7.Active = false;
            Light8.Active = false;
        }

        protected override void OnMouseMove(MouseEventArgs e) { }
        protected override void OnMouseDown(MouseEventArgs e) { }
        protected override void OnMouseUp(MouseEventArgs e) { }
        protected override void OnMouseWheel(MouseEventArgs e) { }
    }
}
