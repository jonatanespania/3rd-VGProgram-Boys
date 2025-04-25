using UnityEngine;

public class ActivarAnimacion : MonoBehaviour
{
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
   void OnTriggerEnter(Collider other)
   {
       // Activar la transición de animación
       animator.SetTrigger("Activar");
   }
}