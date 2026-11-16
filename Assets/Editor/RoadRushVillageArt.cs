using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

internal partial class RoadRushArt
{
    public GameObject[] BuildVillageFleet()
    {
        var result = new GameObject[4];
        var orange = Paint("Coach orange", 0xf17725, .46f);
        var gold = Paint("Coach golden stripe", 0xf8b94d, .35f);
        var ivory = Paint("Coach ivory", 0xf3eee4, .48f);
        var teal = Paint("Delivery teal", 0x167c79, .51f);
        var boxPaint = Paint("Delivery aluminum", 0x689893, .39f);
        var green = Paint("Becak green", 0x238465, .35f);
        var canvas = Paint("Becak canvas", 0xefd18d, .12f);
        var blue = Paint("Motorbike cobalt", 0x3d71c4, .52f);
        var rider = Paint("Rider jacket", 0x594c65, .23f);
        var helmet = Paint("Helmet cream", 0xe3dfbf, .58f);

        result[0] = SaveTraffic("Bus Harapan Jaya", EnemyMovement.SoundKind.Bus, new Vector3(2.16f, 2.74f, 7.10f), 1.54f, root =>
        {
            Part(root, "Coach body", ivory, 0,1.52f,0,2.25f,2.30f,7.22f, carBody);
            Part(root, "Lower coach body", orange, 0,.71f,0,2.24f,.78f,7.18f, carBody);
            Part(root, "Roof AC", ivory, 0,2.72f,.44f,1.3f,.16f,2.4f, carBody);
            Part(root, "Windshield", glass, 0,1.96f,3.48f,2.03f,1.03f,.198f, cabin);
            Part(root, "Rear window", glass, 0,2.00f,-3.49f,1.91f,.82f,.20f, cabin);
            RearLights(root, -3.58f, .86f, .87f);
            Part(root, "Coach plate", metal, 0,.52f,-3.60f,.41f,.17f,.035f);
            Lettering(root, "HARAPAN JAYA", new Vector3(0,1.43f,-3.59f), 0, .052f);
            foreach (int side in new[] { -1, 1 })
            {
                Part(root, "Passenger windows", glass, side*1.113f,2.06f,-.15f,.062f,.89f,5.91f);
                for (int window = 0; window < 5; window++)
                    Part(root, "Window pillars", roof, side*1.151f,2.07f,-2.7f + window*1.14f,.036f,.91f,.095f);
                Part(root, "Orange livery", orange, side*1.132f,.96f,0,.046f,.47f,6.58f);
                Part(root, "Golden livery", gold, side*1.153f,1.19f,0,.039f,.106f,6.45f);
                Part(root, "Mirror support", lip, side*1.19f,2.04f,2.97f,.39f,.065f,.11f);
                Part(root, "Coach mirror", lip, side*1.38f,1.99f,2.98f,.13f,.24f,.18f, carBody);
                Part(root, "Headlight", headlight, .79f*side,.77f,3.57f,.37f,.16f,.105f);
                Lettering(root, "HARAPAN JAYA", new Vector3(side*1.166f,1.48f,.12f), -side*90, .072f);
                AddWheel(root, side*.97f,2.16f,.43f,.26f,side);
                AddWheel(root, side*.97f,-1.78f,.43f,.26f,side);
                AddWheel(root, side*.97f,-2.65f,.39f,.25f,side);
            }
        });

        result[1] = SaveTraffic("Truk Boks", EnemyMovement.SoundKind.Truck, new Vector3(1.99f, 2.48f, 5.44f), 1.42f, root =>
        {
            Part(root, "Chassis", lip, 0,.55f,0,1.87f,.38f,5.63f, carBody);
            Part(root, "Truck cab", teal, 0,1.16f,1.55f,2.05f,1.51f,2.27f, carBody);
            Part(root, "Truck glass", glass, 0,1.65f,2.49f,1.83f,.54f,.27f,cabin);
            Part(root, "Cargo box", boxPaint, 0,1.55f,-.98f,2.10f,2.04f,3.49f);
            Part(root, "Cargo roof", metal, 0,2.59f,-.98f,2.12f,.08f,3.55f);
            Part(root, "Cargo doors", teal, 0,1.59f,-2.744f,2.05f,1.92f,.08f);
            Part(root, "Rear step", lip, 0,.45f,-2.81f,2.13f,.16f,.28f);
            Lettering(root, "LOGISTIK", new Vector3(0,1.85f,-2.802f), 0, .070f);
            RearLights(root,-2.96f,.50f,.80f);
            foreach (int side in new[] { -1, 1 })
            {
                Part(root, "Cab side glass", glass, side*1.02f,1.57f,1.76f,.065f,.52f,1.23f,cabin);
                Part(root, "Door lock", metal, side*.28f,1.36f,-2.821f,.055f,1.31f,.05f);
                Part(root, "Cargo stripe", metal, side*1.056f,1.04f,-.97f,.04f,.14f,3.42f);
                for (int rib = 0; rib < 4; rib++)
                    Part(root, "Cargo rib", teal,1.062f*side,1.60f,-2.45f + rib*.76f,.04f,1.88f,.07f);
                Part(root, "Truck headlamp", headlight,.77f*side,.80f,2.62f,.35f,.19f,.12f);
                AddWheel(root,.91f*side,1.70f,.40f,.25f,side);
                AddWheel(root,.91f*side,-1.55f,.43f,.29f,side);
            }
        });

        result[2] = SaveTraffic("Sepeda Motor", EnemyMovement.SoundKind.Motorcycle, new Vector3(.75f, 1.46f, 2.13f), .86f, root =>
        {
            AddWheel(root,0,-.72f,.32f,.19f,0);
            AddWheel(root,0,.75f,.32f,.17f,0);
            Part(root, "Swing arm", metal,0,.39f,-.31f,.23f,.12f,.95f);
            Part(root, "Bike frame", lip,0,.59f,0,.32f,.37f,1.35f,carBody);
            Part(root, "Fuel tank", blue,0,.85f,.18f,.55f,.37f,.95f,carBody);
            Part(root, "Rear cowl", blue,0,.68f,-.73f,.44f,.20f,.59f,carBody);
            Part(root, "Seat", lip,0,.84f,-.40f,.40f,.12f,.65f,carBody);
            Part(root, "Handlebars", metal,0,1.08f,.54f,.78f,.058f,.067f);
            Part(root, "Headlamp", headlight,0,.82f,.91f,.36f,.17f,.13f);
            Part(root, "Tail light", tail,0,.72f,-1.04f,.33f,.11f,.065f);
            Rider(root, -.23f,.82f,rider,helmet);
        });

        result[3] = SaveTraffic("Becak", EnemyMovement.SoundKind.Becak, new Vector3(1.50f, 1.54f, 2.78f), .86f, root =>
        {
            AddWheel(root,0,-1.02f,.34f,.12f,0);
            AddWheel(root,-.68f,.69f,.36f,.13f,-1);
            AddWheel(root,.68f,.69f,.36f,.13f,1);
            Part(root, "Pedal frame", metal,0,.42f,-.66f,.27f,.18f,1.39f);
            Part(root, "Passenger carriage", green,0,.55f,.57f,1.28f,.53f,1.48f,carBody);
            Part(root, "Passenger seat", lip,0,.80f,.24f,1.07f,.22f,.31f,carBody);
            Part(root, "Canopy", canvas,0,1.39f,.58f,1.41f,.24f,1.51f,carBody);
            Part(root, "Canopy valance", green,0,1.26f,1.25f,1.31f,.13f,.09f);
            Part(root, "Canopy rear", canvas,0,1.18f,-.055f,1.24f,.41f,.09f,carBody);
            Part(root, "Rear reflector", tail,0,.45f,-1.34f,.18f,.10f,.045f);
            foreach (int side in new[] { -1, 1 })
            {
                Part(root, "Canopy rail", metal,.60f*side,1.02f,.98f,.075f,.64f,.075f);
                Part(root, "Front reflector", yellow,.44f*side,.58f,1.31f,.12f,.095f,.08f);
            }
            Rider(root,-.78f,.69f,rider,canvas);
        });
        return result;
    }

    private static GameObject SaveTraffic(string name, EnemyMovement.SoundKind sound, Vector3 size, float centerY,
        System.Action<Transform> build)
    {
        var obj = new GameObject(name, typeof(BoxCollider), typeof(Rigidbody), typeof(EnemyMovement));
        try
        {
            build(obj.transform);
            obj.GetComponent<EnemyMovement>().ConfigureSound(sound);
            var box = obj.GetComponent<BoxCollider>();
            box.center = new Vector3(0,centerY,0);
            box.size = size;
            var body = obj.GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            return PrefabUtility.SaveAsPrefabAsset(obj, Folder + "/" + name + ".prefab");
        }
        finally { Object.DestroyImmediate(obj); }
    }

    private Transform Part(Transform root, string name, Material m,
        float x, float y, float z, float width, float height, float length, Mesh mesh = null) =>
        Add(root,name,mesh == null ? cube : mesh,m,new Vector3(x,y,z),new Vector3(width,height,length));

    private void RearLights(Transform root, float z, float y, float x)
    {
        foreach (int side in new[] { -1, 1 })
        {
            Part(root,"Rear lights",tail,x*side,y,z,.28f,.22f,.075f);
            Part(root,"Rear amber",yellow,x*side,y-.144f,z,.24f,.064f,.075f);
        }
    }

    private static void Lettering(Transform root, string label, Vector3 pos, float angle, float scale, Color? tint = null)
    {
        var item = new GameObject(label);
        item.transform.SetParent(root, false);
        item.transform.localPosition = pos;
        item.transform.localRotation = Quaternion.Euler(0,angle,0);
        var text = item.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = label;
        text.fontSize = 64;
        text.fontStyle = FontStyle.Bold;
        text.characterSize = scale;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = tint ?? new Color(.06f,.064f,.09f);
        var renderer = item.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = text.font.material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private void Rider(Transform root, float z, float seat, Material clothes, Material head)
    {
        Part(root,"Rider torso",clothes,0,seat+.25f,z,.45f,.54f,.37f,cabin).localRotation = Quaternion.Euler(14,0,0);
        Part(root,"Rider head",head,0,seat+.64f,z+.10f,.34f,.36f,.36f,cabin);
        foreach (int side in new[] { -1, 1 })
        {
            Part(root,"Rider legs",lip,.22f*side,seat-.12f,z+.10f,.13f,.39f,.42f,carBody);
            Part(root,"Rider sleeves",clothes,.22f*side,seat+.27f,z+.23f,.15f,.13f,.51f,carBody);
        }
    }

    private void AddWheel(Transform root, float x, float z, float radius, float width, int side)
    {
        var pivot = new GameObject("Rolling wheel").transform;
        pivot.SetParent(root,false);
        pivot.localPosition = new Vector3(x,radius,z);
        pivot.gameObject.AddComponent<RollingWheel>().Configure(radius);
        Part(pivot,"Tire",rubber,0,0,0,radius*2,width*.5f,radius*2,cylinder)
            .localRotation = Quaternion.Euler(0,0,90);
        Part(pivot,"Alloy rim",metal,0,0,0,radius*1.25f,width*.56f,radius*1.25f,cylinder)
            .localRotation = Quaternion.Euler(0,0,90);
        foreach (float offset in side == 0 ? new[] {-1f,1f} : new[] {(float)side})
        {
            float edge = offset*width*.59f;
            for (int spoke = 0; spoke < 3; spoke++)
                Part(pivot,"Wheel spoke",lip,edge,0,0,.023f,radius*1.06f,.041f)
                    .localRotation = Quaternion.Euler(spoke*60,0,0);
        }
    }

    private void Village(Transform tileRoot, Dictionary<Material,List<CombineInstance>> batch, int tile, int side)
    {
        var grass = Paint("Village grass",0x707c4f,.10f);
        var leaf = Paint("Village leaf",0x40694b,.09f);
        var tips = Paint("Village leaf sunlight",0x718c51,.12f);
        var wood = Paint("Village wood",0x6a4937,.14f);
        var tileRoof = Paint("Village roof",0xa94e3c,.19f);
        var darkRoof = Paint("Village dark roof",0x793d35,.16f);
        var plaster = Paint("Village plaster",0xe8cfaa,.22f);
        var turquoise = Paint("Village turquoise",0x95b5a2,.17f);
        var peach = Paint("Village peach",0xdca285,.20f);
        var glow = Paint("Village warm windows",0xf8c578,.32f,emission:.38f);

        void Place(Material mat, Mesh mesh, Vector3 p, Vector3 size, Quaternion rotation) =>
            Collect(batch,mat,mesh,p,size,rotation);

        Place(grass,cube,new Vector3(side*14.5f,-.168f,0),
            new Vector3(17.7f,.05f,HighwayPresentation.TileLength),Quaternion.identity);
        bool hasFacility = VillageFacility(tileRoot, batch, tile, side);
        int houseCount = hasFacility ? 0 : ((tile + (side > 0 ? 1 : 0)) % 3 == 0 ? 2 : 1);
        for (int i = 0; i < houseCount; i++)
        {
            float z = houseCount == 1 ? (tile % 2 == 0 ? 2 : -5) : -8 + i*15;
            var center = new Vector3(side*(10.8f+i*2.0f+tile%2),-.12f,z);
            var facing = Quaternion.Euler(0,90*side,0);
            var wall = new[] {plaster,turquoise,peach}[(tile+i+(side > 0 ? 1 : 0))%3];
            var rafters = tile % 2 == 0 ? tileRoof : darkRoof;
            void HousePart(Material mat, Vector3 at, Vector3 size, float roll = 0, Mesh mesh = null) =>
                Place(mat,mesh == null ? cube : mesh,center + facing*at,size,facing*Quaternion.Euler(0,0,roll));

            HousePart(gravel,new Vector3(0,.07f,-1.3f),new Vector3(6.9f,.15f,6.8f));
            HousePart(wall,new Vector3(0,1.53f,0),new Vector3(5.6f,2.85f,4.4f));
            HousePart(wood,new Vector3(0,.88f,-2.225f),new Vector3(1.05f,1.70f,.095f));
            HousePart(rafters,new Vector3(0,2.56f,-2.65f),new Vector3(5.85f,.14f,1.28f));
            HousePart(rafters,new Vector3(0,3.48f,0),new Vector3(6.34f,1.36f,5.08f),0,gable);
            for (int k = -1; k <= 1; k += 2)
            {
                HousePart(wood,new Vector3(k*1.68f,1.53f,-2.22f),new Vector3(1.13f,1.00f,.10f));
                HousePart(glow,new Vector3(k*1.68f,1.53f,-2.28f),new Vector3(.87f,.75f,.043f));
                HousePart(wood,new Vector3(k*1.68f,1.53f,-2.31f),new Vector3(.06f,.79f,.043f));
                HousePart(wood,new Vector3(k*2.42f,1.3f,-3.15f),new Vector3(.12f,2.56f,.12f));
                HousePart(glass,new Vector3(k*2.813f,1.55f,.57f),new Vector3(.066f,.88f,1.38f));
            }
        }

        for (int t = 0; t < 3; t++)
        {
            float z = (t-1)*11 + (tile%2 == 0 ? -2 : 2);
            if (hasFacility && t != 2) continue;
            float x = side*(t == 1 ? 17.8f : 8.0f + (tile+t)%2);
            float height = 2.5f + ((tile+t)%3)*.37f;
            Place(wood,cylinder,new Vector3(x,height*.45f,z),new Vector3(.27f,height*.48f,.28f),
                Quaternion.Euler(0,0,5*side));
            for (int crown = 0; crown < 3; crown++)
            {
                var p = new Vector3(x+(crown-1)*.76f,height-.13f+(crown==1 ? .64f : 0),z+(crown-1)*.40f);
                Place(crown==1 ? tips : leaf,boulder,p,new Vector3(1.62f,1.89f,1.32f),
                    Quaternion.Euler(0,45+crown*85+tile*11,0));
            }
        }
    }

    private static Mesh GableMesh()
    {
        var mesh = new Mesh { name = "Terracotta gable" };
        mesh.vertices = new[]
        {
            new Vector3(-.5f,-.5f,-.5f), new Vector3(.5f,-.5f,-.5f), new Vector3(0,.5f,-.5f),
            new Vector3(-.5f,-.5f,.5f), new Vector3(.5f,-.5f,.5f), new Vector3(0,.5f,.5f)
        };
        mesh.triangles = new[] {0,2,1,3,4,5,0,3,5,0,5,2,1,2,5,1,5,4,0,1,4,0,4,3};
        mesh.RecalculateNormals();
        return mesh;
    }
}
