using UnityEngine;

public class FallOffGenerator : MonoBehaviour
{
    public static void ApplyFallOff(ref float[,] map, AnimationCurve a_fallOffCurve = null)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float x = j / (float)width * 2 - 1;
                float y = i / (float)height * 2 - 1;

                if (a_fallOffCurve != null)
                    map[i, j] -= a_fallOffCurve.Evaluate(Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)));
                else
                    map[i, j] -= Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                map[i, j] = Mathf.Clamp01(map[i, j]);
            }
        }
    }

    public static float[,] CreateFallOffMap(int width, int height)
    {
        float[,] map = new float[width, height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float x = (i / (float)width) * 2 - 1;
                float y = (j / (float)height) * 2 - 1;

                map[i, j] = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
            }
        }

        return map;
    }
}
