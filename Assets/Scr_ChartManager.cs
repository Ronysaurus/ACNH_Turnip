using UnityEngine;

public class Scr_ChartManager : MonoBehaviour
{
    public static GameObject[] chartPoints = new GameObject[25];
    public static LineRenderer[] lines;

    public static void Initiate()
    {
        GameObject parent = GameObject.Find("ChartObjects");
        lines = new LineRenderer[24];
        for (int i = 0; i < 24; i++)
        {
            GameObject myObject = new GameObject
            {
                name = "line " + i
            };
            myObject.transform.parent = parent.transform;
            lines[i] = myObject.AddComponent<LineRenderer>();
            lines[i].startWidth = 0.05f;
            lines[i].endWidth = 0.05f;
        }

        for (int i = 0; i < 25; i++)
        {
            chartPoints[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            chartPoints[i].transform.parent = parent.transform;
            chartPoints[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            chartPoints[i].transform.position = new Vector3(200, 200, 200);
            chartPoints[i].AddComponent<TextMesh>();
            chartPoints[i].AddComponent<MeshRenderer>();
        }
    }
}