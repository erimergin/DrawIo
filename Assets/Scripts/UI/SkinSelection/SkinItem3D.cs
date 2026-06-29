using UnityEngine;

public class SkinItem3D : MonoBehaviour
{
    public int Index { get; private set; }

    public void Setup(int index)
    {
        Index = index;
    }
}
