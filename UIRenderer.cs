using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro

public class UIRenderer : MonoBehaviour
{
    public static Dictionary<string, object>[] detections; // this is so bad but it works 
    public static long lastDetectionTime;

    private Material redMaterial;
    private Material grayMaterial;
    private RectTransform rectTransform;
    private float p = 0;

    private void Awake()
    {
        // Create a simple red material
        redMaterial = new Material(Shader.Find("UI/Default")); // im just gonna use this hehe
        redMaterial.color = Color.red;
        grayMaterial = new Material(Shader.Find("UI/Default")); // im just gonna use this hehe
        grayMaterial.color = Color.gray;
    }

    void Start()
    {
        //StartCoroutine(DrawAndUndrawRectangles());
    }

    private void Update()
    {
        // just gonna use update for drawing frames and stuff
        // clear all rectangles
        ClearRectRaw();
        ClearTextRaw();
        // update stuff 
        rectTransform = GetComponent<RectTransform>();
        if (p < 0.5f)
        {
            p += 0.001f;
        }
        else
        {
            p = 0;
        }
        long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        if (now - lastDetectionTime < 5000 && detections != null)
        {
            foreach (Dictionary<string, object> detection in detections)
            {
                float[] box = (float[])((JArray)detection["box"]).ToObject(typeof(float[]));
                float x = box[0] - 0.5f;
                float y = (box[1] - 0.5f);
                float w = box[2];
                float h = box[3];
                //DrawTextNormalized(x, y, 3f, 0.05f, Color.red, (string)detection["class"]);
                DrawRectBoxNormalized(x, y, x + w, h);
            }
        }
        //DrawRectBoxNormalized(-0.5f, -0.5f, 0.9f, 0.9f);
        // draw stuff
        //DrawRectBoxNormalized(-0.1f, -0.05f, 0.2f, 0.1f);
        //DrawTextNormalized(0f, 0f, 3f, 0.1f, Color.white, "deez");
        //DrawRectFilledNormalized(-0.2f, -0.1f, 0.15f, 0.2f);
        //DrawTextNormalized(-0.19f, -0.095f, 0.125f, 0.02f, Color.white, "Unity");
        //DrawTextNormalized(-0.19f, -0.07f, 0.125f, 0.01f, Color.white, "Epic text for Unity popup panel thingy in vr and stuff idk");
    }

    private void DrawRectBoxNormalized(float x, float y, float w, float h)
    {
        // Scale xywh to world coords
        float ww = w * rectTransform.sizeDelta.x;
        float wh = h * rectTransform.sizeDelta.y;
        float wx = x * rectTransform.sizeDelta.x + ww / 2;
        float wy = y * rectTransform.sizeDelta.y + wh / 2;
        DrawRectBoxRaw(new Vector2(wx, wy), new Vector2(ww, wh));
    }

    private void DrawTextNormalized(float x, float y, float w, float size, Color color, string text)
    {
        RectTransform canvasRectTransform = GetComponent<RectTransform>();

        // Create TextMeshPro object
        GameObject textObj = new GameObject("TextMeshProObj");
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = size;
        tmpText.color = color;
        //tmpText.alignment = TextAlignmentOptions.Center; // Align text to center

        // Set the parent to the canvas with worldPositionStays set to false to keep the local orientation.
        textObj.transform.SetParent(canvasRectTransform, false);

        // Set the anchors and pivot to position the text correctly.
        // Pivot would be set to (0.5, 1) if we wanted to make it so that x is center (0.5) and y is top (1).
        RectTransform rectTransform = tmpText.rectTransform;
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);

        // Convert the normalized position \
        float anchoredX = x * canvasRectTransform.sizeDelta.x;// + canvasRectTransform.sizeDelta.x / 2;
        float anchoredY = -y * canvasRectTransform.sizeDelta.y;

        // Apply the computed position
        rectTransform.localPosition = new Vector2(anchoredX, anchoredY);

        // Set the size of the text box based on the width parameter 'w'
        rectTransform.sizeDelta = new Vector2(w * canvasRectTransform.sizeDelta.x, size);

        // Set the name to identify it when clearing
        textObj.name = "EpicTextThingy";
    }



    private IEnumerator DrawAndUndrawRectangles()
    {
        while (true)
        {
            // Draw 1 rectangle
            Debug.Log("Drawing!");
            DrawRectBoxRaw(new Vector2(0f, 0f), new Vector2(0.2f, 0.1f));
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Clearing!");
            ClearRectRaw();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void DrawRectBoxRaw(Vector2 centerPosition, Vector2 size)
    {
        float edgeThickness = 0.003f; // Set the thickness of your edges

        // Calculate half-thickness to offset the edge positions so they meet at corners
        float halfThickness = edgeThickness / 2;

        // Create GameObjects for the edges of the rectangle
        // Adjusted the positions so the inner edges line up exactly at the corners
        CreateEdgeRaw(new Vector2(centerPosition.x, centerPosition.y + size.y / 2 - halfThickness), new Vector3(size.x, edgeThickness, 1), 0); // Top edge
        CreateEdgeRaw(new Vector2(centerPosition.x, centerPosition.y - size.y / 2 + halfThickness), new Vector3(size.x, edgeThickness, 1), 0); // Bottom edge
        CreateEdgeRaw(new Vector2(centerPosition.x - size.x / 2 + halfThickness, centerPosition.y), new Vector3(edgeThickness, size.y, 1), 0); // Left edge
        CreateEdgeRaw(new Vector2(centerPosition.x + size.x / 2 - halfThickness, centerPosition.y), new Vector3(edgeThickness, size.y, 1), 0); // Right edge
    }

    private void CreateEdgeRaw(Vector2 position, Vector3 scale, float zPosition)
    {
        GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(edge.GetComponent<Collider>()); // Remove the collider component if not needed
        edge.transform.SetParent(transform, false);
        edge.transform.localPosition = new Vector3(position.x, position.y, zPosition);
        edge.transform.localScale = scale;
        edge.GetComponent<Renderer>().material = redMaterial;
        edge.name = "EpicEdgeThingy";
    }

    private void DrawRectFilledNormalized(float x, float y, float w, float h)
    {
        // Scale xywh to world coords
        float ww = w * rectTransform.sizeDelta.x;
        float wh = h * rectTransform.sizeDelta.y;
        float wx = x * rectTransform.sizeDelta.x + ww / 2;
        float wy = y * rectTransform.sizeDelta.y + wh / 2;
        DrawRectFilledRaw(new Vector2(wx, wy), new Vector2(ww, wh));
    }

    private void DrawRectFilledRaw(Vector2 centerPosition, Vector2 size)
    {
        GameObject filledRect = new GameObject("FilledRectangle");
        filledRect.transform.SetParent(transform, false);

        // Set the name to identify it when clearing
        filledRect.name = "EpicFilledRectThingy";

        // Create a RectTransform for positioning
        RectTransform rectTransform = filledRect.AddComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(centerPosition.x, centerPosition.y, 0);
        rectTransform.sizeDelta = size;

        // Create a CanvasRenderer and apply the material
        CanvasRenderer renderer = filledRect.AddComponent<CanvasRenderer>();
        Image image = filledRect.AddComponent<Image>();
        image.material = grayMaterial;
        image.color = grayMaterial.color;
    }


    private void ClearRectRaw()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "EpicEdgeThingy" || child.name == "EpicFilledRectThingy")
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ClearTextRaw()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "EpicTextThingy")
            {
                Destroy(child.gameObject);
            }
        }
    }

}