using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using System.ComponentModel;

namespace Playground
{
    class UnitDisplay : BlockReference
    {
        public Unit SourceUnit { get; private set; }
        private Mesh Mesh { get; set; }
        public Block Block { get; private set; }

        public UnitDisplay(Unit a_sourceUnit, bool a_isPlayer)
            : base(0, 0, 0, "" + a_sourceUnit.UId, 1, 1, 1, 0)
        {
            SourceUnit = a_sourceUnit;
            Mesh = Mesh.CreateBox(1, 1, 1);
            Mesh.LightWeight = true;
            Mesh.Color = System.Drawing.Color.Red;
            Mesh.ColorMethod = colorMethodType.byEntity;
            
            Mesh.MaterialName = a_isPlayer ? "playerUnit" : "ennemyUnit";
            
            SourceUnit.PropertyChanged += OnUnitPropertyChanged;

            Block = new Block();
            Block.Entities.Add(Mesh);
        }
        
        public override void MoveTo(DrawParams data)
        {
            base.MoveTo(data);
            data.RenderContext.TranslateMatrixModelView(SourceUnit.Position.X, SourceUnit.Position.Y, 0);
        }

        private void OnUnitPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals("Position"))
            {
                Mesh.Translate(SourceUnit.Position.X, SourceUnit.Position.Y, 0);
            }
        }
    }
}
