using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    
    // Game Manager Data
    public static GameManager Instance { get; private set;}
    
    
    // Scene Management Data
        public enum SceneList { 
        StartMenu,
        PauseMenu,
        GameOverScreen
    }
    [HideInInspector]
    public SceneList sceneList;
    
    // Scene to start on Play - meant to help with scene creation and debugging.
    [SerializeField] SceneList startingScene;
    
    
    
    
    //-----------------------------------------//
    //           Awake, Enable, Start          //
    //-----------------------------------------//
    
    private void Awake() 
    {    
        // -- Singleton Setup --
        if( Instance != null && Instance != this )
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }  
                
    }
     
    private void OnEnable() 
    {      
        // -- Singleton Setup --
        if( Instance != null && Instance != this )
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }
    
    private void Start() 
    {
        
        // Load the start menu
        // Don't load the PauseMenu or GameOver scenes first - if passed, revert to StartMenu.
        if(     startingScene == SceneList.PauseMenu ||
                startingScene == SceneList.GameOverScreen
        )
        {
            Debug.LogWarning("Cannot start game with the '" + startingScene + "' scene. Loading 'StartMenu' instead.");
            startingScene = SceneList.StartMenu;            
        }
        StartCoroutine(LoadScene(startingScene));    
    }
    
    
    
    //-----------------------------------------//
    //                 Updates                 //
    //-----------------------------------------//
     
    //void Update()
    //{
    //    
    //}
    


    //-----------------------------------------//
    //              Scene Loading              //
    //-----------------------------------------//
    
    
    // Shorthand function for loading scenes - intended for use outside of this file.
    public void BeginLoadingScene(SceneList loadingScene)
    {
        StartCoroutine(LoadScene(loadingScene));
    }
    
    // Shorthand function for unloading scenes - intended for use outside fo this file.
    public void BeginUnloadingScene(SceneList unloadingScene)
    {
        StartCoroutine(UnloadScene(unloadingScene));
    }
    
    
    // Load the given scene - all loads are ADDITIVE, but the scene focus stays on the GameManager scene.
    IEnumerator LoadScene(SceneList loadingScene)
    {
        
        string sceneName = "scene_" + loadingScene.ToString();
        string path = "Assets/Scenes/" + sceneName + ".unity";
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(path);
        
        // Check if the scene exists before attempting to load
        //  - scene 0 is always the GameManager scene, so check for less than 1 instead of 0.
        if( buildIndex < 1 || buildIndex > SceneManager.sceneCountInBuildSettings )
        {
            Debug.LogWarning(loadingScene + " does not exist or has not been added to the build index. Scene loading cancelled.");
            yield break;
        }
        
        // Load the scene if it's not already been loaded.
        if( SceneManager.GetSceneByBuildIndex(buildIndex).isLoaded == false )
        {
            print(sceneName + " is loading.");
            yield return SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
        }
        
    }
    
    // Unload the given scene
    IEnumerator UnloadScene(SceneList unloadingScene)
    {
        string sceneName = "scene_" + unloadingScene.ToString();
        string path = "Assets/Scenes/" + sceneName + ".unity";
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(path);
        
        // Check if the scene exists before attempting to load
        //  - scene 0 is always the GameManager scene, that cannot be unloaded.
        if( buildIndex < 1 || buildIndex > SceneManager.sceneCountInBuildSettings )
        {
            Debug.LogWarning(unloadingScene + " does not exist or has not been added to the build index. Scene unloading cancelled.");
            yield break;
        }
        
        yield return SceneManager.UnloadSceneAsync(buildIndex);
    }
    
}
