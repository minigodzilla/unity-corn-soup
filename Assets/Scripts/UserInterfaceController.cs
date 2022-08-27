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

    public VisualElement pickScreen;
    public Label nativeName;
    public Label englishName;

    public VisualElement dialogueScreen;
    public Label dialogueText;
    public Queue<string> dialogueQueue;

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
        freshCornNames["en"] = "Fresh Corn";
        freshCornNames["mo"] = "Ohnawénha";
        ingredientDict.Add(Collectable.IngredientType.freshCorn, freshCornNames);

        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        hudScreen = root.Query<VisualElement>("hud-screen").First();
        statCorn = hudScreen.Query<GroupBox>("stat-corn").First();
        statCornLabel = statCorn.Query<Label>("label").First();

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
    }

    // Update is called once per frame
    void Update()
    {
        // HUD Handling
        if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) <= 0) {
            statCorn.style.display = DisplayStyle.None;
        }
        else {
            statCorn.style.display = DisplayStyle.Flex;
        }

        statCornLabel.text = PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn).ToString();

        // PickScreen Handling
        if (PlayerManager.Instance.ingredientInView == null) {
            pickScreen.style.display = DisplayStyle.None;
        }
        else {
           pickScreen.style.display = DisplayStyle.Flex;
           nativeName.text = ingredientDict[PlayerManager.Instance.ingredientInView.type]["mo"];
           englishName.text = ingredientDict[PlayerManager.Instance.ingredientInView.type]["en"];
        }

        if (dialogueQueue.Count > 0) {
            pickScreen.style.display = DisplayStyle.None;
            dialogueScreen.style.display = DisplayStyle.Flex;
            dialogueText.text = dialogueQueue.Peek();

            if (Input.GetMouseButtonDown(0)) {
                dialogueQueue.Dequeue();
            }
        }
        else {
            dialogueScreen.style.display = DisplayStyle.None;
            if (PlayerApproachedDoorWithAllIngredients && !recipeSequenceStarted) {
                // OnAllIngredientsRetrieved();
            }
        }

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

            if (Input.GetMouseButtonDown(0)) {
                Debug.Log(recipeQueue.Peek());
                recipeQueue.Dequeue();
                Debug.Log(recipeQueue.Peek());
            }
        }
        else {
            if (recipeSequenceStarted) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public void OnPlayerApproachedDoor() {
        dialogueQueue = new();
        dialogueQueue.Enqueue("Let's see what you brought me...");
        if (PlayerManager.Instance) {
            if (PlayerManager.Instance.IngredientCount(Collectable.IngredientType.freshCorn) > 0) {
                dialogueQueue.Enqueue($"You have corn...");
            }
            else {
                dialogueQueue.Enqueue($"You don't have any corn yet... Grab some from the cornfield...");
            }
        }
    }

    public void OnStartGame() {
        dialogueQueue = new();
        dialogueQueue.Enqueue("We're making corn soup today... Here's what you need to bring me...");
        dialogueQueue.Enqueue("Some fresh corn...");
        dialogueQueue.Enqueue("Some beans...");
        dialogueQueue.Enqueue("And bring strawberries for a surprise...");
        dialogueQueue.Enqueue("Off you go!");
    }

    public void OnAllIngredientsRetrieved() {
        recipeSequenceStarted = true;
        recipeScreen.style.display = DisplayStyle.Flex;
        recipeQueue = new();
        recipeQueue.Enqueue(new Tuple<string,string>("Now we make the corn soup...","recipe-part-1"));
        recipeQueue.Enqueue(new Tuple<string,string>("Watch carefully...","recipe-part-2"));
    }
}
