using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using String = System.String;

public class MainGame : MonoBehaviour
{
    private UnityEvent mainEvent;
    private static GameState gameState;
    private enum GameState
    {
        Prepare,
        Ready,
        Move,
        Finish
    }

    public const float MoveDuration = .3f;

    private static IEnumerator waiting;

    private const int optionCount = 5;
    private static GameObject[] optionObjects;
    private static Option[] options;

    private const int buttonCount = 4;
    private const float buttonX = .42f, buttonY = .34f;
    private static GameObject[] buttonObjects;
    private static SpriteBox[] buttonSprites;

    private const int textGroupCount = 5;
    private static GameObject[] textGroupObjects;
    private static TextGroup[] textGroups;
    private static int openTextGroup;

    private static bool errorOccurred;


    void Awake()
    {
        optionObjects = new GameObject[optionCount];
        options = new Option[optionCount];
        for (int i = 0; i < optionCount; i++)
        {
            optionObjects[i] = General.AddChild(gameObject, $"Message Box{i}");
            options[i] = optionObjects[i].AddComponent<Option>();
        }

        buttonObjects = new GameObject[buttonCount];
        buttonSprites = new SpriteBox[buttonCount];
        buttonObjects[0] = General.AddChild(gameObject, $"Rules");
        buttonObjects[1] = General.AddChild(gameObject, $"Info");
        buttonObjects[2] = General.AddChild(gameObject, $"Share");
        buttonObjects[3] = General.AddChild(gameObject, $"Settings");
        for (int i = 0; i < buttonCount; i++) buttonSprites[i] = buttonObjects[i].AddComponent<SpriteBox>();

        textGroupObjects = new GameObject[textGroupCount];
        textGroups = new TextGroup[textGroupCount];
        for (int i = 0; i < textGroupCount; i++)
        {
            textGroupObjects[i] = General.AddChild(gameObject, $"Text Group{i}");
            textGroups[i] = textGroupObjects[i].AddComponent<TextGroup>();
        }
        openTextGroup = -1;

        VirtualKeyboard.PreInitialize(gameObject);
        Guess.PreInitialize(gameObject);

        mainEvent = new UnityEvent();
        mainEvent.AddListener(ChangeState);

        errorOccurred = false;
    }
    void Start()
    {
        VirtualKeyboard.Initialize();
        Data.LoadRandomBoard();
        //Data.AssignBoard("RDXXX, X, DDDXXXX, DDRXXXX, DDXLXXX");
        Debug.Log($"Seed {Data.Seed}");
        Guess.Initialize();

        for (int i = 1; i < optionCount; i++)
        {
            options[i].Initialize("UI", 0, null, 1f, 1f, 1, null, Graphics.Font.Mops, 5.6f, Graphics.Gray,
                Vector2.zero, false, lineSpacing: -6f, alignment: TextAlignmentOptions.Midline);
        }
        optionObjects[0].name = "Fatal Error";
        options[0].Initialize("Text Group", 0, null, 1f, 1f, 32767, $"FATAL ERROR{Environment.NewLine}Contact the developer",
            Graphics.Font.RecursoBold, 4.8f, Graphics.Error, Vector2.zero, false, lineSpacing: -6f, alignment: TextAlignmentOptions.MidlineLeft);
        optionObjects[0].transform.localPosition = new Vector3(0, -2.23f, 0);
        optionObjects[1].name = "Seed";
        options[1].Initialize("UI", 0, null, 1f, 1f, 32767, Data.RandomMode ? "#???" : $"#{Data.Seed}",
            Graphics.Font.RecursoBold, 1.8f, Graphics.Gray, Vector2.zero, false, lineSpacing: -6f, alignment: TextAlignmentOptions.MidlineLeft);
        optionObjects[1].transform.localPosition = new Vector3(3.54f, -4.78f, 0);
        for (int i = 0; i < optionCount; i++) optionObjects[i].SetActive(false);
        optionObjects[1].SetActive(true);

        buttonSprites[0].Initialize(Graphics.icon[7], "UI", 0, Vector3.zero, useCollider: true);
        buttonSprites[1].Initialize(Graphics.icon[9], "UI", 0, Vector3.zero, useCollider: true);
        buttonSprites[2].Initialize(Graphics.icon[8], "UI", 0, Vector3.zero, useCollider: true);
        buttonSprites[3].Initialize(Graphics.icon[6], "UI", 0, Vector3.zero, useCollider: true);

        textGroups[0].Initialize(TextGroup.Type.Page1);
        textGroups[1].Initialize(TextGroup.Type.Page2);
        textGroups[2].Initialize(TextGroup.Type.Info);
        textGroups[3].Initialize(TextGroup.Type.Share);
        textGroups[4].Initialize(TextGroup.Type.Settings);
        for (int i = 0; i < textGroupCount; i++) textGroupObjects[i].SetActive(false);
        openTextGroup = 0;
        textGroupObjects[openTextGroup].SetActive(true);
        Graphics.ShadowObject.SetActive(true);

        gameState = GameState.Ready;
    }

    void Update()
    {
        buttonObjects[0].transform.localPosition = new Vector3(-Graphics.Width / 2 + buttonX, Graphics.Height / 2 - buttonY, 0);
        buttonObjects[1].transform.localPosition = new Vector3(-Graphics.Width / 2 + buttonX * 2.5f, Graphics.Height / 2 - buttonY, 0);
        buttonObjects[2].transform.localPosition = new Vector3(Graphics.Width / 2 - buttonX * 2.5f, Graphics.Height / 2 - buttonY, 0);
        buttonObjects[3].transform.localPosition = new Vector3(Graphics.Width / 2 - buttonX, Graphics.Height / 2 - buttonY, 0);
        if (SceneLoader.Loading || errorOccurred) return;

        switch (gameState)
        {
            case GameState.Ready:
                if (openTextGroup < 0)
                {
                    if (buttonSprites[0].Mouse.GetMouseClick())
                    {
                        openTextGroup = 0;
                        textGroupObjects[openTextGroup].SetActive(true);
                        Graphics.ShadowObject.SetActive(true);
                    }
                    else if (buttonSprites[1].Mouse.GetMouseClick())
                    {
                        openTextGroup = 2;
                        textGroupObjects[openTextGroup].SetActive(true);
                        Graphics.ShadowObject.SetActive(true);
                    }
                    else if (buttonSprites[2].Mouse.GetMouseClick())
                    {
                        openTextGroup = 3;
                        textGroupObjects[openTextGroup].SetActive(true);
                        Graphics.ShadowObject.SetActive(true);
                    }
                    else if (buttonSprites[3].Mouse.GetMouseClick())
                    {
                        openTextGroup = 4;
                        textGroupObjects[openTextGroup].SetActive(true);
                        Graphics.ShadowObject.SetActive(true);
                    }
                    else
                    {
                        float wait = -1;

                        if (VirtualKeyboard.HandleInput())
                        {
                            gameState = GameState.Move;
                            StartCoroutine(EndGame());
                        }
                        else Guess.HandleGrabbedShape();

                        if (wait > 0)
                        {
                            waiting = General.WaitEvent(mainEvent, wait);
                            StartCoroutine(waiting);
                        }
                    }
                }
                else textGroups[openTextGroup].HandleInput();
                break;

            case GameState.Move:
                break;
            case GameState.Finish:
                if (openTextGroup < 0)
                {
                    if (buttonSprites[0].Mouse.GetMouseClick())
                    {
                        openTextGroup = 0;
                        textGroupObjects[openTextGroup].SetActive(true);
                        Graphics.ShadowObject.SetActive(true);
                    }
                    else if (buttonSprites[1].Mouse.GetMouseClick())
                    {
                        openTextGroup = 2;
                        textGroupObjects[openTextGroup].SetActive(true);
                        Graphics.ShadowObject.SetActive(true);
                    }
                    else if (buttonSprites[2].Mouse.GetMouseClick())
                    {
                        openTextGroup = 3;
                        textGroupObjects[openTextGroup].SetActive(true);
                        Graphics.ShadowObject.SetActive(true);
                    }
                    else if (buttonSprites[3].Mouse.GetMouseClick())
                    {
                        openTextGroup = 4;
                        textGroupObjects[openTextGroup].SetActive(true);
                        Graphics.ShadowObject.SetActive(true);
                    }
                    else VirtualKeyboard.HandleInput();
                }
                else textGroups[openTextGroup].HandleInput();
                break;
            default:
                Debug.LogWarning($"Update: not implemented for type {Enum.GetName(typeof(GameState), gameState)}");
                break;
        }
    }


    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(.8f);
        gameState = GameState.Finish;
        openTextGroup = 3;
        textGroupObjects[openTextGroup].SetActive(true);
        textGroups[openTextGroup].UpdateResult();
        Graphics.ShadowObject.SetActive(true);
    }
    private void ChangeState()
    {
        switch (gameState)
        {
            case GameState.Prepare:
                gameState = GameState.Ready;
                break;

            case GameState.Move:
                gameState = GameState.Ready;
                break;

            default:
                Debug.LogWarning($"ChangeState: not implemented yet for state {Enum.GetNames(typeof(GameState))[(int)gameState]}");
                break;
        }
    }

    public static void CloseTextGroup(bool nextPage = false)
    {
        textGroupObjects[openTextGroup].SetActive(false);
        if (nextPage)
        {
            switch (openTextGroup)
            {
                case 0:
                case 1:
                    openTextGroup = 1 - openTextGroup;
                    textGroupObjects[openTextGroup].SetActive(true);
                    break;
                default:
                    Debug.LogWarning($"MainGame.CloseTextGroup: next page is not defined for textGroup[{openTextGroup}]");
                    Graphics.ShadowObject.SetActive(false);
                    openTextGroup = -1;
                    break;
            }
        }
        else
        {
            Graphics.ShadowObject.SetActive(false);
            openTextGroup = -1;
        }
    }

    public static void ApplyColorblindMode()
    {
        textGroups[0].ApplyColorblindMode();
        textGroups[1].ApplyColorblindMode();
        textGroups[3].ApplyColorblindMode();
    }

    public static void NotifyError()
    {
        optionObjects[0].SetActive(true);
        errorOccurred = true;
    }
}
