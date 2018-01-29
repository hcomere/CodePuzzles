using devDept.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    class MaterialLib
    {
        public static void InitMaterials(Viewport a_eyeshotViewport)
        {
            Material floorMaterial = new Material();
            floorMaterial.Ambient = Color.Black;
            floorMaterial.Diffuse = Color.Green;
            floorMaterial.Specular = Color.Black;
            floorMaterial.Shininess = 0;
            floorMaterial.Environment = 0;
            a_eyeshotViewport.Materials.Add("floor", floorMaterial);

            Material ennemyUnitMaterial = new Material();
            ennemyUnitMaterial.Ambient = Color.Black;
            ennemyUnitMaterial.Diffuse = Color.Red;
            ennemyUnitMaterial.Specular = Color.Black;
            ennemyUnitMaterial.Shininess = 0;
            ennemyUnitMaterial.Environment = 0;
            a_eyeshotViewport.Materials.Add("ennemyUnit", ennemyUnitMaterial);

            Material playerUnitMaterial = new Material();
            playerUnitMaterial.Ambient = Color.Black;
            playerUnitMaterial.Diffuse = Color.Blue;
            playerUnitMaterial.Specular = Color.Black;
            playerUnitMaterial.Shininess = 0;
            playerUnitMaterial.Environment = 0;
            a_eyeshotViewport.Materials.Add("playerUnit", playerUnitMaterial);
        }
    }
}
