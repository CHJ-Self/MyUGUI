using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class MyShortcutKey : MonoBehaviour
{
    [MenuItem("MyCommands/Sort Name Copy Command %#d")]
    static void SortNameCopy()
    {
        if (Selection.gameObjects[0] != null)
        {
            //先获取最大的数
            int endNum = 0;
            foreach (GameObject go in Selection.gameObjects)
            {
                if (Regex.IsMatch(go.name, "[0-9]+$"))
                {
                    string endStr = Regex.Replace(go.name, @"[^0-9]+", "");
                    int endNum_Temp = int.Parse(endStr);
                    if (endNum_Temp > endNum)
                    {
                        endNum = endNum_Temp;
                    }
                }
            }
            foreach (GameObject go in Selection.gameObjects)
            {
                Transform parent = go.transform.parent;
                GameObject newGameObject = Instantiate(go);
                newGameObject.transform.parent = parent;
                newGameObject.transform.localScale = Vector3.one;
                if (newGameObject.GetComponent<RectTransform>() == null)
                {
                    newGameObject.AddComponent<RectTransform>();
                }
                newGameObject.transform.localPosition = Vector3.zero;
                newGameObject.GetComponent<RectTransform>().localScale = go.GetComponent<RectTransform>().localScale;
                newGameObject.GetComponent<RectTransform>().sizeDelta = go.GetComponent<RectTransform>().sizeDelta;
                newGameObject.GetComponent<RectTransform>().anchoredPosition = go.GetComponent<RectTransform>().anchoredPosition;
                newGameObject.GetComponent<RectTransform>().anchorMin = go.GetComponent<RectTransform>().anchorMin;
                newGameObject.GetComponent<RectTransform>().anchorMax = go.GetComponent<RectTransform>().anchorMax;
                newGameObject.GetComponent<RectTransform>().pivot = go.GetComponent<RectTransform>().pivot;
                if (Regex.IsMatch(go.name, "[0-9]+$"))
                {
                    string endStr = Regex.Replace(go.name, @"[^0-9]+", "");
                    newGameObject.name = go.name.Replace(endStr, "") + (endNum + 1).ToString().PadLeft(endStr.Length, '0');
                }
                Undo.RegisterCreatedObjectUndo(newGameObject, "创建GameObject : " + newGameObject.name);
                Selection.objects = new Object[] { newGameObject };
                endNum++;
            }
        }
    }

    [MenuItem("MyCommands/Same Name Copy Command %&d")]
    static void SameNameCopy()
    {
        if (Selection.gameObjects[0] != null)
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Transform parent = go.transform.parent;
                GameObject newGameObject = Instantiate(go);
                newGameObject.transform.parent = parent;
                newGameObject.transform.localScale = Vector3.one;
                if (newGameObject.GetComponent<RectTransform>() == null)
                {
                    newGameObject.AddComponent<RectTransform>();
                }
                newGameObject.transform.localPosition = Vector3.zero;
                newGameObject.GetComponent<RectTransform>().localScale = go.GetComponent<RectTransform>().localScale;
                newGameObject.GetComponent<RectTransform>().sizeDelta = go.GetComponent<RectTransform>().sizeDelta;
                newGameObject.GetComponent<RectTransform>().anchoredPosition = go.GetComponent<RectTransform>().anchoredPosition;
                newGameObject.GetComponent<RectTransform>().anchorMin = go.GetComponent<RectTransform>().anchorMin;
                newGameObject.GetComponent<RectTransform>().anchorMax = go.GetComponent<RectTransform>().anchorMax;
                newGameObject.GetComponent<RectTransform>().pivot = go.GetComponent<RectTransform>().pivot;
                newGameObject.name = go.name;
                Undo.RegisterCreatedObjectUndo(newGameObject, "创建GameObject : " + newGameObject.name);
                Selection.objects = new Object[] { newGameObject };
            }
        }
    }
}