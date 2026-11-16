using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

internal partial class RoadRushArt
{
    internal const string Folder = "Assets/RoadRush";
    private readonly Mesh cube, cylinder, carBody, cabin, boulder, gable;
    private readonly Material lip, rubber, metal, glass, roof, tail, headlight, playerPaint;
    private readonly Material asphalt, gravel, sand, white, yellow, rail, rust, rock, rockLight, brush;
    public Material[] TrafficPaints { get; }

    public RoadRushArt()
    {
        cube = Primitive(PrimitiveType.Cube);
        cylinder = Primitive(PrimitiveType.Cylinder);
        carBody = SaveMesh(BeveledMesh("Pressed body", .12f, .13f, .20f, .84f), "CarBody");
        cabin = SaveMesh(BeveledMesh("Sloped cabin", .18f, .22f, .20f, .73f), "CarCabin");
        boulder = SaveMesh(RockMesh(), "Sandstone");
        gable = SaveMesh(GableMesh(), "Terracotta gable");
        lip = Paint("Carbon trim", 0x182435, .35f);
        rubber = Paint("Tire rubber", 0x152029, .12f);
        metal = Paint("Brushed alloy", 0x98abb4, .56f, .40f);
        glass = Paint("Tinted glass", 0x294554, .83f, .28f);
        roof = Paint("Cabin roof", 0x344954, .55f, .2f);
        tail = Paint("Tail light", 0xf33737, .44f, emission: 1.15f);
        headlight = Paint("Headlight", 0xdef9ed, .5f, emission: .52f);
        playerPaint = Paint("Rush orange", 0xe94827, .63f, .25f);
        TrafficPaints = new[]
        {
            Paint("Patrol blue", 0x2085af, .58f, .24f),
            Paint("Sunset ochre", 0xd49734, .56f, .24f),
            Paint("Jade green", 0x32977d, .56f, .24f),
            Paint("Slate silver", 0x9daabc, .6f, .29f),
            Paint("Orchid", 0x835280, .57f, .24f)
        };
        asphalt = Paint("Highway asphalt", 0x323947, .1f);
        gravel = Paint("Paved shoulder", 0x565361, .15f);
        sand = Paint("Desert floor", 0x835749, .13f);
        white = Paint("Lane paint", 0xd8e4de, .15f);
        yellow = Paint("Safety amber", 0xf4ba56, .21f, emission: .15f);
        rail = Paint("Guard rail", 0x758394, .44f, .32f);
        rust = Paint("Red sandstone", 0x89514a, .05f);
        rock = Paint("Dark sandstone", 0x684858, .05f);
        rockLight = Paint("Mesa cap", 0xa76354, .06f);
        brush = Paint("Desert scrub", 0x5a6250, .11f);
    }

    public Material Sky()
    {
        const string path = Folder + "/Desert Sky.mat";
        var sky = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (sky != null) return sky;
        sky = new Material(Shader.Find("ROAD RUSH/Desert Sky"));
        AssetDatabase.CreateAsset(sky, path);
        return sky;
    }

    public Transform BuildVehicle(Transform car, bool player)
    {
        var art = car.Find("Vehicle Art");
        if (art != null)
        {
            if (player)
            {
                art.Find("CarBody")?.SetParent(car, false);
                art.Find("CarCabin")?.SetParent(car, false);
            }
            Object.DestroyImmediate(art.gameObject);
        }
        var group = new GameObject("Vehicle Art").transform;
        group.SetParent(car, false);
        var body = car.Find(player ? "CarBody" : "EnemyBody") ??
            Add(car, player ? "CarBody" : "EnemyBody", carBody, player ? playerPaint : TrafficPaints[0],
                Vector3.zero, Vector3.one);
        var glassRoot = car.Find(player ? "CarCabin" : "EnemyCabin") ??
            Add(car, player ? "CarCabin" : "EnemyCabin", cabin, glass, Vector3.zero, Vector3.one);

        // The traffic paint reference stays at the original EnemyBody path.
        body.SetParent(player ? group : car, false);
        SetPart(body, carBody, player ? playerPaint : TrafficPaints[0], new Vector3(0,.57f,0),
            new Vector3(1.76f,.56f,3.32f));
        glassRoot.SetParent(player ? group : car, false);
        SetPart(glassRoot, cabin, glass, new Vector3(0,.99f,-.19f),
            new Vector3(1.48f,.59f,1.83f));

        Add(group, "Roof cap", carBody, roof, new Vector3(0,1.264f,-.24f), new Vector3(1.07f,.10f,1.26f));
        Add(group, "Rear diffuser", carBody, lip, new Vector3(0,.42f,-1.48f), new Vector3(1.63f,.22f,.44f));
        Add(group, "Front splitter", carBody, lip, new Vector3(0,.40f,1.48f), new Vector3(1.63f,.15f,.43f));
        Add(group, "Rear fascia", cube, lip, new Vector3(0,.63f,-1.629f), new Vector3(1.34f,.19f,.045f));
        Add(group, "License plate", cube, metal, new Vector3(0,.58f,-1.669f), new Vector3(.33f,.115f,.022f));
        foreach (int side in new[] { -1, 1 })
        {
            Add(group, "Rear lamp", cube, tail, new Vector3(.57f*side,.693f,-1.66f), new Vector3(.41f,.091f,.04f));
            Add(group, "Front lamp", cube, headlight, new Vector3(.57f*side,.67f,1.65f), new Vector3(.41f,.10f,.04f));
            Add(group, "Side skirt", carBody, lip, new Vector3(.86f*side,.39f,0), new Vector3(.12f,.13f,1.52f));
            Add(group, "Side mirror", carBody, roof, new Vector3(.77f*side,1f,.54f), new Vector3(.28f,.13f,.24f));
            Add(group, "Exhaust", cube, metal, new Vector3(.53f*side,.43f,-1.69f), new Vector3(.19f,.063f,.10f));
            foreach (int axle in new[] { -1, 1 })
            {
                AddWheel(group, side*.80f, axle*1.04f, .31f, .266f, side);
            }
        }
        if (player)
        {
            Add(group, "Spoiler", carBody, lip, new Vector3(0,1.03f,-1.46f), new Vector3(1.55f,.085f,.31f));
            foreach (int side in new[] { -1, 1 })
                Add(group, "Spoiler mount", cube, lip, new Vector3(side*.51f,.91f,-1.45f),
                    new Vector3(.09f,.22f,.095f));
            foreach (Transform part in car)
            {
                if (part == group || part.GetComponent<Camera>() != null) continue;
                part.gameObject.SetActive(false);
            }
        }
        return group;
    }

    public Transform[] Highway(Transform parent, float z)
    {
        var tiles = new Transform[6];
        for (int i = 0; i < tiles.Length; i++)
        {
            var tile = new GameObject("Village highway " + (i + 1)).transform;
            tile.SetParent(parent, false);
            tile.position = new Vector3(0, 0, z - 36 + i * HighwayPresentation.TileLength);
            tiles[i] = tile;
            var groups = new Dictionary<Material, List<CombineInstance>>();
            void Box(Material m, float x, float y, float depth, float w, float h, float length) =>
                Collect(groups, m, cube, new Vector3(x,y,depth), new Vector3(w,h,length), Quaternion.identity);

            const float length = HighwayPresentation.TileLength;
            Box(asphalt, 0,-.13f,0,9.25f,.26f,length);
            Box(sand, 0,-.37f,0,270,.40f,length);
            foreach (int side in new[] { -1, 1 })
            {
                Box(gravel, side*5f,-.14f,0,1.15f,.25f,length);
                Box(yellow, side*4.48f,.009f,0,.105f,.017f,length);
                Box(rail, side*5.85f,.58f,0,.11f,.19f,length);
                Box(rail, side*5.88f,.34f,0,.09f,.10f,length);
                for (int k = 0; k < 6; k++)
                {
                    float at = -15f + k * 6f;
                    Box(white, side*1.5f,.011f,at,.103f,.016f,2.8f);
                    Box(rail, side*5.96f,.23f,at,.11f,.83f,.12f);
                    Box(yellow, side*5.78f,.686f,at,.073f,.086f,.19f);
                }

                var rng = new System.Random(i * 313 + side * 23 + 701);
                for (int k = 0; k < 18; k++)
                {
                    float x = side * (23.4f + (float)rng.NextDouble() * 20f);
                    float at = -17 + (float)rng.NextDouble() * 34f;
                    float size = .3f + (float)rng.NextDouble() * 1.9f;
                    Collect(groups, k % 3 == 0 ? brush : rust, boulder, new Vector3(x,.1f,at),
                        new Vector3(size,size*.48f,size*.77f), Quaternion.Euler(0,rng.Next(360),0));
                }
                for (int m = 0; m < 3; m++)
                {
                    float x = side * (38 + m * 23 + rng.Next(10));
                    float at = rng.Next(-13,14);
                    float h = 4 + m * 4 + rng.Next(7);
                    Collect(groups, m == 1 ? rockLight : rock, boulder,
                        new Vector3(x,-1.6f,at), new Vector3(10+m*6,h,9+m*4),
                        Quaternion.Euler(0,rng.Next(360),0));
                }
                Village(tile, groups, i, side);
            }

            if (i == 2 || i == 5)
            {
                for (int post = 0; post < 2; post++)
                    Box(rail, 7.25f + post * 2.0f, 1.25f, 2f, .15f, 3f, .15f);
                Box(lip, 8.25f, 2.77f, 2f, 3.25f, 1.05f, .12f);
                for (int arrow = 0; arrow < 3; arrow++)
                {
                    var center = new Vector3(7.25f + arrow, 2.77f, 1.927f);
                    Collect(groups, yellow, cube, center + new Vector3(-.12f,.13f,0),
                        new Vector3(.16f,.43f,.04f), Quaternion.Euler(0,0,43));
                    Collect(groups, yellow, cube, center + new Vector3(-.12f,-.13f,0),
                        new Vector3(.16f,.43f,.04f), Quaternion.Euler(0,0,-43));
                }
            }

            foreach (var pair in groups)
            {
                var mesh = new Mesh { name = pair.Key.name };
                mesh.CombineMeshes(pair.Value.ToArray());
                mesh = SaveMesh(mesh, $"Tile-{i + 1}-{pair.Key.name}");
                var part = Add(tile, pair.Key.name, mesh, pair.Key, Vector3.zero, Vector3.one);
                part.GetComponent<MeshRenderer>().shadowCastingMode =
                    pair.Key == rail || pair.Key == rock || pair.Key == rockLight ||
                    (pair.Key.name.StartsWith("Village ") && pair.Key.name != "Village grass")
                        ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
        }
        return tiles;
    }

    private static void Collect(Dictionary<Material,List<CombineInstance>> batches, Material material,
        Mesh mesh, Vector3 p, Vector3 s, Quaternion r)
    {
        if (!batches.TryGetValue(material, out var batch)) batches.Add(material, batch = new());
        batch.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(p,r,s) });
    }

    private static Transform Add(Transform parent, string name, Mesh mesh, Material mat, Vector3 p, Vector3 s)
    {
        var part = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)).transform;
        part.SetParent(parent, false);
        SetPart(part, mesh, mat, p, s);
        return part;
    }

    private static void SetPart(Transform part, Mesh mesh, Material mat, Vector3 p, Vector3 s)
    {
        part.GetComponent<MeshFilter>().sharedMesh = mesh;
        part.GetComponent<MeshRenderer>().sharedMaterial = mat;
        part.localPosition = p;
        part.localRotation = Quaternion.identity;
        part.localScale = s;
        part.gameObject.SetActive(true);
    }

    private static Mesh Primitive(PrimitiveType type)
    {
        var primitive = GameObject.CreatePrimitive(type);
        Mesh mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(primitive);
        return mesh;
    }

    private static Material Paint(string name, int hex, float smoothness, float metallic = 0, float emission = 0)
    {
        string path = Folder + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;
        mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        Color color = new Color32((byte)(hex >> 16), (byte)(hex >> 8), (byte)hex, 255);
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        if (emission > 0)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color.linear * emission);
        }
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static Mesh SaveMesh(Mesh mesh, string name)
    {
        mesh.name = name;
        string path = Folder + "/" + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
        EditorUtility.CopySerialized(mesh, existing);
        Object.DestroyImmediate(mesh);
        return existing;
    }

    private static Mesh BeveledMesh(string name, float cutX, float cutZ, float bevel, float topScale)
    {
        var ring = new[]
        {
            new Vector2(-.5f+cutX,-.5f), new Vector2(.5f-cutX,-.5f), new Vector2(.5f,-.5f+cutZ),
            new Vector2(.5f,.5f-cutZ), new Vector2(.5f-cutX,.5f), new Vector2(-.5f+cutX,.5f),
            new Vector2(-.5f,.5f-cutZ), new Vector2(-.5f,-.5f+cutZ)
        };
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        float[] heights = { -.5f, -.5f + bevel, .5f - bevel, .5f };
        float[] scales = { .86f, 1f, Mathf.Lerp(1f, topScale, .62f), topScale };
        for (int y = 0; y < heights.Length; y++)
        for (int i = 0; i < ring.Length; i++)
            vertices.Add(new Vector3(ring[i].x * scales[y], heights[y], ring[i].y * scales[y]));

        int count = ring.Length;
        for (int y = 1; y < heights.Length; y++)
        for (int i = 0; i < count; i++)
        {
            int a = (y - 1) * count + i, b = (y - 1) * count + (i + 1) % count;
            triangles.AddRange(new[] { a, a + count, b, b, a + count, b + count });
        }
        int top = (heights.Length - 1) * count;
        for (int i = 1; i < count - 1; i++)
            triangles.AddRange(new[] { top, top + i + 1, top + i, 0, i, i + 1 });
        var mesh = new Mesh { name = name };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Mesh RockMesh()
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var rng = new System.Random(94);
        const int sides = 8;
        for (int ring = 0; ring < 3; ring++)
        for (int i = 0; i < sides; i++)
        {
            float a = i * Mathf.PI * 2 / sides;
            float width = (ring == 2 ? .63f : 1f) * (.80f + (float)rng.NextDouble() * .24f);
            vertices.Add(new Vector3(Mathf.Cos(a)*width + ring*.10f,
                ring*.44f + (float)rng.NextDouble()*.13f, Mathf.Sin(a)*width));
        }
        for (int ring = 0; ring < 2; ring++)
        for (int i = 0; i < sides; i++)
        {
            int a = ring*sides+i, b = ring*sides+(i+1)%sides;
            triangles.AddRange(new[] { a, a+sides, b, b, a+sides, b+sides });
        }
        for (int i = 1; i < sides-1; i++) triangles.AddRange(new[] { 16, 17+i, 16+i });

        var flat = new List<Vector3>();
        for (int i = 0; i < triangles.Count; i++)
        {
            flat.Add(vertices[triangles[i]]);
            triangles[i] = i;
        }
        var mesh = new Mesh { name = "Sandstone" };
        mesh.SetVertices(flat);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }
}
