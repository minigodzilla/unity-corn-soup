using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UserInterfaceController : MonoBehaviour
{

    public static UserInterfaceController Instance { get; private set; }

    public Dictionary<Collectable.IngredientType,Dictionary<string,string>> ingredientDict;

    public UIDocument uiDocument;
    public VisualElement root;

    public VisualElement hudScreen;
    public GroupBox statCorn;
    public Label statCornLabel;
    public GroupBox statStrawb;
    public Label statStrawbLabel;

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

        Dictionary<string,string> strawbNames = new();
        strawbNames["en"] = "Strawberries";
        strawbNames["mo"] = "Ken'niiohontésha";
        ingredientDict.Add(Collectable.IngredientType.strawberry, strawbNames);

        // Onon’tara osahe:ta = soup beans
        // Salt pork

        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        hudScreen = root.Query<VisualElement>("hud-screen").First();

        statCorn = hudScreen.Query<GroupBox>("stat-corn").First();
        statCornLabel = statCorn.Query<Label>("label").First();

        statStrawb = hudScreen.Query<GroupBox>("stat-strawb").First();
        statStrawbLabel = statStrawb.Query<Label>("label").First();

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

        OnStartGame();

        InputManager.Instance.anyButtonEvent.AddListener(AdvanceDialogue);
        InputManager.Instance.anyButtonEvent.AddListener(AdvanceRecipe);
    }

    // Update is called once per frame
    void Update()
    {
        // HUD Handling

        // fresh corn
        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) <= 0) {
            statCorn.style.display = DisplayStyle.None;
        }
        else {
            statCorn.style.display = DisplayStyle.Flex;
        }
        statCornLabel.text = PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn).ToString();

        // strawberry
        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.strawberry) <= 0) {
            statStrawb.style.display = DisplayStyle.None;
        }
        else {
            statStrawb.style.display = DisplayStyle.Flex;
        }
        statStrawbLabel.text = PlayerManager.Instance.IngredientCount(Collectable.IngredientType.strawberry).ToString();

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

    public void OnPlayerApproachedDoor() {
        ClearAndEnqueueDialogue("Let's see what you brought me...");
        if (PlayerManager.Instance) {
            if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) > 0) {
                EnqueueDialogue($"You have corn...");
            }
            else {
                EnqueueDialogue($"You don't have any corn yet... Grab some from the cornfield...");
            }
        }
    }

    public void OnStartGame() {
        dialogueQueue = new();
        EnqueueDialogue("Oh! There ya are, Grandchild. I was startin’ to wonder when you’d drag yourself outta bed and join me outside. Beautiful summer day, eh?");
        EnqueueDialogue("Listen, tomorrow’s our big family get-together and we’ll be serving up my world-famous corn soup.");
        EnqueueDialogue("The stuff is so famous that Buffy Sainte-Marie told my cousin’s auntie’s fourth husband that it was the best corn soup to ever hit her heavenly lips!");
        EnqueueDialogue("Every year we make it from ingredients grown right here in my own garden. Now it's your turn to help your To'ta make the soup.");
        EnqueueDialogue("So, listen: go get some corn, beans, and the salt pork that Uncle Matty left us, and bring it all back here.");
        EnqueueDialogue("And if you find any strawberries, pick those too, I have a surprise for you!");
        EnqueueDialogue("Off you go!");
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
        Debug.Log(recipeQueue.Peek());
        Tuple<string,string> recipe;
        recipeQueue.TryDequeue(out recipe);
        Debug.Log(recipeQueue.Peek());

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
