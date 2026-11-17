
using System.Collections;
using UnityEngine;

public class SpikesController : MonoBehaviour
{
    [Header("Spike timing")]
    public float hiddenTime = 0.7f;
    public float raisedTime = 1.6f;
    public float animationTime = 0.4f;

    public bool IsDangerous { get; private set; }

    private Animator animator;

    private IEnumerator Start()
    {
        animator = GetComponent<Animator>();

        while (true)
        {
            // Bodlje su u zemlji.
            IsDangerous = false;
            animator.Play("SpikesDown", 0, 1f);

            yield return new WaitForSeconds(hiddenTime);

            // Bodlje izlaze iz zemlje.
            animator.Play("SpikesUp", 0, 0f);

            yield return new WaitForSeconds(animationTime);

            // Bodlje su potpuno podignute.
            IsDangerous = true;

            yield return new WaitForSeconds(raisedTime);

            // Bodlje se spustaju.
            IsDangerous = false;
            animator.Play("SpikesDown", 0, 0f);

            yield return new WaitForSeconds(animationTime);
        }
    }
}
