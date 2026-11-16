using System.Collections.Generic;
using UnityEngine;

internal partial class RoadRushArt
{
    private bool VillageFacility(Transform tileRoot, Dictionary<Material,List<CombineInstance>> batch, int tile, int side)
    {
        int facility = (tile, side) switch
        {
            (1, 1) => 1,
            (2, -1) => 2,
            (3, 1) => 3,
            _ => 0
        };
        if (facility == 0) return false;

        string title = new[] { "", "Koperasi Desa", "Polsek", "Dapur MBG" }[facility];
        Material accent = facility switch
        {
            1 => Paint("Village cooperative green", 0x286357, .27f),
            2 => Paint("Village police blue", 0x35516d, .30f),
            _ => Paint("Village kitchen blue", 0x3c7890, .27f)
        };
        var wall = Paint("Village civic cream", 0xf1dbb7, .20f);
        var trim = Paint("Village civic stone", 0xa6a69a, .19f);
        var bright = Paint("Village civic lettering panel", 0xeee6cc, .24f);
        var red = Paint("Village flag red", 0xd94a49, .20f);
        var roofTile = Paint("Village roof", 0xa94e3c, .19f);
        var crate = Paint("Village produce crate", 0xb1814d, .19f);

        const float z = 1.5f;
        var center = new Vector3(side * 13.4f, -.15f, z);
        var facing = Quaternion.Euler(0, side * 58, 0);
        var marker = new GameObject(title).transform;
        marker.SetParent(tileRoot, false);

        void Local(Material m, float x, float y, float depth, float w, float h, float length, Mesh mesh = null)
        {
            Collect(batch, m, mesh == null ? cube : mesh, center + facing * new Vector3(x,y,depth),
                new Vector3(w,h,length), facing);
        }

        Local(trim, 0,.015f,-1.8f,9.6f,.15f,9.5f);
        Local(wall, 0,1.58f,0,7.5f,2.94f,5.4f);
        Local(accent, 0,.39f,0,7.54f,.52f,5.44f);
        Local(facility == 2 ? accent : roofTile, 0,3.46f,0,8.14f,1.05f,6.03f,gable);
        Local(accent, 0,2.42f,-3.25f,7.66f,.18f,1.54f);
        Local(trim, 0,.12f,-3.05f,6.65f,.19f,1.3f);
        Local(glass, 0,1.02f,-2.728f,1.48f,1.80f,.095f);
        Local(metal, 0,1.02f,-2.791f,.055f,1.78f,.054f);
        Local(metal, .18f,1.10f,-2.806f,.21f,.055f,.062f);

        for (int edge = -1; edge <= 1; edge += 2)
        {
            Local(accent, 3.28f*edge,1.30f,-3.86f,.17f,2.42f,.21f);
            Local(trim, 2.43f*edge,1.38f,-2.72f,1.78f,1.28f,.085f);
            Local(glass, 2.43f*edge,1.38f,-2.78f,1.57f,1.06f,.063f);
            Local(metal, 2.43f*edge,1.38f,-2.83f,.055f,1.08f,.042f);
            Local(glass, 3.77f*edge,1.48f,.40f,.067f,1.03f,1.71f);
        }

        Local(bright, 0,2.76f,-2.75f,5.5f,.51f,.15f);
        Lettering(marker,title.ToUpperInvariant(),center+facing*new Vector3(0,2.77f,-2.844f),58*side,.058f);

        var sign = new Vector3(side*8.5f,2.02f,z-6.7f);
        Collect(batch, accent, cube, sign,new Vector3(3.8f,.81f,.17f),Quaternion.identity);
        Collect(batch, bright, cube, sign+new Vector3(0,.36f,-.095f),new Vector3(3.57f,.065f,.035f),Quaternion.identity);
        foreach (float edge in new[] { -1.43f, 1.43f })
            Collect(batch, rail, cube, sign + new Vector3(edge,-1.05f,.03f),
                new Vector3(.14f,1.87f,.14f),Quaternion.identity);
        Lettering(marker,title.ToUpperInvariant(), sign + new Vector3(0,-.025f,-.105f),0,.043f,
            new Color(.98f,.93f,.81f));

        switch (facility)
        {
            case 1:
                Local(crate, 2.47f,.43f,-3.16f,1.46f,.61f,.62f);
                Local(brush, 2.47f,.75f,-3.16f,1.37f,.16f,.56f);
                Local(crate, 3.30f,.29f,-3.25f,.57f,.44f,.65f);
                Local(accent,-2.46f,.65f,-3.22f,1.41f,.83f,.63f);
                Local(bright,-2.46f,1.11f,-3.24f,1.56f,.10f,.82f);
                break;
            case 2:
                Local(rail,-3.95f,2.06f,-3.50f,.065f,2.06f,.065f,cylinder);
                Local(red,-3.52f,3.59f,-3.50f,.87f,.30f,.046f);
                Local(white,-3.52f,3.30f,-3.50f,.87f,.30f,.046f);
                Local(red,0,2.38f,-3.94f,1.27f,.10f,.065f);
                break;
            case 3:
                Local(metal,2.96f,1.02f,-.06f,.53f,1.66f,1.61f);
                Local(metal,1.98f,3.48f,1.35f,.37f,1.07f,.39f);
                Local(roof,1.98f,4.03f,1.35f,.57f,.095f,.59f);
                Local(crate,2.58f,.40f,-3.26f,1.29f,.56f,.75f);
                Local(bright,2.58f,.76f,-3.26f,1.25f,.15f,.71f);
                break;
        }
        return true;
    }
}
