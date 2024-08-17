using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class StartMenuManager : MonoBehaviour
{
    
    enum MenuType { main, options, credits }
   
    
    [System.Serializable]
    struct ImageData
    {
        public string name;
        public bool hideImage;
        public Image image;
        public Vector2 showPos;
        public bool updateShowPos;
        public Vector2 hidePos;
        public bool updateHidePos;
        public MenuType menuType;
    }
    
    [SerializeField] List<ImageData> imageList;
    
    
    [SerializeField] float imageEaseSpeed = 0.25f;
    float fadeEaseTracker = 1f;
    float startMenuEaseTracker = 0f;
    float optionsMenuEaseTracker = 0f;
    float creditsEaseTracker = 0f;
    bool showFade = false;
    bool showMenu = true;
    bool showOptions = false;
    bool showCredits = false;
    EasingFunction.Function ease_Image = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutCubic);
    
    
    // Fullscreen Fade
    [SerializeField] Image fullscreenFade;    
    Vector4 fadeOutColor;
    EasingFunction.Function ease_Fade = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutCubic);
    
    
    // Options Panel
    
    


    #if UNITY_EDITOR
        // Sets the Start or End positions for associated UI Image in the editor.
        // Only called when a change occurs in the editor.
        private void OnValidate() 
        {
            // Main Start Menu
            // Go through the whole list of UI Images and see if any positions needs to be updated.
            //ImageData 
            for (int i = 0; i < imageList.Count; i++)
            {
                if (imageList[i].image != null)
                {  
                    ImageData newData = imageList[i];
                    
                    // Hide Image Check
                    if (imageList[i].hideImage == true)                    
                    {
                        imageList[i].image.rectTransform.anchoredPosition = imageList[i].hidePos;
                        newData.hideImage = false;
                    }
                                    
                    // Show Pos
                    if (imageList[i].updateShowPos == true)
                    {
                        newData.showPos = imageList[i].image.rectTransform.anchoredPosition;
                        newData.updateShowPos = false;
                    }
                    
                    // Hide
                    if (imageList[i].updateHidePos == true)
                    {
                        newData.hidePos = imageList[i].image.rectTransform.anchoredPosition;
                        newData.updateHidePos = false;
                    }
                    
                    newData.name = imageList[i].image.name;
                    imageList[i] = newData;
                }
            }
            
            // Options Panel
            
            /*
            // Credits Image
            if (updateCreditsShowPos == true)
            {
                creditsShowPos = credits.rectTransform.anchoredPosition;
                updateCreditsShowPos = false;
            }
            if (updateCreditsHidePos == true)
            {
                creditsHidePos = credits.rectTransform.anchoredPosition;
                updateCreditsHidePos = false;
            }
            */
        }
    #endif

 
    
    private void OnEnable() 
    {
        fadeEaseTracker = 1f;
        ShowMenu();
    }
    
    
    // 
    void Update()
    {   
        StartMenuEasing();
    }
    
    
    public void ShowMenu()
    {  
        showFade = false;
        showMenu = true;
        showOptions = false;
        showCredits = false;              
    }
    
    public void ShowOptions()
    {
        showFade = false;
        showMenu = false;
        showOptions = true;
        showCredits = false;
    }
    
    public void ShowCredits()
    {
        showFade = false;
        showMenu = false;
        showOptions = false;
        showCredits = true;
    }
    
    public void FadeOut()
    {
        showFade = true;
        showMenu = false;
        showOptions = false;
        showCredits = false;
    }

     
    void StartMenuEasing()
    {
        
        // Fade Image
        float fadeTarget = showFade ? 1f : 0f;
        fadeEaseTracker = Mathf.MoveTowards(fadeEaseTracker, fadeTarget, imageEaseSpeed * Time.deltaTime);
        
        // Main Start Menu
        float startTarget = showMenu ? 1f : 0f;
        startMenuEaseTracker = Mathf.MoveTowards(startMenuEaseTracker, startTarget, imageEaseSpeed * Time.deltaTime);
        
        // Options Panel
        
        
        // Credits
        float creditsTarget = showCredits ? 1f : 0f;   
        creditsEaseTracker = Mathf.MoveTowards(creditsEaseTracker, creditsTarget, imageEaseSpeed * Time.deltaTime);
    
           
        // Loop through all the UI Images and ease them between their Hiding Positions and Showing Positions
        float newX, newY;
        float t;
        for (int i = 0; i < imageList.Count; i++)
        {
            // get the corresponding easing tracker
            if (imageList[i].menuType == MenuType.main) 
                t = startMenuEaseTracker;
            else if (imageList[i].menuType == MenuType.options)
                t = optionsMenuEaseTracker;
            else 
                t = creditsEaseTracker;
                
            if (0f < t && t < 1) // only do easing for this object if there will be any change
            {
                newX = ease_Image(imageList[i].hidePos.x, imageList[i].showPos.x, t);
                newY = ease_Image(imageList[i].hidePos.y, imageList[i].showPos.y, t);
                imageList[i].image.rectTransform.anchoredPosition = new Vector2(newX, newY);  
            }
        }
               
        // Fade out the fullScreen fade but only if there will be any change.
        if (0f < fadeEaseTracker && fadeEaseTracker < 1f)
        {
            fadeOutColor[3] = ease_Fade(0f, 1f, fadeEaseTracker);
            fullscreenFade.color = fadeOutColor;
        }
        
    }
    
    
}
