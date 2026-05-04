using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public Animator unitAnimator;
    [SerializeField] private MainMenuHandler mainMenuHandler;
    [SerializeField] private SoundHandler soundHandler;

    [Header("Menus")]
    [SerializeField] private RectTransform mainMenu;
    [SerializeField] private RectTransform multiplayerMenu;
    [SerializeField] private RectTransform singleplayerMenu;

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

    public void Return() 
    {
        if (mainMenuHandler.currentUnitPosition == MainMenuHandler.UnitPosition.MAIN)
        {
            return;
        }

        void OnCameraMoved()
        {
            mainMenu.gameObject.SetActive(true);
            mainMenuHandler.AfterCameraMoved -= OnCameraMoved;
        }
        mainMenuHandler.moveUnitsToMain();
        DisableAllMenus();
        mainMenuHandler.AfterCameraMoved += OnCameraMoved;
    }

    public void ChangeToMultiplayer()
    {
        if (mainMenuHandler.currentUnitPosition == MainMenuHandler.UnitPosition.MULTIPLAYER)
        {
            return;
        }
        void OnCameraMoved()
        {
            multiplayerMenu.gameObject.SetActive(true);
            mainMenuHandler.AfterCameraMoved -= OnCameraMoved;
        }
        DisableAllMenus();
        mainMenuHandler.moveUnitsToMultiplayer();
        mainMenuHandler.AfterCameraMoved += OnCameraMoved;
    }

    public void ChangeToSingleplayer()
    {
        if (mainMenuHandler.currentUnitPosition == MainMenuHandler.UnitPosition.SINGLEPLAYER)
        {
            return;
        }
        void OnCameraMoved()
        {
            singleplayerMenu.gameObject.SetActive(true);
            mainMenuHandler.AfterCameraMoved -= OnCameraMoved;
        }
        DisableAllMenus();
        mainMenuHandler.moveUnitsToSingleplayer();
        mainMenuHandler.AfterCameraMoved += OnCameraMoved;
    }

    void DisableAllMenus()
    {
        mainMenu.gameObject.SetActive(false);
        multiplayerMenu.gameObject.SetActive(false);
        singleplayerMenu.gameObject.SetActive(false);
    }

    public void PlaySound(int index)
    {
        soundHandler.playAudioClip(index);
    }
}
