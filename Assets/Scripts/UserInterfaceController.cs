using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UserInterfaceController : MonoBehaviour
{

    public static UserInterfaceController Instance { get; private set; }

    public Dictionary<Collectable.IngredientType,Dictionary<string,string>> ingredientDict;

    public UIDocument uiDocument;
    public VisualElement root;

    public VisualElement titleScreen;
    public Button startButton;
    public Button creditsButton;

    public VisualElement creditsScreen;
    public Button returnToMenuButton;

    public VisualElement hudScreen;
    public VisualElement statFreshCorn;
    public VisualElement statDriedCorn;
    public VisualElement statFreshBeans;
    public VisualElement statDriedBeans;
    public VisualElement statAsh;
    public VisualElement statStrawberry;
    public VisualElement statVenison;
    public VisualElement statRecipeBook;

    public VisualElement pickScreen;
    public Label nativeName;
    public Label englishName;

    public VisualElement dialogueScreen;
    public Label dialogueText;
    private Queue<string> dialogueQueue;

    public VisualElement recipeScreen;
    public GroupBox recipeVisual;
    public Label recipeText;
    //           Tuple<textString,elementIdString>
    public Queue<Tuple<string,string>> recipeQueue;
    public bool recipeSequenceStarted = false;

    public bool PlayerApproachedDoorWithAllIngredients = false;

    private List<Button> titleScreenButtons = new List<Button>();
    private List<Button> creditsScreenButtons = new List<Button>();

    private int titleScreenButtonsIndex;
    private int creditsScreenButtonsIndex;

    public bool titleVisible
    {
        get {
            return titleScreen.style.display == DisplayStyle.Flex;
        }
    }

    public bool creditsVisible
    {
        get
        {
            return creditsScreen.style.display == DisplayStyle.Flex;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 

        // Populate the ingredient dictionary
        ingredientDict = new();

        Dictionary<string,string> freshCornNames = new();
        freshCornNames["en"] = "White corn";
        freshCornNames["mo"] = "Onenhakén:ra";
        ingredientDict.Add(Collectable.IngredientType.freshCorn, freshCornNames);

        Dictionary<string,string> driedCornNames = new();
        driedCornNames["en"] = "Dried corn";
        driedCornNames["mo"] = "Kanenhathà:ton";
        ingredientDict.Add(Collectable.IngredientType.driedCorn, driedCornNames);

        Dictionary<string,string> freshBeansNames = new();
        freshBeansNames["en"] = "String beans";
        freshBeansNames["mo"] = "O’rhótsheri";
        ingredientDict.Add(Collectable.IngredientType.freshBeans, freshBeansNames);

        Dictionary<string,string> driedBeansNames = new();
        driedBeansNames["en"] = "Dry beans (for soup)";
        driedBeansNames["mo"] = "Onon’tara osahe:ta";
        ingredientDict.Add(Collectable.IngredientType.driedBeans, driedBeansNames);

        Dictionary<string,string> ashNames = new();
        ashNames["en"] = "Ashes";
        ashNames["mo"] = "O’kèn:ra";
        ingredientDict.Add(Collectable.IngredientType.ash, ashNames);

        Dictionary<string,string> strawberryNames = new();
        strawberryNames["en"] = "Strawberries";
        strawberryNames["mo"] = "Ken'niiohontésha";
        ingredientDict.Add(Collectable.IngredientType.strawberry, strawberryNames);

        Dictionary<string,string> venisonNames = new();
        venisonNames["en"] = "Pork";
        venisonNames["mo"] = "Kwéskwes o’wá:ron";
        ingredientDict.Add(Collectable.IngredientType.venison, venisonNames);

        Dictionary<string,string> recipeBookNames = new();
        recipeBookNames["en"] = "Recipe book";
        recipeBookNames["mo"] = "Kahiatón’sera ...";
        ingredientDict.Add(Collectable.IngredientType.recipeBook, recipeBookNames);

        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        hudScreen = root.Query<VisualElement>("hud-screen").First();

        statFreshCorn = hudScreen.Query<VisualElement>("stat-freshCorn").First();
        statDriedCorn = hudScreen.Query<VisualElement>("stat-driedCorn").First();
        statFreshBeans = hudScreen.Query<VisualElement>("stat-freshBeans").First();
        statDriedBeans = hudScreen.Query<VisualElement>("stat-driedBeans").First();
        statAsh = hudScreen.Query<VisualElement>("stat-ash").First();
        statStrawberry = hudScreen.Query<VisualElement>("stat-strawberry").First();
        statVenison = hudScreen.Query<VisualElement>("stat-venison").First();
        statRecipeBook = hudScreen.Query<VisualElement>("stat-recipeBook").First();

        titleScreen = root.Query<VisualElement>("title-screen").First();
        startButton = titleScreen.Query<Button>("btn-start").First();
        creditsButton = titleScreen.Query<Button>("btn-to-credits").First();

        creditsScreen = root.Query<VisualElement>("credits-screen").First();
        returnToMenuButton = creditsScreen.Query<Button>("btn-back").First();

        pickScreen = root.Query<VisualElement>("pick-screen").First();
        nativeName = pickScreen.Query<Label>("native-name").First();
        englishName = pickScreen.Query<Label>("english-name").First();

        dialogueScreen = root.Query<VisualElement>("dialogue-screen").First();
        dialogueText = dialogueScreen.Query<Label>("dialogue-text").First();

        recipeScreen = root.Query<VisualElement>("recipe-screen").First();
        recipeVisual = recipeScreen.Query<GroupBox>("recipe-visual").First();
        recipeText = recipeScreen.Query<Label>("recipe-text").First();
        recipeScreen.style.display = DisplayStyle.None;

        recipeQueue = new();
        dialogueQueue = new();

        InputManager.Instance.anyButtonEvent.AddListener(AdvanceDialogue);
        InputManager.Instance.anyButtonEvent.AddListener(AdvanceRecipe);

        titleScreen.style.display = DisplayStyle.Flex;
        creditsScreen.style.display = DisplayStyle.None;

        // Add listeners for title screen buttons
        startButton.clicked += StartGame;
        creditsButton.clicked += delegate { ToggleCredits(true); };
        returnToMenuButton.clicked += delegate { ToggleCredits(false); };

        InputManager.Instance.anyButtonEvent.AddListener(SelectMenuItem);

        // Create list for title and credits screen buttons (for navigation)
        titleScreenButtons.Add(startButton);
        titleScreenButtons.Add(creditsButton);

        creditsScreenButtons.Add(returnToMenuButton);

        // Select first button in start screen
        startButton.Focus();
    }

    // Update is called once per frame
    void Update()
    {
        // HUD Handling

        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) == 0) {
            statFreshCorn.style.display = DisplayStyle.None;
        }
        else {
            statFreshCorn.style.display = DisplayStyle.Flex;
        }

        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedCorn) == 0) {
            statDriedCorn.style.display = DisplayStyle.None;
        }
        else {
            statDriedCorn.style.display = DisplayStyle.Flex;
        }

        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshBeans) == 0) {
            statFreshBeans.style.display = DisplayStyle.None;
        }
        else {
            statFreshBeans.style.display = DisplayStyle.Flex;
        }

        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedBeans) == 0) {
            statDriedBeans.style.display = DisplayStyle.None;
        }
        else {
            statDriedBeans.style.display = DisplayStyle.Flex;
        }

        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.ash) == 0) {
            statAsh.style.display = DisplayStyle.None;
        }
        else {
            statAsh.style.display = DisplayStyle.Flex;
        }

        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.strawberry) == 0) {
            statStrawberry.style.display = DisplayStyle.None;
        }
        else {
            statStrawberry.style.display = DisplayStyle.Flex;
        }

        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.venison) == 0) {
            statVenison.style.display = DisplayStyle.None;
        }
        else {
            statVenison.style.display = DisplayStyle.Flex;
        }

        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.recipeBook) == 0) {
            statRecipeBook.style.display = DisplayStyle.None;
        }
        else {
            statRecipeBook.style.display = DisplayStyle.Flex;
        }


        // PickScreen Handling
        if (PlayerManager.Instance.ingredientInView == null) {
            pickScreen.style.display = DisplayStyle.None;
        }
        else {
           pickScreen.style.display = DisplayStyle.Flex;
           nativeName.text = ingredientDict[PlayerManager.Instance.ingredientInView.type]["mo"];
           englishName.text = ingredientDict[PlayerManager.Instance.ingredientInView.type]["en"];
        }
    }

    // FRESH INGREDIENTS: freshCorn, freshBeans, ash, venison
    // DRY INGREDIENTS: dryCorn, dryBeans
    // recipeBook
    // strawberry

    public bool playerHasNothing() {
        if(
            PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) == 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshBeans) == 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedCorn) == 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedBeans) == 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.ash) == 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.strawberry) == 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.venison) == 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.recipeBook) == 0
        ) {
            return true;
        } else {
            return false;
        }
    }

    public bool playerHasAtLeastOneIngredient() {
        if(
            PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) > 0
            || PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshBeans) > 0
            || PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedCorn) > 0
            || PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedBeans) > 0
            || PlayerManager.Instance.IngredientCount(Collectable.IngredientType.ash) > 0
            || PlayerManager.Instance.IngredientCount(Collectable.IngredientType.strawberry) > 0
            || PlayerManager.Instance.IngredientCount(Collectable.IngredientType.venison) > 0
            || PlayerManager.Instance.IngredientCount(Collectable.IngredientType.recipeBook) > 0
        ) {
            return true;
        } else {
            return false;
        }
    }

    public bool playerHasAllFreshIngredientsButNoRecipeBook() {
        if(
            PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshBeans) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.ash) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.venison) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.recipeBook) == 0
        ) {
            return true;
        } else {
            return false;
        }
    }

    // at this point we assume player has all fresh ingredients and the recipe book

    public bool playerHasNoDryIngredients() {
        if(
            PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshBeans) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.ash) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.venison) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.recipeBook) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedCorn) == 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedBeans) == 0
        ) {
            return true;
        } else {
            return false;
        }
    }

    public bool playerHasSomeDryIngredients() {
        if(
            PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshBeans) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.ash) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.venison) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.recipeBook) > 0
        ) {
            if(
                PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedCorn) > 0
                || PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedBeans) > 0
            ) {
                return true;
            } else {
                return false;
            }
        } else {
        return false;
        }
    }

    public bool playerHasAllIngredientsExceptStrawberries() {
        if(
            PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshBeans) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.ash) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.venison) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.recipeBook) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedCorn) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedBeans) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.strawberry) == 0
        ) {
            return true;
        } else {
            return false;
        }
    }

    public bool playerHasAllIngredients() {
        if(
            PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshBeans) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.ash) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.venison) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.recipeBook) > 0
            &&PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedCorn) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.driedBeans) > 0
            && PlayerManager.Instance.IngredientCount(Collectable.IngredientType.strawberry) > 0
        ) {
            return true;
        } else {
            return false;
        }
    }

    public void OnPlayerApproachedDoor() {
        if (PlayerManager.Instance) {
            if (playerHasAllIngredients()) {
                ClearAndEnqueueDialogue("Let's see what you brought me...");
                EnqueueDialogue($"You found all ingredients AND strawberries! You get to learn how to make soup AND juice!");
                return;
            }
            if (playerHasAllIngredientsExceptStrawberries()) {
                ClearAndEnqueueDialogue("Let's see what you brought me...");
                EnqueueDialogue($"You found all ingredients! You get to learn how to make the soup!");
                return;
            }
            if (playerHasSomeDryIngredients()) {
                ClearAndEnqueueDialogue($"Grandchild! We need dried corn\n*and* dried beans! Go back and\nmake sure you have both!");
                return;
            }
            if (playerHasNoDryIngredients()) {
                ClearAndEnqueueDialogue($"Oh, you silly grandchild! When I said\nget some corn and beans, I meant\nget some *dried* corn and beans!");
                EnqueueDialogue($"Go grab some of the dried corn and\nbeans that I keep over by the shed.");
                EnqueueDialogue($"Hurry up! I'm almost ready to make\nthe soup!");
                return;
            }
            if (playerHasAllFreshIngredientsButNoRecipeBook()) {
                ClearAndEnqueueDialogue($"Oh, good, you found more\ningredients.\n\nBut we can't get cookin' without\nthat recipe book!");
                EnqueueDialogue($"Follow that pup's footsteps! I think\nI saw him hanging around the fire pit\na minute ago.");
                return;
            }
            if (playerHasAtLeastOneIngredient()) {
                ClearAndEnqueueDialogue($"That's a good start, but where's the\nrest?");
                EnqueueDialogue($"Take a good look around the garden\nand around back. Don't come back\nuntil you have everything!");
                EnqueueDialogue($"And don't forget the cookbook!\nWhere did that pup run off with it?");
                return;
            }
            if(playerHasNothing()) {
                ClearAndEnqueueDialogue("Let's see what you brought me...");
                EnqueueDialogue($"You didn't pick anything!");
                EnqueueDialogue($"Don't be lazy like your brother! Go\ngrab what we need for our soup!\nHurry!");
                return;
            }
        }
    }

    public void StartGame() {
        // Hide title screen
        titleScreen.style.display = DisplayStyle.None;

        LookWithMouse.LockPressed();

        dialogueQueue = new();
        EnqueueDialogue("Oh! There ya are, Grandchild.\nI was startin’ to wonder when\nyou’d drag yourself outta bed and\njoin me outside.\n\nBeautiful summer day, eh?");
        EnqueueDialogue("Tomorrow’s our big family get-\ntogether and we’ll be serving up\nmy world-famous corn soup.");
        EnqueueDialogue("The stuff is so famous that Buffy\nSainte-Marie told my cousin’s\nauntie’s fourth husband that it was\nthe best corn soup to ever hit her\nheavenly lips!");
        EnqueueDialogue("Every year we make it with\ningredients from right here in my\ngarden.\n\nNow it’s your turn to help me make\nthe soup!");
        EnqueueDialogue("So, listen: go get some corn,\nbeans, the salt pork that Uncle\nMatty left us, and then scoop some\nash from the fire pit.");
        EnqueueDialogue("Bring all that stuff back here for\nyour To’ta.");
        EnqueueDialogue("And hurry up! Making my delicious\ncorn soup takes all day.\n\nWe got the whole family coming\nlater, and you know how hangry\nyour cousins can get...");
        EnqueueDialogue("Now, where did I put that recipe\nbook..?");
        EnqueueDialogue("GYAH! GONE!! It was just here a\nsecond ago!");
        EnqueueDialogue("Ohhh. Y’know what? Owí:ra the rez\npup was sniffin’ around here a few\nminutes ago. I saw him saunterin’\noff into the woods carryin’\nsomethin’ with a smirk.");
        EnqueueDialogue("Figured it was just a bone\nhe wanted to bury somewhere for\nchewin’ on later.");
        EnqueueDialogue("I’ll bet my bingo winnings that he\ntook the recipe book!");
        EnqueueDialogue("Anywho! I’ll need to finish preppin’\nin the kitchen. Could ya be a dear\nand go look for that recipe book in\nthe woods while you’re out there?");
        EnqueueDialogue("Oh, and it’s strawberry season...\n\nIf you find any, bring ’em back here\ntoo, I have a surprise for you!");
        EnqueueDialogue("Off you go!");
    }

    public void ToggleCredits(bool show)
    {
        if (show)
        {
            creditsScreen.style.display = DisplayStyle.Flex;
            creditsScreenButtons[creditsScreenButtonsIndex].Focus();
        }
        else
        {
            creditsScreen.style.display = DisplayStyle.None;
            titleScreenButtons[titleScreenButtonsIndex].Focus();
        }
    }

    public void NextMenuItem()
    {
        if (titleVisible)
        {
            titleScreenButtonsIndex++;
            if (titleScreenButtonsIndex >= titleScreenButtons.Count) titleScreenButtonsIndex = 0;

            // Select next button
            titleScreenButtons[titleScreenButtonsIndex].Focus();

        }
        else if (creditsVisible)
        {
            creditsScreenButtonsIndex++;
            if (creditsScreenButtonsIndex >= creditsScreenButtons.Count) creditsScreenButtonsIndex = 0;

            // Select next button
            creditsScreenButtons[creditsScreenButtonsIndex].Focus();
        }
    }

    public void PreviousMenuItem()
    {
        if (titleVisible)
        {
            titleScreenButtonsIndex--;
            if (titleScreenButtonsIndex < 0) titleScreenButtonsIndex = titleScreenButtons.Count - 1;

            // Select next button
            titleScreenButtons[titleScreenButtonsIndex].Focus();

        }
        else if (creditsVisible)
        {
            creditsScreenButtonsIndex--;
            if (creditsScreenButtonsIndex < 0) creditsScreenButtonsIndex = creditsScreenButtons.Count - 1;

            // Select next button
            creditsScreenButtons[creditsScreenButtonsIndex].Focus();
        }
    }

    public void SelectMenuItem()
    {
        if (titleVisible)
        {
            titleScreenButtons[titleScreenButtonsIndex].HandleEvent(new MouseDownEvent());

        }
        else if (creditsVisible)
        {
            creditsScreenButtons[creditsScreenButtonsIndex].HandleEvent(new MouseDownEvent());
        }
    }

    public void OnAllIngredientsRetrieved() {
        recipeSequenceStarted = true;
        recipeScreen.style.display = DisplayStyle.Flex;
        ClearRecipes();
        EnqueueRecipe(new Tuple<string,string>("Now we make the corn soup...","recipe-part-1"));
        EnqueueRecipe(new Tuple<string,string>("Watch carefully...","recipe-part-2"));
    }

    private void AdvanceDialogue() {
        Debug.Log("Advancing Dialogue");
        string dialogue;
        dialogueQueue.TryDequeue(out dialogue);
        if (dialogueQueue.Count > 0) {
            pickScreen.style.display = DisplayStyle.None;
            dialogueScreen.style.display = DisplayStyle.Flex;
            dialogueText.text = dialogueQueue.Peek();

            PlayerMovement.Instance.ToggleMovement(false);
        }
        else {
            dialogueScreen.style.display = DisplayStyle.None;
            if (PlayerApproachedDoorWithAllIngredients && !recipeSequenceStarted) {
                // OnAllIngredientsRetrieved();
            }
            PlayerMovement.Instance.ToggleMovement(true);
        }
    }

    private void EnqueueDialogue(string dialogue) {
        Debug.Log("Enqueue Dialogue");
        dialogueQueue.Enqueue(dialogue);

        pickScreen.style.display = DisplayStyle.None;
        dialogueScreen.style.display = DisplayStyle.Flex;
        dialogueText.text = dialogueQueue.Peek();

        PlayerMovement.Instance.ToggleMovement(false);
    }

    private void ClearAndEnqueueDialogue(string dialogue) {
        dialogueQueue.Clear();
        EnqueueDialogue(dialogue);
    }

    private void EnqueueRecipe(Tuple<string, string> recipe) {
        recipeQueue.Enqueue(recipe);

        PlayerMovement.Instance.ToggleMovement(false);
    }

    private void AdvanceRecipe() {
        Tuple<string,string> recipe;
        recipeQueue.TryDequeue(out recipe);

        if (recipeQueue.Count > 0) {
            pickScreen.style.display = DisplayStyle.None;
            dialogueScreen.style.display = DisplayStyle.None;
            recipeScreen.style.display = DisplayStyle.Flex;
            Tuple<string,string> peekValue = recipeQueue.Peek();
            recipeText.text = peekValue.Item1;

            foreach(VisualElement element in recipeVisual?.Children()) {
                if (element.name == peekValue.Item2) {
                    element.style.display = DisplayStyle.Flex;
                }
                else {
                    element.style.display = DisplayStyle.None;
                }
            }
            PlayerMovement.Instance.ToggleMovement(false);
        }
        else {
            if (recipeSequenceStarted) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                PlayerMovement.Instance.ToggleMovement(false);
            }
        }

    }

    private void ClearRecipes() {
        recipeQueue.Clear();
    }
}
