using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public static class VirtualKeyboard
{
    private static GameObject keyboardObject;
    private static GameObject[] keyObjects, symbolObjects, colorblindObjects;
    private static Vector3[] KeyPositions;
    private static SpriteBox[] keySprites, symbolSprites, colorblindSprites;
    private static Data.Color[] polyominoColors;
    private static GameObject[] optionObjects;
    private static Option[] options;
    private const int FirstRow = 9, SecondRow = 8, KeyCount = FirstRow + SecondRow, OptionCount = 3;
    private const float KeyboardPositionX = 0f, KeyboardPositionY = -3.57f, KeyIntervalX = .76f, KeyOffsetY = .42f;

    private static Data.Shape.Rotation rotation;
    private static bool flipped;

    public static void PreInitialize(GameObject parentObject)
    {
        keyboardObject = General.AddChild(parentObject, "Keyboard");
        keyObjects = new GameObject[KeyCount];
        symbolObjects = new GameObject[KeyCount];
        colorblindObjects = new GameObject[FirstRow + 1];
        KeyPositions = new Vector3[KeyCount];
        keySprites = new SpriteBox[KeyCount];
        symbolSprites = new SpriteBox[KeyCount];
        colorblindSprites = new SpriteBox[FirstRow + 1];
        polyominoColors = new Data.Color[FirstRow];
        optionObjects = new GameObject[OptionCount];
        options = new Option[OptionCount];
        for (int i = 0; i < KeyCount; i++)
        {
            keyObjects[i] = General.AddChild(keyboardObject, $"Key{i}");
            keySprites[i] = keyObjects[i].AddComponent<SpriteBox>();
            if (i < FirstRow || i > FirstRow && i < KeyCount - 2)
            {
                symbolObjects[i] = General.AddChild(keyObjects[i], $"Symbol{i}");
                symbolSprites[i] = symbolObjects[i].AddComponent<SpriteBox>();
            }
        }
        for (int i = 0; i < FirstRow; i++) polyominoColors[i] = Data.Color.Unused;
        for (int i = 0; i < FirstRow + 1; i++)
        {
            colorblindObjects[i] = General.AddChild(keyObjects[i], $"Colorblind{i}");
            colorblindSprites[i] = colorblindObjects[i].AddComponent<SpriteBox>();
        }
        colorblindObjects[FirstRow].transform.parent = keyObjects[KeyCount - 2].transform;
        optionObjects[0] = General.AddChild(keyObjects[FirstRow], "Go!");
        optionObjects[1] = General.AddChild(keyObjects[KeyCount - 2], "Don't touch");
        optionObjects[2] = General.AddChild(keyObjects[KeyCount - 1], "Show ans.");
        for (int i = 0; i < OptionCount; i++) options[i] = optionObjects[i].AddComponent<Option>();
    }
    
    public static void Initialize()
    {
        keyboardObject.transform.localPosition = new Vector3(KeyboardPositionX, KeyboardPositionY, 0);
        for (int i = 0; i < FirstRow; i++) KeyPositions[i] = new Vector3(KeyIntervalX * ((FirstRow - 1) * -.5f + i), +KeyOffsetY, 0);
        for (int i = 0; i < SecondRow; i++) KeyPositions[FirstRow + i] = new Vector3(KeyIntervalX * ((SecondRow - 1) * -.5f + i), -KeyOffsetY, 0);
        for (int i = 0; i < KeyCount; i++)
        {
            keySprites[i].Initialize(Graphics.symbol[0], "Keyboard", 0, KeyPositions[i], useCollider: true);
        }
        for (int i = 0; i < FirstRow; i++)
        {
            symbolSprites[i].Initialize(Graphics.symbol[8 + i], "Keyboard", 2, Vector3.zero);
            colorblindSprites[i].Initialize(null, "Keyboard", 1, Vector3.zero);
            colorblindObjects[i].SetActive(Guess.Colorblind);
        }
        colorblindSprites[FirstRow].Initialize(Graphics.accessibility[8], "Keyboard", 1, Vector3.zero);
        colorblindSprites[FirstRow].spriteRenderer.color = Graphics.KeyTransparent;
        colorblindObjects[FirstRow].SetActive(false);
        for (int i = 0; i < SecondRow - OptionCount; i++)
        {
            if (i == 3) continue;
            symbolSprites[FirstRow + 1 + i].Initialize(Graphics.symbol[26 + i], "Keyboard", 2, Vector3.zero);
        }
        symbolSprites[FirstRow + 4].Initialize(Graphics.symbol[28], "Keyboard", 2, Vector3.zero);
        symbolObjects[FirstRow + 4].transform.localEulerAngles = new Vector3(0, 0, 90);
        options[0].Initialize("Keyboard", 0, null, 1f, 1f, 2, "Go!", Graphics.Font.RecursoBold, 1.8f, Graphics.Black, Vector2.zero, false);
        options[1].Initialize("Keyboard", 0, null, 1f, 1f, 2, $"Show{Environment.NewLine}ans.",
            Graphics.Font.RecursoBold, 1.8f, Graphics.Black, Vector2.zero, false);
        options[2].Initialize("Keyboard", 0, null, 1f, 1f, 2, $"Don't{Environment.NewLine}touch",
            Graphics.Font.RecursoBold, 1.8f, Graphics.Black, Vector2.zero, false);
        ChangeKeyColor(9, Data.Color.Wrong);
        ChangeKeyColor(14, Data.Color.Wrong);
        ChangeKeyColor(15, Data.Color.Wrong);

        rotation = Data.Shape.Rotation.Zero;
        flipped = false;
    }

    // returns if the game finished
    public static bool HandleInput()
    {
        if (Guess.Grabbed) return false;

        bool noMoreInput = false, finish = false;
        Data.Shape? shape = null;

        // handle input for the first row
        if (Guess.BoardIndex < Guess.GuessCount && !Guess.Win)
        {
            if (!noMoreInput && keySprites[0].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.Monomino, rotation, flipped);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[1].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.Domino, rotation, flipped);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[2].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.TriominoI, rotation, flipped);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[3].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.TriominoL, rotation, flipped);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[4].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.TetrominoI, rotation, flipped);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[5].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.TetrominoL, rotation, flipped);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[6].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.TetrominoT, rotation, flipped);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[7].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.TetrominoO, rotation, flipped);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[8].Mouse.GetMouseClick())
            {
                shape = new Data.Shape(Data.Shape.Type.TetrominoZ, rotation, flipped);
                noMoreInput = true;
            }

            if (shape != null) Guess.Grab((Data.Shape)shape);
        }

        // handle input for the second row
        if (Guess.BoardIndex < Guess.GuessCount && !Guess.Win)
        {
            if (!noMoreInput && keySprites[FirstRow].Mouse.GetMouseClick())
            {
                // make a guess
                Guess.MakeGuess();
                finish = Guess.Win || Guess.BoardIndex == Guess.GuessCount;
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[FirstRow + 1].Mouse.GetMouseClick())
            {
                rotation = (Data.Shape.Rotation)(((int)rotation + 1) % 4);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[FirstRow + 2].Mouse.GetMouseClick())
            {
                rotation = (Data.Shape.Rotation)(((int)rotation + 3) % 4);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[FirstRow + 3].Mouse.GetMouseClick())
            {
                flipped = !flipped;
                if ((int)rotation % 2 == 1) rotation = (Data.Shape.Rotation)(((int)rotation + 2) % 4);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[FirstRow + 4].Mouse.GetMouseClick())
            {
                flipped = !flipped;
                if ((int)rotation % 2 == 0) rotation = (Data.Shape.Rotation)(((int)rotation + 2) % 4);
                noMoreInput = true;
            }
            if (!noMoreInput && keySprites[FirstRow + 5].Mouse.GetMouseClick())
            {
                Guess.ResetBoard();
                noMoreInput = true;
            }
        }
        else if (Guess.BoardIndex == Guess.GuessCount && !Guess.Win)
        {
            if (!noMoreInput && keySprites[FirstRow + 6].Mouse.GetMouseClick())
            {
                if (Guess.BoardIndex == Guess.GuessCount && !Guess.Win)
                {
                    Guess.ShowAnswer();
                    options[1].ChangeText(Guess.AnswerShown ? $"Hide{Environment.NewLine}ans." : $"Show{Environment.NewLine}ans.");
                    ChangeKeyColor(FirstRow + 6, Guess.AnswerShown ? Data.Color.Correct : Data.Color.Unused);
                    noMoreInput = true;
                }
            }
        }
        if (!noMoreInput && keySprites[FirstRow + 7].Mouse.GetMouseClick())
        {
            General.Shuffle(KeyPositions);
            for (int i = 0; i < KeyCount; i++) keyObjects[i].transform.localPosition = KeyPositions[i];
            noMoreInput = true;
        }

        // transform the symbols
        for (int i = 0; i < FirstRow; i++)
        {
            symbolObjects[i].transform.localScale = new Vector3(flipped ? -1 : 1, 1, 1);
            symbolObjects[i].transform.localEulerAngles = new Vector3(0, 0, (int)rotation * 90);
        }

        return finish;
    }

    public static void ChangeKeyColor(int key, Data.Color color)
    {
        switch (key)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
                if ((int)polyominoColors[key] < (int)color)
                {
                    polyominoColors[key] = color;
                    keySprites[key].spriteRenderer.sprite = Graphics.symbol[(int)color];
                    symbolSprites[key].spriteRenderer.sprite = Graphics.symbol[color == Data.Color.Unused ? (8 + key) : (17 + key)];
                    switch (color)
                    {
                        case Data.Color.Far:
                        case Data.Color.Close:
                        case Data.Color.Correct:
                            colorblindSprites[key].spriteRenderer.sprite = Graphics.accessibility[4 + (int)color];
                            colorblindSprites[key].spriteRenderer.color = Graphics.KeyTransparent;
                            break;
                        default:
                            colorblindSprites[key].spriteRenderer.sprite = null;
                            break;
                    }
                }
                break;
            case 9:
                keySprites[key].spriteRenderer.sprite = Graphics.symbol[(int)color];
                options[0].ChangeColor(color == Data.Color.Unused ? Graphics.Black : Graphics.White);
                break;
            case 10:
            case 11:
            case 12:
            case 14:
                keySprites[key].spriteRenderer.sprite = Graphics.symbol[(int)color];
                symbolSprites[key].spriteRenderer.sprite = Graphics.symbol[color == Data.Color.Unused ? (26 + (key - 10)) : (57 + (key - 10))];
                break;
            case 13:
                keySprites[key].spriteRenderer.sprite = Graphics.symbol[(int)color];
                symbolSprites[key].spriteRenderer.sprite = Graphics.symbol[color == Data.Color.Unused ? 28 : 59];
                break;
            case 15:
                keySprites[key].spriteRenderer.sprite = Graphics.symbol[(int)color];
                options[1].ChangeColor(color == Data.Color.Unused ? Graphics.Black : Graphics.White);
                colorblindObjects[FirstRow].SetActive(color == Data.Color.Unused ? false : Guess.Colorblind);
                break;
            default:
                Debug.LogWarning($"VirtualKeyboard.ChangeKeyColor: not implemented for key {key}");
                break;
        }
    }

    public static void ApplyColorblindMode()
    {
        for (int i = 0; i < FirstRow; i++) colorblindObjects[i].SetActive(Guess.Colorblind);
        colorblindObjects[FirstRow].SetActive(Guess.Colorblind && Guess.AnswerShown);
    }
}
