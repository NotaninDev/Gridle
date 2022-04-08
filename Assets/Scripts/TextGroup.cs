using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using String = System.String;

public class TextGroup : MonoBehaviour
{
    public enum Type { Page1, Page2, Info, Share, Settings }
    private Type _Type;

    private GameObject frameObject;
    private SpriteBox frameSprite;
    private const float page1FrameWidth = 6.75f, page1FrameHeight = 6.75f;
    private const float page2FrameWidth = 6.98f, page2FrameHeight = 6.38f;
    private const float infoFrameWidth = 6.04f, infoFrameHeight = 1.79f;
    private const float shareFrameWidth = 6.75f, shareFrameHeight = 4.34f;
    private const float settingsFrameWidth = 5.37f, settingsFrameHeight = 1.79f;

    private int optionCount;
    private GameObject[] optionObjects;
    private Option[] options;

    private GameObject boardObject;
    private Guess.Board board;

    private int spriteCount, iconCount;
    private GameObject[] spriteObjects, iconObjects;
    private SpriteBox[] sprites, iconSprites;

    private GameObject resultObject;
    private const float resultShiftX = -.99f, resultCellInterval = .35f, toggleDistance = .33f / 2;
    private bool maskColor, narrowResult;
    private string clipboardText;

    private int coroutineCount;
    private Coroutine[] coroutines;

    void Awake()
    {
        frameObject = General.AddChild(gameObject, "Frame");
        frameSprite = frameObject.AddComponent<SpriteBox>();
    }

    public void Initialize(Type type)
    {
        frameSprite.Initialize(Graphics.optionBox[0], "Text Group", 0, Vector3.zero, useCollider: true);
        frameSprite.spriteRenderer.drawMode = SpriteDrawMode.Sliced;

        _Type = type;
        Data.Color[] guessColors;
        switch (type)
        {
            case Type.Page1:
                frameSprite.ChangeSize(new Vector2(page1FrameWidth, page1FrameHeight));

                optionCount = 7;
                optionObjects = new GameObject[optionCount];
                options = new Option[optionCount];
                optionObjects[0] = General.AddChild(gameObject, $"Result Text");
                optionObjects[0].transform.localPosition = new Vector3(0, 2.32f, 0);
                options[0] = optionObjects[0].AddComponent<Option>();
                options[0].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Guess the <b>Gridle</b> in 6 tries.{Environment.NewLine}" +
                    $"Each guess must be a polyomino tiling of a 4x4 grid.{Environment.NewLine}" +
                    $"After each guess, the color of the tiles will change{Environment.NewLine}to show how close your guess was to the answer.",
                    Graphics.Font.Recurso, 2.4f, Graphics.Black, Vector2.zero, false,
                    lineSpacing: 30f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[1] = General.AddChild(gameObject, $"Example");
                optionObjects[1].transform.localPosition = new Vector3(-2.38f, 1.16f, 0);
                options[1] = optionObjects[1].AddComponent<Option>();
                options[1].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Example", Graphics.Font.RecursoBold, 3.2f, Graphics.Black,
                    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                optionObjects[2] = General.AddChild(gameObject, $"Correct Example");
                optionObjects[2].transform.localPosition = new Vector3(.82f, .49f, 0);
                options[2] = optionObjects[2].AddComponent<Option>();
                options[2].Initialize("Text Group", 1, null, 1f, 1f, 2, $"The <b>I tetromino</b>         is in the{Environment.NewLine}" +
                    $"answer and in the correct spot.", Graphics.Font.Recurso, 2.4f, Graphics.Black,
                    Vector2.zero, false, lineSpacing: 18f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[3] = General.AddChild(gameObject, $"Close Example");
                optionObjects[3].transform.localPosition = new Vector3(.8f, -.57f, 0);
                options[3] = optionObjects[3].AddComponent<Option>();
                options[3].Initialize("Text Group", 1, null, 1f, 1f, 2, $"The <b>Z tetromino</b>      is in the{Environment.NewLine}" +
                    $"answer with the correct orien-{Environment.NewLine}" +
                    $"tation, but in the wrong spot.", Graphics.Font.Recurso, 2.4f, Graphics.Black,
                    Vector2.zero, false, lineSpacing: 18f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[4] = General.AddChild(gameObject, $"Far Example");
                optionObjects[4].transform.localPosition = new Vector3(-.20f, -1.57f, 0);
                options[4] = optionObjects[4].AddComponent<Option>();
                options[4].Initialize("Text Group", 1, null, 1f, 1f, 2,
                    $"The <b>L tetromino</b>       is in the answer, but its{Environment.NewLine}" +
                    $"orientation is wrong (possibly flipped).", Graphics.Font.Recurso, 2.4f, Graphics.Black,
                    Vector2.zero, false, lineSpacing: 18f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[5] = General.AddChild(gameObject, $"Wrong Example");
                optionObjects[5].transform.localPosition = new Vector3(-.32f, -2.35f, 0);
                options[5] = optionObjects[5].AddComponent<Option>();
                options[5].Initialize("Text Group", 1, null, 1f, 1f, 2, $"The <b>monomino</b>    and <b>L triomino</b>     are" +
                    $" not{Environment.NewLine}in the answer.", Graphics.Font.Recurso, 2.4f, Graphics.Black, Vector2.zero, false,
                    lineSpacing: 18f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[6] = General.AddChild(gameObject, $"Page 1");
                optionObjects[6].transform.localPosition = new Vector3(0, -3.03f, 0);
                options[6] = optionObjects[6].AddComponent<Option>();
                options[6].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Page 1", Graphics.Font.RecursoBold, 2.4f, Graphics.Black,
                    new Vector2(1.58f, .35f), true, lineSpacing: 18f, alignment: TextAlignmentOptions.Capline, useCollider: true);

                boardObject = General.AddChild(gameObject, "Example Board");
                board = boardObject.AddComponent<Guess.Board>();
                board.SetBoard(Data.Board.Decode("RRRXXXX, X, DLDXXXX, RXDXX, DLLXXXX"), isExample: true);
                guessColors = new Data.Color[] { Data.Color.Correct, Data.Color.Wrong, Data.Color.Close, Data.Color.Wrong, Data.Color.Far };
                board.ChangeColor(guessColors);
                boardObject.transform.localPosition = new Vector3(-2.17f, -.09f, 0);
                boardObject.transform.localScale = new Vector3(.75f, .75f, 1);

                iconCount = 8;
                iconObjects = new GameObject[iconCount];
                iconSprites = new SpriteBox[iconCount];
                iconObjects[0] = General.AddChild(gameObject, $"Icon Close");
                iconSprites[0] = iconObjects[0].AddComponent<SpriteBox>();
                iconSprites[0].Initialize(Graphics.icon[1], "Text Group", 1,
                    new Vector3(page1FrameWidth / 2 - .3f, page1FrameHeight / 2 - .3f, 0), useCollider: true);
                iconObjects[1] = General.AddChild(optionObjects[2], $"I Tetromino");
                iconSprites[1] = iconObjects[1].AddComponent<SpriteBox>();
                iconSprites[1].Initialize(Graphics.symbol[12], "Text Group", 2, new Vector3(.52f, .17f, 0));
                iconObjects[1].transform.localEulerAngles = new Vector3(0, 0, 90);
                iconObjects[2] = General.AddChild(optionObjects[3], $"Z Tetromino");
                iconSprites[2] = iconObjects[2].AddComponent<SpriteBox>();
                iconSprites[2].Initialize(Graphics.symbol[16], "Text Group", 2, new Vector3(.46f, .34f, 0));
                iconObjects[2].transform.localEulerAngles = new Vector3(0, 0, 90);
                iconObjects[3] = General.AddChild(optionObjects[4], $"L Tetromino");
                iconSprites[3] = iconObjects[3].AddComponent<SpriteBox>();
                iconSprites[3].Initialize(Graphics.symbol[13], "Text Group", 2, new Vector3(-.45f, .19f, 0));
                iconObjects[3].transform.localEulerAngles = new Vector3(0, 0, -90);
                iconObjects[3].transform.localScale = new Vector3(-1, 1, 1);
                iconObjects[4] = General.AddChild(optionObjects[5], $"Monomino");
                iconSprites[4] = iconObjects[4].AddComponent<SpriteBox>();
                iconSprites[4].Initialize(Graphics.symbol[8], "Text Group", 2, new Vector3(-.65f, .14f, 0));
                iconObjects[5] = General.AddChild(optionObjects[5], $"L Triomino");
                iconSprites[5] = iconObjects[5].AddComponent<SpriteBox>();
                iconSprites[5].Initialize(Graphics.symbol[11], "Text Group", 2, new Vector3(1.58f, .15f, 0));
                iconObjects[6] = General.AddChild(optionObjects[6], $"Page Left");
                iconSprites[6] = iconObjects[6].AddComponent<SpriteBox>();
                iconSprites[6].Initialize(Graphics.icon[11], "Text Group", 2, new Vector3(-.64f, 0, 0));
                iconObjects[7] = General.AddChild(optionObjects[6], $"Page Right");
                iconSprites[7] = iconObjects[7].AddComponent<SpriteBox>();
                iconSprites[7].Initialize(Graphics.icon[10], "Text Group", 2, new Vector3(.64f, 0, 0));
                iconObjects[7].transform.localEulerAngles = new Vector3(0, 0, 180);
                break;

            case Type.Page2:
                frameSprite.ChangeSize(new Vector2(page2FrameWidth, page2FrameHeight));

                optionCount = 5;
                optionObjects = new GameObject[optionCount];
                options = new Option[optionCount];
                optionObjects[0] = General.AddChild(gameObject, $"Tips");
                optionObjects[0].transform.localPosition = new Vector3(-2.92f, 2.7f, 0);
                options[0] = optionObjects[0].AddComponent<Option>();
                options[0].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Tips", Graphics.Font.RecursoBold, 3.2f, Graphics.Black,
                    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                optionObjects[1] = General.AddChild(gameObject, $"Symmetry");
                optionObjects[1].transform.localPosition = new Vector3(-.38f, 1.79f, 0);
                options[1] = optionObjects[1].AddComponent<Option>();
                options[1].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Orientation over symmetry does not matter.{Environment.NewLine}" +
                    $"For example, a domino has only 2 possible{Environment.NewLine}orientation        .", Graphics.Font.Recurso, 2.4f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[2] = General.AddChild(gameObject, $"Multiple");
                optionObjects[2].transform.localPosition = new Vector3(1.02f, -.07f, 0);
                options[2] = optionObjects[2].AddComponent<Option>();
                options[2].Initialize("Text Group", 1, null, 1f, 1f, 2, $"If a guess has multiple polyominoes{Environment.NewLine}" +
                    $"of the same shape, only the number{Environment.NewLine}" +
                    $"of these polyominoes in the answer{Environment.NewLine}" +
                    $"get colored. In this example, exactly{Environment.NewLine}" +
                    $"6 dominoes are in the answer, and{Environment.NewLine}" +
                    $"4 of them are horizontal.", Graphics.Font.Recurso, 2.4f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[3] = General.AddChild(gameObject, $"Monomino");
                optionObjects[3].transform.localPosition = new Vector3(-.13f, -1.9f, 0);
                options[3] = optionObjects[3].AddComponent<Option>();
                options[3].Initialize("Text Group", 1, null, 1f, 1f, 2, $"The answer is guaranteed to have at most{Environment.NewLine}" +
                    $"1 monomino    . You can use as many as you want{Environment.NewLine}in your guess tho :)", Graphics.Font.Recurso, 2.4f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[4] = General.AddChild(gameObject, $"Page 2");
                optionObjects[4].transform.localPosition = new Vector3(0, -2.83f, 0);
                options[4] = optionObjects[4].AddComponent<Option>();
                options[4].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Page 2", Graphics.Font.RecursoBold, 2.4f, Graphics.Black,
                    new Vector2(1.58f, .35f), true, lineSpacing: 18f, alignment: TextAlignmentOptions.Capline, useCollider: true);

                boardObject = General.AddChild(gameObject, "Example Board");
                board = boardObject.AddComponent<Guess.Board>();
                board.SetBoard(Data.Board.Decode("RXX, RXX, RXX, RXX, RXX, RXX, RXX, RXX"), isExample: true);
                guessColors = new Data.Color[] { Data.Color.Close, Data.Color.Close, Data.Color.Close, Data.Color.Far, Data.Color.Far,
                    Data.Color.Wrong, Data.Color.Correct, Data.Color.Wrong };
                board.ChangeColor(guessColors);
                boardObject.transform.localPosition = new Vector3(-2.31f, -.07f, 0);
                boardObject.transform.localScale = new Vector3(.75f, .75f, 1);

                iconCount = 6;
                iconObjects = new GameObject[iconCount];
                iconSprites = new SpriteBox[iconCount];
                iconObjects[0] = General.AddChild(gameObject, $"Icon Close");
                iconSprites[0] = iconObjects[0].AddComponent<SpriteBox>();
                iconSprites[0].Initialize(Graphics.icon[1], "Text Group", 1,
                    new Vector3(page2FrameWidth / 2 - .3f, page2FrameHeight / 2 - .3f, 0), useCollider: true);
                iconObjects[1] = General.AddChild(optionObjects[1], $"Domino Vertical");
                iconSprites[1] = iconObjects[1].AddComponent<SpriteBox>();
                iconSprites[1].Initialize(Graphics.symbol[9], "Text Group", 2, new Vector3(-1.17f, -.39f, 0));
                iconObjects[2] = General.AddChild(optionObjects[1], $"Domino Horizontal");
                iconSprites[2] = iconObjects[2].AddComponent<SpriteBox>();
                iconSprites[2].Initialize(Graphics.symbol[9], "Text Group", 2, new Vector3(-.89f, -.39f, 0));
                iconObjects[2].transform.localEulerAngles = new Vector3(0, 0, 90);
                iconObjects[3] = General.AddChild(optionObjects[3], $"Monomino");
                iconSprites[3] = iconObjects[3].AddComponent<SpriteBox>();
                iconSprites[3].Initialize(Graphics.symbol[8], "Text Group", 2, new Vector3(-1.35f, .01f, 0));
                iconObjects[4] = General.AddChild(optionObjects[4], $"Page Left");
                iconSprites[4] = iconObjects[4].AddComponent<SpriteBox>();
                iconSprites[4].Initialize(Graphics.icon[10], "Text Group", 2, new Vector3(-.64f, 0, 0));
                iconObjects[5] = General.AddChild(optionObjects[4], $"Page Right");
                iconSprites[5] = iconObjects[5].AddComponent<SpriteBox>();
                iconSprites[5].Initialize(Graphics.icon[11], "Text Group", 2, new Vector3(.64f, 0, 0));
                iconObjects[5].transform.localEulerAngles = new Vector3(0, 0, 180);
                break;

            case Type.Info:
                frameSprite.ChangeSize(new Vector2(infoFrameWidth, infoFrameHeight));

                optionCount = 3;
                optionObjects = new GameObject[optionCount];
                options = new Option[optionCount];
                optionObjects[0] = General.AddChild(gameObject, $"Made by");
                optionObjects[0].transform.localPosition = new Vector3(-1.75f, .16f, 0);
                options[0] = optionObjects[0].AddComponent<Option>();
                options[0].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Made by", Graphics.Font.RecursoBold, 3.2f, Graphics.Black,
                    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                optionObjects[1] = General.AddChild(gameObject, $"Notan");
                optionObjects[1].transform.localPosition = new Vector3(-.12f, .16f, 0);
                options[1] = optionObjects[1].AddComponent<Option>();
                options[1].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Notan", Graphics.Font.RecursoBold, 3.2f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                optionObjects[2] = General.AddChild(gameObject, $"Address");
                optionObjects[2].transform.localPosition = new Vector3(.87f, -.33f, 0);
                options[2] = optionObjects[2].AddComponent<Option>();
                options[2].Initialize("Text Group", 1, null, 1f, 1f, 2, $"https://notaninart.itch.io/", Graphics.Font.Recurso, 2.4f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);

                iconCount = 4;
                iconObjects = new GameObject[iconCount];
                iconSprites = new SpriteBox[iconCount];
                iconObjects[0] = General.AddChild(gameObject, $"Icon Close");
                iconSprites[0] = iconObjects[0].AddComponent<SpriteBox>();
                iconSprites[0].Initialize(Graphics.icon[1], "Text Group", 1,
                    new Vector3(infoFrameWidth / 2 - .3f, infoFrameHeight / 2 - .3f, 0), useCollider: true);
                for (int i = 1; i < iconCount; i++)
                {
                    iconObjects[i] = General.AddChild(gameObject, $"Bird{i - 1}");
                    iconSprites[i] = iconObjects[i].AddComponent<SpriteBox>();
                    iconSprites[i].Initialize(Graphics.notanBird, "Text Group", 1, new Vector3(.16f + .61f * i, .15f, 0));
                    iconObjects[i].transform.localScale = new Vector3(.3f, .3f, 1);
                }
                iconSprites[1].spriteRenderer.color = Graphics.Orange;
                iconSprites[2].spriteRenderer.color = Graphics.Yellow;
                iconSprites[3].spriteRenderer.color = Graphics.Green;

                //frameSprite.ChangeSize(new Vector2(8f, 8f));

                //optionCount = 6;
                //optionObjects = new GameObject[optionCount];
                //options = new Option[optionCount];
                //optionObjects[0] = General.AddChild(gameObject, $"G");
                //optionObjects[0].transform.localPosition = new Vector3(-1.35f, -.45f, 0);
                //options[0] = optionObjects[0].AddComponent<Option>();
                //options[0].Initialize("Text Group", 10, null, 1f, 1f, 11, $"G", Graphics.Font.RecursoBold, 7.6f, Graphics.White,
                //    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                //optionObjects[0].transform.localEulerAngles = new Vector3(0, 0, 90);
                //optionObjects[1] = General.AddChild(gameObject, $"R");
                //optionObjects[1].transform.localPosition = new Vector3(-1.35f, .45f, 0);
                //options[1] = optionObjects[1].AddComponent<Option>();
                //options[1].Initialize("Text Group", 10, null, 1f, 1f, 11, $"R", Graphics.Font.RecursoBold, 7.6f, Graphics.White,
                //    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                //optionObjects[1].transform.localEulerAngles = new Vector3(0, 0, 90);
                //optionObjects[2] = General.AddChild(gameObject, $"I");
                //optionObjects[2].transform.localPosition = new Vector3(-.45f, .45f, 0);
                //options[2] = optionObjects[2].AddComponent<Option>();
                //options[2].Initialize("Text Group", 10, null, 1f, 1f, 11, $"I", Graphics.Font.RecursoBold, 7.6f, Graphics.White,
                //    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                //optionObjects[2].transform.localEulerAngles = new Vector3(0, 0, 90);
                //optionObjects[3] = General.AddChild(gameObject, $"D");
                //optionObjects[3].transform.localPosition = new Vector3(-.45f, 1.35f, 0);
                //options[3] = optionObjects[3].AddComponent<Option>();
                //options[3].Initialize("Text Group", 10, null, 1f, 1f, 11, $"D", Graphics.Font.RecursoBold, 7.6f, Graphics.White,
                //    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                //optionObjects[3].transform.localEulerAngles = new Vector3(0, 0, 90);
                //optionObjects[4] = General.AddChild(gameObject, $"L");
                //optionObjects[4].transform.localPosition = new Vector3(.45f, -.45f, 0);
                //options[4] = optionObjects[4].AddComponent<Option>();
                //options[4].Initialize("Text Group", 10, null, 1f, 1f, 11, $"L", Graphics.Font.RecursoBold, 7.6f, Graphics.White,
                //    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);
                //optionObjects[5] = General.AddChild(gameObject, $"E");
                //optionObjects[5].transform.localPosition = new Vector3(1.35f, -.45f, 0);
                //options[5] = optionObjects[5].AddComponent<Option>();
                //options[5].Initialize("Text Group", 10, null, 1f, 1f, 11, $"E", Graphics.Font.RecursoBold, 7.6f, Graphics.White,
                //    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);

                //boardObject = General.AddChild(gameObject, "Example Board");
                //board = boardObject.AddComponent<Guess.Board>();
                //board.SetBoard(Data.Board.Decode("X, DLDXXXX, RDLXXXX, DRXLXXX, RXX, X"), isExample: true);
                //guessColors = new Data.Color[] { Data.Color.Unused, Data.Color.Correct, Data.Color.Unused, Data.Color.Unused,
                //    Data.Color.Correct, Data.Color.Unused };
                //board.ChangeColor(guessColors, cover: true);
                //boardObject.transform.localScale = new Vector3(1.5f, 1.5f, 1);

                //iconCount = 1;
                //iconObjects = new GameObject[iconCount];
                //iconSprites = new SpriteBox[iconCount];
                //iconObjects[0] = General.AddChild(gameObject, $"Icon Close");
                //iconSprites[0] = iconObjects[0].AddComponent<SpriteBox>();
                //iconSprites[0].Initialize(Graphics.icon[1], "Text Group", 1,
                //    new Vector3(8f / 2 - .3f, 8f / 2 - .3f, 0), useCollider: true);
                break;

            case Type.Share:
                maskColor = true;
                narrowResult = false;

                float smallFrameWidth = 4.84f, smallFrameHeight = 1.84f;
                frameSprite.ChangeSize(new Vector2(smallFrameWidth, smallFrameHeight));

                optionCount = 7;
                optionObjects = new GameObject[optionCount];
                options = new Option[optionCount];
                optionObjects[0] = General.AddChild(gameObject, $"Result Text");
                optionObjects[0].transform.localPosition = new Vector3(0, 0, 0);
                options[0] = optionObjects[0].AddComponent<Option>();
                options[0].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Play today's Gridle first{Environment.NewLine}to share your result",
                    Graphics.Font.RecursoBold, 3.2f, Graphics.Black, Vector2.zero, false,
                    lineSpacing: -6f, alignment: TextAlignmentOptions.Midline);
                optionObjects[1] = General.AddChild(gameObject, $"Mask Color");
                optionObjects[1].transform.localPosition = new Vector3(1.86f, 1.17f, 0);
                options[1] = optionObjects[1].AddComponent<Option>();
                options[1].Initialize("Text Group", 1, null, 1f, 1f, 2, "Mask Color", Graphics.Font.Recurso, 2.4f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: -6f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[2] = General.AddChild(gameObject, $"Narrow");
                optionObjects[2].transform.localPosition = new Vector3(1.65f, .73f, 0);
                options[2] = optionObjects[2].AddComponent<Option>();
                options[2].Initialize("Text Group", 1, null, 1f, 1f, 2, "Narrow", Graphics.Font.Recurso, 2.4f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: -6f, alignment: TextAlignmentOptions.MidlineLeft);
                optionObjects[3] = General.AddChild(gameObject, $"Share");
                optionObjects[3].transform.localPosition = new Vector3(2.24f, -.18f, 0);
                options[3] = optionObjects[3].AddComponent<Option>();
                options[3].Initialize("Text Group", 1, Graphics.optionBox[2], 1f, 1f, 2, "SHARE", Graphics.Font.RecursoBold, 3.2f,
                    Graphics.White, new Vector2(1.68f, .53f), true, lineSpacing: -6f, alignment: TextAlignmentOptions.Midline, useCollider: true);
                optionObjects[4] = General.AddChild(gameObject, $"Next Gridle");
                optionObjects[4].transform.localPosition = new Vector3(2.24f, -.83f, 0);
                options[4] = optionObjects[4].AddComponent<Option>();
                options[4].Initialize("Text Group", 1, null, 1f, 1f, 2, "Next Gridle", Graphics.Font.RecursoBold, 3.2f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: -6f, alignment: TextAlignmentOptions.Midline);
                optionObjects[5] = General.AddChild(gameObject, $"Time");
                optionObjects[5].transform.localPosition = new Vector3(2.24f, -1.18f, 0);
                options[5] = optionObjects[5].AddComponent<Option>();
                options[5].Initialize("Text Group", 1, null, 1f, 1f, 2, "never", Graphics.Font.RecursoBold, 3.2f,
                    Graphics.Black, Vector2.zero, false, lineSpacing: -6f, alignment: TextAlignmentOptions.Midline);
                optionObjects[6] = General.AddChild(gameObject, $"Clipboard Message");
                optionObjects[6].transform.localPosition = new Vector3(0, -2.81f, 0);
                options[6] = optionObjects[6].AddComponent<Option>();
                options[6].Initialize("Text Group", 10, Graphics.optionBox[1], 1f, 1f, 11, null, Graphics.Font.RecursoBold, 3.2f,
                    Graphics.White, new Vector2(.53f, .15f), false, lineSpacing: -6f, alignment: TextAlignmentOptions.Midline);
                for (int i = 1; i < optionCount; i++) optionObjects[i].SetActive(false);

                resultObject = General.AddChild(gameObject, "Result Parent");
                resultObject.transform.localPosition = new Vector3(resultShiftX, 0, 0);

                clipboardText = "";
                coroutineCount = 2;
                coroutines = new Coroutine[coroutineCount];
                for (int i = 0; i < coroutineCount; i++) coroutines[i] = null;

                spriteCount = Data.Board.Size * Guess.GuessCount;
                spriteObjects = new GameObject[spriteCount * 2];
                sprites = new SpriteBox[spriteCount * 2];
                for (int i = 0; i < spriteCount; i++)
                {
                    spriteObjects[i] = General.AddChild(resultObject, $"Result Square{i}");
                    sprites[i] = spriteObjects[i].AddComponent<SpriteBox>();
                    sprites[i].Initialize(Graphics.symbol[44], "Text Group", 1, Vector3.zero);
                    spriteObjects[spriteCount + i] = General.AddChild(spriteObjects[i], $"Result Colorblind{i}");
                    sprites[spriteCount + i] = spriteObjects[spriteCount + i].AddComponent<SpriteBox>();
                    sprites[spriteCount + i].Initialize(null, "Text Group", 2, Vector3.zero);
                    spriteObjects[spriteCount + i].SetActive(Guess.Colorblind);
                }
                resultObject.SetActive(false);

                iconCount = 5;
                iconObjects = new GameObject[iconCount];
                iconSprites = new SpriteBox[iconCount];
                for (int i = 0; i < iconCount; i++)
                {
                    iconObjects[i] = General.AddChild(gameObject, $"Icon{i}");
                    iconSprites[i] = iconObjects[i].AddComponent<SpriteBox>();
                }
                iconObjects[0].name = "Close";
                iconSprites[0].Initialize(Graphics.icon[1], "Text Group", 1,
                    new Vector3(smallFrameWidth / 2 - .3f, smallFrameHeight / 2 - .3f, 0), useCollider: true);
                iconObjects[1].name = "Mask Color Toggle";
                iconSprites[1].Initialize(Graphics.icon[maskColor ? 3 : 2], "Text Group", 1, new Vector3(2.95f, 1.17f, 0), useCollider: true);
                iconObjects[2].transform.parent = iconObjects[1].transform;
                iconObjects[2].name = "Mask Color Button";
                iconSprites[2].Initialize(Graphics.icon[5], "Text Group", 2, new Vector3(maskColor ? toggleDistance : -toggleDistance, 0, 0));
                iconObjects[3].name = "Narrow Toggle";
                iconSprites[3].Initialize(Graphics.icon[narrowResult ? 3 : 2], "Text Group", 1, new Vector3(2.95f, .73f, 0), useCollider: true);
                iconObjects[4].transform.parent = iconObjects[3].transform;
                iconObjects[4].name = "Narrow Button";
                iconSprites[4].Initialize(Graphics.icon[5], "Text Group", 2, new Vector3(narrowResult ? toggleDistance : -toggleDistance, 0, 0));
                for (int i = 1; i < iconCount; i++) iconObjects[i].SetActive(false);
                break;

            case Type.Settings:
                frameSprite.ChangeSize(new Vector2(settingsFrameWidth, settingsFrameHeight));

                optionCount = 3;
                optionObjects = new GameObject[optionCount];
                options = new Option[optionCount];
                optionObjects[0] = General.AddChild(gameObject, $"Colorblind");
                optionObjects[0].transform.localPosition = new Vector3(-.57f, .32f, 0);
                options[0] = optionObjects[0].AddComponent<Option>();
                options[0].Initialize("Text Group", 1, null, 1f, 1f, 2, $"Colorblind Mode", Graphics.Font.RecursoBold, 3.2f, Graphics.Black,
                    Vector2.zero, false, lineSpacing: 30f, alignment: TextAlignmentOptions.Midline);

                iconCount = 14;
                iconObjects = new GameObject[iconCount];
                iconSprites = new SpriteBox[iconCount];
                for (int i = 0; i < iconCount; i++)
                {
                    iconObjects[i] = General.AddChild(gameObject, $"Icon{i}");
                    iconSprites[i] = iconObjects[i].AddComponent<SpriteBox>();
                }
                iconObjects[0].name = "Close";
                iconSprites[0].Initialize(Graphics.icon[1], "Text Group", 1,
                    new Vector3(settingsFrameWidth / 2 - .3f, settingsFrameHeight / 2 - .3f, 0), useCollider: true);
                iconObjects[1].name = "Color Blind Toggle";
                iconSprites[1].Initialize(Graphics.icon[Guess.Colorblind ? 3 : 2], "Text Group", 1, new Vector3(1.49f, .32f, 0), useCollider: true);
                iconObjects[2].transform.parent = iconObjects[1].transform;
                iconObjects[2].name = "Color Blind Button";
                iconSprites[2].Initialize(Graphics.icon[5], "Text Group", 2,
                    new Vector3(Guess.Colorblind ? toggleDistance : -toggleDistance, 0, 0));
                iconObjects[3].name = "Cell Wrong";
                iconObjects[4].name = "Cell Far";
                iconObjects[5].name = "Cell Close";
                iconObjects[6].name = "Cell Correct";
                for (int i = 0; i < 4; i++)
                {
                    iconSprites[3 + i].Initialize(Graphics.symbol[33 + i], "Text Group", 1, new Vector3(-.96f + .32f * i, -.33f, 0));
                    iconObjects[7 + i].transform.parent = iconObjects[3 + i].transform;
                    iconObjects[7 + i].name = $"Center{i}";
                    iconSprites[7 + i].Initialize(Graphics.symbol[41], "Text Group", 4, Vector3.zero);

                }
                iconSprites[4].Initialize(Graphics.symbol[34], "Text Group", 1, new Vector3(-.32f, -.33f, 0));
                iconSprites[5].Initialize(Graphics.symbol[35], "Text Group", 1, new Vector3(.32f, -.33f, 0));
                iconSprites[6].Initialize(Graphics.symbol[36], "Text Group", 1, new Vector3(.96f, -.33f, 0));
                iconObjects[11].name = "Accessibility Far";
                iconObjects[12].name = "Accessibility Close";
                iconObjects[13].name = "Accessibility Correct";
                for (int i = 0; i < 3; i++)
                {
                    iconObjects[11 + i].transform.parent = iconObjects[4 + i].transform;
                    iconSprites[11 + i].Initialize(Graphics.accessibility[i], "Text Group", 2, Vector3.zero);
                    iconSprites[11 + i].spriteRenderer.color = Graphics.ColorblindTransparent;
                    iconObjects[11 + i].SetActive(Guess.Colorblind);
                }
                for (int i = 0; i < 4; i++) iconObjects[3 + i].transform.localScale = new Vector3(.75f, .75f, 1);
                break;

            default:
                Debug.LogWarning($"TextGroup.Initialize: not implemented for type {Enum.GetName(typeof(Type), (int)type)}");
                break;
        }
    }

    private static Vector3 GetResultPosition(bool narrowResult, int index, float verticalShift)
    {
        if (narrowResult)
        {
            return new Vector3(index % Data.Board.Width - (float)(Data.Board.Width - 1) / 2,
                -index / Data.Board.Width, 0) * resultCellInterval + new Vector3(0, verticalShift, 0);
        }
        else
        {
            return new Vector3((index / Data.Board.Size % Guess.Width - (float)(Guess.Width - 1) / 2) * Data.Board.Width +
                index % Data.Board.Width - (float)(Data.Board.Width - 1) / 2,
                -index / Data.Board.Size / Guess.Width * Data.Board.Height + (float)(Data.Board.Height - 1) / 2 -
                index % Data.Board.Size / Data.Board.Width, 0) * resultCellInterval + new Vector3(0, verticalShift, 0);
        }
    }

    public void HandleInput()
    {
        switch (_Type)
        {
            case Type.Page1:
                if (iconSprites[0].Mouse.GetMouseClick() || Input.GetMouseButtonDown(0) && !frameSprite.Mouse.GetMouseClick())
                {
                    MainGame.CloseTextGroup();
                    return;
                }
                else if (options[6].Mouse.GetMouseClick())
                {
                    MainGame.CloseTextGroup(nextPage: true);
                    return;
                }
                break;
            case Type.Page2:
                if (iconSprites[0].Mouse.GetMouseClick() || Input.GetMouseButtonDown(0) && !frameSprite.Mouse.GetMouseClick())
                {
                    MainGame.CloseTextGroup();
                    return;
                }
                else if (options[4].Mouse.GetMouseClick())
                {
                    MainGame.CloseTextGroup(nextPage: true);
                    return;
                }
                break;
            case Type.Info:
                if (iconSprites[0].Mouse.GetMouseClick() || Input.GetMouseButtonDown(0) && !frameSprite.Mouse.GetMouseClick())
                {
                    MainGame.CloseTextGroup();
                    return;
                }
                break;
            case Type.Share:
                TimeSpan untilNext = Data.NextUtcDate - DateTime.UtcNow;
                options[5].ChangeText((untilNext.TotalSeconds < 0 ? (untilNext.Days < 0 ? $"{untilNext.Days}:" : "-") : "") +
                    untilNext.ToString(@"hh\:mm\:ss"));
                if (iconSprites[0].Mouse.GetMouseClick() || Input.GetMouseButtonDown(0) && !frameSprite.Mouse.GetMouseClick())
                {
                    MainGame.CloseTextGroup();
                    return;
                }
                else if (options[3].Mouse.GetMouseClick())
                {
                    bool success = true;
#if UNITY_WEBGL && !UNITY_EDITOR
                    success = General.CopyToClipboard(clipboardText);
#else
                    GUIUtility.systemCopyBuffer = clipboardText;
#endif
                    string clipboardMessage = success ? "Copied to clipboard!" : "Failed to copy to clipboard :(";
                    optionObjects[6].SetActive(true);
                    options[6].ChangeText(clipboardMessage);
                    for (int i = 0; i < coroutineCount; i++)
                    {
                        if (coroutines[i] != null) StopCoroutine(coroutines[i]);
                    }
                    optionObjects[6].transform.localPosition = new Vector3(0, -2.81f, 0);
                    coroutines[0] = StartCoroutine(Fade(optionObjects[6], new Vector3(0, -2.81f, 0), new Vector3(0, -3.32f, 0), .2f, delay: 1.6f));
                    coroutines[1] = StartCoroutine(Graphics.ChangeColor(options[6].OptionRenderers[0], Graphics.White, Graphics.SemiTransparent,
                        duration: .2f, delay: 1.6f));
                }
                else if (iconSprites[1].Mouse.GetMouseClick())
                {
                    maskColor = !maskColor;
                    iconSprites[1].spriteRenderer.sprite = Graphics.icon[maskColor ? 3 : 2];
                    iconObjects[2].transform.localPosition = new Vector3(maskColor ? toggleDistance : -toggleDistance, 0, 0);
                    UpdateResult();
                }
                else if (iconSprites[3].Mouse.GetMouseClick())
                {
                    narrowResult = !narrowResult;
                    iconSprites[3].spriteRenderer.sprite = Graphics.icon[narrowResult ? 3 : 2];
                    iconObjects[4].transform.localPosition = new Vector3(narrowResult ? toggleDistance : -toggleDistance, 0, 0);
                    UpdateResult();
                }
                break;
            case Type.Settings:
                if (iconSprites[0].Mouse.GetMouseClick() || Input.GetMouseButtonDown(0) && !frameSprite.Mouse.GetMouseClick())
                {
                    MainGame.CloseTextGroup();
                    return;
                }
                else if (iconSprites[1].Mouse.GetMouseClick())
                {
                    Guess.Colorblind = !Guess.Colorblind;
                    iconSprites[1].spriteRenderer.sprite = Graphics.icon[Guess.Colorblind ? 3 : 2];
                    iconObjects[2].transform.localPosition = new Vector3(Guess.Colorblind ? toggleDistance : -toggleDistance, 0, 0);
                    for (int i = 0; i < 3; i++) iconObjects[11 + i].SetActive(Guess.Colorblind);
                    Guess.ApplyColorblindMode();
                    VirtualKeyboard.ApplyColorblindMode();
                    MainGame.ApplyColorblindMode();
                }
                break;
            default:
                Debug.LogWarning($"TextGroup.HandleInput: not implemented for type {Enum.GetName(typeof(Type), (int)_Type)}");
                break;
        }
    }

    public void UpdateResult()
    {
        if (_Type != Type.Share)
        {
            Debug.LogWarning($"TextGroup.UpdateResult: not allowed to call this function for type {Enum.GetName(typeof(Type), (int)_Type)}");
            return;
        }


        for (int i = 1; i < iconCount; i++) iconObjects[i].SetActive(true);

        resultObject.SetActive(true);
        Data.Color?[] resultColors = Guess.GetResultColors();
        if (resultColors == null)
        {
            Debug.LogWarning($"TextGroup.UpdateResult: failed to get result colors");
            return;
        }

        bool altFormat = narrowResult && resultColors.Length / Data.Board.Size > 2;
        for (int i = 1; i < optionCount - 1; i++) optionObjects[i].SetActive(true);
        optionObjects[optionCount - 1].SetActive(false);
        string resultText = $"Gridle {(Data.RandomMode ? "???" : Data.Seed.ToString())}{(altFormat ? Environment.NewLine : "    ")}" +
            $"{(Guess.Win ? (Guess.BoardIndex + 1).ToString() : "X")}/{Guess.GuessCount}";
        options[0].ChangeText(resultText);
        optionObjects[0].transform.localPosition = altFormat ? new Vector3(2.24f, 1.87f, 0) : new Vector3(0, 1.78f, 0);

        float currentFrameHeight = altFormat ? (resultColors.Length / Data.Board.Width * resultCellInterval +
            (resultColors.Length / Data.Board.Size == 3 ? 1.2f : .5f)) : shareFrameHeight;
        frameSprite.ChangeSize(new Vector2(shareFrameWidth, currentFrameHeight));
        iconObjects[0].transform.localPosition = new Vector3(shareFrameWidth / 2 - .3f, currentFrameHeight / 2 - .3f);

        // squares before unmaskedIndex are maksed
        int unmaskedIndex = Guess.Win ? (Data.Board.Size * Guess.BoardIndex) : (Data.Board.Size * Guess.GuessCount);
        float verticalShift = (narrowResult ? ((float)((resultColors.Length + Data.Board.Width - 1) / Data.Board.Width - 1) / 2) :
            ((float)(((resultColors.Length + Data.Board.Size - 1) / Data.Board.Size + Guess.Width - 1) / Guess.Width - 1) / 2 *
            Data.Board.Height)) * resultCellInterval;
        resultObject.transform.localPosition = new Vector3(((narrowResult || resultColors.Length / Data.Board.Size >= Guess.Width) ?
            0 : ((float)(Guess.Width - resultColors.Length / Data.Board.Size) / 2 * Data.Board.Width * resultCellInterval)) + resultShiftX, 0, 0);
        for (int i = 0; i < resultColors.Length; i++)
        {
            spriteObjects[i].SetActive(true);
            spriteObjects[i].transform.localPosition = GetResultPosition(narrowResult, i, verticalShift);
            if (resultColors[i] == null)
            {
                sprites[i].spriteRenderer.sprite = Graphics.symbol[44];
                sprites[spriteCount + i].spriteRenderer.sprite = null;
            }
            else
            {
                // mask colors
                if (i < unmaskedIndex)
                {
                    switch (resultColors[i])
                    {
                        case Data.Color.Far:
                            sprites[i].spriteRenderer.sprite = Graphics.symbol[maskColor ? 48 : 47];
                            sprites[spriteCount + i].spriteRenderer.sprite = Graphics.accessibility[maskColor ? 4 : 3];
                            sprites[spriteCount + i].spriteRenderer.color = Graphics.ColorblindTransparent;
                            break;
                        case Data.Color.Close:
                            sprites[i].spriteRenderer.sprite = Graphics.symbol[48];
                            sprites[spriteCount + i].spriteRenderer.sprite = Graphics.accessibility[4];
                            sprites[spriteCount + i].spriteRenderer.color = Graphics.ColorblindTransparent;
                            break;
                        case Data.Color.Correct:
                            sprites[i].spriteRenderer.sprite = Graphics.symbol[maskColor ? 48 : 49];
                            sprites[spriteCount + i].spriteRenderer.sprite = Graphics.accessibility[maskColor ? 4 : 5];
                            sprites[spriteCount + i].spriteRenderer.color = Graphics.ColorblindTransparent;
                            break;
                        default:
                            sprites[i].spriteRenderer.sprite = Graphics.symbol[45 + (int)resultColors[i]];
                            sprites[spriteCount + i].spriteRenderer.sprite = null;
                            break;
                    }
                }
                else
                {
                    sprites[i].spriteRenderer.sprite = Graphics.symbol[45 + (int)resultColors[i]];
                    sprites[spriteCount + i].spriteRenderer.sprite = Graphics.accessibility[1 + (int)resultColors[i]];
                    sprites[spriteCount + i].spriteRenderer.color = Graphics.ColorblindTransparent;
                }
            }
        }
        for (int i = resultColors.Length; i < Data.Board.Size * Guess.GuessCount; i++) spriteObjects[i].SetActive(false);
        for (int i = 0; i < spriteCount; i++) spriteObjects[spriteCount + i].SetActive(Guess.Colorblind);

        // format result text
        string[] strips = new string[resultColors.Length / Data.Board.Width];
        for (int i = 0; i < strips.Length; i++)
        {
            string strip = "";
            for (int j = 0; j < Data.Board.Width; j++)
            {
                int index = i * Data.Board.Width + j;
                if (resultColors[index] == null) strip += "\uD83D\uDD33";
                else
                {
                    switch (resultColors[index])
                    {
                        case Data.Color.Wrong:
                            strip += "\u2B1B";
                            break;
                        case Data.Color.Far:
                            strip += (maskColor && index < unmaskedIndex) ? "\uD83D\uDFE8" : "\uD83D\uDFE7";
                            break;
                        case Data.Color.Close:
                            strip += "\uD83D\uDFE8";
                            break;
                        case Data.Color.Correct:
                            strip += (maskColor && index < unmaskedIndex) ? "\uD83D\uDFE8" : "\uD83D\uDFE9";
                            break;
                        default:
                            strip += "\uD83D\uDD33";
                            break;
                    }
                }
            }
            strips[i] = strip;
        }
        clipboardText = $"Gridle {(Data.RandomMode ? "???" : Data.Seed.ToString())} " +
            $"{(Guess.Win ? (Guess.BoardIndex + 1).ToString() : "X")}/{Guess.GuessCount}{Environment.NewLine}";
        if (narrowResult)
        {
            // narrow formatting
            for (int i = 0; i < strips.Length; i++)
            {
                clipboardText += strips[i];
                if (i < strips.Length - 1) clipboardText += Environment.NewLine;
            }
        }
        else
        {
            // normal formatting
            if (resultColors.Length / Data.Board.Size < Guess.Width)
            {
                for (int i = 0; i < Data.Board.Height; i++)
                {
                    for (int j = 0; j < resultColors.Length / Data.Board.Size; j++) clipboardText += strips[j * Data.Board.Height + i];
                    if (i < Data.Board.Height - 1) clipboardText += Environment.NewLine;
                }
            }
            else
            {
                for (int i = 0; i < Data.Board.Height; i++)
                {
                    for (int j = 0; j < Guess.Width; j++) clipboardText += strips[j * Data.Board.Height + i];
                    if (i < Data.Board.Height - 1) clipboardText += Environment.NewLine;
                }
                if (resultColors.Length / Data.Board.Size > Guess.Width)
                {
                    clipboardText += Environment.NewLine;
                    for (int i = 0; i < Data.Board.Height; i++)
                    {
                        for (int j = 0; j < resultColors.Length / Data.Board.Size - Guess.Width; j++)
                        {
                            clipboardText += strips[(Guess.Width + j) * Data.Board.Height + i];
                        }
                        if (i < Data.Board.Height - 1) clipboardText += Environment.NewLine;
                    }
                }
            }
        }

        // GUIUtility.systemCopyBuffer = result;
        // white square button "\uD83D\uDD33";
        // white "\u2B1C";
        // black "\u2B1B";
        // orange "\uD83D\uDFE7";
        // yellow "\uD83D\uDFE8";
        // green "\uD83D\uDFE9";
    }

    public void ApplyColorblindMode()
    {
        switch (_Type)
        {
            case Type.Page1:
            case Type.Page2:
                board.ApplyColorblindMode();
                break;
            case Type.Share:
                for (int i = 0; i < spriteCount; i++) spriteObjects[spriteCount + i].SetActive(Guess.Colorblind);
                break;
            default:
                Debug.LogWarning($"TextGroup.ApplyColorblindMode: " +
                    $"not allowed to call this function for type {Enum.GetName(typeof(Type), (int)_Type)}");
                break;
        }
    }

    private static IEnumerator Fade(GameObject target, Vector3 start, Vector3 end, float duration, float delay = 0)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        if (target == null)
        {
            Debug.LogWarning(String.Format($"TextGroup.Move: target GameObject should not be null."));
            yield break;
        }

        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            target.transform.localPosition = new Vector3(start.x + (end.x - start.x) * time / duration,
                start.y + (end.y - start.y) * time / duration, start.z + (end.z - start.z) * time / duration);
            yield return new WaitForSeconds(0);
        }

        target.SetActive(false);
    }
}
