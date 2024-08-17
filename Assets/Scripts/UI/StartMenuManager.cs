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
    float startMenuEaseTracker = 0f;
    float optionsMenuEaseTracker = 0f;
    float creditsEaseTracker = 0f;
    bool showMenu = true;
    bool showOptions = false;
    bool showCredits = false;
    EasingFunction.Function ease_Image = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutCubic);
    
    
    // Fullscreen Fade
    [SerializeField] Image fullscreenFade;    
    Vector4 fadeOutColor;
    EasingFunction.Function ease_Fade = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutCubic);
    
    
    // Options Panel
    
    /*
    // Credits Image
    [SerializeField] Image credits;
    [SerializeField] Vector2 creditsShowPos;
    [SerializeField] bool updateCreditsShowPos = false;
    [SerializeField] Vector2 creditsHidePos;
    [SerializeField] bool updateCreditsHidePos = false;
    */
    


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
                        imageList[i].image.enabled = false;
                    else
                        imageList[i].image.enabled = true;
                                    
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
        ShowMenu();
    }
    
    
    // 
    void Update()
    {   
        StartMenuEasing();
        //OptionsMenuEasing();
        //CreditsEasing();
    }
    
    
    public void ShowMenu()
    {  
        showMenu = true;
        showOptions = false;
        showCredits = false;              
    }
    
    public void ShowOptions()
    {
        showMenu = false;
        showOptions = true;
        showCredits = false;
    }
    
    public void ShowCredits()
    {
        showMenu = false;
        showOptions = false;
        showCredits = true;
    }
    
    public void FadeOut()
    {
        showMenu = false;
        showOptions = false;
        showCredits = false;
    }

     
    void StartMenuEasing()
    {
        // Main Start Menu
        float startTarget = showMenu ? 1f : 0f;
        startMenuEaseTracker = Mathf.MoveTowards(startMenuEaseTracker, startTarget, imageEaseSpeed * Time.deltaTime);
        
        // Options Panel
        
        // Credits
        float creditsTarget = showCredits ? 1f : 0f;   
        creditsEaseTracker = Mathf.MoveTowards(creditsEaseTracker, creditsTarget, imageEaseSpeed * Time.deltaTime);
        
        
        //if (startMenuEaseTracker <= 0f || 1f <= startMenuEaseTracker) // if easing tracker is at either limit, don't do anything else.
        //    return;
           
        // Loop through all the UI Images and ease them between their Hiding Positions and Showing Positions
        float newX, newY;
        float t;
        for (int i = 0; i < imageList.Count; i++)
        {
            if (imageList[i].menuType == MenuType.main) 
                t = startMenuEaseTracker;
            else if (imageList[i].menuType == MenuType.options)
                t = optionsMenuEaseTracker;
            else 
                t = creditsEaseTracker;
            
            newX = ease_Image(imageList[i].hidePos.x, imageList[i].showPos.x, t);
            newY = ease_Image(imageList[i].hidePos.y, imageList[i].showPos.y, t);
            imageList[i].image.rectTransform.anchoredPosition = new Vector2(newX, newY);  
        }
               
        // Fade out the fullScreen fade   
        fadeOutColor[3] = ease_Fade(1f, 0f, startMenuEaseTracker);
        fullscreenFade.color = fadeOutColor;
        
    }
    
    /*
    void CreditsEasing()
    {
        float target = showCredits ? 1f : 0f;
        creditsEaseTracker = Mathf.MoveTowards(creditsEaseTracker, target, imageEaseSpeed * Time.deltaTime);
        
        if (creditsEaseTracker <= 0f || 1f <= creditsEaseTracker) // if easing tracker is at either limit, don't do anything else.
            return;
            
        // Move the credits image between it's showing and hiding place
        float newX = ease_Image(creditsHidePos.x, creditsShowPos.x, creditsEaseTracker);
        float newY = ease_Image(creditsHidePos.y, creditsShowPos.y, creditsEaseTracker);
        credits.rectTransform.anchoredPosition = new Vector2(newX, newY);
    }
    */
    
}
