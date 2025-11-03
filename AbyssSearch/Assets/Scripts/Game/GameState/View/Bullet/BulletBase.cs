using UnityEngine;

public class BulletBase:MonoBehaviour
{
    public void Reset()
    {
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localRotation = new Quaternion(0, 0, 0, 0);
    }
}