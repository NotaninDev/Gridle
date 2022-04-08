using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = System.Random;

public static class Data
{
    public enum Color { Unused, Wrong, Far, Close, Correct }
    public static DateTime UtcTime { get; private set; }
    public static DateTime NextUtcDate { get; private set; }
    public static int Seed { get; private set; }
    public const int FixedYear = 2022, FixedMonth = 3, FixedDay = 7;

    public const bool RandomMode = false;

    public struct Shape
    {
        public enum Type { Monomino, Domino, TriominoI, TriominoL, TetrominoI, TetrominoL, TetrominoT, TetrominoO, TetrominoZ }
        public enum Rotation : byte { Zero = 0, One = 1, Two = 2, Three = 3 }
        public Type _Type { get; private set; }
        public Rotation _Rotation;
        public bool Flipped { get; private set; }
        public Shape(Type type, Rotation rotation, bool flipped)
        {
            _Type = type;
            _Rotation = rotation;
            Flipped = flipped;
        }

        // implement equality
        public override bool Equals(object obj) => obj is Shape other && this.Equals(other);
        public bool Equals(Shape s)
        {
            if (_Type != s._Type) return false;
            switch (_Type)
            {
                case Type.Monomino:
                    return true;
                case Type.Domino:
                    return (byte)_Rotation % 2 == (byte)s._Rotation % 2;
                case Type.TriominoI:
                    return (byte)_Rotation % 2 == (byte)s._Rotation % 2;
                case Type.TriominoL:
                    return Flipped == s.Flipped && _Rotation == s._Rotation ||
                        Flipped && !s.Flipped && ((byte)_Rotation - (byte)s._Rotation + (byte)4) % 4 == (byte)1 ||
                        !Flipped && s.Flipped && ((byte)_Rotation - (byte)s._Rotation + (byte)4) % 4 == (byte)3;
                case Type.TetrominoI:
                    return (byte)_Rotation % 2 == (byte)s._Rotation % 2;
                case Type.TetrominoL:
                    return _Rotation == s._Rotation && Flipped == s.Flipped;
                case Type.TetrominoT:
                    return Flipped == s.Flipped && _Rotation == s._Rotation ||
                        Flipped != s.Flipped && ((byte)_Rotation - (byte)s._Rotation + (byte)4) % 4 == (byte)2;
                case Type.TetrominoO:
                    return true;
                case Type.TetrominoZ:
                    return Flipped == s.Flipped && (byte)_Rotation % 2 == (byte)s._Rotation % 2;
                default:
                    Debug.LogWarning($"Data.Shape.Equals: not implemented for type {Enum.GetName(typeof(Type), _Type)}");
                    return _Rotation == s._Rotation && Flipped == s.Flipped;
            }
        }
        public override int GetHashCode() => (int)_Type;
        public static bool operator ==(Shape lhs, Shape rhs) => lhs.Equals(rhs);
        public static bool operator !=(Shape lhs, Shape rhs) => !(lhs == rhs);

        public override string ToString()
        {
            return $"{Enum.GetName(typeof(Type), _Type)}, {Enum.GetName(typeof(Rotation), _Rotation)}, {Flipped}";
        }
    }
    public class Board
    {
        public const int Width = 4, Height = 4, Size = Width * Height;

        public Dictionary<int, Shape> Shapes;
        public Board()
        {
            Shapes = new Dictionary<int, Shape>();
        }
        public void Reset()
        {
            Shapes.Clear();
        }
        public static Board Decode(string description)
        {
            Board board = new Board();
            bool[] used = new bool[Size];
            for (int i = 0; i < Size; i++) used[i] = false;

            // the main part of decode
            {
                char[] separators = new char[] { ' ', ',' };
                string[] shapeDescriptions = description.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                Match match = Regex.Match(description, @"(\b[RDLUX]+\b)", RegexOptions.Compiled);

                // cursor is the index of the top-left cell of the shape in the board
                int cursor = 0, count = 0;
                bool failed = false;
                while (match.Success)
                {
                    string shapeDescription = match.Value;
                    match = match.NextMatch();
                    switch (shapeDescription)
                    {
                        // Monomino
                        case "X":
                            used[cursor] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.Monomino, Shape.Rotation.Zero, false));
                            break;

                        // Domino
                        case "DXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.Domino, Shape.Rotation.Zero, false));
                            break;
                        case "RXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.Domino, Shape.Rotation.One, false));
                            break;

                        // TriominoI
                        case "DDXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width * 2] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TriominoI, Shape.Rotation.Zero, false));
                            break;
                        case "RRXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + 2] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TriominoI, Shape.Rotation.One, false));
                            break;

                        // TriominoL
                        case "RXDXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + Width] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TriominoL, Shape.Rotation.Zero, false));
                            break;
                        case "DRXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width + 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TriominoL, Shape.Rotation.One, false));
                            break;
                        case "DLXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width - 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TriominoL, Shape.Rotation.Two, false));
                            break;
                        case "RDXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + Width + 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TriominoL, Shape.Rotation.Three, false));
                            break;

                        // TetrominoI
                        case "DDDXXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width * 2] = true;
                            used[cursor + Width * 3] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoI, Shape.Rotation.Zero, false));
                            break;
                        case "RRRXXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + 2] = true;
                            used[cursor + 3] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoI, Shape.Rotation.One, false));
                            break;

                        // TetrominoL
                        case "RXDDXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width * 2] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoL, Shape.Rotation.Zero, false));
                            break;
                        case "DRRXXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width + 1] = true;
                            used[cursor + Width + 2] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoL, Shape.Rotation.One, false));
                            break;
                        case "DDLXXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width * 2] = true;
                            used[cursor + Width * 2 - 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoL, Shape.Rotation.Two, false));
                            break;
                        case "RRDXXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + 2] = true;
                            used[cursor + Width + 2] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoL, Shape.Rotation.Three, false));
                            break;
                        case "RDDXXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + Width + 1] = true;
                            used[cursor + Width * 2 + 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoL, Shape.Rotation.Zero, true));
                            break;
                        case "RRXXDXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + 2] = true;
                            used[cursor + Width] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoL, Shape.Rotation.One, true));
                            break;
                        case "DDRXXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width * 2] = true;
                            used[cursor + Width * 2 + 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoL, Shape.Rotation.Two, true));
                            break;
                        case "DLLXXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width - 1] = true;
                            used[cursor + Width - 2] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoL, Shape.Rotation.Three, true));
                            break;

                        // TetrominoT
                        case "DRXDXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width + 1] = true;
                            used[cursor + Width * 2] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoT, Shape.Rotation.Zero, false));
                            break;
                        case "DRXLXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width + 1] = true;
                            used[cursor + Width - 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoT, Shape.Rotation.One, false));
                            break;
                        case "DDXLXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width * 2] = true;
                            used[cursor + Width - 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoT, Shape.Rotation.Two, false));
                            break;
                        case "RRXDXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + 2] = true;
                            used[cursor + Width + 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoT, Shape.Rotation.Three, false));
                            break;

                        // TetrominoO
                        case "RDLXXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + Width + 1] = true;
                            used[cursor + Width] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoO, Shape.Rotation.Zero, false));
                            break;

                        // TetrominoZ
                        case "RDRXXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + Width + 1] = true;
                            used[cursor + Width + 2] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoZ, Shape.Rotation.Zero, false));
                            break;
                        case "DLDXXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width - 1] = true;
                            used[cursor + Width * 2 - 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoZ, Shape.Rotation.One, false));
                            break;
                        case "RXDLXXX":
                            used[cursor] = true;
                            used[cursor + 1] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width - 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoZ, Shape.Rotation.Zero, true));
                            break;
                        case "DRDXXXX":
                            used[cursor] = true;
                            used[cursor + Width] = true;
                            used[cursor + Width + 1] = true;
                            used[cursor + Width * 2 + 1] = true;
                            board.Shapes.Add(cursor, new Shape(Shape.Type.TetrominoZ, Shape.Rotation.One, true));
                            break;

                        default:
                            Debug.LogWarning($"Data.Board.Decode({description}): invalid shape description {shapeDescription}");
                            failed = true;
                            break;
                    }

                    // find the next cursor point
                    int start = cursor;
                    for (int j = start; j < Size; j++)
                    {
                        if (!used[j])
                        {
                            cursor = j;
                            break;
                        }
                    }
                    if (start == cursor && match.Success && !failed)
                    {
                        Debug.LogWarning($"Data.Board.Decode({description}): cursor did not move after {count}th shape description");
                        break;
                    }
                    count++;
                }

                // error check
                for (int i = 0; i < Size; i++)
                {
                    if (!used[i])
                    {
                        Debug.LogError($"Data.Board.Decode({description}): unused cell found at {i}");
                        return null;
                    }
                }
            }

            return board;
        }

        // the array size of the return value is same as the number of shapes in guess
        public static Color[] Compare(Board guess)
        {
            int[] guessIndices = new int[guess.Shapes.Count], answerIndices = new int[Answer.Shapes.Count];
            Shape[] shapes = new Shape[guess.Shapes.Count];
            guess.Shapes.Keys.CopyTo(guessIndices, 0);
            Answer.Shapes.Keys.CopyTo(answerIndices, 0);
            guess.Shapes.Values.CopyTo(shapes, 0);

            Color[] result = new Color[guess.Shapes.Count];
            bool[] guessMatched = new bool[guess.Shapes.Count], answerMatched = new bool[Answer.Shapes.Count];
            for (int i = 0; i < guess.Shapes.Count; i++)
            {
                result[i] = Color.Wrong;
                guessMatched[i] = false;
            }
            for (int i = 0; i < Answer.Shapes.Count; i++) answerMatched[i] = false;

            // correct
            for (int i = 0; i < Answer.Shapes.Count; i++)
            {
                if (guess.Shapes.ContainsKey(answerIndices[i]) && guess.Shapes[answerIndices[i]] == Answer.Shapes[answerIndices[i]])
                {
                    int index = 0;
                    for (int j = 0; j < guessIndices.Length; j++)
                    {
                        if (guessIndices[j] == answerIndices[i])
                        {
                            index = j;
                            break;
                        }
                    }
                    result[index] = Color.Correct;
                    guessMatched[index] = true;
                    answerMatched[i] = true;
                }
            }

            // close
            for (int i = 0; i < Answer.Shapes.Count; i++)
            {
                if (answerMatched[i]) continue;

                // find the minimum shape index in guess that is close to the shape in Answer
                int minJ = Size;
                for (int j = 0; j < shapes.Length; j++)
                {
                    if (!guessMatched[j] && shapes[j] == Answer.Shapes[answerIndices[i]])
                    {
                        if (minJ == Size || guessIndices[j] < guessIndices[minJ]) minJ = j;
                    }
                }
                if (minJ < Size)
                {
                    result[minJ] = Color.Close;
                    guessMatched[minJ] = true;
                    answerMatched[i] = true;
                }
            }

            // far
            for (int i = 0; i < Answer.Shapes.Count; i++)
            {
                if (answerMatched[i]) continue;

                // find the minimum shape index in guess that is close to the shape in Answer
                int minJ = Size;
                for (int j = 0; j < shapes.Length; j++)
                {
                    if (!guessMatched[j] && shapes[j]._Type == Answer.Shapes[answerIndices[i]]._Type)
                    {
                        if (minJ == Size || guessIndices[j] < guessIndices[minJ]) minJ = j;
                    }
                }
                if (minJ < Size)
                {
                    result[minJ] = Color.Far;
                    guessMatched[minJ] = true;
                }
            }

            return result;
        }
    }

    public static Board Answer { get; private set; }
    public static void LoadRandomBoard(int seed = 0)
    {
        UtcTime = DateTime.UtcNow;
        NextUtcDate = new DateTime(UtcTime.Year, UtcTime.Month, UtcTime.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
        DateTime FixedDate = new DateTime(FixedYear, FixedMonth, FixedDay, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan interval = UtcTime - FixedDate;
        Seed = RandomMode ? -1 : (seed == 0 ? interval.Days : seed);

        string filePath = RandomMode ? $"Library/Debug_4x4_Max4_MonoMax1" : $"Library/4x4_Max4_MonoMax1";
        TextAsset textFile = Resources.Load<TextAsset>(filePath);
        string fileText;
        if (textFile == null)
        {
            Debug.LogWarning($"Data.LoadRandomBoard: failed to load file {filePath}");
            fileText = "";
        }
        else fileText = textFile.text;

        Match lines = Regex.Match(fileText, "^(.*?)$", RegexOptions.Multiline | RegexOptions.Compiled);
        int boardCount;
        if (!lines.Success)
        {
            Debug.LogWarning($"Data.LoadRandomBoard: failed to match lines");
            return;
        }
        if (!int.TryParse(lines.Value, out boardCount))
        {
            Debug.LogWarning($"Data.LoadRandomBoard: invalid board count: {lines.Value}");
            return;
        }

        Random seededRandom = RandomMode ? new Random() : new Random(Seed);
        int boardIndex = seededRandom.Next(boardCount);
        for (int i = 0; i < boardIndex; i++)
        {
            lines = lines.NextMatch();
            if (!lines.Success)
            {
                Debug.LogWarning($"Data.LoadRandomBoard: failed to read line {i + 2}");
                return;
            }
        }

        if (RandomMode) Debug.Log($"random board: {lines.Value}");
        Answer = Board.Decode(lines.Value);
        if (Answer == null) MainGame.NotifyError();
    }
    public static void AssignBoard(string description)
    {
        UtcTime = DateTime.UtcNow;
        NextUtcDate = new DateTime(UtcTime.Year, UtcTime.Month, UtcTime.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
        Seed = 0;

        Debug.Log($"assigned board: {description}");
        Answer = Board.Decode(description);
        if (Answer == null) MainGame.NotifyError();
    }
}
