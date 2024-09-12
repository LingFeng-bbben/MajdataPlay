using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SlideGenerator : MonoBehaviour
{
    LineRenderer lineRenderer;
    public string type;
    public Sprite slide;
    public float step;
    public float rad;
    public bool showLine = false;
    public bool generate = false;
    // Start is called before the first frame update
    void Start()
    {
        if (showLine)
        {
            lineRenderer = GetComponent<LineRenderer>();
            var positions = new List<Vector3>();
            for (int i = 0; i <= 100; i++)
            {
                positions.Add((Vector2)GetPointAtPosition(type, i / 100f));
            }
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
        }
        if (generate)
        {
            GenerateSlides(type, step);
        }
    }

    void GenerateSlides(string type, float step)
    {

        for (float i = step; i < 1f; i +=step)
        {
            var result = GetPointAtPosition(type, i);
            var obj = new GameObject("Slide_"+(1f-i));
            obj.transform.parent = transform;
            obj.transform.position = (Vector2)result;
            obj.transform.rotation = Quaternion.Euler(0, 0, result.z);
            var rend = obj.AddComponent<SpriteRenderer>();
            rend.sortingLayerName = "Slides";
            rend.sprite = slide;
        }
    }

    Vector3 GetPointAtPosition(string type,float position)
    {
        if(type == "-")
        {
            var startPoint = GetPositionFromDistance(4.8f, 1);
            var endPoint = GetPositionFromDistance(4.8f, 7);
            var lerp = Vector2.Lerp(startPoint, endPoint, position);
            var vect = endPoint - startPoint;
            var angle = Mathf.Rad2Deg*Mathf.Atan2(vect.x, vect.y); 
            return new Vector3(lerp.x, lerp.y, -angle-90f);
        }
        else if(type == "q")
        {
            var start = 1;
            var end = 8;
            var startPoint = GetPositionFromDistance(4.8f, start);
            var endPoint = GetPositionFromDistance(rad, 7.5f);
            var vect = endPoint - startPoint;
            var curv_part = 0.75f;
            var line_s = vect.magnitude;
            var curv_s = Mathf.PI*rad*2f* curv_part;
            var lineseg = line_s / (line_s + curv_s + line_s);
            var curvseg = (line_s+ curv_s) / (line_s + curv_s + line_s);
            
            if (position < lineseg)
            {
                startPoint = GetPositionFromDistance(4.8f, start);
                endPoint = GetPositionFromDistance(rad, 7.5f);
                var lerp = Vector2.Lerp(startPoint, endPoint, position / lineseg);
                vect = endPoint - startPoint;
                var angle = Mathf.Rad2Deg * Mathf.Atan2(vect.x, vect.y);
                return new Vector3(lerp.x, lerp.y, angle+180);
            }
            else if (position < curvseg)
            {
                position = ((position- curvseg) /(curvseg-lineseg)) * 2f *Mathf.PI * -curv_part;
                position += 45f * Mathf.Deg2Rad;
                var circle = new Vector2(rad * Mathf.Sin(position), rad * Mathf.Cos(position));
                var angle = Mathf.Rad2Deg * Mathf.Atan2(circle.x, circle.y);
                return new Vector3(circle.x,circle.y, -angle);
            }
            else if (position <= 1f)
            {
                startPoint = GetPositionFromDistance(rad, end+1.5f);
                endPoint = GetPositionFromDistance(4.8f, end);
                var lerp = Vector2.Lerp(startPoint, endPoint, (position - curvseg) / lineseg);
                vect = endPoint - startPoint;
                var angle = Mathf.Rad2Deg * Mathf.Atan2(vect.x, vect.y);
                return new Vector3(lerp.x, lerp.y, -angle-90f);
            }
            /*position = position * 6.28f;
            var circle = new Vector2(rad*Mathf.Sin(position), rad*Mathf.Cos(position));
            return circle;*/
        }
        else if(type == ">")
        {
            var pos = (0.0625f + position * 0.125f)*2 * Mathf.PI;
            var circle = new Vector2(4.8f * Mathf.Sin(pos), 4.8f * Mathf.Cos(pos));
            var angle = Mathf.Rad2Deg * Mathf.Atan2(circle.x, circle.y);
            return new Vector3(circle.x, circle.y, -angle+180f);
        }
        return new Vector3();
    }

     Vector3 GetPositionFromDistance(float distance, float position)
    {
        return new Vector3(
            distance * Mathf.Cos((position * -2f + 5f) * 0.125f * Mathf.PI),
            distance * Mathf.Sin((position * -2f + 5f) * 0.125f * Mathf.PI));
    }
}
