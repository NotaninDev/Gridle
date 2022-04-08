using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class Guess
{
    private const float CellInterval = .6f;
    public class Shape : MonoBehaviour
    {
        public Data.Shape ShapeData { get; private set; }
        public Data.Color Color { get; private set; }
        private GameObject[] cellObjects, centerObjects, accessibilityObjects, connectionObjects;
        private SpriteBox[] cellSprites, centerSprites, accessibilitySprites, connectionSprites;
        public int Size { get; private set; }
        private int connectionCount;

        public void Initialize(Data.Shape shape, bool grabbed = false, int index = -1, bool isExample = false)
        {
            ShapeData = shape;
            Color = Data.Color.Unused;
            int maxX, maxY;
            (int X, int Y)[] coordinates;
            switch (shape._Type)
            {
                case Data.Shape.Type.Monomino:
                    Size = 1;
                    connectionCount = 0;
                    maxX = maxY = 1;
                    coordinates = new (int X, int Y)[1];
                    coordinates[0] = (0, 0);
                    break;
                case Data.Shape.Type.Domino:
                    Size = 2;
                    connectionCount = 1;
                    maxX = 1;
                    maxY = 2;
                    coordinates = new (int X, int Y)[2];
                    coordinates[0] = (0, 0);
                    coordinates[1] = (0, 1);
                    break;
                case Data.Shape.Type.TriominoI:
                    Size = 3;
                    connectionCount = 2;
                    maxX = 1;
                    maxY = 3;
                    coordinates = new (int X, int Y)[3];
                    coordinates[0] = (0, 0);
                    coordinates[1] = (0, 1);
                    coordinates[2] = (0, 2);
                    break;
                case Data.Shape.Type.TriominoL:
                    Size = 3;
                    connectionCount = 2;
                    maxX = 2;
                    maxY = 2;
                    coordinates = new (int X, int Y)[3];
                    coordinates[0] = (0, 0);
                    coordinates[1] = (0, 1);
                    coordinates[2] = (1, 1);
                    break;
                case Data.Shape.Type.TetrominoI:
                    Size = 4;
                    connectionCount = 3;
                    maxX = 1;
                    maxY = 4;
                    coordinates = new (int X, int Y)[4];
                    coordinates[0] = (0, 0);
                    coordinates[1] = (0, 1);
                    coordinates[2] = (0, 2);
                    coordinates[3] = (0, 3);
                    break;
                case Data.Shape.Type.TetrominoL:
                    Size = 4;
                    connectionCount = 3;
                    maxX = 1;
                    maxY = 3;
                    coordinates = new (int X, int Y)[4];
                    coordinates[0] = (0, 0);
                    coordinates[1] = (0, 1);
                    coordinates[2] = (0, 2);
                    coordinates[3] = (1, 2);
                    break;
                case Data.Shape.Type.TetrominoT:
                    Size = 4;
                    connectionCount = 3;
                    maxX = 1;
                    maxY = 3;
                    coordinates = new (int X, int Y)[4];
                    coordinates[0] = (0, 0);
                    coordinates[1] = (0, 1);
                    coordinates[2] = (0, 2);
                    coordinates[3] = (1, 1);
                    break;
                case Data.Shape.Type.TetrominoO:
                    Size = 4;
                    connectionCount = 4;
                    maxX = 2;
                    maxY = 2;
                    coordinates = new (int X, int Y)[4];
                    coordinates[0] = (0, 0);
                    coordinates[1] = (0, 1);
                    coordinates[2] = (1, 0);
                    coordinates[3] = (1, 1);
                    break;
                case Data.Shape.Type.TetrominoZ:
                    Size = 4;
                    connectionCount = 3;
                    maxX = 3;
                    maxY = 2;
                    coordinates = new (int X, int Y)[4];
                    coordinates[0] = (0, 1);
                    coordinates[1] = (1, 1);
                    coordinates[2] = (1, 0);
                    coordinates[3] = (2, 0);
                    break;
                default:
                    Debug.LogWarning($"Guess.Shape.Initialize: not implemented for type {Enum.GetName(typeof(Data.Shape.Type), shape._Type)}");
                    Size = 1;
                    connectionCount = 0;
                    maxX = maxY = 1;
                    coordinates = new (int X, int Y)[1];
                    coordinates[0] = (1, 1);
                    break;
            }
            if (shape.Flipped)
            {
                for (int i = 0; i < coordinates.Length; i++) coordinates[i] = (maxX - 1 - coordinates[i].X, coordinates[i].Y);
            }
            switch (shape._Rotation)
            {
                case Data.Shape.Rotation.Zero:
                    break;
                case Data.Shape.Rotation.One:
                    for (int i = 0; i < coordinates.Length; i++) coordinates[i] = (maxY - 1 - coordinates[i].Y, coordinates[i].X);
                    break;
                case Data.Shape.Rotation.Two:
                    for (int i = 0; i < coordinates.Length; i++)
                    {
                        coordinates[i] = (maxX - 1 - coordinates[i].X, maxY - 1 - coordinates[i].Y);
                    }
                    break;
                case Data.Shape.Rotation.Three:
                    for (int i = 0; i < coordinates.Length; i++) coordinates[i] = (coordinates[i].Y, maxX - 1 - coordinates[i].X);
                    break;
                default:
                    Debug.LogWarning($"Guess.Shape.Initialize: " +
                        $"not implemented for rotation {Enum.GetName(typeof(Data.Shape.Rotation), shape._Rotation)}");
                    break;
            }

            Vector3 centroid;
            if (index >= 0 && index < Data.Board.Size)
            {
                centroid = -new Vector3(index % Data.Board.Width - (float)(Data.Board.Width - 1) / 2,
                    (float)(Data.Board.Height - 1) / 2 - index / Data.Board.Width, 0);

                // find the top-left cell
                int topLeft = 0;
                for (int i = 1; i < Size; i++)
                {
                    if (coordinates[i].Y > coordinates[topLeft].Y ||
                        coordinates[i].Y == coordinates[topLeft].Y && coordinates[i].X < coordinates[topLeft].X)
                    {
                        topLeft = i;
                    }
                }
                centroid += new Vector3(coordinates[topLeft].X, coordinates[topLeft].Y, 0);
            }
            else
            {
                centroid = Vector3.zero;
                for (int i = 0; i < Size; i++) centroid += new Vector3(coordinates[i].X, coordinates[i].Y, 0);
                centroid = centroid / Size;
            }

            // initialize GameObjects for cells and centers
            cellObjects = new GameObject[Size];
            centerObjects = new GameObject[Size];
            accessibilityObjects = new GameObject[Size];
            cellSprites = new SpriteBox[Size];
            centerSprites = new SpriteBox[Size];
            accessibilitySprites = new SpriteBox[Size];
            for (int i = 0; i < Size; i++)
            {
                cellObjects[i] = General.AddChild(gameObject, $"Cell{i}");
                centerObjects[i] = General.AddChild(cellObjects[i], $"Center{i}");
                accessibilityObjects[i] = General.AddChild(cellObjects[i], $"Color Blind{i}");
                cellSprites[i] = cellObjects[i].AddComponent<SpriteBox>();
                centerSprites[i] = centerObjects[i].AddComponent<SpriteBox>();
                accessibilitySprites[i] = accessibilityObjects[i].AddComponent<SpriteBox>();
                cellSprites[i].Initialize(Graphics.symbol[32], isExample ? "Text Group" : "Board", (grabbed ? 5 : 0) + (isExample ? 1 : 0),
                    (new Vector3(coordinates[i].X, coordinates[i].Y, 0) - centroid) * CellInterval);
                centerSprites[i].Initialize(Graphics.symbol[40], isExample ? "Text Group" : "Board",
                    (grabbed ? 8 : 3) + (isExample ? 1 : 0), Vector3.zero);
                accessibilitySprites[i].Initialize(null, isExample ? "Text Group" : "Board",
                    (grabbed ? 6 : 1) + (isExample ? 1 : 0), Vector3.zero);
                accessibilityObjects[i].SetActive(Colorblind);
            }

            // initialize GameObjects for connections
            connectionObjects = new GameObject[connectionCount];
            connectionSprites = new SpriteBox[connectionCount];
            {
                int count = 0;
                for (int i = 0; i < Size - 1; i++)
                {
                    for (int j = i + 1; j < Size; j++)
                    {
                        if (Math.Abs(coordinates[i].X - coordinates[j].X) + Math.Abs(coordinates[i].Y - coordinates[j].Y) == 1)
                        {
                            if (count >= connectionCount)
                            {
                                Debug.LogWarning($"Guess.Shape.Initialize: found more than {connectionCount} connection for " +
                                    $"shape {Enum.GetName(typeof(Data.Shape.Type), (int)shape._Type)}");
                                break;
                            }
                            connectionObjects[count] = General.AddChild(gameObject, $"Connection{count}");
                            connectionSprites[count] = connectionObjects[count].AddComponent<SpriteBox>();
                            connectionSprites[count].Initialize(Graphics.symbol[42], isExample ? "Text Group" : "Board",
                                (grabbed ? 7 : 2) + (isExample ? 1 : 0), (new Vector3((float)(coordinates[i].X + coordinates[j].X) / 2,
                                (float)(coordinates[i].Y + coordinates[j].Y) / 2, 0) - centroid) * CellInterval);
                            if (coordinates[i].X == coordinates[j].X)
                            {
                                connectionObjects[count].transform.localEulerAngles = new Vector3(0, 0, 90);
                            }
                            count++;
                        }
                    }
                }
                if (count != connectionCount)
                {
                    Debug.LogWarning($"Guess.Shape.Initialize: could find only {count} connection for " +
                        $"shape {Enum.GetName(typeof(Data.Shape.Type), (int)shape._Type)}");
                }
            }
        }
        public void DestroyCells()
        {
            for (int i = 0; i < cellObjects.Length; i++) Destroy(cellObjects[i]);
            for (int i = 0; i < connectionObjects.Length; i++) Destroy(connectionObjects[i]);
        }

        // given position in Master Area, returns an array of index each cell should go in the board
        // returns null if the shape doesn't fit in the board
        public int[] GetIndexInBoard(Vector3 boardPosition)
        {
            int[] result = new int[Size];
            for (int i = 0; i < Size; i++)
            {
                int x = (int)Math.Floor((transform.localPosition.x + cellObjects[i].transform.localPosition.x - boardPosition.x) / CellInterval +
                    (float)(Data.Board.Width - 1) / 2 + .5f),
                    y = (int)Math.Floor((float)(Data.Board.Height - 1) / 2 + .5f -
                    (transform.localPosition.y + cellObjects[i].transform.localPosition.y - boardPosition.y) / CellInterval);
                if (x < 0 || x >= Data.Board.Width || y < 0 || y >= Data.Board.Height) return null;
                result[i] = x + y * Data.Board.Width;
            }
            return result;
        }
        // destination is the destination of cellObject[0]
        public void SetCellPosition(Vector3 destination)
        {
            Vector3 difference = destination - cellObjects[0].transform.localPosition;
            for (int i = 0; i < Size; i++) cellObjects[i].transform.localPosition = cellObjects[i].transform.localPosition + difference;
            for (int i = 0; i < connectionCount; i++)
            {
                connectionObjects[i].transform.localPosition = connectionObjects[i].transform.localPosition + difference;
            }
        }
        public void MoveCenter(Vector3 difference)
        {
            for (int i = 0; i < Size; i++) cellObjects[i].transform.localPosition = cellObjects[i].transform.localPosition - difference;
            for (int i = 0; i < connectionCount; i++)
            {
                connectionObjects[i].transform.localPosition = connectionObjects[i].transform.localPosition - difference;
            }
        }
        public void ChangeSortingOrder(bool grabbed)
        {
            for (int i = 0; i < Size; i++)
            {
                cellSprites[i].spriteRenderer.sortingOrder = grabbed ? 5 : 0;
                centerSprites[i].spriteRenderer.sortingOrder = grabbed ? 8 : 3;
                accessibilitySprites[i].spriteRenderer.sortingOrder = grabbed ? 6 : 1;
            }
            for (int i = 0; i < connectionCount; i++) connectionSprites[i].spriteRenderer.sortingOrder = grabbed ? 7 : 2;
        }
        public void ChangeColor(Data.Color color)
        {
            for (int i = 0; i < Size; i++)
            {
                cellSprites[i].spriteRenderer.sprite = Graphics.symbol[32 + (int)color];
                centerSprites[i].spriteRenderer.sprite = Graphics.symbol[color == Data.Color.Unused ? 40 : 41];
                switch (color)
                {
                    case Data.Color.Far:
                        accessibilitySprites[i].spriteRenderer.sprite = Graphics.accessibility[0];
                        accessibilitySprites[i].spriteRenderer.color = Graphics.ColorblindTransparent;
                        break;
                    case Data.Color.Close:
                        accessibilitySprites[i].spriteRenderer.sprite = Graphics.accessibility[1];
                        accessibilitySprites[i].spriteRenderer.color = Graphics.ColorblindTransparent;
                        break;
                    case Data.Color.Correct:
                        accessibilitySprites[i].spriteRenderer.sprite = Graphics.accessibility[2];
                        accessibilitySprites[i].spriteRenderer.color = Graphics.ColorblindTransparent;
                        break;
                    default:
                        accessibilitySprites[i].spriteRenderer.sprite = null;
                        break;
                }
            }
            for (int i = 0; i < connectionCount; i++)
            {
                connectionSprites[i].spriteRenderer.sprite = Graphics.symbol[color == Data.Color.Unused ? 42 : 43];
            }
            Color = color;
        }
        public void ApplyColorblindMode()
        {
            for (int i = 0; i < Size; i++) accessibilityObjects[i].SetActive(Colorblind);
        }
    }
    public class Board : MonoBehaviour
    {
        public Data.Board BoardData { get; private set; }
        private GameObject[] cellObjects;
        private SpriteBox[] cellSprites;
        // shapeInCell is an array of reference to the shape occupying each cell
        // if the cell is empty, the entry is null
        private Shape[] shapeInCell;

        void Awake()
        {
            BoardData = new Data.Board();
            cellObjects = new GameObject[Data.Board.Size];
            cellSprites = new SpriteBox[Data.Board.Size];
            shapeInCell = new Shape[Data.Board.Size];
            for (int i = 0; i < Data.Board.Size; i++)
            {
                cellObjects[i] = General.AddChild(gameObject, $"Cell{i}");
                cellSprites[i] = cellObjects[i].AddComponent<SpriteBox>();
                Vector3 position = new Vector3(CellInterval * (i % Data.Board.Width - (float)(Data.Board.Width - 1) / 2),
                    CellInterval * ((float)(Data.Board.Height - 1) / 2 - i / Data.Board.Height), 0);
                cellSprites[i].Initialize(Graphics.symbol[31], "Board", 0, position);
                shapeInCell[i] = null;
            }
        }

        public void Reset()
        {
            BoardData.Reset();
            for (int i = 0; i < Data.Board.Size; i++)
            {
                cellObjects[i].SetActive(true);
                if (shapeInCell[i] != null)
                {
                    Destroy(shapeInCell[i].gameObject);
                    shapeInCell[i] = null;
                }
            }
        }

        public void SetBoard(Data.Board boardData, bool isExample = false)
        {
            for (int i = 0; i < Data.Board.Size; i++) cellObjects[i].SetActive(false);
            BoardData = boardData;
            GameObject[] shapeObjects = new GameObject[BoardData.Shapes.Count];
            Shape[] shapes = new Shape[BoardData.Shapes.Count];
            int[] indices = new int[BoardData.Shapes.Count];
            Data.Shape[] shapeData = new Data.Shape[BoardData.Shapes.Count];
            BoardData.Shapes.Keys.CopyTo(indices, 0);
            BoardData.Shapes.Values.CopyTo(shapeData, 0);
            for (int i = 0; i < shapes.Length; i++)
            {
                shapeObjects[i] = General.AddChild(gameObject, $"Shape{i}");
                shapes[i] = shapeObjects[i].AddComponent<Shape>();
                shapes[i].Initialize(shapeData[i], grabbed: false, index: indices[i], isExample: isExample);
                shapes[i].ChangeColor(Data.Color.Correct);
                shapeInCell[indices[i]] = shapes[i];
            }
        }

        public void ChangeCellSprites(bool selected)
        {
            for (int i = 0; i < Data.Board.Size; i++) cellSprites[i].spriteRenderer.sprite = Graphics.symbol[selected ? 32 : 31];
        }

        // returns if the shape is successfully placed on the board
        public bool AddShape(Shape shape)
        {
            int[] indices = shape.GetIndexInBoard(GetBoardPosition(BoardIndex));
            if (indices == null) return false;

            int minIndex = Data.Board.Size;
            Vector3[] positions = new Vector3[indices.Length];
            shape.transform.parent = transform;
            shape.transform.localPosition = Vector3.zero;
            for (int i = 0; i < indices.Length; i++)
            {
                if (shapeInCell[indices[i]] != null)
                {
                    for (int j = 0; j < Data.Board.Size; j++)
                    {
                        if (j != indices[i] && shapeInCell[j] == shapeInCell[indices[i]])
                        {
                            BoardData.Shapes.Remove(j);
                            shapeInCell[j] = null;
                            cellObjects[j].SetActive(true);
                        }
                    }
                    BoardData.Shapes.Remove(indices[i]);
                    Destroy(shapeInCell[indices[i]].gameObject);
                }
                shapeInCell[indices[i]] = shape;
                cellObjects[indices[i]].SetActive(false);
                if (indices[i] < minIndex) minIndex = indices[i];
            }
            Vector3 destination = new Vector3(CellInterval * (indices[0] % Data.Board.Width - (float)(Data.Board.Width - 1) / 2),
                    CellInterval * ((float)(Data.Board.Height - 1) / 2 - indices[0] / Data.Board.Width), 0);
            shape.SetCellPosition(destination);
            shape.ChangeSortingOrder(false);
            BoardData.Shapes.Add(minIndex, shape.ShapeData);
            return true;
        }

        public Shape Grab()
        {
            Vector3 mousePosition = Graphics.GetMousePositionInMasterArea(), boardPosition = GetBoardPosition(BoardIndex);
            int x = (int)Math.Floor((mousePosition.x - boardPosition.x) / CellInterval + (float)(Data.Board.Width - 1) / 2 + .5f),
                y = (int)Math.Floor((float)(Data.Board.Height - 1) / 2 + .5f - (mousePosition.y - boardPosition.y) / CellInterval);
            if (x < 0 || x >= Data.Board.Width || y < 0 || y >= Data.Board.Height) return null;
            if (shapeInCell[x + y * Data.Board.Width] != null)
            {
                Shape shape = shapeInCell[x + y * Data.Board.Width];
                bool foundData = false;
                for (int i = 0; i < Data.Board.Size; i++)
                {
                    if (shapeInCell[i] == shape)
                    {
                        if (!foundData)
                        {
                            BoardData.Shapes.Remove(i);
                            foundData = true;
                        }
                        shapeInCell[i] = null;
                        cellObjects[i].SetActive(true);
                    }
                }
                shape.transform.parent = parentObject.transform;
                shape.MoveCenter(mousePosition - shape.transform.localPosition);
                shape.ChangeSortingOrder(true);
                shape.transform.localPosition = mousePosition;
                return shape;
            }
            return null;
        }

        // returns if the board is full
        public bool IsFull()
        {
            for (int i = 0; i < Data.Board.Size; i++)
            {
                if (shapeInCell[i] == null) return false;
            }
            return true;
        }
        public void ChangeColor(Data.Color[] result)
        {
            int count = 0;
            foreach (int key in BoardData.Shapes.Keys)
            {
                Shape shape = shapeInCell[key];
                for (int i = 0; i < Data.Board.Size; i++)
                {
                    if (shapeInCell[i] == shape) shapeInCell[i].ChangeColor(result[count]);
                }
                count++;
            }
        }
        public void ApplyColorblindMode()
        {
            for (int i = 0; i < Data.Board.Size; i++)
            {
                if (shapeInCell[i] != null) shapeInCell[i].ApplyColorblindMode();
            }
        }

        // Don't call this function for the answer board
        // since it assumes shapeInCell is fully synched
        public Data.Color?[] GetColors()
        {
            Data.Color?[] colors = new Data.Color?[Data.Board.Size];
            for (int i = 0; i < Data.Board.Size; i++)
            {
                if (shapeInCell[i] == null) colors[i] = null;
                else colors[i] = shapeInCell[i].Color;
            }
            return colors;
        }
    }

    private static GameObject grabbedShapeObject, parentObject;
    private static Shape grabbedShape;
    public static bool Grabbed { get; private set; }
    public static bool Win { get; private set; }
    public static bool AnswerShown { get; private set; }

    private static GameObject[] boardObjects;
    private static GameObject answerBoardObject;
    private static Board[] boards;
    private static Board answerBoard;
    public static int BoardIndex { get; private set; }
    private const float BoardX = 0, BoardY = .79f;
    public const int Width = 3, Height = 2;
    public const int GuessCount = Width * Height;
    public static bool Colorblind { get; set; }

    private static Vector3 GetBoardPosition(int index)
    {
        return new Vector3(BoardX + (index % Width - (float)(Width - 1) / 2) * (CellInterval * Data.Board.Width + .08f),
            BoardY + ((float)(Height - 1) / 2 - index / Width) * (CellInterval * Data.Board.Height + .08f), 0);
    }

    public static void PreInitialize(GameObject parentObject)
    {
        Guess.parentObject = parentObject;
        Grabbed = false;
        Win = false;
        Colorblind = false;

        boardObjects = new GameObject[GuessCount];
        answerBoardObject = General.AddChild(parentObject, "Answer Board");
        boards = new Board[GuessCount];
        answerBoard = answerBoardObject.AddComponent<Board>();
        answerBoardObject.transform.localPosition = new Vector3(BoardX, BoardY, 0);
        for (int i = 0; i < GuessCount; i++)
        {
            boardObjects[i] = General.AddChild(parentObject, $"Board{i}");
            boards[i] = boardObjects[i].AddComponent<Board>();
            boardObjects[i].transform.localPosition = GetBoardPosition(i);
        }
    }

    public static void Initialize()
    {
        BoardIndex = 0;
        for (int i = 0; i < GuessCount; i++) boards[i].ChangeCellSprites(i == BoardIndex);
        answerBoard.SetBoard(Data.Answer);
        answerBoardObject.SetActive(false);
        AnswerShown = false;
    }

    public static void Grab(Data.Shape shape)
    {
        if (Grabbed) return;

        grabbedShapeObject = General.AddChild(parentObject, Enum.GetName(typeof(Data.Shape.Type), shape._Type));
        grabbedShape = grabbedShapeObject.AddComponent<Shape>();
        grabbedShape.Initialize(shape, grabbed: true);
        Grabbed = true;
    }

    public static void HandleGrabbedShape()
    {
        if (BoardIndex >= GuessCount) return;

        if (!Grabbed && Input.GetMouseButtonDown(0))
        {
            grabbedShape = boards[BoardIndex].Grab();
            if (grabbedShape != null)
            {
                grabbedShapeObject = grabbedShape.gameObject;
                Grabbed = true;
            }
        }

        if (!Grabbed) return;

        grabbedShapeObject.transform.localPosition = Graphics.GetMousePositionInMasterArea();
        if (Input.GetMouseButtonUp(0))
        {
            if (!boards[BoardIndex].AddShape(grabbedShape))
            {
                grabbedShape.DestroyCells();
                UnityEngine.Object.Destroy(grabbedShapeObject);
            }
            else
            {
                grabbedShapeObject = null;
                grabbedShape = null;
            }
            Grabbed = false;
            VirtualKeyboard.ChangeKeyColor(9, boards[BoardIndex].IsFull() ? Data.Color.Unused : Data.Color.Wrong);
            VirtualKeyboard.ChangeKeyColor(14, boards[BoardIndex].BoardData.Shapes.Count > 0 ? Data.Color.Unused : Data.Color.Wrong);
        }
    }

    public static void ResetBoard()
    {
        boards[BoardIndex].Reset();
        VirtualKeyboard.ChangeKeyColor(14, Data.Color.Wrong);
    }

    public static bool MakeGuess()
    {
        if (BoardIndex >= GuessCount)
        {
            Debug.LogWarning($"Guess.MakeGuess: used all guesses");
            return false;
        }
        if (!boards[BoardIndex].IsFull()) return false;

        Data.Color[] result;
        try
        {
            result = Data.Board.Compare(boards[BoardIndex].BoardData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Guess.MakeGuess: error occured while comparing the guess: {e}");
            MainGame.NotifyError();
            return false;
        }
        boards[BoardIndex].ChangeColor(result);
        boards[BoardIndex].ChangeCellSprites(false);

        bool win = true;
        Data.Shape[] shapeData = new Data.Shape[result.Length];
        boards[BoardIndex].BoardData.Shapes.Values.CopyTo(shapeData, 0);
        for (int i = 0; i < result.Length; i++)
        {
            VirtualKeyboard.ChangeKeyColor((int)shapeData[i]._Type, result[i]);
            if (result[i] != Data.Color.Correct) win = false;
        }
        if (!win)
        {
            BoardIndex++;
            if (BoardIndex < GuessCount)
            {
                boards[BoardIndex].ChangeCellSprites(true);
                VirtualKeyboard.ChangeKeyColor(9, Data.Color.Wrong);
                VirtualKeyboard.ChangeKeyColor(14, Data.Color.Wrong);
            }
            else
            {
                for (int i = 9; i < 15; i++) VirtualKeyboard.ChangeKeyColor(i, Data.Color.Wrong);
                VirtualKeyboard.ChangeKeyColor(15, Data.Color.Unused);
            }
        }
        if (win)
        {
            for (int i = 9; i < 15; i++) VirtualKeyboard.ChangeKeyColor(i, Data.Color.Wrong);
            Win = true;
        }
        return win;
    }

    public static Data.Color?[] GetResultColors()
    {
        if (!Win && BoardIndex < GuessCount)
        {
            Debug.LogWarning($"Guess.GetResultColor: the game has not ended");
            return null;
        }

        int boardCount = Win ? (BoardIndex + 1) : GuessCount;
        Data.Color?[] results = new Data.Color?[Data.Board.Size * boardCount];
        for (int i = 0; i < boardCount; i++) boards[i].GetColors().CopyTo(results, Data.Board.Size * i);

        return results;
    }

    public static void ShowAnswer()
    {
        for (int i = 0; i < GuessCount; i++) boardObjects[i].SetActive(AnswerShown);
        AnswerShown = !AnswerShown;
        answerBoardObject.SetActive(AnswerShown);
    }

    public static void ApplyColorblindMode()
    {
        for (int i = 0; i < GuessCount; i++) boards[i].ApplyColorblindMode();
        answerBoard.ApplyColorblindMode();
        if (grabbedShape != null) grabbedShape.ApplyColorblindMode();
    }
}
