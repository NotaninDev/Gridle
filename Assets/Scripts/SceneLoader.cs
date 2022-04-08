using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private const int row = 4, column = 7, squareCount = row * column;
    private static GameObject[] intermediateObjects, squareObjects;
    private static SpriteRenderer[] squareRenderers;
    private static float squareSize;

    private static bool loading = false;
    public static bool Loading { get { return loading; } }

    public class StringEvent : UnityEvent<string> { }
    public static StringEvent sceneEvent;

    void Awake()
    {
        // initialize squares for scene transition
        intermediateObjects = new GameObject[squareCount * 2];
        squareObjects = new GameObject[squareCount * 2];
        squareRenderers = new SpriteRenderer[squareCount * 2];
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                int k = i * column + j;
                intermediateObjects[k] = General.AddChild(gameObject, "Intermediate Parent" + k.ToString());
                intermediateObjects[squareCount + k] = General.AddChild(intermediateObjects[k], "Intermediate Child" + k.ToString());
                squareObjects[k] = General.AddChild(intermediateObjects[k], "Square Parent" + k.ToString());
                squareObjects[squareCount + k] = General.AddChild(intermediateObjects[squareCount + k], "Square Child" + k.ToString());
                squareRenderers[k] = squareObjects[k].AddComponent<SpriteRenderer>();
                squareRenderers[squareCount + k] = squareObjects[squareCount + k].AddComponent<SpriteRenderer>();
                intermediateObjects[squareCount + k].transform.localScale = new Vector3(.8f, .8f, 1);
                squareRenderers[k].sortingLayerName = "Border";
                squareRenderers[k].sortingOrder = 0 + (k % 2 == 0 ? 0 : 4) + (i + j) % 4 / 2 * 2;
                squareRenderers[squareCount + k].sortingLayerName = "Border";
                squareRenderers[squareCount + k].sortingOrder = 1 + (k % 2 == 0 ? 0 : 4) + (i + j) % 4 / 2 * 2;
            }
        }
        loading = false;

        sceneEvent = new StringEvent();
        sceneEvent.AddListener(LoadNextScene);
        Keyboard.Initialize();
        StartCoroutine(LoadFirstScene());
    }

    void Start()
    {
        float spriteWidth = Graphics.plainWhite.bounds.size.x;
        SceneLoader.squareSize = Graphics.Height * .37f;
        Vector3 squareScale = new Vector3(SceneLoader.squareSize / spriteWidth, SceneLoader.squareSize / spriteWidth, 1);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                int k = i * column + j;
                squareRenderers[k].sprite = Graphics.plainWhite;
                squareRenderers[k].color = Graphics.Brown;
                squareRenderers[squareCount + k].sprite = Graphics.plainWhite;
                squareRenderers[squareCount + k].color = Graphics.LightBrown;
                intermediateObjects[k].transform.localPosition = new Vector3(Graphics.Width / column * ((column - 1) * -.5f + j),
                    Graphics.Height / row * ((row - 1) * -.5f + i), 0);
                intermediateObjects[squareCount + k].transform.localPosition = new Vector3(Graphics.Width * -.01f, Graphics.Height * .01f, 0);
                squareObjects[k].transform.localScale = squareScale;
                squareObjects[squareCount + k].transform.localScale = squareScale;
                intermediateObjects[k].SetActive(false);
            }
        }
    }

    private IEnumerator LoadFirstScene()
    {
        loading = true;
        GameManager.currentScene = "MainScene";
        SceneManager.LoadScene("MainScene", LoadSceneMode.Additive);
        yield return null;
        Camera.main.gameObject.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainScene"));
        loading = false;
    }

    public void LoadNextScene(string sceneName)
    {
        if (loading)
        {
            Debug.LogWarning("LoadNextScene: loading another scene now");
            return;
        }
        loading = true;
        StartCoroutine(_LoadNextScene(sceneName));
    }
    private IEnumerator _LoadNextScene(string sceneName)
    {
        Coroutine[] coroutines = new Coroutine[squareCount * 2];
        UnityEngine.AsyncOperation asyncLoad, asyncUnload;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                int k = i * column + j;
                float delay = .06f * (row - 1 - i) + .04f * j;
                intermediateObjects[k].SetActive(true);
                intermediateObjects[k].transform.localScale = Vector3.zero;
                StartCoroutine(Graphics.Resize(intermediateObjects[k], Vector3.zero, Vector3.one, duration: .5f, delay: delay));
                //coroutines[k] = StartCoroutine(Graphics.Rotate(squareObjects[k], 0, 0, 120, lap: -1, delay: 0));
                //coroutines[squareCount + k] = StartCoroutine(Graphics.Rotate(squareObjects[squareCount + k], -5, 0, 120, lap: -1, delay: 0));
            }
        }
        yield return new WaitForSeconds(.5f + .06f * row + .04f * column);

        // load and unload scenes
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("BorderScene"));
        GameManager.previousScene = GameManager.currentScene;
        asyncUnload = SceneManager.UnloadSceneAsync(activeScene);
        asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        GameManager.currentScene = sceneName;
        while (!asyncLoad.isDone || !asyncUnload.isDone) { yield return null; }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                int k = i * column + j;
                float delay = .06f * (row - 1 - i) + .04f * j;
                StartCoroutine(Graphics.Resize(intermediateObjects[k], Vector3.one, Vector3.zero, duration: .5f, delay: delay));
            }
        }
        yield return new WaitForSeconds(.5f + .06f * row + .04f * column);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                int k = i * column + j;
                //StopCoroutine(coroutines[k]);
                //StopCoroutine(coroutines[squareCount + k]);
                intermediateObjects[k].SetActive(false);
            }
        }
        loading = false;
    }
}
