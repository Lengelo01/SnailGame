
using UnityEngine;

public class SpiderRoomTrigger : MonoBehaviour
{
    public SpiderChase spider;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spider.StartChasing();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spider.StopChasing();
        }
    }
}
