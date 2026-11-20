using UnityEngine;

/// <summary>
/// Builds Grid, VShape, and Compact formation slot positions.
/// </summary>
public static class FormationUtility
{
    public static Vector3[] BuildSlots(FormationType type, int count, float spacing, Vector3 center)
    {
        Vector3[] slots = new Vector3[Mathf.Max(0, count)];
        if (count <= 0)
        {
            return slots;
        }

        switch (type)
        {
            case FormationType.VShape:
                BuildVShape(slots, spacing, center);
                break;
            case FormationType.Compact:
                BuildCompact(slots, spacing, center);
                break;
            default:
                BuildGrid(slots, spacing, center);
                break;
        }

        return slots;
    }

    private static void BuildGrid(Vector3[] slots, float spacing, Vector3 center)
    {
        int count = slots.Length;
        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt(count / (float)columns);
        float width = (columns - 1) * spacing;
        float height = (rows - 1) * spacing;
        Vector3 origin = center + new Vector3(-width * 0.5f, height * 0.5f, 0f);

        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int col = i % columns;
            slots[i] = origin + new Vector3(col * spacing, -row * spacing, 0f);
        }
    }

    private static void BuildVShape(Vector3[] slots, float spacing, Vector3 center)
    {
        slots[0] = center;
        int side = 1;
        for (int i = 1; i < slots.Length; i++)
        {
            int rank = (i + 1) / 2;
            float x = side * rank * spacing;
            float y = -rank * spacing * 0.75f;
            slots[i] = center + new Vector3(x, y, 0f);
            side = -side;
        }
    }

    private static void BuildCompact(Vector3[] slots, float spacing, Vector3 center)
    {
        float tight = spacing * 0.65f;
        BuildGrid(slots, tight, center);
    }
}
