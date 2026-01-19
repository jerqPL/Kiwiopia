using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Animator unitAnimator;
    public void QuitAnimation()
    {
        unitAnimator.SetBool("isDead", true);
        Invoke("Quit", 6);
    }

    private void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
