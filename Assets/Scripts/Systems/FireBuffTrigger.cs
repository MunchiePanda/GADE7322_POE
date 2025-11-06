using UnityEngine;

public class FireBuffTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsBuffed())
        {
            enemy.ApplyBuff();
        }
    }
}
