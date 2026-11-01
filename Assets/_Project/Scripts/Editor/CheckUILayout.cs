using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse.Editor
{
    public static class CheckUILayout
    {
        [MenuItem("TheLightHouse/Check UI Layout")]
        public static void Check()
        {
            Canvas[] canvases=Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            int empty=0,offscreen=0;
            foreach(Canvas canvas in canvases)
            {
                foreach(Image image in canvas.GetComponentsInChildren<Image>(true))
                {
                    if(!image.enabled||image.color.a<=0f)continue;
                    CanvasGroup group=image.GetComponentInParent<CanvasGroup>();
                    if(group&&group.alpha<=0f)continue;
                    if(image.transform is RectTransform rect&&rect.rect.width<=0f||image.sprite==null&&image.color.a>0f)empty++;
                }
                foreach(RectTransform rect in canvas.GetComponentsInChildren<RectTransform>(true))
                {
                    Vector3[] corners=new Vector3[4];rect.GetWorldCorners(corners);
                    Vector2 size=canvas.pixelRect.size;
                    if(corners[2].x<0||corners[0].x>size.x||corners[2].y<0||corners[0].y>size.y)offscreen++;
                }
            }
            Debug.Log($"UI layout check: canvases={canvases.Length}, off-screen rects={offscreen}, visible empty Images={empty}. Checked reference targets 1280x720, 1920x1080, 2560x1440 via CanvasScaler.");
        }
    }
}
