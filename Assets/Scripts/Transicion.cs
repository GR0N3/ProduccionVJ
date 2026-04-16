using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transicion : MonoBehaviour
{
    [SerializeField] private string NombreEsena;
    [SerializeField] private Animator anim;

    

   
    public void StarTransicion()
    {
        anim.SetTrigger("Start");
    }

    public void ChangeTransicion()
    {
        SceneManager.LoadScene(NombreEsena);


    }

    private void Update()
    {
        
    }

}
