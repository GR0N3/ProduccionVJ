using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transicion : MonoBehaviour
{
    [SerializeField] private string NombreEsena;

    private Animator animator;

    private void Start()
    {
        animator= GetComponent<Animator>();

    }
    public void StarTransicion()
    {
        animator.SetTrigger("Start");
    }

    public void ChangeTransicion()
    {
        SceneManager.LoadScene(NombreEsena);


    }

    private void Update()
    {
        
    }

}
