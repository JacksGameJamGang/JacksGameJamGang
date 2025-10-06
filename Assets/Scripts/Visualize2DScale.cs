using UnityEngine;

[ExecuteAlways]
public class Visualize2DScale : MonoBehaviour
{
    public Color color = Color.green;

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        // Draw a 2D rectangle in XY plane representing scale
        Vector2 size = new Vector2(transform.localScale.x, transform.localScale.y);
        Gizmos.DrawWireCube(transform.position, size);
    }
}
